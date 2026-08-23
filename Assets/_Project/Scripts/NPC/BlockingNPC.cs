using UnityEngine;
using Kinnly;

/// <summary>
/// NPC chặn đường theo cơ chế 3 giai đoạn kiểu Pokémon:
///   GĐ 1: NPC đứng chắn, đòi một món đồ từ Shop.
///   GĐ 2: Người chơi mua đồ, quay lại đưa cho NPC.
///   GĐ 3: NPC nhận đồ → khen thưởng + dạy bắt thú → tự ẩn đi để mở đường.
/// 
/// Cách dùng:
///   1. Kéo script này lên GameObject NPC (phải có Collider2D Is Trigger = true).
///   2. Điền các trường trong Inspector.
///   3. Kéo Collider2D "chặn đường vật lý" (không phải trigger) vào ô blockingCollider.
/// </summary>
public class BlockingNPC : MonoBehaviour, IInteractable
{
    // ─── Inspector Fields ───────────────────────────────────────────
    [Header("Thông tin NPC")]
    [SerializeField] private string npcName = "Ông Lão";
    [SerializeField] private Sprite npcAvatar;

    [Header("Vật phẩm cần đem lại")]
    [Tooltip("Kéo Item ScriptableObject mà người chơi phải mang lại (ví dụ: Cà Phê)")]
    [SerializeField] private Item requiredItem;

    [Tooltip("Số lượng Item cần mang lại")]
    [SerializeField] private int requiredAmount = 1;

    [Header("Phần thưởng khi hoàn thành")]
    [Tooltip("Item tặng khi hoàn thành (thường là Bóng Thu Phục)")]
    [SerializeField] private Item rewardItem;

    [Tooltip("Số lượng phần thưởng")]
    [SerializeField] private int rewardAmount = 5;

    [Header("Collider chặn đường vật lý")]
    [Tooltip("Kéo BoxCollider2D (isTrigger=false) của NPC vào đây — collider này sẽ bị tắt khi mở đường")]
    [SerializeField] private Collider2D blockingCollider;

    [Header("Quest ID liên kết (tuỳ chọn)")]
    [Tooltip("Quest ID để mở khóa sau khi NPC nhận đồ. 0 = không dùng.")]
    [SerializeField] private int questIdToUnlockAfter = 0;

    // ─── Thoại theo từng giai đoạn ─────────────────────────────────
    [Header("Thoại Giai Đoạn 1 — Chặn đường, đòi đồ")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialoguePhase1 = new string[]
    {
        "Này... ta đứng đây mà mắt mờ, chân yếu quá. Cháu có thể mua cho ta một tách Cà Phê từ Cửa Hàng trong làng không?",
        "Mang Cà Phê lại cho ta, ta sẽ chỉ cháu cách bắt những chú Thú hoang dã đấy nhé!"
    };

    [Header("Thoại Giai Đoạn 2 — Chưa mang đủ đồ")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialoguePhase2 = new string[]
    {
        "Ồ... cháu chưa mang Cà Phê lại cho ta à? Cửa Hàng ngay trong làng đó, đừng quên nhé!"
    };

    [Header("Thoại Giai Đoạn 3 — Nhận đồ, dạy bắt thú, mở đường")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialoguePhase3 = new string[]
    {
        "Ồ! Cháu tốt bụng quá! Đây đúng là thứ ta cần rồi. Uống xong tỉnh hẳn người!",
        "Ta sẽ dạy cháu bí quyết bắt Thú:\n★ Máu quái càng thấp → Tỉ lệ bắt càng cao!\n★ Dùng Kỹ Năng làm yếu quái trước khi ném Bóng.\n★ Cứ thử vài lần — Bóng có thể thất bại, đừng nản!",
        "Bây giờ ta tặng cháu {0} Bóng Thu Phục để bắt đầu hành trình nhé!\nChúc cháu may mắn! Đường phía trước đã rộng mở rồi đó!"
    };

    // ─── Private State ───────────────────────────────────────────────
    private bool hasCompleted = false;
    private const string SAVE_KEY_PREFIX = "BlockingNPC_Done_";
    private string SaveKey => SAVE_KEY_PREFIX + gameObject.name + "_" + gameObject.GetInstanceID();

    // ─── Unity Lifecycle ─────────────────────────────────────────────
    private void Start()
    {
        // Tải trạng thái đã hoàn thành từ lần chơi trước
        hasCompleted = PlayerPrefs.GetInt(SaveKey, 0) == 1;

        if (hasCompleted)
        {
            // Đã hoàn thành trước đó → ẩn NPC và mở đường ngay
            OpenPath();
        }
    }

    // ─── IInteractable ───────────────────────────────────────────────
    public void Interact(PlayerInventory playerInventory)
    {
        if (hasCompleted)
        {
            // NPC đã nhường đường, nói vài câu bình thường
            ShowDialogue(new string[]
            {
                "Haha, cháu vẫn ổn chứ? Hãy cẩn thận khi đi sâu vào hoang dã nhé, đặc biệt là nhớ làm yếu Thú trước khi ném Bóng!"
            }, null);
            return;
        }

        bool playerHasItem = CheckPlayerHasItem(playerInventory);

        if (!playerHasItem)
        {
            // Giai đoạn 1 & 2: Chưa có đồ
            ShowDialogue(dialoguePhase1, null);
        }
        else
        {
            // Giai đoạn 3: Có đồ → thu đồ, tặng thưởng, mở đường
            ShowDialogue(BuildPhase3Dialogue(), () =>
            {
                CompleteQuest(playerInventory);
            });
        }
    }

    // ─── Private Helpers ─────────────────────────────────────────────

    private bool CheckPlayerHasItem(PlayerInventory playerInventory)
    {
        if (requiredItem == null || playerInventory == null) return false;

        int count = CountItemInInventory(playerInventory, requiredItem);
        return count >= requiredAmount;
    }

    /// <summary>
    /// Đếm số lượng một Item trong toàn bộ Inventory + Toolbar của người chơi.
    /// </summary>
    private int CountItemInInventory(PlayerInventory playerInventory, Item targetItem)
    {
        if (playerInventory == null || targetItem == null) return 0;

        int total = 0;
        var allSlots = new System.Collections.Generic.List<GameObject>();
        if (playerInventory.InventorySlots != null) allSlots.AddRange(playerInventory.InventorySlots);
        if (playerInventory.ToolbarSlots != null) allSlots.AddRange(playerInventory.ToolbarSlots);

        foreach (var slot in allSlots)
        {
            if (slot == null) continue;
            var invItem = slot.GetComponentInChildren<InventoryItem>(true);
            if (invItem != null && invItem.Item != null)
            {
                if (invItem.Item == targetItem ||
                    invItem.Item.name.Equals(targetItem.name, System.StringComparison.OrdinalIgnoreCase))
                {
                    total += invItem.Amount;
                }
            }
        }
        return total;
    }

    /// <summary>
    /// Thu đồ, tặng thưởng, tăng Quest ID, ẩn NPC mở đường.
    /// </summary>
    private void CompleteQuest(PlayerInventory playerInventory)
    {
        // 1. Thu item từ người chơi
        if (requiredItem != null && playerInventory != null)
        {
            int remaining = requiredAmount;
            var allSlots = new System.Collections.Generic.List<GameObject>();
            if (playerInventory.InventorySlots != null) allSlots.AddRange(playerInventory.InventorySlots);
            if (playerInventory.ToolbarSlots != null) allSlots.AddRange(playerInventory.ToolbarSlots);

            foreach (var slot in allSlots)
            {
                if (remaining <= 0) break;
                if (slot == null) continue;
                var invItem = slot.GetComponentInChildren<InventoryItem>(true);
                if (invItem != null && invItem.Item != null &&
                    (invItem.Item == requiredItem ||
                     invItem.Item.name.Equals(requiredItem.name, System.StringComparison.OrdinalIgnoreCase)))
                {
                    int toRemove = Mathf.Min(remaining, invItem.Amount);
                    playerInventory.RemoveItem(invItem, toRemove);
                    remaining -= toRemove;
                }
            }
        }

        // 2. Tặng Item phần thưởng (Bóng Thu Phục)
        if (rewardItem != null && playerInventory != null)
        {
            playerInventory.AddItem(rewardItem, rewardAmount);
            Debug.Log($"<color=green>[BlockingNPC]</color> Đã tặng {rewardAmount}x {rewardItem.name} cho người chơi.");
        }

        // 3. Tăng Quest ID nếu có cấu hình
        if (questIdToUnlockAfter > 0 && QuestManager.Instance != null && QuestManager.Instance.playerData != null)
        {
            if (QuestManager.Instance.playerData.currentMainQuestId < questIdToUnlockAfter)
            {
                QuestManager.Instance.playerData.currentMainQuestId = questIdToUnlockAfter;
                QuestManager.Instance.playerData.Save();
                Debug.Log($"<color=cyan>[BlockingNPC]</color> Quest tiến đến ID: {questIdToUnlockAfter}");
            }
        }

        // 4. Đánh dấu đã hoàn thành và lưu
        hasCompleted = true;
        PlayerPrefs.SetInt(SaveKey, 1);
        PlayerPrefs.Save();

        // 5. Mở đường
        OpenPath();
        Debug.Log($"<color=yellow>[BlockingNPC]</color> {npcName} đã nhường đường!");
    }

    /// <summary>
    /// Tắt Collider vật lý và ẩn NPC để mở đường cho người chơi đi qua.
    /// </summary>
    private void OpenPath()
    {
        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }

        // Giữ NPC visible nhưng cho phép đi xuyên qua
        // (hoặc comment dòng dưới nếu muốn NPC tự đi sang một bên)
        // gameObject.SetActive(false);
    }

    private string[] BuildPhase3Dialogue()
    {
        var lines = new string[dialoguePhase3.Length];
        for (int i = 0; i < dialoguePhase3.Length; i++)
        {
            lines[i] = string.Format(dialoguePhase3[i], rewardAmount);
        }
        return lines;
    }

    private void ShowDialogue(string[] lines, System.Action onFinished)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(npcName, lines, onFinished, npcAvatar);
        }
        else
        {
            Debug.LogWarning($"[BlockingNPC] Không tìm thấy DialogueManager! Nội dung: {string.Join(" | ", lines)}");
            onFinished?.Invoke();
        }
    }

    // ─── Gizmo để dễ nhìn trong Scene View ──────────────────────────
    private void OnDrawGizmos()
    {
        Gizmos.color = hasCompleted ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
