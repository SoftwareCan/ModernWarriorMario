using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private Transform player; // Oyuncu Hedefi
    [SerializeField] private float stoppingDistance = 1.5f; // Oyuncuya yakla�ma mesafesi
    [SerializeField] private float attackRange = 2f; // Sald�r� menzili
    [SerializeField] private float attackCoolDown = 2f; // Sald�r� bekleme s�resi
    [SerializeField] private float damage = 10f; // Verilen hasar
    [SerializeField] private float moveSpeed = 2.5f; // Hareket h�z�
    [SerializeField] private float maxHealth = 100f; // Enemy'nin can�
    [SerializeField] private float knockbackDistance = 0.5f; // Geri savrulma mesafesi 
    [SerializeField] private float knockbackDuration = 0.2f; // Geri savrulma s�resi 
    [SerializeField] private float activationDistance = 15f; // Zombinin aktif olaca�� mesafe

    // Can bar� i�in (Slider ile)
    [SerializeField] private Slider healthBarSlider; // Can bar� olarak Slider
    [SerializeField] private Transform healthBarTransform; // Can bar�n�n transform'u (Canvas)

    // Event eklicez Skor artmas� i�in Enemy �l�m�nden sonra
    public static event System.Action OnEnemyDied;

    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isPlayerInRange;
    private float health;

    private Animator animator;
    // Animasyon hash ID'leri
    private int idleAnimID;
    private int walkAnimID;
    private int punchAnimID;
    private int takeDamageAnimID;
    private int deathID;
    private int spawnAnimID;

    // Durum de�i�kenleri
    private bool isAttacking;
    private bool isTakingDamage;
    private bool isDead; // D��man�n �l�m durumu
    private bool isKnockedBack; // Geri savrulma durumu 
    private bool isSpawned; // Spawn animasyonu oynuyor mu?
    private bool isActive; // Zombi aktif mi?

    private Camera mainCamera;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        // Animasyon hash ID'lerini al
        idleAnimID = Animator.StringToHash("Idle");
        walkAnimID = Animator.StringToHash("Walk");
        punchAnimID = Animator.StringToHash("Punch");
        takeDamageAnimID = Animator.StringToHash("TakeDamage");
        deathID = Animator.StringToHash("Zombie Death");
        spawnAnimID = Animator.StringToHash("Spawn");

        health = maxHealth;

        // Kameray� bul
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("Main Camera not found!");

        // Can bar�n� kontrol et
        if (healthBarSlider != null)
        {
            healthBarSlider.minValue = 0;
            healthBarSlider.maxValue = 1;
            UpdateHealthBar(); // Can bar�n� ba�lang��ta tam dolu yap
        }
    }

    private void Update()
    {
        if (isDead) return;

        // Spawn animasyonu oynuyorsa bekle
        if (isSpawned)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == spawnAnimID && stateInfo.normalizedTime >= 1f)
            {
                isSpawned = false;
                Debug.Log($"Spawn animation finished for zombie: {gameObject.name}");
            }
            return;
        }

        // Zombi aktif de�ilse ve oyuncu yak�nsa aktif et
        if (!isActive && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Debug.Log($"Zombie {gameObject.name} checking activation: Distance to player = {distanceToPlayer}, Activation Distance = {activationDistance}");
            if (distanceToPlayer <= activationDistance)
            {
                Activate();
            }
            return;
        }

        UpdateEnemyState();
    }

    private void LateUpdate()
    {
        // Can bar�n� kameraya do�ru d�nd�r
        if (healthBarTransform != null && mainCamera != null)
        {
            healthBarTransform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
        }
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        Debug.Log($"Player set for zombie: {gameObject.name}, Player: {player}");
    }

    public void PlaySpawnAnimation()
    {
        isSpawned = true;
        if (agent != null)
        {
            agent.enabled = false; // Spawn s�ras�nda hareket etme
        }
        // Animasyon varsa oynat
        if (animator.HasState(0, spawnAnimID))
        {
            animator.Play(spawnAnimID);
            Debug.Log($"Playing spawn animation for zombie: {gameObject.name}");
        }
        else
        {
            isSpawned = false; // Animasyon yoksa hemen aktif ol
            Debug.LogWarning($"Spawn animasyonu bulunamad� for zombie: {gameObject.name}");
        }
    }

    public void Activate()
    {
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent is null on zombie: " + gameObject.name);
            return;
        }
        isActive = true;
        agent.enabled = true; // Hareketi ba�lat
        Debug.Log($"Zombie {gameObject.name} activated. Agent enabled: {agent.enabled}, Player: {player}");
    }

    public bool IsSpawning()
    {
        return isSpawned;
    }

    private void UpdateEnemyState()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= attackRange;

        // �ncelik 1: Hasar alma durumu
        if (isTakingDamage)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == takeDamageAnimID && stateInfo.normalizedTime >= 1f)
            {
                isTakingDamage = false;
                UpdateEnemyBehavior(distanceToPlayer);
            }
            return;
        }

        // �ncelik 2: Sald�r� durumu
        if (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == punchAnimID && stateInfo.normalizedTime >= 1f)
            {
                isAttacking = false;
                UpdateEnemyBehavior(distanceToPlayer);
            }
            return;
        }

        // Normal davran��
        UpdateEnemyBehavior(distanceToPlayer);
    }

    private void UpdateEnemyBehavior(float distanceToPlayer)
    {
        
        if (isPlayerInRange)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
            TryAttack();
        }
        else
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            animator.Play(walkAnimID);
        }
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCoolDown)
        {
            lastAttackTime = Time.time;
            AttackPlayer();
            isAttacking = true;
            animator.Play(punchAnimID);
        }
    }

    private void AttackPlayer()
    {
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        if (playerManager != null)
        {
            playerManager.TakeDamage(damage);
            Debug.Log($"D��man oyuncuya {damage} hasar verdi! Zombie: {gameObject.name}");
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // Zaten �l� ise hasar alma

        health -= damage;
        health = Mathf.Max(health, 0);

        isTakingDamage = true;
        isAttacking = false; // Sald�r� animasyonu kesiliyor
        animator.Play(takeDamageAnimID);

        UpdateHealthBar(); // Can bar�n� g�ncelle

        // Geri savrulma efektini ba�lat
        if (!isDead) // �l� de�ilse geri savrul
        {
            ApplyKnockback();
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback()
    {
        if (isKnockedBack) return; // Zaten savruluyorsa tekrar ba�latma
        isKnockedBack = true;

        // NavMeshAgent'� ge�ici olarak devre d��� b�rak
        if (agent != null && agent.enabled)
        {
            agent.enabled = false;
        }

        // Geri savrulma y�n�: d��mandan oyuncuya ters y�nde
        Vector3 knockbackDirection = (transform.position - player.position).normalized;
        knockbackDirection.y = 0; // Y ekseninde hareket olmas�n
        Vector3 targetPosition = transform.position + knockbackDirection * knockbackDistance;

        // DOTween ile p�r�zs�z hareket
        transform.DOMove(targetPosition, knockbackDuration)
            .SetEase(Ease.OutQuad) // Yumu�ak bir yava�lama
            .OnComplete(() =>
            {
                // Savrulma bitti, NavMeshAgent'� geri a�
                if (agent != null && !isDead)
                {
                    agent.enabled = true;
                }
                isKnockedBack = false;
            });
    }

    private void UpdateHealthBar()
    {
        // Can bar�n� health oran�na g�re g�ncelle
        if (healthBarSlider != null)
        {
            float healthRatio = health / maxHealth; // 0 ile 1 aras�nda bir oran
            healthBarSlider.value = healthRatio; // Slider'�n de�erini g�ncelle
        }
    }

    private void Die()
    {
        isDead = true;
        // NavMeshAgent aktifse durdur, de�ilse hata vermesin
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
        animator.Play(deathID);
        OnEnemyDied?.Invoke();
        Debug.Log("Enemy died: " + gameObject.name);

        // Zombie Death animasyonunun s�resi kadar bekle ve objeyi yok et
        AnimatorStateInfo deathStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float deathAnimLength = deathStateInfo.length;
        Invoke(nameof(DestroyEnemy), deathAnimLength);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}