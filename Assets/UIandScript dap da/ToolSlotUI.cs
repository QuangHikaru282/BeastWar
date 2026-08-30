using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kinnly
{
    public class ToolSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image selectionBorder;
        [SerializeField] private TMP_Text toolNameText;
        [SerializeField] private Button button;

        private Item currentTool;
        private Action<Item> selectAction;

        public void Setup(
            Item tool,
            Action<Item> onSelected
        )
        {
            currentTool = tool;
            selectAction = onSelected;

            if (currentTool == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (iconImage != null)
            {
                iconImage.sprite = currentTool.image;
                iconImage.enabled = currentTool.image != null;
                iconImage.preserveAspect = true;
            }

            if (toolNameText != null)
            {
                toolNameText.text = currentTool.name;
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(SelectThisTool);
            }

            SetSelected(false);
        }

        private void SelectThisTool()
        {
            selectAction?.Invoke(currentTool);
        }

        public void SetSelected(bool selected)
        {
            if (selectionBorder != null)
            {
                selectionBorder.gameObject.SetActive(selected);
            }
        }

        public Item GetTool()
        {
            return currentTool;
        }
    }
}