using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kinnly;

/// <summary>
/// Gắn vào CraftingStation GameObject.
/// Khi Player tương tác → mở panel danh sách công thức → nhấn Craft để chế tạo.
/// </summary>
public class CraftingStation : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class CraftingRecipe
    {
        public string recipeName = "Mồi Nhử";
        public Sprite recipeIcon;                   // Icon sản phẩm
        public string requiredItemName = "Tomato";  // Tên nguyên liệu
        public int requiredAmount = 1;
        public string rewardItemName = "Bait";      // Tên vật phẩm chế tạo ra
        public int rewardAmount = 1;
        [TextArea] public string description = "Dùng 1 Cà Chua để chế tạo Mồi Nhử.";
    }

    [Header("Danh sách công thức")]
    public List<CraftingRecipe> recipes = new List<CraftingRecipe>();

    // ── UI References (kéo thả trong Inspector) ──
    [Header("UI Panel")]
    public GameObject craftingPanel;            // Panel bọc ngoài (tắt mặc định)
    public Transform recipeListParent;          // Content ScrollView chứa các recipe row
    public GameObject recipeRowPrefab;          // Prefab 1 hàng công thức (xem hướng dẫn)
    public Button btnClose;                     // Nút đóng panel

    [Header("Chi tiết công thức đang chọn")]
    public Image selectedIcon;
    public TextMeshProUGUI selectedName;
    public TextMeshProUGUI selectedDesc;
    public TextMeshProUGUI selectedCost;
    public Button btnCraft;

    private PlayerInventory currentPlayerInv;
    private CraftingRecipe selectedRecipe;

    // ─────────────────────────────────────────────
    //  IInteractable — Gọi khi Player ấn E
    // ─────────────────────────────────────────────
    public void Interact(PlayerInventory playerInventory)
    {
        if (playerInventory == null) return;
        currentPlayerInv = playerInventory;
        OpenPanel();
    }

    private void Start()
    {
        if (craftingPanel != null) craftingPanel.SetActive(false);
        if (btnClose != null) btnClose.onClick.AddListener(ClosePanel);
        if (btnCraft != null) btnCraft.onClick.AddListener(DoCraft);
    }

    // ─────────────────────────────────────────────
    //  Mở / Đóng panel
    // ─────────────────────────────────────────────
    private void OpenPanel()
    {
        if (craftingPanel == null) { FallbackCraft(); return; }

        craftingPanel.SetActive(true);
        PopulateRecipeList();
        ClearDetail();

        // Pause movement
        Time.timeScale = 0f;
    }

    private void ClosePanel()
    {
        if (craftingPanel != null) craftingPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ─────────────────────────────────────────────
    //  Tạo danh sách công thức
    // ─────────────────────────────────────────────
    private void PopulateRecipeList()
    {
        if (recipeListParent == null || recipeRowPrefab == null) return;

        // Xóa cũ
        foreach (Transform child in recipeListParent)
            Destroy(child.gameObject);

        foreach (var recipe in recipes)
        {
            GameObject row = Instantiate(recipeRowPrefab, recipeListParent);

            // Icon sản phẩm
            Image icon = row.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && recipe.recipeIcon != null) icon.sprite = recipe.recipeIcon;

            // Tên công thức
            TextMeshProUGUI nameText = row.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = recipe.recipeName;

            // Trạng thái có đủ nguyên liệu không
            bool canCraft = HasEnoughIngredients(recipe);
            Image bg = row.GetComponent<Image>();
            if (bg != null) bg.color = canCraft ? new Color(0.6f, 1f, 0.6f, 0.9f) : new Color(1f, 1f, 1f, 0.7f);

            // Gắn sự kiện chọn hàng
            CraftingRecipe captured = recipe;
            Button rowBtn = row.GetComponent<Button>();
            if (rowBtn != null) rowBtn.onClick.AddListener(() => SelectRecipe(captured));
        }
    }

    // ─────────────────────────────────────────────
    //  Chọn công thức → hiện chi tiết
    // ─────────────────────────────────────────────
    private void SelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;

        if (selectedIcon != null) selectedIcon.sprite = recipe.recipeIcon;
        if (selectedName != null) selectedName.text = recipe.recipeName;
        if (selectedDesc != null) selectedDesc.text = recipe.description;
        if (selectedCost != null) selectedCost.text = $"Cần: {recipe.requiredAmount}x {recipe.requiredItemName}";

        if (btnCraft != null)
        {
            btnCraft.interactable = HasEnoughIngredients(recipe);
        }
    }

    private void ClearDetail()
    {
        selectedRecipe = null;
        if (selectedIcon != null) selectedIcon.sprite = null;
        if (selectedName != null) selectedName.text = "← Chọn công thức";
        if (selectedDesc != null) selectedDesc.text = "";
        if (selectedCost != null) selectedCost.text = "";
        if (btnCraft != null) btnCraft.interactable = false;
    }

    // ─────────────────────────────────────────────
    //  Chế tạo
    // ─────────────────────────────────────────────
    private void DoCraft()
    {
        if (selectedRecipe == null || currentPlayerInv == null) return;

        // Tìm nguyên liệu
        InventoryItem itemToConsume = null;
        foreach (GameObject slot in currentPlayerInv.InventorySlots)
        {
            var invItem = slot.GetComponentInChildren<InventoryItem>(true);
            if (invItem != null && invItem.Item != null
                && invItem.Item.name == selectedRecipe.requiredItemName
                && invItem.Amount >= selectedRecipe.requiredAmount)
            {
                itemToConsume = invItem;
                break;
            }
        }

        if (itemToConsume == null)
        {
            Debug.Log($"<color=orange>[Crafting]</color> Không đủ nguyên liệu: {selectedRecipe.requiredItemName}");
            if (btnCraft != null) btnCraft.interactable = false;
            return;
        }

        // Trừ nguyên liệu
        currentPlayerInv.RemoveItem(itemToConsume, selectedRecipe.requiredAmount);

        // Thêm sản phẩm vào túi
        Item rewardItem = QuestManager.Instance?.GetItemByName(selectedRecipe.rewardItemName);
        if (rewardItem == null)
        {
            var allItems = Resources.LoadAll<Item>("");
            rewardItem = System.Array.Find(allItems, x => x != null && x.name == selectedRecipe.rewardItemName);
        }
        if (rewardItem != null)
        {
            currentPlayerInv.AddItem(rewardItem, selectedRecipe.rewardAmount);
        }

        // Lưu kho đồ
        currentPlayerInv.SaveNow();

        // Thông báo QuestManager
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnBaitCrafted();

        Debug.Log($"<color=green>[Crafting]</color> Đã chế tạo: {selectedRecipe.recipeName}");

        // Refresh danh sách
        PopulateRecipeList();
        ClearDetail();
    }

    // ─────────────────────────────────────────────
    //  Helper
    // ─────────────────────────────────────────────
    private bool HasEnoughIngredients(CraftingRecipe recipe)
    {
        if (currentPlayerInv == null) return false;
        foreach (GameObject slot in currentPlayerInv.InventorySlots)
        {
            var invItem = slot.GetComponentInChildren<InventoryItem>(true);
            if (invItem != null && invItem.Item != null
                && invItem.Item.name == recipe.requiredItemName
                && invItem.Amount >= recipe.requiredAmount)
                return true;
        }
        return false;
    }

    /// <summary>Fallback nếu không có UI Panel được gán — craft thẳng như cũ.</summary>
    private void FallbackCraft()
    {
        if (recipes.Count == 0) return;
        var recipe = recipes[0];

        InventoryItem itemToConsume = null;
        foreach (GameObject slot in currentPlayerInv.InventorySlots)
        {
            var invItem = slot.GetComponentInChildren<InventoryItem>(true);
            if (invItem != null && invItem.Item != null
                && invItem.Item.name == recipe.requiredItemName
                && invItem.Amount >= recipe.requiredAmount)
            { itemToConsume = invItem; break; }
        }

        if (itemToConsume != null)
        {
            currentPlayerInv.RemoveItem(itemToConsume, recipe.requiredAmount);
            Item reward = QuestManager.Instance?.GetItemByName(recipe.rewardItemName);
            if (reward != null) currentPlayerInv.AddItem(reward, recipe.rewardAmount);
            currentPlayerInv.SaveNow();
            if (QuestManager.Instance != null) QuestManager.Instance.OnBaitCrafted();
        }
    }
}
