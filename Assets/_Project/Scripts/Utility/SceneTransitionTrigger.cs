using UnityEngine;

/// <summary>
/// Chuyển sang một Scene chỉ định khi đối tượng có tag "Player" chạm vào.
/// Yêu cầu: 
/// 1. Object gắn script này phải có Collider2D (hoặc Collider) và tích chọn "Is Trigger".
/// 2. Object người chơi phải có tag là "Player".
/// </summary>
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Cấu hình chuyển Scene")]
    [Tooltip("Tên của Scene muốn chuyển đến (phải khớp chính xác và đã thêm vào Build Settings)")]
    [SerializeField] private string targetSceneName;

    [Tooltip("Thông điệp hiển thị khi chuyển cảnh")]
    [SerializeField] private string loadingMessage = "Đang di chuyển...";

    [Header("Yêu cầu Mở khóa (Stage Lock)")]
    [Tooltip("Tick vào đây nếu muốn chặn người chơi qua cổng khi chưa vượt ải.")]
    public bool requireStageUnlock;
    
    [Tooltip("Số ải yêu cầu đã vượt qua (vd: nhập 6 nghĩa là phải vượt xong ải 5)")]
    public int requiredStageIndex = 6;
    
    [Tooltip("Tin nhắn báo lỗi nếu chưa đủ điều kiện")]
    public string lockedMessage = "Bản đồ này chưa được mở khóa! Hãy vượt qua ải 5 ở World Map để tiến lên!";
    
    [Tooltip("Kéo file PlayerData vào đây để đọc tiến trình")]
    public PlayerData playerData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là Player không
        if (other.CompareTag("Player"))
        {
            if (CheckUnlockRequirement())
            {
                TriggerTransition();
            }
        }
    }

    // Hỗ trợ cả game 3D nếu sau này bạn cần dùng
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (CheckUnlockRequirement())
            {
                TriggerTransition();
            }
        }
    }

    private bool CheckUnlockRequirement()
    {
        // Nếu không yêu cầu mở khóa thì cho qua luôn
        if (!requireStageUnlock) return true;

        if (playerData == null)
        {
            Debug.LogWarning("[SceneTransitionTrigger] Chưa gán PlayerData nhưng lại bật tính năng yêu cầu mở khóa!");
            return false; // Chặn lại để báo lỗi
        }

        // Kiểm tra xem tiến trình đã tới ải yêu cầu chưa
        if (playerData.highestUnlockedStage >= requiredStageIndex)
        {
            return true;
        }
        else
        {
            // Báo lỗi ra console (hoặc UI nếu bạn có hệ thống hiển thị thông báo)
            Debug.Log($"<color=red><b>HỆ THỐNG BÁO:</b></color> {lockedMessage}");
            return false;
        }
    }

    private void TriggerTransition()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName, loadingMessage);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }
}
