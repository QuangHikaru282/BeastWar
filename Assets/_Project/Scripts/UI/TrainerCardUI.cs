using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý Giao Diện Thẻ Huấn Luyện Viên (Trainer Card UI) chuẩn Pokémon:
/// 1. Mặt trước hiển thị: Ảnh Avatar, Tên Huấn Luyện Viên, Mã số IDNo., Tiền Vàng, Thời Gian Chơi, Số Lượng Beast Đã Bắt.
/// 2. Phía dưới là Khay Nhung đựng 5 Huy Hiệu Hội Quán (Badges):
///    - Huy hiệu chưa nhận: Hiện bóng mờ / ô trống.
///    - Huy hiệu đã nhận: Sáng rực rỡ lấp lánh.
/// 3. Bấm vào từng huy hiệu sẽ hiển thị chi tiết: Tên Huy Hiệu, Gym Leader trao tặng, và Quyền lợi / Buff.
/// </summary>
public class TrainerCardUI : MonoBehaviour
{
    public static TrainerCardUI Instance { get; private set; }

    [Header("1. Khung Thẻ Trainer")]
    [Tooltip("Panel toàn bộ Thẻ Trainer")]
    [SerializeField] private GameObject cardPanel;

    [Tooltip("Nút đóng Thẻ")]
    [SerializeField] private Button closeButton;

    [Header("2. Dữ Liệu Người Chơi")]
    [SerializeField] private PlayerData playerData;

    [Header("3. Thông Tin Trainer Trên Thẻ")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private Sprite maleAvatarSprite;
    [SerializeField] private Sprite femaleAvatarSprite;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI idNoText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI beastDexCountText;
    [SerializeField] private TextMeshProUGUI playTimeText;

    [Header("4. Cấu Hình 5 Huy Hiệu Hội Quán")]
    [Tooltip("Danh sách cấu hình 5 Huy Hiệu (ID, Tên, Leader, Sprite, Buff)")]
    [SerializeField] private List<BadgeInfo> badgeList = new List<BadgeInfo>();

    [Header("5. Khay Nhung Chứa Huy Hiệu (Container)")]
    [Tooltip("Transform chứa 5 ô Huy Hiệu (Horizontal Layout Group)")]
    [SerializeField] private Transform badgeTrayContainer;

    [Tooltip("Prefab ô Huy Hiệu (Gắn BadgeItemSlotUI)")]
    [SerializeField] private GameObject badgeSlotPrefab;

    [Header("6. Bảng Chi Tiết Khi Click Vào Huy Hiệu (Phía Dưới)")]
    [SerializeField] private GameObject badgeDetailPanel;
    [SerializeField] private TextMeshProUGUI detailBadgeNameText;
    [SerializeField] private TextMeshProUGUI detailGymLeaderText;
    [SerializeField] private TextMeshProUGUI detailBuffText;

    [Header("7. Âm Thanh (Tùy Chọn)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openCardSFX;
    [SerializeField] private AudioClip clickBadgeSFX;

    private List<BadgeItemSlotUI> spawnedBadgeSlots = new List<BadgeItemSlotUI>();
    private float sessionStartTime;

    public bool IsOpen => cardPanel != null && cardPanel.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        sessionStartTime = Time.time;

        if (cardPanel != null) cardPanel.SetActive(false);
        if (badgeDetailPanel != null) badgeDetailPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseCard);
        }
    }

    private void Update()
    {
        if (IsOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)))
        {
            CloseCard();
        }
    }

    public void OpenCard()
    {
        if (cardPanel != null) cardPanel.SetActive(true);

        // Khóa di chuyển nhân vật
        var pmc = UnityEngine.Object.FindFirstObjectByType<PlayerMapController>();
        if (pmc != null) pmc.SetCanMove(false);

        if (audioSource != null && openCardSFX != null)
        {
            audioSource.PlayOneShot(openCardSFX);
        }

        RefreshCardData();
    }

    public void CloseCard()
    {
        if (cardPanel != null) cardPanel.SetActive(false);

        // Mở lại di chuyển nhân vật
        var pmc = UnityEngine.Object.FindFirstObjectByType<PlayerMapController>();
        if (pmc != null) pmc.SetCanMove(true);
    }

    public void RefreshCardData()
    {
        if (playerData == null) return;

        // 1. Cập nhật thông tin nhân vật
        if (playerNameText != null)
        {
            playerNameText.text = $"NAME: <color=#FFE600>{(string.IsNullOrEmpty(playerData.playerName) ? "Trainer" : playerData.playerName)}</color>";
        }

        if (idNoText != null)
        {
            idNoText.text = $"IDNo. {playerData.trainerId:D5}";
        }

        if (moneyText != null)
        {
            moneyText.text = $"MONEY: <color=#00FF66>{playerData.gold:N0}</color> G";
        }

        if (beastDexCountText != null)
        {
            int caughtCount = playerData.ownedBeasts != null ? playerData.ownedBeasts.Count : 0;
            beastDexCountText.text = $"BEASTDEX: {caughtCount}";
        }

        if (playTimeText != null)
        {
            float totalSeconds = Time.time;
            int hours = (int)(totalSeconds / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);
            playTimeText.text = $"TIME: {hours:D2}:{minutes:D2}";
        }

        if (avatarImage != null)
        {
            bool isFemale = playerData.characterGender == "Female";
            avatarImage.sprite = isFemale ? (femaleAvatarSprite != null ? femaleAvatarSprite : avatarImage.sprite)
                                          : (maleAvatarSprite != null ? maleAvatarSprite : avatarImage.sprite);
        }

        // 2. Cập nhật Khay Nhung 5 Huy Hiệu
        PopulateBadgeTray();
    }

    private void PopulateBadgeTray()
    {
        if (badgeTrayContainer == null || badgeSlotPrefab == null) return;

        // Xóa slot cũ
        for (int i = badgeTrayContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(badgeTrayContainer.GetChild(i).gameObject);
        }
        spawnedBadgeSlots.Clear();

        List<string> ownedBadges = playerData.gymBadges ?? new List<string>();

        for (int i = 0; i < badgeList.Count; i++)
        {
            var badgeInfo = badgeList[i];
            if (badgeInfo == null) continue;

            bool isOwned = ownedBadges.Contains(badgeInfo.badgeId);

            GameObject slotObj = Instantiate(badgeSlotPrefab, badgeTrayContainer);
            BadgeItemSlotUI slotUI = slotObj.GetComponent<BadgeItemSlotUI>();

            if (slotUI != null)
            {
                int capturedIndex = i;
                slotUI.Setup(badgeInfo, isOwned, (info, owned) =>
                {
                    OnBadgeClicked(info, owned, capturedIndex);
                });

                spawnedBadgeSlots.Add(slotUI);
            }
        }

        // Mặc định hiển thị chi tiết huy hiệu đầu tiên nếu có
        if (badgeList.Count > 0)
        {
            bool firstOwned = ownedBadges.Contains(badgeList[0].badgeId);
            OnBadgeClicked(badgeList[0], firstOwned, 0);
        }
    }

    private void OnBadgeClicked(BadgeInfo info, bool isOwned, int slotIndex)
    {
        if (audioSource != null && clickBadgeSFX != null)
        {
            audioSource.PlayOneShot(clickBadgeSFX);
        }

        // Highlight slot
        for (int i = 0; i < spawnedBadgeSlots.Count; i++)
        {
            if (spawnedBadgeSlots[i] != null)
            {
                spawnedBadgeSlots[i].SetHighlight(i == slotIndex);
            }
        }

        if (badgeDetailPanel != null) badgeDetailPanel.SetActive(true);

        if (info == null) return;

        if (isOwned)
        {
            if (detailBadgeNameText != null) detailBadgeNameText.text = $"<color=#FFD700>★ {info.badgeName}</color>";
            if (detailGymLeaderText != null) detailGymLeaderText.text = $"Người Trao: <color=#00E5FF>{info.gymLeaderName}</color>";
            if (detailBuffText != null) detailBuffText.text = info.buffDescription;
        }
        else
        {
            if (detailBadgeNameText != null) detailBadgeNameText.text = "<color=#888888>??? (Chưa Mở Khóa)</color>";
            if (detailGymLeaderText != null) detailGymLeaderText.text = "Người Trao: ???";
            if (detailBuffText != null) detailBuffText.text = "Hãy đánh bại Gym Leader tại Hội Quán tương ứng để nhận Huy Hiệu này!";
        }
    }
}
