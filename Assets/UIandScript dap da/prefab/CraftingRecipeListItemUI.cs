using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kinnly
{
    /// <summary>
    /// Một hàng trong danh sách công thức: kết quả, nguyên liệu và nút chọn.
    /// </summary>
    public class CraftingRecipeListItemUI : MonoBehaviour
    {
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

        [Header("Result")]
        [SerializeField] private Image resultIcon;
        [SerializeField] private TMP_Text resultNameText;
        [SerializeField] private TMP_Text resultAmountText;

        [Header("Ingredients")]
        [SerializeField] private Transform ingredientsContent;
        [SerializeField] private CraftingRecipeIngredientUI ingredientPrefab;

        [Header("Selection")]
        [SerializeField] private Button selectButton;
        [SerializeField] private TMP_Text availabilityText;

        [Header("Status Colors")]
        [SerializeField] private Color availableColor = new Color(0.3f, 1f, 0.45f);
        [SerializeField] private Color unavailableColor = new Color(1f, 0.45f, 0.45f);

        private CraftingRecipeData boundRecipe;
        private Action<CraftingRecipeData> onSelected;

        private void Awake()
        {
            if (selectButton == null)
                selectButton = GetComponent<Button>();
        }

        public void Setup(
            CraftingRecipeData recipe,
            PlayerInventory playerInventory,
            Action<CraftingRecipeData> selectedCallback)
        {
            boundRecipe = recipe;
            onSelected = selectedCallback;

            if (resultIcon != null)
            {
                resultIcon.sprite = recipe != null && recipe.resultItem != null
                    ? recipe.resultItem.image
                    : null;
                resultIcon.enabled = resultIcon.sprite != null;
                resultIcon.preserveAspect = true;
            }

            if (resultNameText != null)
            {
                resultNameText.text = recipe != null && recipe.resultItem != null
                    ? recipe.resultItem.name
                    : "Công thức không hợp lệ";
            }

            if (resultAmountText != null)
            {
                resultAmountText.text = recipe != null
                    ? "x" + Mathf.Max(1, recipe.resultAmount)
                    : "";
            }

            ClearIngredientObjects();

            bool canCraft = recipe != null &&
                            recipe.resultItem != null &&
                            playerInventory != null;

            if (recipe != null && recipe.ingredients != null)
            {
                List<IngredientAmount> ingredients = AggregateIngredients(recipe);

                foreach (IngredientAmount ingredient in ingredients)
                {
                    int ownedAmount = playerInventory != null
                        ? playerInventory.GetItemAmount(ingredient.item)
                        : 0;

                    if (ownedAmount < ingredient.amount)
                        canCraft = false;

                    if (ingredientsContent != null && ingredientPrefab != null)
                    {
                        CraftingRecipeIngredientUI newIngredient = Instantiate(
                            ingredientPrefab,
                            ingredientsContent
                        );

                        newIngredient.gameObject.SetActive(true);
                        newIngredient.Setup(
                            ingredient.item,
                            ingredient.amount,
                            ownedAmount
                        );
                    }
                }

                if (ingredients.Count == 0)
                    canCraft = false;
            }
            else
            {
                canCraft = false;
            }

            if (availabilityText != null)
            {
                availabilityText.text = canCraft
                    ? "ĐỦ NGUYÊN LIỆU"
                    : "THIẾU NGUYÊN LIỆU";
                availabilityText.color = canCraft
                    ? availableColor
                    : unavailableColor;
            }

            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(SelectThisRecipe);
                selectButton.onClick.AddListener(SelectThisRecipe);
                selectButton.interactable = recipe != null && recipe.resultItem != null;
            }
        }

        private List<IngredientAmount> AggregateIngredients(
            CraftingRecipeData recipe)
        {
            List<IngredientAmount> result = new List<IngredientAmount>();

            foreach (CraftingIngredient ingredient in recipe.ingredients)
            {
                if (ingredient == null || ingredient.item == null || ingredient.amount <= 0)
                    continue;

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

        private void ClearIngredientObjects()
        {
            if (ingredientsContent == null)
                return;

            foreach (Transform child in ingredientsContent)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        private void SelectThisRecipe()
        {
            if (boundRecipe != null)
                onSelected?.Invoke(boundRecipe);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(SelectThisRecipe);
        }
    }
}