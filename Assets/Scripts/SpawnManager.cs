using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnPoint
    {
        public Transform point; // Spawn noktas�
        public GameObject enemyPrefab; // Spawn edilecek zombi prefab��
    }

    [SerializeField] private List<SpawnPoint> spawnPoints; // Spawn noktalar� (3 tane zombi i�in)
    [SerializeField] private Transform player; // Oyuncu referans�
    private bool hasTriggered = false; // Trigger��n sadece bir kez �al��mas� i�in

    private void Awake()
    {
        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerManager>()?.transform;
            if (player == null)
                Debug.LogError("Player not found!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
        {
            Debug.Log($"Trigger skipped: hasTriggered={hasTriggered}, Tag={other.tag}");
            return;
        }

        hasTriggered = true;
        Debug.Log("Player entered spawn trigger! Starting SpawnZombies coroutine...");
        StartCoroutine(SpawnZombies());
    }

    private IEnumerator SpawnZombies()
    {
        Debug.Log($"SpawnZombies started. Spawn points count: {spawnPoints.Count}");
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned in SpawnManager!");
            yield break;
        }

        // T�m zombileri ayn� anda spawn et
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            var spawnPoint = spawnPoints[i];
            if (spawnPoint.point == null || spawnPoint.enemyPrefab == null)
            {
                Debug.LogError($"Invalid spawn point at index {i}: point={spawnPoint.point}, prefab={spawnPoint.enemyPrefab}");
                continue;
            }
            Debug.Log($"Spawning zombie {i + 1} at {spawnPoint.point.position}");
            StartCoroutine(SpawnZombie(spawnPoint)); // Her zombiyi ayr� korutinle spawn et
        }

        // T�m zombilerin spawn�u ba�lat�ld�, korutin biter
        Debug.Log("All zombies spawned simultaneously.");
        yield return null;
    }

    private IEnumerator SpawnZombie(SpawnPoint spawnPoint)
    {
        Debug.Log($"SpawnZombie called for point {spawnPoint.point.position}");
        if (spawnPoint.enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is null in spawn point!");
            yield break;
        }

        // Zombiyi direkt spawn noktas�nda instantiate et
        GameObject zombie = Instantiate(spawnPoint.enemyPrefab, spawnPoint.point.position, Quaternion.identity);
        Debug.Log($"Zombie instantiated at {spawnPoint.point.position} for zombie: {zombie.name}");

        // EnemyManager�� al ve player�� set et
        EnemyManager enemyManager = zombie.GetComponent<EnemyManager>();
        if (enemyManager == null)
        {
            Debug.LogError("EnemyManager component not found on spawned zombie: " + zombie.name);
            yield break;
        }

        if (player == null)
        {
            Debug.LogError("Player reference is null in SpawnManager!");
            yield break;
        }

        enemyManager.SetPlayer(player);
        enemyManager.PlaySpawnAnimation();

        // Spawn animasyonunun bitmesini bekle
        float timeout = 5f; // Maksimum 5 saniye bekle (sonsuz d�ng� �nlemek i�in)
        float timer = 0f;
        while (enemyManager.IsSpawning() && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (timer >= timeout)
        {
            Debug.LogWarning($"Spawn animation timeout for zombie: {zombie.name}. Activating anyway.");
        }

        // Animasyon bitti veya timeout, zombiyi aktif et
        enemyManager.Activate();
        Debug.Log($"Zombie activated after spawn animation for zombie: {zombie.name}");
    }

    private void OnDrawGizmos()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.point != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(spawnPoint.point.position, 0.5f);
            }
        }
    }
}