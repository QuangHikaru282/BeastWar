using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Component gắn trên Prefab Nút bấm của Menu Tab (Menu Action Button Prefab).
/// Dễ dàng nhân bản và tái sử dụng cho bất kỳ tính năng mới nào trong tương lai.
/// </summary>
public class MenuActionButtonUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusSubText;
    [SerializeField] private Image selectionHighlight;

    private Action onClickCallback;

    public void Setup(string title, Sprite icon, string subText, bool isInteractable, Action onClick)
    {
        onClickCallback = onClick;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        if (statusSubText != null)
        {
            if (!string.IsNullOrEmpty(subText))
            {
                statusSubText.text = subText;
                statusSubText.gameObject.SetActive(true);
            }
            else
            {
                statusSubText.gameObject.SetActive(false);
            }
        }

        if (button != null)
        {
            button.interactable = isInteractable;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke());
        }

        SetHighlighted(false);
    }

    public void UpdateSubText(string subText, bool isInteractable)
    {
        if (statusSubText != null)
        {
            statusSubText.text = subText;
            statusSubText.gameObject.SetActive(!string.IsNullOrEmpty(subText));
        }

        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }

    public void SetHighlighted(bool isHighlighted)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.gameObject.SetActive(isHighlighted);
        }
    }
}
