using UnityEngine;
using UnityEngine.UI;

public class LoginAndCharSelectManager : MonoBehaviour
{
    [Header("Dữ liệu")]
    [SerializeField] private PlayerData playerData;

    [Header("UI Chọn nhân vật")]
    [SerializeField] private GameObject charSelectPanel;
    [SerializeField] private Button maleButton;
    [SerializeField] private Button femaleButton;

    [Header("Cấu hình chuyển cảnh")]
    [SerializeField] private string targetScene = "MapScene";
    [SerializeField] private string loadingMessage = "Đang bước vào thế giới BeastWar...";

    private void Start()
    {
        // Gán sự kiện cho các nút bấm
        if (maleButton != null)
            maleButton.onClick.AddListener(() => ConfirmAndEnterGame("Male"));

        if (femaleButton != null)
            femaleButton.onClick.AddListener(() => ConfirmAndEnterGame("Female"));

        // Đảm bảo ẩn Panel lúc đầu, chỉ hiển thị khi bấm Play
        if (charSelectPanel != null) charSelectPanel.SetActive(false);
    }

    /// <summary>
    /// Kích hoạt luồng chọn nhân vật.
    /// </summary>
    public void StartCharSelectFlow()
    {
        if (charSelectPanel != null)
        {
            charSelectPanel.SetActive(true);
        }
        else
        {
            // Fallback nếu không có bảng chọn: Vào Map luôn với nhân vật Nam
            ConfirmAndEnterGame("Male");
        }
    }

    private void ConfirmAndEnterGame(string gender)
    {
        if (playerData != null)
        {
            playerData.ResetData();
            playerData.characterGender = gender;
        }

        Debug.Log($"[MainMenu] Đã xác nhận giới tính: {gender}. Đang tải Map...");

        if (charSelectPanel != null) charSelectPanel.SetActive(false);

        // Chạy Loading Cutscene chuyển cảnh
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(targetScene, loadingMessage);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
    }
}
