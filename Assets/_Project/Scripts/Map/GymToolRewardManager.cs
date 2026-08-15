using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script tự động trao Công cụ nông trại / Vật phẩm đặc biệt (Cuốc, Liềm, Bình tưới, Rìu, Giày bay, Silph Scope...)
/// ngay sau khi người chơi đánh thắng Gym Leader hoặc Boss tương ứng.
/// Gắn script này vào 1 GameObject Manager trong GameCore hoặc Scene.
/// </summary>
public class GymToolRewardManager : MonoBehaviour
{
    [System.Serializable]
    public class GymRewardConfig
    {
        [Tooltip("ID của Huy hiệu Gym hoặc Boss (Ví dụ: EarthBadge, GrassBadge, WaterBadge, FightBadge, FireBadge, WindBadge)")]
        public string badgeOrTrainerId;

        [Tooltip("Vật phẩm sẽ trao tặng vào Inventory (Kéo Item asset vào đây)")]
        public Kinnly.Item rewardItem;

        [Tooltip("Số lượng tặng")]
        public int rewardAmount = 1;

        [Tooltip("Tăng tốc độ di chuyển của Player (Dành cho Giày Bay)")]
        public bool isRunningShoesReward = false;

        [Tooltip("Tốc độ cộng thêm cho Player khi nhận Giày Bay")]
        public float speedBonus = 2f;

        [TextArea(2, 3)]
        [Tooltip("Hội thoại thông báo khi nhận thưởng")]
        public string rewardAnnouncement = "Bạn nhận được một công cụ mới!";
    }

    [Header("Dữ liệu Người chơi")]
    [SerializeField] private PlayerData playerData;

    [Header("Danh Sách Phần Thưởng 6 Gym & Boss")]
    public List<GymRewardConfig> gymRewardConfigs = new List<GymRewardConfig>();

    private HashSet<string> rewardedIds = new HashSet<string>();

    private void Start()
    {
        CheckAndGrantRewards();
    }

    private void OnEnable()
    {
        CheckAndGrantRewards();
    }

    /// <summary>
    /// Kiểm tra xem người chơi vừa có Badge / đánh bại Boss mới nào chưa được nhận thưởng.
    /// </summary>
    public void CheckAndGrantRewards()
    {
        PlayerData pData = playerData != null ? playerData : (global::QuestManager.Instance != null ? global::QuestManager.Instance.playerData : null);
        if (pData == null) return;

        foreach (var config in gymRewardConfigs)
        {
            if (string.IsNullOrEmpty(config.badgeOrTrainerId)) continue;
            if (rewardedIds.Contains(config.badgeOrTrainerId)) continue;

            bool hasEarned = false;

            // Kiểm tra trong danh sách Gym Badges
            if (pData.gymBadges != null && pData.gymBadges.Contains(config.badgeOrTrainerId))
            {
                hasEarned = true;
            }
            // Hoặc trong danh sách Trainer đã đánh bại
            else if (pData.defeatedTrainers != null && pData.defeatedTrainers.Contains(config.badgeOrTrainerId))
            {
                hasEarned = true;
            }

            if (hasEarned)
            {
                rewardedIds.Add(config.badgeOrTrainerId);

                // Trao Item vào Inventory nếu có
                if (config.rewardItem != null)
                {
                    Kinnly.PlayerInventory playerInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
                    if (playerInv != null)
                    {
                        playerInv.AddItem(config.rewardItem, config.rewardAmount);
                        Debug.Log($"[GymReward] Đã trao {config.rewardAmount}x {config.rewardItem.name} cho người chơi!");
                    }
                }

                // Tăng tốc độ nếu là Giày Bay
                if (config.isRunningShoesReward)
                {
                    PlayerMapController ctrl = FindFirstObjectByType<PlayerMapController>();
                    if (ctrl != null)
                    {
                        ctrl.MoveSpeed += config.speedBonus;
                        Debug.Log($"[GymReward] Đã nâng cấp tốc độ chạy lên {ctrl.MoveSpeed}!");
                    }
                }
            }
        }
    }
}
