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
    [Header("HIỆU ỨNG UI")]
    [SerializeField] private ShopPanelHoverSetup hoverSetup;

    [Tooltip("Chức năng bán chưa làm. Có thể để trống.")]
    [SerializeField] private Button sellButton;

    [Header("TIỀN NGƯỜI CHƠI")]
    [SerializeField] private PlayerData playerData;
    [Header("QUEST")]
    [SerializeField] private QuestUIManager questUIManager;

    [Header("ITEM ĐƯỢC BÁN")]
    [SerializeField]
    private List<ShopItemData> items =
        new List<ShopItemData>();

    private readonly List<ShopItemUI> visibleItemSlots =
        new List<ShopItemUI>();

    private ShopItemData selectedItem;
    private ShopItemCategory? currentCategory;
    private bool isSellingMode = false;
    private Kinnly.InventoryItem selectedInventoryItemToSell;

    public bool IsOpen
    {
        get
        {
            return shopPanel != null && shopPanel.activeSelf;
        }
    }

    private void Awake()
    {
        if (playerData == null)
        {
            playerData = Resources.Load<PlayerData>("PlayerData");
        }
        
        AutoFindDetailReferences();
        EnsureFarmingItemsAvailable();
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

    private void EnsureFarmingItemsAvailable()
    {
        if (items == null) items = new List<ShopItemData>();

        bool HasItem(string keyword) => items.Exists(x => x != null && (
            (!string.IsNullOrEmpty(x.itemID) && x.itemID.ToLower().Contains(keyword)) ||
            (!string.IsNullOrEmpty(x.itemName) && x.itemName.ToLower().Contains(keyword))
        ));

        // 1. Cuốc (Hoe)
        if (!HasItem("cuốc") && !HasItem("hoe"))
        {
            var hoeItem = Resources.Load<Kinnly.Item>("Item/Kinnly/Kinnly_Hoe");
            if (hoeItem != null)
            {
                items.Add(new ShopItemData
                {
                    itemID = "item_hoe",
                    itemName = "Cuốc Nông Trại",
                    kinnlyItem = hoeItem,
                    icon = hoeItem.image,
                    category = ShopItemCategory.Item,
                    price = 50,
                    itemTypeText = "Công cụ",
                    description = "Dùng để cuốc đất gieo hạt giống. Chọn trên tay và click chuột trái.",
                    durability = 100,
                    startingOwned = 10
                });
            }
        }

        // 2. Bình tưới (WaterCan)
        if (!HasItem("bình") && !HasItem("water"))
        {
            var waterItem = Resources.Load<Kinnly.Item>("Item/Kinnly/Kinnly_WaterCan");
            if (waterItem != null)
            {
                items.Add(new ShopItemData
                {
                    itemID = "item_watercan",
                    itemName = "Bình Tưới Nước",
                    kinnlyItem = waterItem,
                    icon = waterItem.image,
                    category = ShopItemCategory.Item,
                    price = 50,
                    itemTypeText = "Công cụ",
                    description = "Dùng để tưới nước cho cây trồng mau lớn.",
                    durability = 100,
                    startingOwned = 10
                });
            }
        }

        // 3. Hạt giống Cà Chua
        if (!HasItem("cà chua") && !HasItem("tomato"))
        {
            var seed = Resources.Load<Kinnly.Item>("Item/Kinnly/tomato/Kinnly_TomatoSeed");
            if (seed != null)
            {
                items.Add(new ShopItemData
                {
                    itemID = "tomato_seed",
                    itemName = "Hạt Giống Cà Chua",
                    kinnlyItem = seed,
                    icon = seed.image,
                    category = ShopItemCategory.Seed,
                    price = 30,
                    itemTypeText = "Hạt Giống",
                    description = "Hạt giống cà chua chín đỏ mọng nước.",
                    durability = 100,
                    startingOwned = 20
                });
            }
        }

        // 4. Hạt giống Cà Rốt
        if (!HasItem("cà rốt") && !HasItem("carrot"))
        {
            var seed = Resources.Load<Kinnly.Item>("Item/Carrot/Kinnly_CarrotSeed");
            if (seed != null)
            {
                items.Add(new ShopItemData
                {
                    itemID = "carrot_seed",
                    itemName = "Hạt Giống Cà Rốt",
                    kinnlyItem = seed,
                    icon = seed.image,
                    category = ShopItemCategory.Seed,
                    price = 35,
                    itemTypeText = "Hạt Giống",
                    description = "Hạt giống cà rốt tươi ngon giòn ngọt.",
                    durability = 100,
                    startingOwned = 20
                });
            }
        }

        // 5. Hạt giống Khoai Tây
        if (!HasItem("khoai tây") && !HasItem("potato"))
        {
            var seed = Resources.Load<Kinnly.Item>("Item/Potato/Kinnly_PotatoSeed");
            if (seed != null)
            {
                items.Add(new ShopItemData
                {
                    itemID = "potato_seed",
                    itemName = "Hạt Giống Khoai Tây",
                    kinnlyItem = seed,
                    icon = seed.image,
                    category = ShopItemCategory.Seed,
                    price = 40,
                    itemTypeText = "Hạt Giống",
                    description = "Củ giống khoai tây bổ dưỡng dễ chăm sóc.",
                    durability = 100,
                    startingOwned = 20
                });
            }
        }

        // 6. Hạt giống Bắp (Ngô)
        if (!HasItem("ngô") && !HasItem("bắp") && !HasItem("corn"))
        {
            var seed = Resources.Load<Kinnly.Item>("Item/Corn/Kinnly_CornSeed");
            if (seed != null)
            {
                items.Add(new ShopItemData
                {
                    itemID = "corn_seed",
                    itemName = "Hạt Giống Bắp (Ngô)",
                    kinnlyItem = seed,
                    icon = seed.image,
                    category = ShopItemCategory.Seed,
                    price = 45,
                    itemTypeText = "Hạt Giống",
                    description = "Hạt giống bắp ngọt thơm bùi, năng suất cao.",
                    durability = 100,
                    startingOwned = 20
                });
            }
        }

        // 7. Thêm 5 loại Đá Tiến Hóa (Fire, Water, Grass, Light, Dark)
        EnsureEvolutionStone(items, "Fire_Evole_Stone", "Đá Tiến Hóa Lửa", "Viên đá chứa năng lượng hỏa diệm rực cháy, dùng để kích hoạt tiến hóa cho Beast hệ Lửa.");
        EnsureEvolutionStone(items, "Water_Evole_Stone", "Đá Tiến Hóa Nước", "Viên đá kết tinh từ đại dương xanh thẳm, dùng để kích hoạt tiến hóa cho Beast hệ Nước.");
        EnsureEvolutionStone(items, "Grass_Evole_Stone", "Đá Tiến Hóa Cỏ", "Viên ngọc tích tụ sinh khí rừng rậm ngàn năm, dùng để kích hoạt tiến hóa cho Beast hệ Cỏ.");
        EnsureEvolutionStone(items, "Light_Evole_Stone", "Đá Tiến Hóa Ánh Sáng", "Viên pha lê tỏa ánh hào quang thuần khiết, dùng để kích hoạt tiến hóa cho Beast hệ Quang.");
        EnsureEvolutionStone(items, "Dark_Evole_Stone", "Đá Tiến Hóa Bóng Tối", "Viên thạch anh huyền bí từ cõi hư vô, dùng để kích hoạt tiến hóa cho Beast hệ Ám.");
    }

    private void EnsureEvolutionStone(List<ShopItemData> list, string assetName, string displayName, string desc)
    {
        if (list.Exists(x => x != null && (x.itemID == assetName || (x.itemName != null && x.itemName.Equals(displayName, System.StringComparison.OrdinalIgnoreCase)))))
            return;

        var stoneItem = Resources.Load<Kinnly.Item>($"Item/EvolutionStones/{assetName}");
        if (stoneItem != null)
        {
            list.Add(new ShopItemData
            {
                itemID = assetName,
                itemName = displayName,
                kinnlyItem = stoneItem,
                icon = stoneItem.image,
                category = ShopItemCategory.Item,
                price = 500,
                itemTypeText = "Đá Tiến Hóa",
                description = desc,
                durability = 100,
                startingOwned = 5
            });
        }
    }

    private void AutoFindDetailReferences()
    {
        Transform root = transform;
        if (shopPanel != null) root = shopPanel.transform;

        // Luôn luôn ưu tiên tự động tìm đúng ô 'icon' dưới detailspanel/detailfame
        Transform iconTr = root.Find("detailspanel/detailfame/icon");
        if (iconTr == null) iconTr = root.Find("detailspanel/detailfame/Icon");
        if (iconTr == null) iconTr = root.Find("detailspanel/icon");
        if (iconTr == null) iconTr = root.Find("detailspanel/Icon");

        if (iconTr != null)
        {
            detailIcon = iconTr.GetComponent<Image>();
        }
        else
        {
            Transform detailPanelTr = root.Find("detailspanel");
            if (detailPanelTr != null)
            {
                Image[] images = detailPanelTr.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img.gameObject.name.Equals("icon", System.StringComparison.OrdinalIgnoreCase))
                    {
                        detailIcon = img;
                        break;
                    }
                }
            }
        }

        Transform detailGO = root.Find("GameObject");
        if (detailGO == null) detailGO = root;

        if (detailGO != null)
        {
            if (detailName == null) detailName = detailGO.Find("DetailName")?.GetComponent<TMP_Text>();
            if (detailType == null) detailType = detailGO.Find("DetailType")?.GetComponent<TMP_Text>();
            if (durabilityText == null) durabilityText = detailGO.Find("DurabilityText")?.GetComponent<TMP_Text>();
            if (descriptionText == null) descriptionText = detailGO.Find("DescriptionTitle")?.GetComponent<TMP_Text>();
            if (detailPriceText == null) detailPriceText = detailGO.Find("PriceText")?.GetComponent<TMP_Text>();

            if (buyButton == null) buyButton = detailGO.Find("ButtonBuy")?.GetComponent<Button>();
            if (cancelButton == null) cancelButton = detailGO.Find("ButtonCancel")?.GetComponent<Button>();
        }
    }

    private int openFrameCount = -1;

    private void Update()
    {
        if (IsOpen && Time.frameCount != openFrameCount)
        {
            if (EscapePressed() || Input.GetKeyDown(KeyCode.F))
            {
                CloseShop();
            }
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

        if (sellButton != null)
        {
            sellButton.interactable = true;
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(EnterSellMode);
        }
    }

    public void OpenShop()
    {
        openFrameCount = Time.frameCount;
        Debug.Log($"<color=green>[ShopManager]</color> OpenShop() đang được gọi trên Object: <b>{gameObject.name}</b>, thuộc Scene: <b>{gameObject.scene.name}</b>");

        if (shopPanel == null)
        {
            Debug.LogError("ShopManager chưa được gán ShopPanel.");
            return;
        }

        Debug.Log($"<color=yellow>[ShopManager]</color> Đang bật hiển thị cho bảng: <b>{shopPanel.name}</b>");
        
        // Đảm bảo tất cả Object cha (như PanelShop) được bật active
        Transform parentTr = shopPanel.transform.parent;
        while (parentTr != null && parentTr.GetComponent<Canvas>() == null)
        {
            parentTr.gameObject.SetActive(true);
            parentTr = parentTr.parent;
        }

        shopPanel.SetActive(true);

        RectTransform shopRect = shopPanel.GetComponent<RectTransform>();
        if (shopRect != null)
        {
            shopRect.anchorMin = new Vector2(0.5f, 0.5f);
            shopRect.anchorMax = new Vector2(0.5f, 0.5f);
            shopRect.pivot = new Vector2(0.5f, 0.5f);
            shopRect.anchoredPosition = Vector2.zero;
        }

        InteractHintManager.Instance?.RegisterPanelOpen();

        UpdateGoldText();
        CloseDetailPanel();
        EnterBuyMode();
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

        InteractHintManager.Instance?.RegisterPanelClose();
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

    private void EnterBuyMode()
    {
        isSellingMode = false;
        if (buyButton != null)
        {
            TMP_Text btnText = buyButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = "MUA";
        }
    }

    public void EnterSellMode()
    {
        isSellingMode = true;
        currentCategory = null;
        if (buyButton != null)
        {
            TMP_Text btnText = buyButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = "BÁN";
        }
        CreateSellItemList();
    }

    private void ShowAllItems()
    {
        EnterBuyMode();
        currentCategory = null;
        CreateItemList();
    }

    private void ShowWeapons()
    {
        EnterBuyMode();
        currentCategory = ShopItemCategory.Weapon;
        CreateItemList();
    }

    private void ShowArmor()
    {
        EnterBuyMode();
        currentCategory = ShopItemCategory.Armor;
        CreateItemList();
    }

    private void ShowNormalItems()
    {
        EnterBuyMode();
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
                // Cho phép hiển thị Hạt giống khi người chơi xem tab Vật Phẩm (Item)
                if (!(currentCategory.Value == ShopItemCategory.Item && item.category == ShopItemCategory.Seed))
                {
                    continue;
                }
            }

            // Hiển thị đầy đủ tất cả vật phẩm và công cụ đã cấu hình trong Shop
            ShopItemUI newSlot = Instantiate(itemPrefab, content);

            newSlot.gameObject.SetActive(true);
            newSlot.Setup(item, this);

            Button slotBtn = newSlot.GetComponent<Button>();
            if (slotBtn == null) slotBtn = newSlot.GetComponentInChildren<Button>();
            if (slotBtn != null)
            {
                ShopItemData captureData = item;
                slotBtn.onClick.RemoveAllListeners();
                slotBtn.onClick.AddListener(() => {
                    SelectItem(captureData);
                });
            }

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
        if (hoverSetup != null)
        {
            hoverSetup.RefreshHoverEffects();
        }
    }

    private void CreateSellItemList()
    {
        if (content == null || itemPrefab == null) return;

        visibleItemSlots.Clear();
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }

        var playerInventory = Kinnly.Player.Instance?.GetComponent<Kinnly.PlayerInventory>();
        if (playerInventory != null)
        {
            foreach (GameObject slot in playerInventory.InventorySlots)
            {
                if (slot.transform.childCount > 0)
                {
                    Kinnly.InventoryItem invItem = slot.GetComponentInChildren<Kinnly.InventoryItem>();
                    if (invItem != null && invItem.Item != null)
                    {
                        // Bỏ qua không cho bán các vật phẩm đặc biệt (Cuốc, Bình nước, Rìu, Cần câu...)
                        if (IsSpecialOrKeyItem(invItem.Item))
                        {
                            continue;
                        }

                        ShopItemData tempSellData = new ShopItemData
                        {
                            itemID = invItem.Item.name,
                            itemName = invItem.Item.name,
                            icon = invItem.Item.image,
                            category = ShopItemCategory.Item,
                            price = invItem.Item.price > 0 ? invItem.Item.price : 10,
                            description = invItem.Item.description + "\n\n<color=yellow>(Vật phẩm trong túi của bạn)</color>",
                            startingOwned = invItem.Amount,
                            owned = invItem.Amount
                        };

                        ShopItemUI newSlot = Instantiate(itemPrefab, content);
                        newSlot.gameObject.SetActive(true);
                        newSlot.Setup(tempSellData, this);
                        // Cấu hình tạm để truyền InventoryItem qua
                        newSlot.GetComponent<Button>().onClick.AddListener(() => {
                            selectedInventoryItemToSell = invItem;
                            SelectItem(tempSellData);
                        });
                        visibleItemSlots.Add(newSlot);
                    }
                }
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        if (itemScrollRect != null) itemScrollRect.verticalNormalizedPosition = 1f;
        if (hoverSetup != null) hoverSetup.RefreshHoverEffects();
    }

    public static bool IsSpecialOrKeyItem(Kinnly.Item item)
    {
        if (item == null) return false;

        // 1. Kiểm tra cờ isSpecialItem hoặc isTools
        if (item.isSpecialItem || item.isTools || item.isAxe || item.isPickaxe) return true;

        // 2. Kiểm tra tên vật phẩm nhiệm vụ đặc biệt
        string nameLower = item.name.ToLower();
        if (nameLower.Contains("cuốc") || nameLower.Contains("hoe") ||
            nameLower.Contains("bình nước") || nameLower.Contains("bình tưới") || nameLower.Contains("watercan") ||
            nameLower.Contains("cần câu") || nameLower.Contains("rod") ||
            nameLower.Contains("rìu") || nameLower.Contains("axe"))
        {
            return true;
        }

        return false;
    }


    public void SelectItem(ShopItemData item)
    {
        if (item == null)
        {
            return;
        }

        selectedItem = item;
        if (!isSellingMode) selectedInventoryItemToSell = null;

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
            Sprite targetSprite = selectedItem.icon;
            if (targetSprite == null && selectedItem.kinnlyItem != null)
            {
                targetSprite = selectedItem.kinnlyItem.image;
            }

            if (targetSprite != null)
            {
                detailIcon.sprite = targetSprite;
                detailIcon.color = Color.white; // Tự động bật Alpha = 255
                detailIcon.enabled = true;
                detailIcon.preserveAspect = true;
            }
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

        if (isSellingMode)
        {
            // BÁN ĐỒ
            if (selectedInventoryItemToSell == null)
            {
                SetMessage("Lỗi không tìm thấy vật phẩm trong túi!");
                return;
            }

            var playerInventory = Kinnly.Player.Instance?.GetComponent<Kinnly.PlayerInventory>();
            if (playerInventory != null)
            {
                // Cộng tiền cho người chơi
                AddGold(selectedItem.price);
                // Xóa 1 item trong kho
                playerInventory.RemoveItem(selectedInventoryItemToSell, 1);
                SetMessage("Đã bán " + selectedItem.itemName);
                Debug.Log("BÁN THÀNH CÔNG: " + selectedItem.itemName + " thu được " + selectedItem.price + " G");

                // Lưu lại thông tin trước khi reset
                string soldItemID = selectedItem.itemID;
                string soldItemName = selectedItem.itemName;
                int soldItemPrice = selectedItem.price;

                // Làm mới danh sách bán
                CreateSellItemList();
                CloseDetailPanel();

                if (questUIManager != null)
                {
                    questUIManager.NotifyItemSold(soldItemID);
                }

                // Cập nhật hệ thống nhiệm vụ mới (Quest 5: Bán hàng cho Shop)
                if (global::QuestManager.Instance != null)
                {
                    global::QuestManager.Instance.OnItemsSold();

                    // Cập nhật Quest 19: Bán cá kiếm tiền
                    string sellName = soldItemName.ToLower();
                    if (sellName.Contains("cá") || sellName.Contains("fish") || sellName.Contains("crab") ||
                        sellName.Contains("willy") || sellName.Contains("pam") || sellName.Contains("shane") || 
                        sellName.Contains("krobus") || sellName.Contains("linus"))
                    {
                        global::QuestManager.Instance.OnFishSold(soldItemPrice);
                    }
                }
            }
        }
        else
        {
            // MUA ĐỒ (CŨ)
            if (playerData != null && playerData.gold < selectedItem.price)
            {
                SetMessage("Không đủ vàng!");
                Debug.Log("Mua thất bại: không đủ vàng.");
                return;
            }

            // THÊM VÀO TÚI ĐỒ NẾU CÓ VẬT PHẨM KINNLY
            var playerInventory = Kinnly.Player.Instance?.GetComponent<Kinnly.PlayerInventory>();
            if (selectedItem.kinnlyItem != null && playerInventory != null)
            {
                // Kiểm tra xem túi đồ có đầy không
                if (!playerInventory.IsSlotAvailable(selectedItem.kinnlyItem, 1))
                {
                    SetMessage("Túi đồ đã đầy!");
                    return;
                }
                playerInventory.AddItem(selectedItem.kinnlyItem, 1);
            }

            if (playerData != null)
            {
                playerData.gold -= selectedItem.price;
                playerData.Save();
            }
            selectedItem.owned++;

            UpdateGoldText();
            RefreshVisibleOwnedAmounts();

            SetMessage("Đã mua " + selectedItem.itemName);
            Debug.Log("MUA THÀNH CÔNG: " + selectedItem.itemName + " | Item ID: " + selectedItem.itemID);

            if (questUIManager != null)
            {
                questUIManager.NotifyItemPurchased("");
            }
        }
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
        if (goldText != null && playerData != null)
        {
            goldText.text = playerData.gold.ToString("N0") + " G";
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
    public void AddGold(int amount)
    {
        if (amount <= 0 || playerData == null)
        {
            return;
        }

        playerData.gold += amount;
        playerData.Save();
        UpdateGoldText();

        Debug.Log("Nhận thêm " + amount + " Gold");
    }
}