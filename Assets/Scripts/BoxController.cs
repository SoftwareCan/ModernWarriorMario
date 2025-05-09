using UnityEngine;
using DG.Tweening;

public class BoxController : MonoBehaviour
{
    private Vector3 originalLocalPosition;
    public float moveAmount = 0.3f;
    public float moveDuration = 0.1f;
    private bool isBumped = false;
    private bool goldSpawned = false;
    public GameObject goldPrefab; // Altýn prefab'ý

    void Start()
    {
        originalLocalPosition = transform.localPosition;

        Rigidbody rb = GetComponent<Rigidbody>();
       
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isBumped) return;

        // Sadece Player'a tepki ver
        if (collision.collider.CompareTag("Player"))
        {
            // Çarpýþma yönünü kontrol et (alttan mý?)
            foreach (ContactPoint contact in collision.contacts)
            {
                // Kutunun altýndan vurulmuþ mu?
                if (contact.point.y < transform.position.y - 0.1f)
                {
                    isBumped = true;

                    transform.DOLocalMoveY(originalLocalPosition.y + moveAmount, moveDuration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            // Kutu eski yerine dönerken altýn oluþtur
                            if (!goldSpawned && goldPrefab != null)
                            {
                                goldSpawned = true;

                                Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + moveAmount + 0.5f, transform.position.z);
                                GameObject gold = Instantiate(goldPrefab, spawnPosition, Quaternion.identity);
                                gold.transform.position = spawnPosition;

                            }

                            transform.DOLocalMoveY(originalLocalPosition.y, moveDuration)
                                .SetEase(Ease.InQuad)
                                .OnComplete(() => isBumped = false);
                        });

                    break; // bir çarpýþma tespit edildiyse daha fazlasýna bakma
                }
            }
        }
    }
}
