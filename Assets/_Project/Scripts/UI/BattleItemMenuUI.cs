using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý Menu Balo trong trận đấu.
/// Toàn bộ UI và Reference đều được gán THỦ CÔNG 100% qua Inspector.
/// </summary>
public class BattleItemMenuUI : MonoBehaviour
{
    public static BattleItemMenuUI Instance { get; private set; }

    [Header("1. Gán thủ công qua Inspector")]
    [Tooltip("Kéo Panel tổng của Balo vào đây (để Bật/Tắt khi mở Balo)")]
    [SerializeField] private GameObject menuPanel;

    [Tooltip("Kéo Transform/GameObject chứa danh sách các ô vật phẩm vào đây")]
    [SerializeField] private Transform itemsContainer;

    [Tooltip("Kéo Prefab ô vật phẩm (ItemSlot) vào đây")]
    [SerializeField] private GameObject itemSlotPrefab;

    [Tooltip("Kéo nút Balo vào đây để nhận sự kiện bấm chuột")]
    [SerializeField] private Button backpackButton;

    [Tooltip("Kéo nút Đóng/Exit vào đây (nếu có)")]
    [SerializeField] private Button closeButton;

    [Header("2. Cài đặt Pokéball")]
    [SerializeField] private bool includePokeball = true;
    [SerializeField] private Sprite pokeballIcon;
    [SerializeField] private string pokeballDisplayName = "Bóng Bắt Thú";

    [Header("3. Phím tắt")]
    [SerializeField] private KeyCode toggleHotkey = KeyCode.B;

    private List<GameObject> spawnedSlots = new List<GameObject>();
    private bool isOpen = false;
    private int lastToggleFrame = -1;

    public bool IsOpen => menuPanel != null ? menuPanel.activeSelf : isOpen;
    public List<GameObject> SpawnedSlots => spawnedSlots;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // Gán sự kiện cho nút Balo được kéo thả thủ công
        if (backpackButton != null)
        {
            backpackButton.onClick.RemoveListener(ToggleMenu);
            backpackButton.onClick.AddListener(ToggleMenu);
        }

        // Gán sự kiện cho nút Đóng được kéo thả thủ công
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseMenu);
            closeButton.onClick.AddListener(CloseMenu);
        }

        // Đảm bảo ban đầu panel ở trạng thái ẩn
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        isOpen = false;
    }

    /// <summary>
    /// Bật / Tắt Menu Balo (Có chống gọi đúp 2 lần trong 1 frame)
    /// </summary>
    public void ToggleMenu()
    {
        // Không cho mở Balo nếu bảng xác nhận thoát đang mở
        if (BattleActionIconsUI.Instance != null && BattleActionIconsUI.Instance.IsEscapeConfirmOpen)
        {
            return;
        }

        if (Time.frameCount == lastToggleFrame) return;
        lastToggleFrame = Time.frameCount;

        if (IsOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        // Không cho mở Balo nếu bảng xác nhận thoát đang mở
        if (BattleActionIconsUI.Instance != null && BattleActionIconsUI.Instance.IsEscapeConfirmOpen)
        {
            return;
        }

        isOpen = true;

        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        BuildSlots();

        // Ép LayoutGroup tính toán vị trí ô ngay lập tức
        if (itemsContainer is RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        Canvas.ForceUpdateCanvases();

        // Tự động đưa khung viền vào ô đầu tiên trong Balo
        if (BattleKeyboardNavigationUI.Instance != null)
        {
            BattleKeyboardNavigationUI.Instance.FocusFirstBackpackSlot();
        }
    }

    public void CloseMenu()
    {
        isOpen = false;

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // Tự động tắt khung viền hoặc trả tiêu điểm về thanh chính
        if (BattleKeyboardNavigationUI.Instance != null)
        {
            BattleKeyboardNavigationUI.Instance.BuildMainBarList();
        }
        else if (UISelectionCursor.Instance != null)
        {
            UISelectionCursor.Instance.Hide();
        }
    }

    /// <summary>
    /// Sinh các ô vật phẩm vào itemsContainer
    /// </summary>
    public void BuildSlots()
    {
        if (itemSlotPrefab == null || itemsContainer == null)
        {
            Debug.LogWarning("[BattleItemMenuUI] Vui lòng kéo thả 'Item Slot Prefab' và 'Items Container' vào Inspector của Backpack!");
            return;
        }

        // Dọn dẹp các ô cũ
        foreach (var slot in spawnedSlots)
        {
            if (slot != null) Destroy(slot);
        }
        spawnedSlots.Clear();

        // 1. Ô Pokéball
        if (includePokeball)
        {
            CreatePokeballSlot();
        }

        // 2. Các ô bình thuốc từ BattleItemHandler
        if (BattleItemHandler.Instance != null && BattleItemHandler.Instance.BattleItems != null)
        {
            var items = BattleItemHandler.Instance.BattleItems;
            for (int i = 0; i < items.Count; i++)
            {
                CreatePotionSlot(items[i]);
            }
        }
    }

    private void CreatePokeballSlot()
    {
        BattleTransferData bData = Resources.Load<BattleTransferData>("BattleTransferData");
        bool isTrainerOrGym = bData != null && (bData.isTrainerBattle || bData.isGymLeaderBattle);

        Sprite icon = pokeballIcon;
        if (icon == null)
        {
            var pokeballAsset = Resources.Load<Kinnly.Item>("Item/Pokeball");
            if (pokeballAsset != null && pokeballAsset.image != null)
            {
                icon = pokeballAsset.image;
            }
        }

        string qtyText;
        bool interactable = true;

        if (isTrainerOrGym)
        {
            qtyText = "X";
            interactable = false;
        }
        else if (BattleCaptureHandler.Instance != null && BattleCaptureHandler.Instance.IsCaptureSessionActive)
        {
            qtyText = $"{BattleCaptureHandler.Instance.ThrowsLeft}/3";
            interactable = BattleCaptureHandler.Instance.ThrowsLeft > 0;
        }
        else
        {
            int inventoryCount = BattleItemHandler.Instance != null ? BattleItemHandler.Instance.GetItemCount("Pokeball") : 0;
            qtyText = $"x{inventoryCount}";
            interactable = inventoryCount > 0;
        }

        GameObject slotObj = Instantiate(itemSlotPrefab, itemsContainer);
        slotObj.SetActive(true);

        BattleItemSlotUI slotUI = slotObj.GetComponent<BattleItemSlotUI>();
        if (slotUI != null)
        {
            slotUI.Setup(icon, pokeballDisplayName, qtyText, interactable, OnPokeballClicked);
        }

        spawnedSlots.Add(slotObj);
    }

    private void CreatePotionSlot(BattleItemHandler.BattleItem item)
    {
        if (item == null) return;

        int qty = BattleItemHandler.Instance != null ? BattleItemHandler.Instance.GetItemCount(item.itemName) : 0;
        string qtyText = $"x{qty}";
        bool interactable = qty > 0;

        GameObject slotObj = Instantiate(itemSlotPrefab, itemsContainer);
        slotObj.SetActive(true);

        BattleItemSlotUI slotUI = slotObj.GetComponent<BattleItemSlotUI>();
        if (slotUI != null)
        {
            var capturedItem = item;
            slotUI.Setup(item.icon, item.displayName, qtyText, interactable, () => OnPotionClicked(capturedItem));
        }

        spawnedSlots.Add(slotObj);
    }

    private void OnPokeballClicked()
    {
        Debug.Log("<color=yellow>[BattleItemMenuUI] Đã bấm vào ô Pokéball!</color>");
        CloseMenu();
        
        var handler = BattleCaptureHandler.Instance != null 
            ? BattleCaptureHandler.Instance 
            : FindFirstObjectByType<BattleCaptureHandler>();

        if (handler != null)
        {
            handler.OnPokeballButtonPressed();
            handler.RefreshPokeballUI();
        }
        else
        {
            Debug.LogError("[BattleItemMenuUI] Không tìm thấy BattleCaptureHandler trong Scene!");
        }
    }

    private void OnPotionClicked(BattleItemHandler.BattleItem item)
    {
        if (BattleItemHandler.Instance != null)
        {
            bool used = BattleItemHandler.Instance.UseItem(item);
            if (used)
            {
                CloseMenu();
            }
            else
            {
                BuildSlots();
                if (BattleKeyboardNavigationUI.Instance != null)
                {
                    BattleKeyboardNavigationUI.Instance.FocusFirstBackpackSlot();
                }
            }
        }
    }

    private void Update()
    {
        // Nếu Panel xác nhận thoát đang mở -> Chặn hoàn toàn phím B
        if (BattleActionIconsUI.Instance != null && BattleActionIconsUI.Instance.IsEscapeConfirmOpen)
        {
            return;
        }

        // Phím tắt B
        if (Input.GetKeyDown(toggleHotkey))
        {
            ToggleMenu();
        }

        // Phím Escape để đóng
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }
}
