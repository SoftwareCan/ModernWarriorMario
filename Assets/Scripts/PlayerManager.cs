using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System;

public class PlayerManager : MonoBehaviour
{
    public float walkSpeed = 5f; // Y�r�me h�z�
    public float runSpeed = 10f; // Ko�ma h�z�
    [SerializeField] private float maxHealth = 100f; // Maksimum can
    private float playerHealth; // Mevcut can
    private Rigidbody _rb; // Oyuncunun fiziksel g�vdesi
    private Vector3 moveDirection; // Hareket y�n�
    private float currentSpeed; // Mevcut h�z

    [SerializeField] private GoldManager goldManager; // Alt�n y�netimi
    [SerializeField] private float _jumpForce; // Z�plama kuvveti
    [SerializeField] private bool _canJump; // Z�plama izni
    [SerializeField] private float _jumpCoolDown; // Z�plama bekleme s�resi
    [SerializeField] private float _playerHeight; // Oyuncu y�ksekli�i
    [SerializeField] private LayerMask _groundLayer; // Zemin katman�
    [SerializeField] private HintDoorManager hintDoorManager; // �pucu kap� y�netimi
    [SerializeField] private UIManager uiManager; // UI y�netimi
    [SerializeField] private Transform cameraTransform; // Kamera transformu

    // Sald�r� i�in menzil ve hasar
    [SerializeField] private float attackRange = 2f; // Sald�r� menzili
    [SerializeField] private float attackDamage = 20f; // Sald�r� hasar�

    public Animator animator; // Oyuncu animat�r�

    // Animasyon hash ID'leri
    private int idleAnimID;
    private int walkAnimID;
    private int runAnimID;
    private int jumpAnimID;
    private int takeDamageAnimID;
    private int kickAnimID;
    private int punchAnimID;
    private int deathID;

    // Durum de�i�kenleri
    private bool isJumping; // Z�pl�yor mu?
    private bool isAttacking; // Sald�r�yor mu?
    private bool isTakingDamage; // Hasar al�yor mu?
    private bool isDead; // �ld� m�?

    // Oyuncunun �ld���n� bildiren event
    public static event Action OnPlayerDied;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("Kamera bulunamad�!");
            }
        }

        playerHealth = maxHealth; // Can� ba�lang��ta tam doldur
        isDead = false; // Oyuncu ba�lang��ta canl�
        _canJump = true; // Z�plama izni ba�lang��ta a��k

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
        UpdateHealthUI(); // UI'da can� g�ncelle
    }

    private void Update()
    {
        if (isDead) return; // Oyuncu �ld�yse input alma
        SetInputs();
    }

    private void FixedUpdate()
    {
        if (isDead) return; // Oyuncu �ld�yse hareket etme
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

        // Hareket y�n�n� g�ncelle (havadayken de �al��s�n)
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

        // �ncelik 1: Sald�r� durumu (Q ve E tu�lar�)
        if (Input.GetKeyDown(KeyCode.Q) && isGrounded)
        {
            isAttacking = true;
            isTakingDamage = false; // Hasar animasyonu kesiliyor
            isJumping = false; // Z�plama kesiliyor
            animator.Play(punchAnimID);
            AttackEnemy(); // D��mana hasar ver
            return;
        }
        else if (Input.GetKeyDown(KeyCode.E) && isGrounded)
        {
            isAttacking = true;
            isTakingDamage = false; // Hasar animasyonu kesiliyor
            isJumping = false; // Z�plama kesiliyor
            animator.Play(kickAnimID);
            AttackEnemy(); // D��mana hasar ver
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

        // �ncelik 2: Hasar alma durumu
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

        // �ncelik 3: Z�plama durumu
        if (isJumping)
        {
            if (isGrounded && _rb.linearVelocity.y <= 0)
            {
                isJumping = false;
                UpdateMovementAnimation(isMoving, isGrounded);
            }
            return;
        }

        // Yeni giri�: Z�plama
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
        // Sahnedeki t�m d��manlar� bul
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
            return; // Havadayken, sald�r� veya hasar alma s�ras�nda hareket animasyonlar�n� de�i�tirme
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
        if (isDead) return; // �l� oyuncunun �arp��malar� dikkate al�nmaz
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
        if (isDead) return; // �l� oyuncunun �arp��malar� dikkate al�nmaz
        if (collision.collider.CompareTag("HintDoor"))
        {
            hintDoorManager.ChangeColorAndOpenWay();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // �l� oyuncuya hasar verme

        playerHealth -= damage;
        playerHealth = Mathf.Max(playerHealth, 0);

        // E�er sald�r� animasyonu oynuyorsa, hasar animasyonunu oynatma
        if (!isAttacking && playerHealth > 0)
        {
            isTakingDamage = true;
            isJumping = false; // Z�plama kesiliyor
            animator.Play(takeDamageAnimID);
        }

        UpdateHealthUI();
        Debug.Log($"Oyuncu {damage} hasar ald�! Kalan can: {playerHealth}");

        if (playerHealth <= 0)
        {
            Die(); // �l�m metodunu �a��r
        }
    }

    private void UpdateHealthUI()
    {
        uiManager.UpdateHealthText(playerHealth); // Can UI'sini g�ncelle
    }

    private void Die()
    {
        if (isDead) return; // Zaten �ld�yse tekrar �al��t�rma
        isDead = true;

        Debug.Log("OYUNCU �LD�!");

        // T�m durumlar� s�f�rla
        isAttacking = false;
        isJumping = false;
        isTakingDamage = false;
        currentSpeed = 0f;
        moveDirection = Vector3.zero;

        // Fiziksel hareketleri durdur
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true; // Fiziksel etkile�imleri kapat

        // �l�m animasyonunu oynat
        animator.Play(deathID);

        // D��manlara oyuncunun �ld���n� bildir
        OnPlayerDied?.Invoke();

        // �l�m animasyonu bittikten sonra eylemleri ba�lat
        StartCoroutine(HandleDeathAnimation());
    }

    private IEnumerator HandleDeathAnimation()
    {
        // �l�m animasyonunun s�resini al
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSecondsRealtime(stateInfo.length);

        // Animasyon bitti, oyun bitti ekran�n� g�ster
        uiManager.ShowGameOverScreen(); // UIManager'da oyun bitti ekran�n� g�ster
        // Alternatif: Sahneyi yeniden y�klemek i�in
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // EnemyManager i�in eklenen getter (iste�e ba�l�, art�k event kullan�yoruz)
    public bool IsDead()
    {
        return isDead;
    }

    // Sald�r� menzilini g�rselle�tirmek i�in
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}