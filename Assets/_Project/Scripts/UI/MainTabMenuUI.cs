using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public enum MenuButtonActionType
{
    TrainerCard,      // Mở Thẻ Huấn Luyện Viên & Hộp Huy Hiệu
    Pokedex,          // Mở BeastDex / Pokédex
    Bicycle,          // Lên / Xuống Xe Đạp
    Party,            // Mở Đội Hình Beast
    Bag,              // Mở Túi Đồ / Balo
    Quest,            // Mở Bảng Nhiệm Vụ
    SaveGame,         // Lưu Tiến Trình Game
    OpenCustomPanel,  // Mở một Panel UI bất kỳ (Tự do gán trong tương lai)
    CustomEvent,      // Kích hoạt một UnityEvent tùy chỉnh
    CloseMenu         // Đóng Menu
}

[System.Serializable]
public class MenuButtonEntry
{
    [Tooltip("Tên hiển thị trên nút (VD: Pokédex, Xe Đạp, Cài Đặt, v.v.)")]
    public string buttonTitle = "Chức Năng Mới";

    [Tooltip("Icon đại diện cho nút (Tùy chọn)")]
    public Sprite icon;

    [Tooltip("Loại hành động khi bấm nút")]
    public MenuButtonActionType actionType = MenuButtonActionType.OpenCustomPanel;

    [Tooltip("Panel UI sẽ mở ra (Dành cho loại OpenCustomPanel)")]
    public GameObject targetPanel;

    [Tooltip("Sự kiện mở rộng chạy khi click (Dành cho loại CustomEvent)")]
    public UnityEvent onCustomClick;
}

/// <summary>
/// Quản lý Menu Tổng Hợp Phím TAB theo kiến trúc Động (Modular / Prefab-based):
/// - Dễ dàng thêm 5, 10 hoặc nhiều nút chức năng mới trong tương lai chỉ bằng cách cấu hình trong Inspector.
/// - Tự động sinh nút từ Button Prefab vào Content Layout.
/// </summary>
public class MainTabMenuUI : MonoBehaviour
{
    public static MainTabMenuUI Instance { get; private set; }

    [Header("1. Khung Menu Chính")]
    [Tooltip("Panel Menu mở ra khi nhấn TAB")]
    [SerializeField] private GameObject menuPanel;

    [Tooltip("Phím tắt mở Menu (Mặc định là Tab)")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    [Header("2. Cấu Hình Prefab Nút")]
    [Tooltip("Prefab của 1 nút Menu (Gắn script MenuActionButtonUI)")]
    [SerializeField] private GameObject buttonPrefab;

    [Tooltip("Transform chứa danh sách các nút (Vertical Layout Group)")]
    [SerializeField] private Transform buttonContainer;

    [Header("3. Danh Sách Các Nút Menu (Tự do thêm bớt trong tương lai)")]
    [SerializeField] private List<MenuButtonEntry> menuButtonEntries = new List<MenuButtonEntry>();

    [Header("4. Dữ Liệu Người Chơi")]
    [SerializeField] private PlayerData playerData;

    [Header("5. Thông Báo Trạng Thái (Tùy chọn)")]
    [SerializeField] private TextMeshProUGUI statusNotificationText;

    [Header("6. Âm Thanh")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openMenuSFX;
    [SerializeField] private AudioClip clickSFX;
    [SerializeField] private AudioClip saveSuccessSFX;

    private List<MenuActionButtonUI> spawnedButtons = new List<MenuActionButtonUI>();
    private MenuActionButtonUI bicycleButtonUI;

    public bool IsOpen => menuPanel != null && menuPanel.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (menuPanel != null) menuPanel.SetActive(false);

        // Khởi tạo danh sách nút động từ Prefab
        InitializeMenuButtons();
    }

    private void Update()
    {
        // Nhấn phím TAB để Bật/Tắt Menu
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }

        // Nhấn ESC để đóng Menu nếu đang mở
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    public void ToggleMenu()
    {
        if (IsOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);

        // Khóa di chuyển nhân vật
        var pmc = UnityEngine.Object.FindFirstObjectByType<PlayerMapController>();
        if (pmc != null) pmc.SetCanMove(false);

        if (audioSource != null && openMenuSFX != null)
        {
            audioSource.PlayOneShot(openMenuSFX);
        }

        UpdateDynamicButtonStates();

        if (statusNotificationText != null)
        {
            statusNotificationText.text = "";
        }
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);

        // Mở lại di chuyển nhân vật
        var pmc = UnityEngine.Object.FindFirstObjectByType<PlayerMapController>();
        if (pmc != null) pmc.SetCanMove(true);
    }

    /// <summary>
    /// Tạo các nút bấm trong Menu từ danh sách cấu hình và Prefab.
    /// </summary>
    private void InitializeMenuButtons()
    {
        if (buttonContainer == null || buttonPrefab == null) return;

        // Dọn dẹp nút cũ nếu có
        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonContainer.GetChild(i).gameObject);
        }
        spawnedButtons.Clear();
        bicycleButtonUI = null;

        foreach (var entry in menuButtonEntries)
        {
            if (entry == null) continue;

            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            MenuActionButtonUI btnUI = btnObj.GetComponent<MenuActionButtonUI>();

            if (btnUI != null)
            {
                var capturedEntry = entry;
                string subText = GetInitialSubText(capturedEntry);
                bool interactable = IsButtonInteractable(capturedEntry);

                btnUI.Setup(capturedEntry.buttonTitle, capturedEntry.icon, subText, interactable, () =>
                {
                    ExecuteButtonAction(capturedEntry, btnUI);
                });

                spawnedButtons.Add(btnUI);

                if (capturedEntry.actionType == MenuButtonActionType.Bicycle)
                {
                    bicycleButtonUI = btnUI;
                }
            }
        }
    }

    private string GetInitialSubText(MenuButtonEntry entry)
    {
        if (entry.actionType == MenuButtonActionType.Bicycle)
        {
            if (playerData == null || !playerData.hasBicycle) return "Chưa có";
            return playerData.isRidingBicycle ? "Xuống xe" : "Lên xe";
        }
        return "";
    }

    private bool IsButtonInteractable(MenuButtonEntry entry)
    {
        if (entry.actionType == MenuButtonActionType.Bicycle)
        {
            return playerData != null && playerData.hasBicycle;
        }
        return true;
    }

    private void UpdateDynamicButtonStates()
    {
        if (bicycleButtonUI != null && playerData != null)
        {
            bool hasBike = playerData.hasBicycle;
            string sub = !hasBike ? "Chưa có" : (playerData.isRidingBicycle ? "Xuống xe" : "Lên xe");
            bicycleButtonUI.UpdateSubText(sub, hasBike);
        }
    }

    private void ExecuteButtonAction(MenuButtonEntry entry, MenuActionButtonUI btnUI)
    {
        PlayClickSound();

        switch (entry.actionType)
        {
            case MenuButtonActionType.TrainerCard:
                CloseMenu();
                if (TrainerCardUI.Instance != null) TrainerCardUI.Instance.OpenCard();
                break;

            case MenuButtonActionType.Pokedex:
                CloseMenu();
                if (BeastDexUI.Instance != null) BeastDexUI.Instance.OpenDex();
                break;

            case MenuButtonActionType.Bicycle:
                if (playerData != null && playerData.hasBicycle)
                {
                    playerData.isRidingBicycle = !playerData.isRidingBicycle;
                    UpdateDynamicButtonStates();
                    CloseMenu();
                }
                else
                {
                    ShowNotification("<color=red>Bạn chưa sở hữu Xe Đạp!</color>");
                }
                break;

            case MenuButtonActionType.Party:
            case MenuButtonActionType.Bag:
            case MenuButtonActionType.Quest:
            case MenuButtonActionType.OpenCustomPanel:
                CloseMenu();
                if (entry.targetPanel != null)
                {
                    entry.targetPanel.SetActive(true);
                }
                break;

            case MenuButtonActionType.SaveGame:
                if (playerData != null)
                {
                    playerData.Save();
                    if (audioSource != null && saveSuccessSFX != null)
                    {
                        audioSource.PlayOneShot(saveSuccessSFX);
                    }
                    ShowNotification("<color=green>Đã lưu game thành công!</color>");
                }
                break;

            case MenuButtonActionType.CustomEvent:
                entry.onCustomClick?.Invoke();
                break;

            case MenuButtonActionType.CloseMenu:
                CloseMenu();
                break;
        }
    }

    private void ShowNotification(string message)
    {
        if (statusNotificationText != null)
        {
            statusNotificationText.text = message;
        }
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSFX != null)
        {
            audioSource.PlayOneShot(clickSFX);
        }
    }
}
