using System.Collections;
using UnityEngine;

/// <summary>
/// Dịch chuyển nhân vật tức thì giữa 2 điểm trên CÙNG 1 SCENE.
/// Gắn script này vào GameObject có Collider2D (đánh dấu isTrigger = true).
/// Kéo thả điểm đến (destinationPoint) thủ công trong Inspector.
/// </summary>
public class LocalTeleportTrigger : MonoBehaviour
{
    [Header("Điểm đến (Thủ công kéo thả)")]
    [Tooltip("Kéo Transform của vị trí đích cần dịch chuyển đến vào đây")]
    [SerializeField] private Transform destinationPoint;

    [Tooltip("Khoảng bù vị trí tại điểm đến (nếu muốn đứng lệch ra một chút để tránh đứng đè lên collider)")]
    [SerializeField] private Vector2 destinationOffset = Vector2.zero;

    [Header("Bộ lọc nhận diện")]
    [Tooltip("Tag của đối tượng được phép dịch chuyển (mặc định là Player)")]
    [SerializeField] private string targetTag = "Player";

    [Header("Thời gian hồi (Chống lặp vô hạn)")]
    [Tooltip("Thời gian chờ tối thiểu (giây) trước khi có thể dịch chuyển tiếp. Giúp tránh trường hợp 2 cổng dịch chuyển đẩy qua đẩy lại liên tục.")]
    [SerializeField] private float cooldownTime = 1.0f;

    // Biến static dùng chung để quản lý thời gian dịch chuyển lần cuối giữa tất cả các cổng
    private static float lastTeleportTime = -999f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Kiểm tra Tag đối tượng chạm vào
        if (!string.IsNullOrEmpty(targetTag) && !collision.CompareTag(targetTag))
        {
            return;
        }

        // 2. Kiểm tra xem điểm đến đã được gán qua Inspector chưa
        if (destinationPoint == null)
        {
            Debug.LogWarning($"[LocalTeleportTrigger] Đối tượng '{gameObject.name}' chưa được gán Destination Point trong Inspector!");
            return;
        }

        // 3. Kiểm tra Cooldown để tránh bị kẹt vòng lặp giữa 2 cổng đặt gần nhau
        if (Time.time - lastTeleportTime < cooldownTime)
        {
            return;
        }

        // Thực hiện dịch chuyển
        TeleportTarget(collision.gameObject);
    }

    private void TeleportTarget(GameObject target)
    {
        lastTeleportTime = Time.time;

        Vector3 targetPos = destinationPoint.position + (Vector3)destinationOffset;
        targetPos.z = target.transform.position.z; // Giữ nguyên độ sâu trục Z của nhân vật

        // Nếu đối tượng có Rigidbody2D (như PlayerMapController), cần cập nhật cả position của Rigidbody2D
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = targetPos;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
#else
            rb.velocity = Vector2.zero;
#endif
        }

        target.transform.position = targetPos;
        Debug.Log($"[LocalTeleport] Đã dịch chuyển '{target.name}' tới '{destinationPoint.name}' ({targetPos})");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (destinationPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, destinationPoint.position + (Vector3)destinationOffset);
            Gizmos.DrawWireSphere(destinationPoint.position + (Vector3)destinationOffset, 0.3f);
        }
    }
#endif
}
