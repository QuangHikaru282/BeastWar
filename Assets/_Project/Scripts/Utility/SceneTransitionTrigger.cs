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

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là Player không
        if (other.CompareTag("Player"))
        {
            TriggerTransition();
        }
    }

    // Hỗ trợ cả game 3D nếu sau này bạn cần dùng
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerTransition();
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
