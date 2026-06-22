using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý các tương tác trên Main Menu (Play, Continue, Setting, Quit).
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Cấu hình Chuyển Cảnh")]
    [Tooltip("Tên scene sẽ tải khi bấm Play (thường là MapScene hoặc Làng)")]
    [SerializeField] private string playTargetScene = "MapScene";
    
    [Tooltip("Thông điệp hiển thị lúc load game")]
    [SerializeField] private string playLoadingMessage = "Đang bước vào thế giới BeastWar...";

    [Header("UI Buttons (Không bắt buộc, có thể gán qua Inspector)")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        // Gán sự kiện tự động nếu các Button được kéo thả vào Inspector
        if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinuePressed);
        if (settingButton != null) settingButton.onClick.AddListener(OnSettingPressed);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitPressed);

        // Kiểm tra xem có dữ liệu đã lưu để mở khóa nút Continue hay không
        UpdateButtonStates();
    }

    /// <summary>
    /// Xử lý khi nhấn nút Play (Chơi mới)
    /// </summary>
    public void OnPlayPressed()
    {
        Debug.Log("MainMenu: Khởi tạo game mới...");
        
        // Thực hiện reset các thông số game mới nếu cần thiết (ví dụ: PlayerPrefs.DeleteAll())
        // PlayerPrefs.DeleteKey("SavedLevel"); 

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(playTargetScene, playLoadingMessage);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(playTargetScene);
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút Continue (Chơi tiếp)
    /// </summary>
    public void OnContinuePressed()
    {
        Debug.Log("MainMenu: Đang tải lại tiến trình cũ...");

        // Giả sử bạn lưu tên scene đã chơi qua PlayerPrefs
        string savedScene = PlayerPrefs.GetString("SavedScene", playTargetScene);
        string continueMessage = "Đang tiếp tục hành trình...";

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(savedScene, continueMessage);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(savedScene);
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút Setting (Cài đặt)
    /// </summary>
    public void OnSettingPressed()
    {
        Debug.Log("MainMenu: Mở bảng cài đặt (Âm thanh, Đồ họa...)");
        // TODO: Kích hoạt UI Panel cài đặt tại đây
        // settingPanel.SetActive(true);
    }

    /// <summary>
    /// Xử lý khi nhấn nút Quit (Thoát game)
    /// </summary>
    public void OnQuitPressed()
    {
        Debug.Log("MainMenu: Đang thoát trò chơi...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Cập nhật trạng thái ẩn hiện hoặc tương tác của các nút dựa trên dữ liệu game
    /// </summary>
    private void UpdateButtonStates()
    {
        if (continueButton != null)
        {
            // Nếu chưa có file save (ở đây ví dụ là "HasSavedGame"), vô hiệu hóa nút Continue
            bool hasSaveData = PlayerPrefs.HasKey("SavedScene");
            continueButton.interactable = hasSaveData;
        }
    }
}
