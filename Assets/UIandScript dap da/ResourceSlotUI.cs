using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kinnly
{
    public class ResourceSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text itemNameText;

        public void Setup(
            Item item,
            int amount,
            bool showAmount,
            ResourceRarity rarity
        )
        {
            if (item == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            iconImage.sprite = item.image;
            iconImage.enabled = item.image != null;
            iconImage.preserveAspect = true;

            if (itemNameText != null)
            {
                itemNameText.text = item.name;
            }

            if (amountText != null)
            {
                amountText.gameObject.SetActive(showAmount);

                if (showAmount)
                {
                    amountText.text = "x" + amount;
                }
            }

            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(rarity);
            }
        }

        private Color GetRarityColor(ResourceRarity rarity)
        {
            switch (rarity)
            {
                case ResourceRarity.Rare:
                    return new Color(0.1f, 0.65f, 1f);

                case ResourceRarity.Epic:
                    return new Color(0.7f, 0.25f, 1f);

                case ResourceRarity.Legendary:
                    return new Color(1f, 0.55f, 0.05f);

                default:
                    return new Color(0.35f, 1f, 0.25f);
            }
        }
    }
}