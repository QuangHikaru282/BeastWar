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

    [Tooltip("ID của Nhiệm vụ cần hoàn thành ĐỂ MỞ KHÓA cổng này. Ví dụ: Rừng Xanh cần hoàn thành Quest 5 thì nhập số 5.")]
    public int requiredQuestIdToUnlock = 0;

    [Header("UI Thông báo (Tùy chọn)")]
    [Tooltip("Dùng để hiển thị thông báo 'Chưa mở khóa' nếu Player chạm vào portal")]
    [SerializeField] private TextMeshProUGUI notificationText;
    
    [Tooltip("Dòng chữ hiện lên khi chưa đủ điều kiện")]
    [SerializeField] private string lockedMessage = "Khu vực này chưa được mở khóa!";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Kiểm tra xem Quest hiện tại đã vượt qua yêu cầu chưa
            bool isUnlocked = true;
            if (global::QuestManager.Instance != null)
            {
                // Nếu currentMainQuestId > requiredQuestIdToUnlock tức là đã qua quest đó rồi (hoặc bằng nếu là kiểu quest hoàn thành)
                // Ở hệ thống chúng ta, khi hoàn thành quest 5 thì currentMainQuestId nhảy lên 6.
                isUnlocked = global::QuestManager.Instance.playerData.currentMainQuestId > requiredQuestIdToUnlock;
            }

            if (isUnlocked)
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
                Debug.Log($"[Portal] Map {targetMapName} bị chặn! Yêu cầu hoàn thành Nhiệm Vụ số {requiredQuestIdToUnlock}.");
                
                if (notificationText != null)
                {
                    notificationText.text = lockedMessage;
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
