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

        private class IngredientAmount
        {
            public Item item;
            public int amount;

            public IngredientAmount(Item item, int amount)
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
        [SerializeField] private CraftingMaterialItemUI materialItemPrefab;

        [Tooltip("Bật: chỉ hiện Item có isWoodResource. Tắt: hiện tất cả Item trong Inventory.")]
        [SerializeField] private bool onlyShowWoodResources = true;

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

        [Header("Recipe Requirements - Hiển thị bên Vật phẩm mới")]
        [SerializeField] private Transform recipeRequirementsContent;
        [SerializeField] private CraftingRecipeIngredientUI recipeRequirementPrefab;

        [Header("Main Buttons")]
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button clearButton;

        [Header("Recipe List - Phần mới")]
        [SerializeField] private Button recipeListButton;
        [SerializeField] private GameObject recipeListPanel;
        [SerializeField] private Button closeRecipeListButton;
        [SerializeField] private Transform recipeListContent;
        [SerializeField] private CraftingRecipeListItemUI recipeListItemPrefab;
        [SerializeField] private bool hideRecipeListAfterSelection = true;

        [Header("Status Colors")]
        [SerializeField] private Color warningColor = new Color(1f, 0.85f, 0.3f);
        [SerializeField] private Color errorColor = new Color(1f, 0.35f, 0.35f);
        [SerializeField] private Color successColor = new Color(0.3f, 1f, 0.45f);

        private readonly List<SelectedMaterial> selectedMaterials =
            new List<SelectedMaterial>();

        private readonly List<CraftingMaterialItemUI> materialCards =
            new List<CraftingMaterialItemUI>();

        // Công thức khớp với các nguyên liệu người chơi tự chọn.
        private CraftingRecipeData currentRecipe;

        // Công thức người chơi bấm trong danh sách để xem trước.
        // Biến này không làm thay đổi selectedMaterials.
        private CraftingRecipeData viewedRecipe;

        private void Start()
        {
            RegisterButton(openButton, OpenCrafting);
            RegisterButton(closeButton, CloseCrafting);
            RegisterButton(clearButton, ClearSelection);
            RegisterButton(craftButton, Craft);
            RegisterButton(recipeListButton, ToggleRecipeList);
            RegisterButton(closeRecipeListButton, HideRecipeList);

            RefreshSelectedSlots();
            UpdateResult();

            if (recipeListPanel != null)
                recipeListPanel.SetActive(false);

            if (craftingPanel != null)
                craftingPanel.SetActive(false);
        }

        private void RegisterButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        public void OpenCrafting()
        {
            if (craftingPanel == null || playerInventory == null)
            {
                Debug.LogError("CraftingUI chưa được gán Crafting Panel hoặc Player Inventory.");
                return;
            }

            craftingPanel.SetActive(true);
            viewedRecipe = null;
            HideRecipeList();
            RefreshMaterials();
            RefreshSelectedSlots();
            UpdateResult();
        }

        public void CloseCrafting()
        {
            HideRecipeList();
            viewedRecipe = null;
            ClearSelection();

            if (craftingPanel != null)
                craftingPanel.SetActive(false);
        }

        public void ToggleRecipeList()
        {
            if (recipeListPanel == null)
            {
                Debug.LogError("Recipe List Panel chưa được gán trong CraftingUI.");
                return;
            }

            if (recipeListPanel.activeSelf)
                HideRecipeList();
            else
                ShowRecipeList();
        }

        public void ShowRecipeList()
        {
            if (recipeListPanel == null ||
                recipeListContent == null ||
                recipeListItemPrefab == null)
            {
                Debug.LogError("Recipe List chưa được gán đủ Panel, Content hoặc Prefab.");
                return;
            }

            recipeListPanel.SetActive(true);
            RefreshRecipeList();
        }

        public void HideRecipeList()
        {
            if (recipeListPanel != null)
                recipeListPanel.SetActive(false);
        }

        public void RefreshRecipeList()
        {
            if (recipeListContent == null ||
                recipeListItemPrefab == null ||
                playerInventory == null)
            {
                return;
            }

            foreach (Transform child in recipeListContent)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            foreach (CraftingRecipeData recipe in recipes)
            {
                if (recipe == null || recipe.resultItem == null)
                    continue;

                CraftingRecipeListItemUI newRow = Instantiate(
                    recipeListItemPrefab,
                    recipeListContent
                );

                newRow.gameObject.SetActive(true);
                newRow.Setup(recipe, playerInventory, SelectRecipeFromList);
            }
        }

        /// <summary>
        /// Chỉ hiển thị công thức ở khu vực "Vật phẩm mới".
        /// Không tự thêm hoặc xóa nguyên liệu ở khu vực "Đã chọn".
        /// </summary>
        public void SelectRecipeFromList(CraftingRecipeData recipe)
        {
            if (recipe == null || recipe.resultItem == null)
                return;

            List<IngredientAmount> ingredients = GetAggregatedIngredients(recipe);

            if (ingredients.Count == 0)
            {
                SetStatus("Công thức không có nguyên liệu hợp lệ", errorColor);
                return;
            }

            viewedRecipe = recipe;
            UpdateResult();

            if (hideRecipeListAfterSelection)
                HideRecipeList();
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
                playerInventory.GetAllItemAmounts(onlyShowWoodResources);

            List<KeyValuePair<Item, int>> sortedItems =
                new List<KeyValuePair<Item, int>>(inventoryItems);

            sortedItems.Sort(
                (first, second) => string.Compare(
                    first.Key.name,
                    second.Key.name,
                    System.StringComparison.Ordinal
                )
            );

            foreach (KeyValuePair<Item, int> entry in sortedItems)
            {
                if (entry.Key == null || entry.Value <= 0)
                    continue;

                CraftingMaterialItemUI newCard = Instantiate(
                    materialItemPrefab,
                    materialsContent
                );

                newCard.gameObject.SetActive(true);
                newCard.Setup(entry.Key, entry.Value, SelectMaterial);
                newCard.SetSelected(GetSelectedIndex(entry.Key) >= 0);
                materialCards.Add(newCard);
            }
        }

        private void SelectMaterial(Item item)
        {
            if (item == null || playerInventory == null)
                return;

            int ownedAmount = playerInventory.GetItemAmount(item);
            int selectedIndex = GetSelectedIndex(item);

            if (selectedIndex >= 0)
            {
                SelectedMaterial selected = selectedMaterials[selectedIndex];

                if (selected.amount >= ownedAmount)
                {
                    SetStatus("Bạn không có thêm vật phẩm này", warningColor);
                    return;
                }

                selected.amount++;
            }
            else
            {
                if (selectedMaterials.Count >= selectedSlots.Count)
                {
                    SetStatus(
                        "Đã chọn tối đa " + selectedSlots.Count + " loại nguyên liệu",
                        warningColor
                    );
                    return;
                }

                selectedMaterials.Add(new SelectedMaterial(item, 1));
            }

            RefreshSelectedSlots();
            RefreshCardSelection();
            UpdateResult();
        }

        private int GetSelectedIndex(Item item)
        {
            for (int i = 0; i < selectedMaterials.Count; i++)
            {
                if (IsSameItem(selectedMaterials[i].item, item))
                    return i;
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
            for (int i = 0; i < selectedSlots.Count; i++)
            {
                if (selectedSlots[i] == null)
                    continue;

                if (i < selectedMaterials.Count)
                {
                    SelectedMaterial selected = selectedMaterials[i];
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
            foreach (CraftingMaterialItemUI card in materialCards)
            {
                if (card != null)
                    card.SetSelected(GetSelectedIndex(card.BoundItem) >= 0);
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

        private bool RecipeMatches(CraftingRecipeData recipe)
        {
            if (recipe == null || recipe.resultItem == null)
                return false;

            List<IngredientAmount> ingredients = GetAggregatedIngredients(recipe);

            if (ingredients.Count != selectedMaterials.Count)
                return false;

            foreach (SelectedMaterial selected in selectedMaterials)
            {
                IngredientAmount matchingIngredient = null;

                foreach (IngredientAmount ingredient in ingredients)
                {
                    if (IsSameItem(ingredient.item, selected.item))
                    {
                        matchingIngredient = ingredient;
                        break;
                    }
                }

                if (matchingIngredient == null ||
                    matchingIngredient.amount != selected.amount)
                {
                    return false;
                }
            }

            return true;
        }

        private List<IngredientAmount> GetAggregatedIngredients(
            CraftingRecipeData recipe)
        {
            List<IngredientAmount> result = new List<IngredientAmount>();

            if (recipe == null || recipe.ingredients == null)
                return result;

            foreach (CraftingIngredient ingredient in recipe.ingredients)
            {
                if (ingredient == null ||
                    ingredient.item == null ||
                    ingredient.amount <= 0)
                {
                    continue;
                }

                IngredientAmount existing = null;

                foreach (IngredientAmount entry in result)
                {
                    if (IsSameItem(entry.item, ingredient.item))
                    {
                        existing = entry;
                        break;
                    }
                }

                if (existing != null)
                    existing.amount += ingredient.amount;
                else
                    result.Add(new IngredientAmount(ingredient.item, ingredient.amount));
            }

            return result;
        }

        private bool HasAllIngredients(CraftingRecipeData recipe)
        {
            if (recipe == null || playerInventory == null)
                return false;

            List<IngredientAmount> ingredients = GetAggregatedIngredients(recipe);

            if (ingredients.Count == 0)
                return false;

            foreach (IngredientAmount ingredient in ingredients)
            {
                if (!playerInventory.HasItem(ingredient.item, ingredient.amount))
                    return false;
            }

            return true;
        }

        private void UpdateResult()
        {
            currentRecipe = FindMatchingRecipe();
            ClearResultDisplay();

            CraftingRecipeData recipeToDisplay =
                viewedRecipe != null ? viewedRecipe : currentRecipe;

            if (recipeToDisplay != null)
                DisplayRecipe(recipeToDisplay);

            if (selectedMaterials.Count == 0)
            {
                if (viewedRecipe != null)
                {
                    SetStatus(
                        "Hãy tự chọn nguyên liệu đúng theo công thức",
                        warningColor
                    );
                }
                else
                {
                    SetStatus("Hãy chọn nguyên liệu", warningColor);
                }

                SetCraftButton(false);
                return;
            }

            if (currentRecipe == null)
            {
                SetStatus(
                    viewedRecipe != null
                        ? "Nguyên liệu đã chọn chưa đúng công thức đang xem"
                        : "Không tìm thấy công thức",
                    errorColor
                );
                SetCraftButton(false);
                return;
            }

            if (viewedRecipe != null && currentRecipe != viewedRecipe)
            {
                SetStatus(
                    "Nguyên liệu đã chọn không khớp công thức đang xem",
                    errorColor
                );
                SetCraftButton(false);
                return;
            }

            if (!HasAllIngredients(currentRecipe))
            {
                SetStatus("Không đủ nguyên liệu", errorColor);
                SetCraftButton(false);
                return;
            }

            SetStatus("Có thể chế tạo", successColor);
            SetCraftButton(true);
        }

        private void DisplayRecipe(CraftingRecipeData recipe)
        {
            if (recipe == null || recipe.resultItem == null)
                return;

            if (resultIcon != null)
            {
                resultIcon.sprite = recipe.resultItem.image;
                resultIcon.gameObject.SetActive(resultIcon.sprite != null);
                resultIcon.preserveAspect = true;
            }

            if (resultAmountText != null)
            {
                resultAmountText.text = "x" + Mathf.Max(1, recipe.resultAmount);
                resultAmountText.gameObject.SetActive(true);
            }

            if (resultNameText != null)
                resultNameText.text = recipe.resultItem.name;

            DisplayRecipeRequirements(recipe);
        }

        private void DisplayRecipeRequirements(CraftingRecipeData recipe)
        {
            if (recipeRequirementsContent == null ||
                recipeRequirementPrefab == null)
            {
                return;
            }

            List<IngredientAmount> ingredients =
                GetAggregatedIngredients(recipe);

            foreach (IngredientAmount ingredient in ingredients)
            {
                int ownedAmount = playerInventory != null
                    ? playerInventory.GetItemAmount(ingredient.item)
                    : 0;

                CraftingRecipeIngredientUI newIngredient = Instantiate(
                    recipeRequirementPrefab,
                    recipeRequirementsContent
                );

                newIngredient.gameObject.SetActive(true);
                newIngredient.Setup(
                    ingredient.item,
                    ingredient.amount,
                    ownedAmount,
                    true
                );
            }
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

            ClearRecipeRequirementObjects();
        }

        private void ClearRecipeRequirementObjects()
        {
            if (recipeRequirementsContent == null)
                return;

            foreach (Transform child in recipeRequirementsContent)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        private void SetStatus(string message, Color color)
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

            if (currentRecipe == null ||
                (viewedRecipe != null && currentRecipe != viewedRecipe) ||
                !HasAllIngredients(currentRecipe))
            {
                UpdateResult();
                return;
            }

            List<IngredientAmount> ingredients =
                GetAggregatedIngredients(currentRecipe);

            foreach (IngredientAmount ingredient in ingredients)
            {
                bool removed = playerInventory.TryRemoveItem(
                    ingredient.item,
                    ingredient.amount
                );

                if (!removed)
                {
                    Debug.LogError(
                        "Không thể trừ nguyên liệu: " + ingredient.item.name
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

            if (recipeListPanel != null && recipeListPanel.activeSelf)
                RefreshRecipeList();

            UpdateResult();
        }
    }
}