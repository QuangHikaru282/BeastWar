using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Hiển thị hàng Huy hiệu Gym chuẩn Pokemon.
/// Xếp sẵn danh sách Huy hiệu theo hàng ngang.
/// Khi chưa sở hữu -> Tối đen (Silhouette / Locked).
/// Khi đã sở hữu -> Sáng rực rỡ (Unlocked).
/// </summary>
public class GymBadgeDisplayUI : MonoBehaviour
{
    [Header("Dữ liệu người chơi")]
    [SerializeField] private PlayerData playerData;

    [Header("Màu sắc trạng thái")]
    [Tooltip("Màu khi đã mở khóa huy hiệu (Thường là trắng nguyên bản Color.white)")]
    [SerializeField] private Color unlockedColor = Color.white;

    [Tooltip("Màu tối đen khi chưa nhận được huy hiệu (Ví dụ: màu đen mờ Color.black với alpha 0.6)")]
    [SerializeField] private Color lockedColor = new Color(0.15f, 0.15f, 0.15f, 0.7f);

    [Header("Danh sách Huy Hiệu hiển thị theo hàng")]
    [Tooltip("Kéo thả danh sách các Slot Huy hiệu (Image UI) được xếp hàng ngang trên Inspector vào đây")]
    [SerializeField] private List<BadgeSlot> badgeSlots = new List<BadgeSlot>();

    [System.Serializable]
    public class BadgeSlot
    {
        [Tooltip("ID của Huy hiệu Gym này (Ví dụ: Badge_1, Badge_2...)")]
        public string badgeId;

        [Tooltip("UI Image của Huy hiệu này")]
        public Image badgeImage;

        [Tooltip("Sprite gốc của Huy hiệu (nếu muốn tự động gán)")]
        public Sprite badgeSprite;
    }

    private void Awake()
    {
        if (playerData == null)
        {
            playerData = Resources.Load<PlayerData>("PlayerData");
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        // Tự động cập nhật tức thì trạng thái huy hiệu
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (playerData == null) return;

        List<string> ownedBadges = playerData.gymBadges ?? new List<string>();

        foreach (var slot in badgeSlots)
        {
            if (slot == null || slot.badgeImage == null) continue;

            // Gán sprite nếu có thiết lập
            if (slot.badgeSprite != null)
            {
                slot.badgeImage.sprite = slot.badgeSprite;
            }

            // Kiểm tra xem người chơi đã có huy hiệu này chưa
            bool isUnlocked = ownedBadges.Contains(slot.badgeId);

            if (isUnlocked)
            {
                // Đã sở hữu -> Sáng nguyên bản
                slot.badgeImage.color = unlockedColor;
            }
            else
            {
                // Chưa sở hữu -> Tối đen
                slot.badgeImage.color = lockedColor;
            }
        }
    }
}
