using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BadgeInfo
{
    [Tooltip("ID khớp với danh sách playerData.gymBadges (VD: Badge_Earth, Badge_Water, Badge_1...)")]
    public string badgeId = "Badge_1";

    [Tooltip("Tên đầy đủ của Huy Hiệu (VD: Huy Hiệu Đất / Boulder Badge)")]
    public string badgeName = "Huy Hiệu Đất";

    [Tooltip("Tên Gym Leader đã trao (VD: Ignar, Spectra...)")]
    public string gymLeaderName = "Gym Leader Ignar";

    [Tooltip("Sprite huy hiệu màu sắc rực rỡ")]
    public Sprite badgeSprite;

    [Tooltip("Quyền lợi / Buff của huy hiệu")]
    [TextArea(2, 4)]
    public string buffDescription = "Cho phép điều khiển Beast đến cấp 25 vâng lời. Mở khóa khả năng chặt cây trên map.";
}

/// <summary>
/// Đại diện cho 1 ô Huy Hiệu trên Khay Nhung của Thẻ Trainer.
/// </summary>
public class BadgeItemSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button slotButton;
    [SerializeField] private Image badgeImage;
    [SerializeField] private Image emptySlotOutline;
    [SerializeField] private Image highlightBorder;

    [Header("Màu sắc")]
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);

    private BadgeInfo currentInfo;
    private bool isOwned;
    private Action<BadgeInfo, bool> onClickCallback;

    public void Setup(BadgeInfo info, bool owned, Action<BadgeInfo, bool> onClick)
    {
        currentInfo = info;
        isOwned = owned;
        onClickCallback = onClick;

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => onClickCallback?.Invoke(currentInfo, isOwned));
        }

        if (badgeImage != null)
        {
            if (info != null && info.badgeSprite != null)
            {
                badgeImage.sprite = info.badgeSprite;
                badgeImage.gameObject.SetActive(true);
            }
            else
            {
                badgeImage.gameObject.SetActive(false);
            }

            badgeImage.color = isOwned ? unlockedColor : lockedColor;
        }

        if (emptySlotOutline != null)
        {
            emptySlotOutline.gameObject.SetActive(!isOwned);
        }

        SetHighlight(false);
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (highlightBorder != null)
        {
            highlightBorder.gameObject.SetActive(isHighlighted);
        }
    }
}
