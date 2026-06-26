using UnityEngine;
using TMPro;

/// <summary>
/// Đặt script này vào một GameObject có Collider2D (isTrigger = true) ở rìa map.
/// Khi Player chạm vào, sẽ kiểm tra điều kiện mở khóa map mới và chuyển cảnh.
/// </summary>
public class MapPortalTrigger : MonoBehaviour
{
    [Header("Cấu hình Portal")]
    [Tooltip("Tên Map mà portal này sẽ dẫn đến (ví dụ: City)")]
    [SerializeField] private string targetMapName = "City";

    [Tooltip("Tên Scene thật sự trong Build Settings")]
    [SerializeField] private string targetSceneName = "City";

    [Header("Dữ liệu")]
    [SerializeField] private PlayerData playerData;

    [Header("UI Thông báo (Tùy chọn)")]
    [Tooltip("Dùng để hiển thị thông báo 'Chưa mở khóa' nếu Player chạm vào portal")]
    [SerializeField] private TextMeshProUGUI notificationText;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerData == null) return;

            // Kiểm tra xem map đã được mở khóa chưa
            if (playerData.IsMapUnlocked(targetMapName) || playerData.IsMapUnlocked(targetSceneName))
            {
                Debug.Log($"[Portal] Đang di chuyển sang map: {targetMapName}...");
                
                // Chuyển cảnh
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.TransitionToScene(targetSceneName, $"Đang di chuyển đến {targetMapName}...");
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
                }
            }
            else
            {
                Debug.Log($"[Portal] Map {targetMapName} chưa được mở khóa! Hãy hoàn thành Arena.");
                
                if (notificationText != null)
                {
                    notificationText.text = $"Khu vực {targetMapName} chưa được mở khóa!\nHãy vượt qua 5 ải Arena trước.";
                    notificationText.gameObject.SetActive(true);
                    Invoke("HideNotification", 3f); // Tắt thông báo sau 3 giây
                }
            }
        }
    }

    private void HideNotification()
    {
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }
}
