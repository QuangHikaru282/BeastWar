using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ShopManager : MonoBehaviour
{
    [Header("SHOP CHÍNH")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TMP_Text goldText;

    [Header("DANH SÁCH ITEM")]
    [SerializeField] private RectTransform content;
    [SerializeField] private ShopItemUI itemPrefab;
    [SerializeField] private ScrollRect itemScrollRect;

    [Header("DETAIL PANEL")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailType;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text durabilityText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text detailPriceText;

    [Header("NÚT DETAIL")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;

    [Tooltip("Không bắt buộc. Có thể để trống.")]
    [SerializeField] private TMP_Text messageText;

    [Header("NÚT DANH MỤC")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button weaponButton;
    [SerializeField] private Button armorButton;
    [SerializeField] private Button itemButton;

    [Tooltip("Chức năng bán chưa làm. Có thể để trống.")]
    [SerializeField] private Button sellButton;

    [Header("TIỀN NGƯỜI CHƠI")]
    [SerializeField, Min(0)] private int playerGold = 12500;

    [Header("ITEM ĐƯỢC BÁN")]
    [SerializeField]
    private List<ShopItemData> items =
        new List<ShopItemData>();

    private readonly List<ShopItemUI> visibleItemSlots =
        new List<ShopItemUI>();

    private ShopItemData selectedItem;
    private ShopItemCategory? currentCategory;

    public bool IsOpen
    {
        get
        {
            return shopPanel != null && shopPanel.activeSelf;
        }
    }

    private void Awake()
    {
        InitializeOwnedAmounts();
        SetupButtons();

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (IsOpen && EscapePressed())
        {
            CloseShop();
        }
    }

    private void InitializeOwnedAmounts()
    {
        foreach (ShopItemData item in items)
        {
            if (item == null)
            {
                continue;
            }

            item.owned = item.startingOwned;
        }
    }

    private void SetupButtons()
    {
        if (allButton != null)
        {
            allButton.onClick.RemoveAllListeners();
            allButton.onClick.AddListener(ShowAllItems);
        }

        if (weaponButton != null)
        {
            weaponButton.onClick.RemoveAllListeners();
            weaponButton.onClick.AddListener(ShowWeapons);
        }

        if (armorButton != null)
        {
            armorButton.onClick.RemoveAllListeners();
            armorButton.onClick.AddListener(ShowArmor);
        }

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(ShowNormalItems);
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuySelectedItem);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CloseDetailPanel);
        }

        // Chức năng Sell chưa được làm.
        if (sellButton != null)
        {
            sellButton.interactable = false;
        }
    }

    public void OpenShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopManager chưa được gán ShopPanel.");
            return;
        }

        shopPanel.SetActive(true);

        UpdateGoldText();
        CloseDetailPanel();
        ShowAllItems();
    }

    public void CloseShop()
    {
        selectedItem = null;

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    public void ToggleShop()
    {
        if (IsOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }

    private void ShowAllItems()
    {
        currentCategory = null;
        CreateItemList();
    }

    private void ShowWeapons()
    {
        currentCategory = ShopItemCategory.Weapon;
        CreateItemList();
    }

    private void ShowArmor()
    {
        currentCategory = ShopItemCategory.Armor;
        CreateItemList();
    }

    private void ShowNormalItems()
    {
        currentCategory = ShopItemCategory.Item;
        CreateItemList();
    }

    private void CreateItemList()
    {
        if (content == null || itemPrefab == null)
        {
            Debug.LogError(
                "ShopManager chưa được gán Content hoặc Item Prefab."
            );

            return;
        }

        visibleItemSlots.Clear();

        // Xóa các item cũ trong Content.
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;

            // Ẩn ngay để Grid Layout không tính object đang chờ Destroy.
            child.SetActive(false);
            Destroy(child);
        }

        // Tạo các item phù hợp với danh mục đang chọn.
        foreach (ShopItemData item in items)
        {
            if (item == null)
            {
                continue;
            }

            if (currentCategory.HasValue &&
                item.category != currentCategory.Value)
            {
                continue;
            }

            ShopItemUI newSlot = Instantiate(itemPrefab, content);

            newSlot.gameObject.SetActive(true);
            newSlot.Setup(item, this);

            visibleItemSlots.Add(newSlot);
        }

        // Yêu cầu Unity tính lại kích thước Grid và Content.
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // Đưa Scroll View về đầu danh sách.
        if (itemScrollRect != null)
        {
            itemScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public void SelectItem(ShopItemData item)
    {
        if (item == null)
        {
            return;
        }

        selectedItem = item;

        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
        }

        RefreshDetailPanel();
        SetMessage("");
    }

    private void RefreshDetailPanel()
    {
        if (selectedItem == null)
        {
            return;
        }

        if (detailIcon != null)
        {
            detailIcon.sprite = selectedItem.icon;
            detailIcon.enabled = selectedItem.icon != null;
            detailIcon.preserveAspect = true;
        }

        if (detailName != null)
        {
            detailName.text = selectedItem.itemName;
        }

        if (detailType != null)
        {
            if (string.IsNullOrWhiteSpace(selectedItem.itemTypeText))
            {
                detailType.text = selectedItem.category.ToString();
            }
            else
            {
                detailType.text = selectedItem.itemTypeText;
            }
        }

        if (attackText != null)
        {
            bool hasAttack = selectedItem.attack > 0;

            attackText.gameObject.SetActive(hasAttack);

            if (hasAttack)
            {
                attackText.text =
                    "Attack: +" + selectedItem.attack;
            }
        }

        if (durabilityText != null)
        {
            bool hasDurability = selectedItem.durability > 0;

            durabilityText.gameObject.SetActive(hasDurability);

            if (hasDurability)
            {
                durabilityText.text =
                    "Durability: " +
                    selectedItem.durability +
                    "/" +
                    selectedItem.durability;
            }
        }

        if (descriptionText != null)
        {
            descriptionText.text = selectedItem.description;
        }

        if (detailPriceText != null)
        {
            detailPriceText.text =
                "Price: " +
                selectedItem.price.ToString("N0") +
                " G";
        }
    }

    private void BuySelectedItem()
    {
        if (selectedItem == null)
        {
            SetMessage("Hãy chọn một vật phẩm.");
            return;
        }

        if (playerGold < selectedItem.price)
        {
            SetMessage("Không đủ vàng!");
            Debug.Log("Không đủ vàng để mua " + selectedItem.itemName);
            return;
        }

        playerGold -= selectedItem.price;
        selectedItem.owned++;

        UpdateGoldText();
        RefreshVisibleOwnedAmounts();

        SetMessage("Đã mua " + selectedItem.itemName);

        Debug.Log(
            "Đã mua " +
            selectedItem.itemName +
            ". Owned: " +
            selectedItem.owned
        );
    }

    private void RefreshVisibleOwnedAmounts()
    {
        foreach (ShopItemUI slot in visibleItemSlots)
        {
            if (slot != null)
            {
                slot.RefreshOwned();
            }
        }
    }

    private void CloseDetailPanel()
    {
        selectedItem = null;

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        SetMessage("");
    }

    private void UpdateGoldText()
    {
        if (goldText != null)
        {
            goldText.text = playerGold.ToString("N0") + " G";
        }
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    private bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}