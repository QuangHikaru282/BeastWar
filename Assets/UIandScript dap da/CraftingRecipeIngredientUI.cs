using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kinnly
{
    /// <summary>
    /// Hiển thị một nguyên liệu trong một hàng công thức.
    /// </summary>
    public class CraftingRecipeIngredientUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text amountText;

        [Header("Colors")]
        [SerializeField] private Color enoughColor = Color.white;
        [SerializeField] private Color missingColor = new Color(1f, 0.45f, 0.45f);

        public void Setup(Item item, int requiredAmount, int ownedAmount)
        {
            Setup(item, requiredAmount, ownedAmount, false);
        }

        public void Setup(
            Item item,
            int requiredAmount,
            int ownedAmount,
            bool showOwnedAndRequired)
        {
            bool hasEnough = item != null && ownedAmount >= requiredAmount;

            if (itemIcon != null)
            {
                itemIcon.sprite = item != null ? item.image : null;
                itemIcon.enabled = item != null && item.image != null;
                itemIcon.preserveAspect = true;
                itemIcon.color = hasEnough ? enoughColor : missingColor;
            }

            if (itemNameText != null)
            {
                itemNameText.text = item != null ? item.name : "Vật phẩm lỗi";
                itemNameText.color = hasEnough ? enoughColor : missingColor;
            }

            if (amountText != null)
            {
                amountText.text = showOwnedAndRequired
                    ? ownedAmount + "/" + requiredAmount
                    : "x" + requiredAmount;
                amountText.color = hasEnough ? enoughColor : missingColor;
            }
        }
    }
}