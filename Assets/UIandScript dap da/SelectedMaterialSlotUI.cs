using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kinnly
{
    public class SelectedMaterialSlotUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image selectedIcon;
        [SerializeField] private TMP_Text selectedAmountText;
        [SerializeField] private Button removeButton;

        private Item currentItem;
        private Action<Item> onRemove;

        public void Setup(
            Item item,
            int amount,
            Action<Item> removeCallback)
        {
            currentItem = item;
            onRemove = removeCallback;

            if (selectedIcon != null)
            {
                selectedIcon.gameObject.SetActive(true);
                selectedIcon.sprite =
                    item != null ? item.image : null;
                selectedIcon.preserveAspect = true;
            }

            if (selectedAmountText != null)
            {
                selectedAmountText.gameObject.SetActive(true);
                selectedAmountText.text = "x" + amount;
            }

            if (removeButton != null)
            {
                removeButton.gameObject.SetActive(true);
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(RemoveItem);
            }
        }

        public void Clear()
        {
            currentItem = null;
            onRemove = null;

            if (selectedIcon != null)
            {
                selectedIcon.sprite = null;
                selectedIcon.gameObject.SetActive(false);
            }

            if (selectedAmountText != null)
            {
                selectedAmountText.text = "";
                selectedAmountText.gameObject.SetActive(false);
            }

            if (removeButton != null)
            {
                removeButton.onClick.RemoveAllListeners();
                removeButton.gameObject.SetActive(false);
            }
        }

        private void RemoveItem()
        {
            if (currentItem != null)
                onRemove?.Invoke(currentItem);
        }
    }
}