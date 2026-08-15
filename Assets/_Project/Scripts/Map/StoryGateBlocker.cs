using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script đa năng dùng để chặn đường / cổng / NPC chặn đường (Snorlax, Cảnh sát, Cổng Gym, Pokémon Tower, Victory Road...).
/// Hỗ trợ kiểm tra:
/// 1. Đủ Quest ID
/// 2. Đã có Huy hiệu Gym cụ thể (Ví dụ: EarthBadge, WindBadge...)
/// 3. Đủ số lượng Huy hiệu (Ví dụ: 6 Huy hiệu để mở Victory Road)
/// 4. Đã có Item trong túi (Ví dụ: Silph Scope, Pokéflute...)
/// </summary>
public class StoryGateBlocker : MonoBehaviour
{
    [Header("Dữ liệu Người chơi")]
    [SerializeField] private PlayerData playerData;

    public enum UnlockConditionType
    {
        QuestId,            // Cần đạt mốc Quest ID
        SpecificBadge,      // Cần có 1 Huy hiệu cụ thể
        TotalBadgeCount,    // Cần có đủ N Huy hiệu
        HasDefeatedTrainer, // Cần đánh bại 1 Trainer/Boss cụ thể
        AlwaysOpen          // Luôn mở
    }

    [Header("Điều Kiện Mở Khóa")]
    [Tooltip("Loại điều kiện để mở cổng này")]
    public UnlockConditionType conditionType = UnlockConditionType.QuestId;

    [Tooltip("Mốc Quest ID tối thiểu (nếu chọn QuestId)")]
    public int requiredQuestId = 1;

    [Tooltip("Tên Huy hiệu cần có (nếu chọn SpecificBadge)")]
    public string requiredBadgeId = "EarthBadge";

    [Tooltip("Số lượng Huy hiệu tối thiểu (nếu chọn TotalBadgeCount, ví dụ 6 để vào Victory Road)")]
    public int requiredBadgeCount = 6;

    [Tooltip("ID Trainer/Boss cần đánh bại trước (nếu chọn HasDefeatedTrainer)")]
    public string requiredDefeatedTrainerId = "Giovanni_1";

    [Header("Hành Động Khi Đã Mở Khóa")]
    [Tooltip("Ẩn GameObject này đi khi đã đủ điều kiện (ví dụ: Snorlax thức giấc bỏ đi, Cảnh sát né đường)")]
    public bool hideGameObjectWhenUnlocked = true;

    [Header("Cấu Hình Thoại Khi Bị Chặn")]
    [SerializeField] private Sprite blockerAvatar;
    [TextArea(2, 4)]
    [SerializeField] private string lockedDialogue = "Khu vực này hiện chưa thể đi qua!";

    private void Start()
    {
        CheckAndApplyState();
    }

    private void OnEnable()
    {
        CheckAndApplyState();
    }

    public bool IsUnlocked()
    {
        PlayerData pData = playerData != null ? playerData : (global::QuestManager.Instance != null ? global::QuestManager.Instance.playerData : null);
        if (pData == null) return true;

        switch (conditionType)
        {
            case UnlockConditionType.QuestId:
                return pData.currentMainQuestId >= requiredQuestId;

            case UnlockConditionType.SpecificBadge:
                return pData.gymBadges != null && pData.gymBadges.Contains(requiredBadgeId);

            case UnlockConditionType.TotalBadgeCount:
                return pData.gymBadges != null && pData.gymBadges.Count >= requiredBadgeCount;

            case UnlockConditionType.HasDefeatedTrainer:
                return pData.defeatedTrainers != null && pData.defeatedTrainers.Contains(requiredDefeatedTrainerId);

            case UnlockConditionType.AlwaysOpen:
            default:
                return true;
        }
    }

    public void CheckAndApplyState()
    {
        if (IsUnlocked())
        {
            if (hideGameObjectWhenUnlocked)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (!IsUnlocked())
        {
            // Dừng Player
            PlayerMapController ctrl = collision.GetComponent<PlayerMapController>();
            if (ctrl != null)
            {
                // Đẩy nhẹ Player lùi lại một chút
                Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            // Hiện thoại nhắc nhở
            if (DialogueManager.Instance != null && !string.IsNullOrEmpty(lockedDialogue))
            {
                DialogueManager.Instance.StartDialogue(
                    "Thông Báo",
                    lockedDialogue,
                    null,
                    blockerAvatar
                );
            }
        }
    }
}
