using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System;

public class PlayerManager : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    [SerializeField] private float maxHealth = 100f;
    private float playerHealth;
    private Rigidbody _rb;
    private Vector3 moveDirection;
    private float currentSpeed;

    [SerializeField] private GoldManager goldManager;
    [SerializeField] private float _jumpForce;
    [SerializeField] private bool _canJump;
    [SerializeField] private float _jumpCoolDown;
    [SerializeField] private float _playerHeight;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private HintDoorManager hintDoorManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float fallThresholdY = -10f; // Düþme sýnýrý

    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 20f;

    public Animator animator;

    private int idleAnimID;
    private int walkAnimID;
    private int runAnimID;
    private int jumpAnimID;
    private int takeDamageAnimID;
    private int kickAnimID;
    private int punchAnimID;
    private int deathID;

    private bool isJumping;
    private bool isAttacking;
    private bool isTakingDamage;
    private bool isDead;

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

        playerHealth = maxHealth;
        isDead = false;
        _canJump = true;

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
        UpdateHealthUI();
    }

    private void Update()
    {
        if (isDead) return;

        // Düþme kontrolü
        if (transform.position.y < fallThresholdY)
        {
            TakeDamage(maxHealth);
            Debug.Log($"Oyuncu y={transform.position.y} ile düþtü, GameOver!");
            return;
        }

        SetInputs();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
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

        if (Input.GetKeyDown(KeyCode.Q) && isGrounded)
        {
            isAttacking = true;
            isTakingDamage = false;
            isJumping = false;
            animator.Play(punchAnimID);
            AttackEnemy();
            return;
        }
        else if (Input.GetKeyDown(KeyCode.E) && isGrounded)
        {
            isAttacking = true;
            isTakingDamage = false;
            isJumping = false;
            animator.Play(kickAnimID);
            AttackEnemy();
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

        if (isJumping)
        {
            if (isGrounded && _rb.linearVelocity.y <= 0)
            {
                isJumping = false;
                UpdateMovementAnimation(isMoving, isGrounded);
            }
            return;
        }

        if (Input.GetKey(KeyCode.Space) && _canJump && isGrounded)
        {
            _canJump = false;
            isJumping = true;
            SetPlayerJumping();
            animator.Play(jumpAnimID);
            Invoke(nameof(ResetJumping), _jumpCoolDown);
            return;
        }

        UpdateMovementAnimation(isMoving, isGrounded);
    }

    private void AttackEnemy()
    {
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
            return;
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
        if (isDead) return;
        if (other.CompareTag("Gold"))
        {
            other.gameObject.SetActive(false);
            if (goldManager != null)
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
        if (isDead) return;
        if (collision.collider.CompareTag("HintDoor"))
        {
            hintDoorManager.ChangeColorAndOpenWay();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        playerHealth -= damage;
        playerHealth = Mathf.Max(playerHealth, 0);

        if (!isAttacking && playerHealth > 0)
        {
            isTakingDamage = true;
            isJumping = false;
            animator.Play(takeDamageAnimID);
        }

        UpdateHealthUI();
        Debug.Log($"Oyuncu {damage} hasar aldý! Kalan can: {playerHealth}");

        if (playerHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        uiManager.UpdateHealthText(playerHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("OYUNCU ÖLDÜ!");

        isAttacking = false;
        isJumping = false;
        isTakingDamage = false;
        currentSpeed = 0f;
        moveDirection = Vector3.zero;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        animator.Play(deathID);

        OnPlayerDied?.Invoke();

        StartCoroutine(HandleDeathAnimation());
    }

    private IEnumerator HandleDeathAnimation()
    {
        // Ölüm animasyonunun süresini al
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        Debug.Log($"Ölüm animasyonu süresi: {animationLength} saniye");

        // Animasyon tamamlanana kadar bekle
        yield return new WaitForSecondsRealtime(animationLength);

        // GameOver ekranýný göster
        uiManager.ShowGameOverScreen();
        Debug.Log("Ölüm animasyonu bitti, GameOver ekraný gösterildi!");
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}