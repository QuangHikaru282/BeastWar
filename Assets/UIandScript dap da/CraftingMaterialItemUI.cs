using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kinnly
{
    public class CraftingMaterialItemUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private GameObject selectedFrame;
        [SerializeField] private Button selectButton;

        public Item BoundItem { get; private set; }

        private Action<Item> onSelected;

        private void Awake()
        {
            if (selectButton == null)
                selectButton = GetComponent<Button>();
        }

        public void Setup(
            Item item,
            int amount,
            Action<Item> selectedCallback)
        {
            BoundItem = item;
            onSelected = selectedCallback;

            if (itemIcon != null)
            {
                itemIcon.sprite = item != null ? item.image : null;
                itemIcon.enabled =
                    item != null && item.image != null;
            }

            if (amountText != null)
                amountText.text = amount.ToString();

            if (itemNameText != null)
            {
                itemNameText.text =
                    item != null ? item.name : "";
            }

            if (selectedFrame != null)
                selectedFrame.SetActive(false);

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(SelectItem);
            }
        }

        private void SelectItem()
        {
            if (BoundItem != null)
                onSelected?.Invoke(BoundItem);
        }

        public void SetSelected(bool selected)
        {
            if (selectedFrame != null)
                selectedFrame.SetActive(selected);
        }
    }
}