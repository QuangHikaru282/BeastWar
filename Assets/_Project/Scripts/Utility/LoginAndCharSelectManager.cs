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

    [Header("Main Menu Controller")]
    [Tooltip("Controller sẽ phát trailer sau khi chọn nhân vật")]
    [SerializeField] private MainMenuController mainMenuController;

    private bool hasSelectedCharacter;

    private void Start()
    {
        // Gán sự kiện cho hai nút chọn nhân vật.
        if (maleButton != null)
        {
            maleButton.onClick.AddListener(
                () => ConfirmCharacter("Male")
            );
        }

        if (femaleButton != null)
        {
            femaleButton.onClick.AddListener(
                () => ConfirmCharacter("Female")
            );
        }

        // Ẩn bảng chọn nhân vật lúc mới vào Main Menu.
        if (charSelectPanel != null)
        {
            charSelectPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Được MainMenuController gọi khi người chơi bấm Play.
    /// </summary>
    public void StartCharSelectFlow()
    {
        hasSelectedCharacter = false;

        if (maleButton != null)
            maleButton.interactable = true;

        if (femaleButton != null)
            femaleButton.interactable = true;

        if (charSelectPanel != null)
        {
            charSelectPanel.SetActive(true);
        }
    }



    /// <summary>
    /// Khi người chơi bấm trực tiếp vào Male hoặc Female.
    /// </summary>
    private void ConfirmCharacter(string gender)
    {
        // Ngăn bấm hai nhân vật nhiều lần.
        if (hasSelectedCharacter)
            return;

        hasSelectedCharacter = true;

        if (maleButton != null)
            maleButton.interactable = false;

        if (femaleButton != null)
            femaleButton.interactable = false;

        // Reset dữ liệu cho game mới và lưu giới tính.
        if (playerData != null)
        {
            playerData.ResetData();
            playerData.characterGender = gender;
        }
        else
        {
            Debug.LogWarning(
                "LoginAndCharSelectManager: Chưa gán PlayerData."
            );
        }

        Debug.Log(
            $"[MainMenu] Đã xác nhận giới tính: {gender}."
        );

        // Ẩn bảng chọn nhân vật.
        if (charSelectPanel != null)
        {
            charSelectPanel.SetActive(false);
        }

        // Gọi MainMenuController để chạy trailer.
        if (mainMenuController != null)
        {
            mainMenuController.PlayTrailerAndEnterGame();
        }
        else
        {
            Debug.LogError(
                "LoginAndCharSelectManager: " +
                "Chưa gán MainMenuController."
            );

            hasSelectedCharacter = false;

            if (maleButton != null)
                maleButton.interactable = true;

            if (femaleButton != null)
                femaleButton.interactable = true;
        }
    }
}