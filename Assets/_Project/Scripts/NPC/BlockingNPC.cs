using System.Collections.Generic;
using UnityEngine;
using Kinnly;

/// <summary>
/// NPC chặn đường theo cơ chế 3 giai đoạn chuẩn Pokémon:
///   GĐ 1: NPC đứng chắn, đòi một món đồ từ Shop (ví dụ: Cà Phê).
///   GĐ 2: Người chơi mua đồ, quay lại đưa cho NPC.
///   GĐ 3: NPC nhận đồ → Khen thưởng, trao Bóng Thu Phục → Kích hoạt trận đấu thị phạm thực chiến để người chơi thực hành ném bóng bắt thú → Ẩn NPC và mở đường vĩnh viễn.
/// </summary>
public class BlockingNPC : MonoBehaviour, IInteractable
{
    // ─── Inspector Fields ───────────────────────────────────────────
    [Header("Thông tin NPC")]
    [SerializeField] private string npcName = "Ông Lão";
    [SerializeField] private Sprite npcAvatar;

    [Header("Mã định danh duy nhất (Save Key)")]
    [Tooltip("ID dùng để lưu trạng thái đã hoàn thành của NPC này")]
    [SerializeField] private string uniqueNpcId = "BlockingNPC_Town";

    [Header("Vật phẩm cần đem lại")]
    [Tooltip("Kéo Item ScriptableObject mà người chơi phải mang lại (ví dụ: Cà Phê)")]
    [SerializeField] private Item requiredItem;

    [Tooltip("Số lượng Item cần mang lại")]
    [SerializeField] private int requiredAmount = 1;

    [Header("Phần thưởng khi hoàn thành")]
    [Tooltip("Item tặng khi hoàn thành (Bóng Thu Phục)")]
    [SerializeField] private Item rewardItem;

    [Tooltip("Số lượng phần thưởng")]
    [SerializeField] private int rewardAmount = 5;

    [Header("Trận Đấu Thị Phạm Bắt Thú")]
    [Tooltip("Tích chọn để tự động mở trận chiến sau khi thoại xong, giúp người chơi thực hành ném bóng bắt thú")]
    [SerializeField] private bool triggerTutorialBattle = true;

    [Tooltip("Kéo BeastData của thú mẫu sẽ xuất hiện trong trận đấu (ví dụ: Rat, Slime...)")]
    [SerializeField] private BeastData tutorialBeast;

    [Tooltip("Level của thú mẫu (nên để level 2-3 để vừa sức)")]
    [SerializeField] private int tutorialBeastLevel = 2;

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
        "Này... ta đứng đây mà đói lả, mắt mờ chân yếu quá. Cháu có thể mua giúp ta một tách Cà Phê từ Cửa Hàng trong làng không?",
        "Mang Cà Phê lại cho ta, ta sẽ chỉ cháu bí quyết và đưa cháu vào thực hành bắt những chú Thú hoang dã đấy nhé!"
    };

    [Header("Thoại Giai Đoạn 2 — Chưa mang đủ đồ")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialoguePhase2 = new string[]
    {
        "Ồ... cháu chưa mang Cà Phê lại cho ta à? Cửa Hàng ở ngay phía dưới làng đó, đừng quên nhé!"
    };

    [Header("Thoại Giai Đoạn 3 — Nhận đồ, dạy bắt thú & vào trận đấu")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialoguePhase3 = new string[]
    {
        "Ồ! Cháu tốt bụng quá! Cà Phê nóng thơm lừng, uống xong tỉnh táo hẳn người!",
        "Ta tặng cháu {0} Bóng Thu Phục để bắt đầu hành trình. Bây giờ ta sẽ đưa cháu vào một trận đấu thực tế để thực hành bắt Thú luôn nhé!",
        "BÍ QUYẾT BẮT THÚ:\n★ Dùng Kỹ Năng đánh cho MÁU QUÁI CÀNG THẤP → TỈ LỆ BẮT CÀNG CAO!\n★ Khi quái yếu máu: Mở Balo/Túi đồ → Chọn Bóng Thu Phục để ném bắt nó!\n★ Sẵn sàng chưa? Hãy vào trận và thử ném bóng ngay nào!"
    };

    // ─── Private State ───────────────────────────────────────────────
    private bool hasCompleted = false;
    private const string SAVE_KEY_PREFIX = "BlockingNPC_Done_";
    private string SaveKey => SAVE_KEY_PREFIX + (string.IsNullOrEmpty(uniqueNpcId) ? gameObject.name : uniqueNpcId);

    // ─── Unity Lifecycle ─────────────────────────────────────────────
    private void Awake()
    {
        // Kiểm tra ngay khi GameObject được khởi tạo
        hasCompleted = PlayerPrefs.GetInt(SaveKey, 0) == 1;
        if (hasCompleted)
        {
            OpenPath();
        }
    }

    private void Start()
    {
        // Kiểm tra lại lần nữa khi Start
        hasCompleted = PlayerPrefs.GetInt(SaveKey, 0) == 1;
        if (hasCompleted)
        {
            OpenPath();
        }
    }

    // ─── IInteractable ───────────────────────────────────────────────
    public void Interact(PlayerInventory playerInventory)
    {
        if (hasCompleted)
        {
            OpenPath();
            return;
        }

        bool playerHasItem = CheckPlayerHasItem(playerInventory);

        if (!playerHasItem)
        {
            ShowDialogue(dialoguePhase1, null);
        }
        else
        {
            ShowDialogue(BuildPhase3Dialogue(), () =>
            {
                CompleteQuestAndStartBattle(playerInventory);
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

    private int CountItemInInventory(PlayerInventory playerInventory, Item targetItem)
    {
        if (playerInventory == null || targetItem == null) return 0;

        int total = 0;
        var allSlots = new List<GameObject>();
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
    /// Thu Cà Phê, tặng 5 Bóng Thu Phục, mở đường và kích hoạt Trận đấu thực chiến.
    /// </summary>
    private void CompleteQuestAndStartBattle(PlayerInventory playerInventory)
    {
        // 1. Thu item từ người chơi
        if (requiredItem != null && playerInventory != null)
        {
            int remaining = requiredAmount;
            var allSlots = new List<GameObject>();
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
            }
        }

        // 4. Đánh dấu đã hoàn thành và lưu vĩnh viễn
        hasCompleted = true;
        PlayerPrefs.SetInt(SaveKey, 1);
        PlayerPrefs.Save();
        Debug.Log($"<color=yellow>[BlockingNPC]</color> Đã lưu hoàn thành NPC với SaveKey: {SaveKey}");

        OpenPath();

        // 5. Vào Trận Đấu Thực Chiến Thị Phạm
        if (triggerTutorialBattle)
        {
            StartTutorialBattle(playerInventory);
        }
    }

    private void StartTutorialBattle(PlayerInventory playerInventory)
    {
        BattleTransferData battleData = Resources.Load<BattleTransferData>("BattleTransferData");
        if (battleData == null)
        {
            battleData = ScriptableObject.CreateInstance<BattleTransferData>();
        }

        battleData.ResetData();
        battleData.originScene = BattleTransferData.OriginScene.Map;
        // lastPlayerPosition và returnToLastPosition được GameSceneManager.GoToBattle() tự xử lý

        // Trận đấu hoang dã (không phải Trainer) để được quyền ném bóng bắt thú!
        battleData.isTrainerBattle = false;
        battleData.isGymLeaderBattle = false;
        battleData.isSingleBattle = false;

        // Tìm quái mẫu: ưu tiên tutorialBeast được kéo vào, nếu không thì tự load quái có sẵn trong Resources
        BeastData enemyBeast = tutorialBeast;
        if (enemyBeast == null)
        {
            enemyBeast = Resources.Load<BeastData>("Rat");
            if (enemyBeast == null) enemyBeast = Resources.Load<BeastData>("Slime");
            if (enemyBeast == null)
            {
                var all = Resources.LoadAll<BeastData>("");
                if (all != null && all.Length > 0) enemyBeast = all[0];
            }
        }

        List<RuntimeBeastData> enemyTeam = new List<RuntimeBeastData>();
        if (enemyBeast != null)
        {
            enemyTeam.Add(new RuntimeBeastData(enemyBeast, Mathf.Max(1, tutorialBeastLevel)));
            Debug.Log($"<color=cyan>[BlockingNPC]</color> Tạo trận đấu thị phạm với: {enemyBeast.beastName} (Lv.{tutorialBeastLevel})");
        }
        else
        {
            Debug.LogWarning("[BlockingNPC] Chưa gán tutorialBeast cho NPC!");
        }

        battleData.SetEnemyTeam(enemyTeam);

        // Chuyển sang Battle Scene
        GameSceneManager.GoToBattle(battleData);
    }

    private void OpenPath()
    {
        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }

        // Ẩn hoàn toàn GameObject của Ông Lão để biến mất khỏi bản đồ
        gameObject.SetActive(false);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = hasCompleted ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
