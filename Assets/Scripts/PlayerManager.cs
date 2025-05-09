using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System;

public class PlayerManager : MonoBehaviour
{
    public float walkSpeed = 5f; // Yürüme hýzý
    public float runSpeed = 10f; // Koþma hýzý
    [SerializeField] private float maxHealth = 100f; // Maksimum can
    private float playerHealth; // Mevcut can
    private Rigidbody _rb; // Oyuncunun fiziksel gövdesi
    private Vector3 moveDirection; // Hareket yönü
    private float currentSpeed; // Mevcut hýz

    [SerializeField] private GoldManager goldManager; // Altýn yönetimi
    [SerializeField] private float _jumpForce; // Zýplama kuvveti
    [SerializeField] private bool _canJump; // Zýplama izni
    [SerializeField] private float _jumpCoolDown; // Zýplama bekleme süresi
    [SerializeField] private float _playerHeight; // Oyuncu yüksekliði
    [SerializeField] private LayerMask _groundLayer; // Zemin katmaný
    [SerializeField] private HintDoorManager hintDoorManager; // Ýpucu kapý yönetimi
    [SerializeField] private UIManager uiManager; // UI yönetimi
    [SerializeField] private Transform cameraTransform; // Kamera transformu

    // Saldýrý için menzil ve hasar
    [SerializeField] private float attackRange = 2f; // Saldýrý menzili
    [SerializeField] private float attackDamage = 20f; // Saldýrý hasarý

    public Animator animator; // Oyuncu animatörü

    // Animasyon hash ID'leri
    private int idleAnimID;
    private int walkAnimID;
    private int runAnimID;
    private int jumpAnimID;
    private int takeDamageAnimID;
    private int kickAnimID;
    private int punchAnimID;
    private int deathID;

    // Durum deðiþkenleri
    private bool isJumping; // Zýplýyor mu?
    private bool isAttacking; // Saldýrýyor mu?
    private bool isTakingDamage; // Hasar alýyor mu?
    private bool isDead; // Öldü mü?

    // Oyuncunun öldüðünü bildiren event
    public static event Action OnPlayerDied;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("Kamera bulunamadý!");
            }
        }

        playerHealth = maxHealth; // Caný baþlangýçta tam doldur
        isDead = false; // Oyuncu baþlangýçta canlý
        _canJump = true; // Zýplama izni baþlangýçta açýk

        // Animasyon hash ID'lerini al
        idleAnimID = Animator.StringToHash("Idle");
        walkAnimID = Animator.StringToHash("Walk");
        runAnimID = Animator.StringToHash("Run");
        jumpAnimID = Animator.StringToHash("Jump");
        takeDamageAnimID = Animator.StringToHash("TakeDamage");
        kickAnimID = Animator.StringToHash("Kick");
        punchAnimID = Animator.StringToHash("Punch");
        deathID = Animator.StringToHash("Death");
    }

    private void Start()
    {
        UpdateHealthUI(); // UI'da caný güncelle
    }

    private void Update()
    {
        if (isDead) return; // Oyuncu öldüyse input alma
        SetInputs();
    }

    private void FixedUpdate()
    {
        if (isDead) return; // Oyuncu öldüyse hareket etme
        SetPlayerMovement();
    }

    private void SetInputs()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = Vector3.zero;
        if (cameraTransform != null)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward = cameraForward.normalized;
            cameraRight = cameraRight.normalized;

            moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        }
        else
        {
            moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        }

        bool isMoving = moveDirection.magnitude > 0.1f;
        bool isGrounded = IsGrounded();

        // Hareket yönünü güncelle (havadayken de çalýþsýn)
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = runSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }
        }
        else
        {
            currentSpeed = 0f;
        }

        // Öncelik 1: Saldýrý durumu (Q ve E tuþlarý)
        if (Input.GetKeyDown(KeyCode.Q) && isGrounded)
        {
            isAttacking = true;
            isTakingDamage = false; // Hasar animasyonu kesiliyor
            isJumping = false; // Zýplama kesiliyor
            animator.Play(punchAnimID);
            AttackEnemy(); // Düþmana hasar ver
            return;
        }
        else if (Input.GetKeyDown(KeyCode.E) && isGrounded)
        {
            isAttacking = true;
            isTakingDamage = false; // Hasar animasyonu kesiliyor
            isJumping = false; // Zýplama kesiliyor
            animator.Play(kickAnimID);
            AttackEnemy(); // Düþmana hasar ver
            return;
        }

        if (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if ((stateInfo.shortNameHash == kickAnimID || stateInfo.shortNameHash == punchAnimID) && stateInfo.normalizedTime >= 1f)
            {
                isAttacking = false;
                UpdateMovementAnimation(isMoving, isGrounded);
            }
            return;
        }

        // Öncelik 2: Hasar alma durumu
        if (isTakingDamage)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == takeDamageAnimID && stateInfo.normalizedTime >= 1f)
            {
                isTakingDamage = false;
                UpdateMovementAnimation(isMoving, isGrounded);
            }
            return;
        }

        // Öncelik 3: Zýplama durumu
        if (isJumping)
        {
            if (isGrounded && _rb.linearVelocity.y <= 0)
            {
                isJumping = false;
                UpdateMovementAnimation(isMoving, isGrounded);
            }
            return;
        }

        // Yeni giriþ: Zýplama
        if (Input.GetKey(KeyCode.Space) && _canJump && isGrounded)
        {
            _canJump = false;
            isJumping = true;
            SetPlayerJumping();
            animator.Play(jumpAnimID);
            Invoke(nameof(ResetJumping), _jumpCoolDown);
            return;
        }

        // Normal hareket durumu
        UpdateMovementAnimation(isMoving, isGrounded);
    }

    private void AttackEnemy()
    {
        // Sahnedeki tüm düþmanlarý bul
        EnemyManager[] enemies = FindObjectsByType<EnemyManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (EnemyManager enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy <= attackRange)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    private void UpdateMovementAnimation(bool isMoving, bool isGrounded)
    {
        if (!isGrounded || isJumping || isAttacking || isTakingDamage)
        {
            return; // Havadayken, saldýrý veya hasar alma sýrasýnda hareket animasyonlarýný deðiþtirme
        }

        if (isMoving)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                animator.Play(runAnimID);
            }
            else
            {
                animator.Play(walkAnimID);
            }
        }
        else
        {
            animator.Play(idleAnimID);
        }
    }

    private void SetPlayerMovement()
    {
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = _rb.linearVelocity.y;
        _rb.linearVelocity = velocity;
    }

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }

    private void SetPlayerJumping()
    {
        Vector3 v = _rb.linearVelocity;
        if (v.y < 0f) v.y = 0f;
        _rb.linearVelocity = v;
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    private void ResetJumping()
    {
        _canJump = true;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _groundLayer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return; // Ölü oyuncunun çarpýþmalarý dikkate alýnmaz
        if (other.CompareTag("Gold"))
        {
            other.gameObject.SetActive(false);
            if(goldManager!=null)
            {
                goldManager.AddGold(10);
            }
            else
            {
                Debug.LogWarning("GoldManager null!!");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return; // Ölü oyuncunun çarpýþmalarý dikkate alýnmaz
        if (collision.collider.CompareTag("HintDoor"))
        {
            hintDoorManager.ChangeColorAndOpenWay();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // Ölü oyuncuya hasar verme

        playerHealth -= damage;
        playerHealth = Mathf.Max(playerHealth, 0);

        // Eðer saldýrý animasyonu oynuyorsa, hasar animasyonunu oynatma
        if (!isAttacking && playerHealth > 0)
        {
            isTakingDamage = true;
            isJumping = false; // Zýplama kesiliyor
            animator.Play(takeDamageAnimID);
        }

        UpdateHealthUI();
        Debug.Log($"Oyuncu {damage} hasar aldý! Kalan can: {playerHealth}");

        if (playerHealth <= 0)
        {
            Die(); // Ölüm metodunu çaðýr
        }
    }

    private void UpdateHealthUI()
    {
        uiManager.UpdateHealthText(playerHealth); // Can UI'sini güncelle
    }

    private void Die()
    {
        if (isDead) return; // Zaten öldüyse tekrar çalýþtýrma
        isDead = true;

        Debug.Log("OYUNCU ÖLDÜ!");

        // Tüm durumlarý sýfýrla
        isAttacking = false;
        isJumping = false;
        isTakingDamage = false;
        currentSpeed = 0f;
        moveDirection = Vector3.zero;

        // Fiziksel hareketleri durdur
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true; // Fiziksel etkileþimleri kapat

        // Ölüm animasyonunu oynat
        animator.Play(deathID);

        // Düþmanlara oyuncunun öldüðünü bildir
        OnPlayerDied?.Invoke();

        // Ölüm animasyonu bittikten sonra eylemleri baþlat
        StartCoroutine(HandleDeathAnimation());
    }

    private IEnumerator HandleDeathAnimation()
    {
        // Ölüm animasyonunun süresini al
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSecondsRealtime(stateInfo.length);

        // Animasyon bitti, oyun bitti ekranýný göster
        uiManager.ShowGameOverScreen(); // UIManager'da oyun bitti ekranýný göster
        // Alternatif: Sahneyi yeniden yüklemek için
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // EnemyManager için eklenen getter (isteðe baðlý, artýk event kullanýyoruz)
    public bool IsDead()
    {
        return isDead;
    }

    // Saldýrý menzilini görselleþtirmek için
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}