using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kinnly
{
    public class CraftingUI : MonoBehaviour
    {
        [System.Serializable]
        private class SelectedMaterial
        {
            public Item item;
            public int amount;

            public SelectedMaterial(Item item, int amount)
            {
                this.item = item;
                this.amount = amount;
            }
        }

        [Header("Main")]
        [SerializeField] private GameObject craftingPanel;
        [SerializeField] private PlayerInventory playerInventory;

        [Header("Material List")]
        [SerializeField] private Transform materialsContent;
        [SerializeField]
        private CraftingMaterialItemUI materialItemPrefab;

        [Tooltip(
            "Bật: chỉ hiện Item có isWoodResource. " +
            "Tắt: hiện tất cả Item trong Inventory."
        )]
        [SerializeField]
        private bool onlyShowWoodResources = true;

        [Header("Selected Slots")]
        [SerializeField]
        private List<SelectedMaterialSlotUI> selectedSlots =
            new List<SelectedMaterialSlotUI>();

        [Header("Recipes")]
        [SerializeField]
        private List<CraftingRecipeData> recipes =
            new List<CraftingRecipeData>();

        [Header("Result UI")]
        [SerializeField] private Image resultIcon;
        [SerializeField] private TMP_Text resultAmountText;
        [SerializeField] private TMP_Text resultNameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button craftButton;

        [Header("Buttons")]
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button clearButton;

        [Header("Status Colors")]
        [SerializeField]
        private Color warningColor =
            new Color(1f, 0.85f, 0.3f);

        [SerializeField]
        private Color errorColor =
            new Color(1f, 0.35f, 0.35f);

        [SerializeField]
        private Color successColor =
            new Color(0.3f, 1f, 0.45f);

        private readonly List<SelectedMaterial>
            selectedMaterials =
                new List<SelectedMaterial>();

        private readonly List<CraftingMaterialItemUI>
            materialCards =
                new List<CraftingMaterialItemUI>();

        private CraftingRecipeData currentRecipe;


        private void Start()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(OpenCrafting);
                openButton.onClick.AddListener(OpenCrafting);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseCrafting);
            }

            if (clearButton != null)
            {
                clearButton.onClick.RemoveAllListeners();
                clearButton.onClick.AddListener(ClearSelection);
            }

            if (craftButton != null)
            {
                craftButton.onClick.RemoveAllListeners();
                craftButton.onClick.AddListener(Craft);
            }

            RefreshSelectedSlots();
            UpdateResult();

            if (craftingPanel != null)
                craftingPanel.SetActive(false);
        }

        public void OpenCrafting()
        {
            if (craftingPanel == null ||
                playerInventory == null)
            {
                Debug.LogError(
                    "CraftingUI chưa được gán Panel hoặc Inventory."
                );
                return;
            }

            craftingPanel.SetActive(true);
            RefreshMaterials();
            RefreshSelectedSlots();
            UpdateResult();
        }

        public void CloseCrafting()
        {
            ClearSelection();

            if (craftingPanel != null)
                craftingPanel.SetActive(false);
        }

        private bool IsSameItem(Item first, Item second)
        {
            if (first == null || second == null)
                return false;

            if (first == second)
                return true;

            if (first.id != 0 && second.id != 0)
                return first.id == second.id;

            return first.name == second.name;
        }

        public void RefreshMaterials()
        {
            if (materialsContent == null ||
                materialItemPrefab == null ||
                playerInventory == null)
            {
                return;
            }

            foreach (Transform child in materialsContent)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            materialCards.Clear();

            Dictionary<Item, int> inventoryItems =
                playerInventory.GetAllItemAmounts(
                    onlyShowWoodResources
                );

            List<KeyValuePair<Item, int>> sortedItems =
                new List<KeyValuePair<Item, int>>(
                    inventoryItems
                );

            sortedItems.Sort(
                (first, second) =>
                    string.Compare(
                        first.Key.name,
                        second.Key.name,
                        System.StringComparison.Ordinal
                    )
            );

            foreach (KeyValuePair<Item, int> entry
                     in sortedItems)
            {
                if (entry.Key == null || entry.Value <= 0)
                    continue;

                CraftingMaterialItemUI newCard =
                    Instantiate(
                        materialItemPrefab,
                        materialsContent
                    );

                newCard.gameObject.SetActive(true);

                newCard.Setup(
                    entry.Key,
                    entry.Value,
                    SelectMaterial
                );

                newCard.SetSelected(
                    GetSelectedIndex(entry.Key) >= 0
                );

                materialCards.Add(newCard);
            }
        }

        private void SelectMaterial(Item item)
        {
            if (item == null || playerInventory == null)
                return;

            int ownedAmount =
                playerInventory.GetItemAmount(item);

            int selectedIndex =
                GetSelectedIndex(item);

            if (selectedIndex >= 0)
            {
                SelectedMaterial selected =
                    selectedMaterials[selectedIndex];

                if (selected.amount >= ownedAmount)
                {
                    SetStatus(
                        "Bạn không có thêm vật phẩm này",
                        warningColor
                    );
                    return;
                }

                selected.amount++;
            }
            else
            {
                if (selectedMaterials.Count >=
                    selectedSlots.Count)
                {
                    SetStatus(
                        "Đã chọn tối đa " +
                        selectedSlots.Count +
                        " loại nguyên liệu",
                        warningColor
                    );
                    return;
                }

                selectedMaterials.Add(
                    new SelectedMaterial(item, 1)
                );
            }

            RefreshSelectedSlots();
            RefreshCardSelection();
            UpdateResult();
        }

        private int GetSelectedIndex(Item item)
        {
            for (int i = 0;
                 i < selectedMaterials.Count;
                 i++)
            {
                if (IsSameItem(
                        selectedMaterials[i].item,
                        item))
                {
                    return i;
                }
            }

            return -1;
        }

        private void RemoveSelectedMaterial(Item item)
        {
            int index = GetSelectedIndex(item);

            if (index >= 0)
                selectedMaterials.RemoveAt(index);

            RefreshSelectedSlots();
            RefreshCardSelection();
            UpdateResult();
        }

        public void ClearSelection()
        {
            selectedMaterials.Clear();

            RefreshSelectedSlots();
            RefreshCardSelection();
            UpdateResult();
        }

        private void RefreshSelectedSlots()
        {
            for (int i = 0;
                 i < selectedSlots.Count;
                 i++)
            {
                if (selectedSlots[i] == null)
                    continue;

                if (i < selectedMaterials.Count)
                {
                    SelectedMaterial selected =
                        selectedMaterials[i];

                    selectedSlots[i].Setup(
                        selected.item,
                        selected.amount,
                        RemoveSelectedMaterial
                    );
                }
                else
                {
                    selectedSlots[i].Clear();
                }
            }
        }

        private void RefreshCardSelection()
        {
            foreach (CraftingMaterialItemUI card
                     in materialCards)
            {
                if (card == null)
                    continue;

                card.SetSelected(
                    GetSelectedIndex(card.BoundItem) >= 0
                );
            }
        }

        private CraftingRecipeData FindMatchingRecipe()
        {
            foreach (CraftingRecipeData recipe in recipes)
            {
                if (RecipeMatches(recipe))
                    return recipe;
            }

            return null;
        }

        private bool RecipeMatches(
            CraftingRecipeData recipe)
        {
            if (recipe == null ||
                recipe.ingredients == null ||
                recipe.resultItem == null)
            {
                return false;
            }

            List<Item> uniqueRecipeItems =
                new List<Item>();

            foreach (CraftingIngredient ingredient
                     in recipe.ingredients)
            {
                if (ingredient == null ||
                    ingredient.item == null ||
                    ingredient.amount <= 0)
                {
                    return false;
                }

                bool alreadyAdded = false;

                foreach (Item item in uniqueRecipeItems)
                {
                    if (IsSameItem(
                            item,
                            ingredient.item))
                    {
                        alreadyAdded = true;
                        break;
                    }
                }

                if (!alreadyAdded)
                    uniqueRecipeItems.Add(ingredient.item);
            }

            if (uniqueRecipeItems.Count !=
                selectedMaterials.Count)
            {
                return false;
            }

            foreach (SelectedMaterial selected
                     in selectedMaterials)
            {
                int requiredAmount = 0;

                foreach (CraftingIngredient ingredient
                         in recipe.ingredients)
                {
                    if (IsSameItem(
                            ingredient.item,
                            selected.item))
                    {
                        requiredAmount += ingredient.amount;
                    }
                }

                if (requiredAmount != selected.amount)
                    return false;
            }

            return true;
        }

        private bool HasAllIngredients(
            CraftingRecipeData recipe)
        {
            if (recipe == null ||
                playerInventory == null)
            {
                return false;
            }

            foreach (CraftingIngredient ingredient
                     in recipe.ingredients)
            {
                if (!playerInventory.HasItem(
                        ingredient.item,
                        ingredient.amount))
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateResult()
        {
            currentRecipe = null;
            ClearResultDisplay();

            if (selectedMaterials.Count < 2)
            {
                SetStatus(
                    "Hãy chọn ít nhất 2 loại nguyên liệu",
                    warningColor
                );

                SetCraftButton(false);
                return;
            }

            currentRecipe = FindMatchingRecipe();

            if (currentRecipe == null)
            {
                SetStatus(
                    "Không tìm thấy công thức",
                    errorColor
                );

                SetCraftButton(false);
                return;
            }

            if (resultIcon != null)
            {
                resultIcon.sprite =
                    currentRecipe.resultItem.image;

                resultIcon.gameObject.SetActive(true);
            }

            if (resultAmountText != null)
            {
                resultAmountText.text =
                    "x" + currentRecipe.resultAmount;

                resultAmountText.gameObject.SetActive(true);
            }

            if (resultNameText != null)
            {
                resultNameText.text =
                    currentRecipe.resultItem.name;
            }

            if (!HasAllIngredients(currentRecipe))
            {
                SetStatus(
                    "Không đủ nguyên liệu",
                    errorColor
                );

                SetCraftButton(false);
                return;
            }

            SetStatus("Có thể chế tạo", successColor);
            SetCraftButton(true);
        }

        private void ClearResultDisplay()
        {
            if (resultIcon != null)
            {
                resultIcon.sprite = null;
                resultIcon.gameObject.SetActive(false);
            }

            if (resultAmountText != null)
            {
                resultAmountText.text = "";
                resultAmountText.gameObject.SetActive(false);
            }

            if (resultNameText != null)
                resultNameText.text = "Chưa có công thức";
        }

        private void SetStatus(
            string message,
            Color color)
        {
            if (statusText == null)
                return;

            statusText.text = message;
            statusText.color = color;
        }

        private void SetCraftButton(bool interactable)
        {
            if (craftButton != null)
                craftButton.interactable = interactable;
        }

        public void Craft()
        {
            currentRecipe = FindMatchingRecipe();

            if (currentRecipe == null)
            {
                UpdateResult();
                return;
            }

            if (!HasAllIngredients(currentRecipe))
            {
                UpdateResult();
                return;
            }

            foreach (CraftingIngredient ingredient
                     in currentRecipe.ingredients)
            {
                bool removed =
                    playerInventory.TryRemoveItem(
                        ingredient.item,
                        ingredient.amount
                    );

                if (!removed)
                {
                    Debug.LogError(
                        "Không thể trừ nguyên liệu: " +
                        ingredient.item.name
                    );

                    RefreshMaterials();
                    UpdateResult();
                    return;
                }
            }

            playerInventory.AddItem(
                currentRecipe.resultItem,
                currentRecipe.resultAmount
            );

            playerInventory.SaveNow();

            RefreshMaterials();
            UpdateResult();
        }
    }
}