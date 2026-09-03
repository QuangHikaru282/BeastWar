using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Đại diện cho 1 dòng/ô trong danh sách BeastDex (Pokédex Slot).
/// </summary>
public class BeastDexSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button slotButton;
    [SerializeField] private Image beastIcon;
    [SerializeField] private Image caughtCheckIcon;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image selectionHighlight;

    [Header("Màu sắc / Silhouette")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color silhouetteColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color unknownColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);

    private BeastData currentBeastData;
    private Action<BeastData> onSelectCallback;

    public void Setup(BeastData data, bool isSeen, bool isCaught, Action<BeastData> onSelect)
    {
        currentBeastData = data;
        onSelectCallback = onSelect;

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => onSelectCallback?.Invoke(currentBeastData));
        }

        if (numberText != null && data != null)
        {
            numberText.text = $"#{data.pokedexNumber:D3}";
        }

        if (caughtCheckIcon != null)
        {
            caughtCheckIcon.gameObject.SetActive(isCaught);
        }

        if (isCaught)
        {
            // Đã bắt: hiện tên + ảnh đầy đủ
            if (nameText != null) nameText.text = data.beastName;
            if (beastIcon != null)
            {
                beastIcon.sprite = data.frontSprite;
                beastIcon.color = normalColor;
                beastIcon.gameObject.SetActive(true);
            }
        }
        else if (isSeen)
        {
            // Đã thấy: hiện tên + bóng đen / silhouette
            if (nameText != null) nameText.text = data.beastName;
            if (beastIcon != null)
            {
                beastIcon.sprite = data.frontSprite;
                beastIcon.color = silhouetteColor;
                beastIcon.gameObject.SetActive(true);
            }
        }
        else
        {
            // Chưa gặp: hiện ???
            if (nameText != null) nameText.text = "???";
            if (beastIcon != null)
            {
                beastIcon.sprite = null;
                beastIcon.color = unknownColor;
                beastIcon.gameObject.SetActive(false);
            }
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.gameObject.SetActive(isSelected);
        }
    }
}
