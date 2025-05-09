using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float rotationSpeed = 4f;
    [SerializeField] private float forwardSmoothSpeed = 5f; // Artırıldı
    [SerializeField] private float heightOffset = 2.5f;
    [SerializeField] private float distanceOffset = 3.5f;
    [SerializeField] private float tiltAngle = -15f;

    // Yakınlaşma/uzaklaşma
    [SerializeField] private float minDistance = 3f; // 2f → 3f, görüş açısı için
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float zoomSpeed = 5f;

    private Vector3 desiredPosition;
    private Vector3 smoothedForward;
    private PlayerManager playerManager; // Yeni

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: Target atanmamış!");
            return;
        }

        // PlayerManager’ı bul
        playerManager = target.GetComponent<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogWarning("CameraFollow: PlayerManager bulunamadı, target.forward kullanılacak!");
        }

        smoothedForward = target.forward;
    }

    private void Update()
    {
        if (target == null) return;

        // Mouse scroll ile yakınlaşma/uzaklaşma
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        distanceOffset -= scrollInput * zoomSpeed;
        distanceOffset = Mathf.Clamp(distanceOffset, minDistance, maxDistance);

        // Oyuncunun hareket yönünü al veya target.forward kullan
        Vector3 targetForward = playerManager != null ? playerManager.GetMoveDirection() : target.forward;
        if (targetForward.magnitude < 0.1f) targetForward = target.forward; // Hareket yoksa forward

        // Kameranın hedef pozisyonunu hesapla
        Vector3 targetPosition = target.position;
        smoothedForward = Vector3.Lerp(smoothedForward, targetForward, 1 - Mathf.Exp(-forwardSmoothSpeed * Time.deltaTime));
        desiredPosition = targetPosition + Vector3.up * heightOffset;
        desiredPosition -= smoothedForward * distanceOffset;

        // Kamerayı yumuşak hareket ettir
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 1 - Mathf.Exp(-followSpeed * Time.deltaTime));

        // Kameranın rotasyonunu ayarla
        Vector3 lookTarget = targetPosition + Vector3.up * heightOffset;
        Vector3 direction = lookTarget - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        targetRotation *= Quaternion.Euler(tiltAngle, 0, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1 - Mathf.Exp(-rotationSpeed * Time.deltaTime));
    }
}
