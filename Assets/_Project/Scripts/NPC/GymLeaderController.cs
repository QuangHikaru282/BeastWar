using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ════════════════════════════════════════════════════════════════
///  GymLeaderController — Script dùng chung cho mọi Gym Leader
/// ════════════════════════════════════════════════════════════════
/// </summary>
public class GymLeaderController : MonoBehaviour, Kinnly.IInteractable
{
    [System.Serializable]
    public class GymBeastSlot
    {
        [Tooltip("Dữ liệu Beast của Gym Leader")]
        public BeastData beastData;

        [Range(1, 100)]
        [Tooltip("Cấp độ (Level) của Beast này khi vào trận đấu")]
        public int beastLevel = 10;
    }

    [Header("── Thông tin Gym Leader")]
    [Tooltip("ID duy nhất để lưu trạng thái (VD: GymLeader_Earth)")]
    public string gymLeaderId = "GymLeader_Earth";

    [Tooltip("Tên hiển thị trong hội thoại")]
    public string gymLeaderName = "Thủ Lĩnh Đất";

    [Tooltip("Ảnh avatar của Gym Leader")]
    public Sprite leaderAvatar;

    [Header("── Hội Thoại")]
    [TextArea(2, 5)]
    public string challengeDialogue = "Đất đai này sẽ nhấn chìm ngươi!";

    [TextArea(2, 5)]
    public string defeatDialogue = "Không thể tin được... Hãy nhận Huy Hiệu này xứng đáng.";

    [TextArea(2, 5)]
    public string rematchDialogue = "Ngươi đã chứng minh bản thân. Ta không có gì để thách đấu thêm.";

    [Header("── Đội Hình Chiến Đấu & Cấp Độ")]
    [Tooltip("Cấu hình danh sách Pet và Level tương ứng của Gym Leader")]
    public List<GymBeastSlot> leaderTeamWithLevel = new List<GymBeastSlot>();

    [Header("── Huy Hiệu & Phần Thưởng")]
    [Tooltip("ID Huy hiệu (VD: EarthBadge, GrassBadge, WaterBadge, FireBadge, WindBadge)")]
    public string badgeId = "EarthBadge";

    [Tooltip("Tên Huy hiệu hiển thị (VD: Huy Hiệu Đất)")]
    public string badgeDisplayName = "Huy Hiệu Đất";

    [Tooltip("Công cụ trao sau khi thắng (kéo Item asset vào đây)")]
    public Kinnly.Item rewardToolItem;

    [Tooltip("Số lượng vật phẩm trao")]
    public int rewardToolAmount = 1;

    [TextArea(1, 3)]
    public string toolRewardDialogue = "Ngoài Huy Hiệu, ta tặng ngươi thêm một vật phẩm đặc biệt!";

    [Header("── References")]
    [Tooltip("Kéo BattleTransferData asset vào đây")]
    public BattleTransferData battleTransferData;

    [Tooltip("Kéo PlayerData asset vào đây")]
    public PlayerData playerData;

    [Tooltip("(Tùy chọn) Dấu chấm than ! hiện lên đầu khi gặp Player")]
    public GameObject exclamationMarkObject;

    [Header("── Trạng Thái (Runtime)")]
    public bool alreadyDefeated = false;

    private bool isEncounterActive = false;

    private void Awake()
    {
        if (exclamationMarkObject != null)
            exclamationMarkObject.SetActive(false);
    }

    private void Start()
    {
        if (playerData != null && playerData.defeatedTrainers != null)
        {
            if (playerData.defeatedTrainers.Contains(gymLeaderId))
                alreadyDefeated = true;
        }

        if (alreadyDefeated)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyDefeated || isEncounterActive) return;
        if (!other.CompareTag("Player")) return;

        isEncounterActive = true;
        StartCoroutine(GymLeaderEncounterRoutine(other.gameObject));
    }

    public void Interact(Kinnly.PlayerInventory playerInventory)
    {
        if (alreadyDefeated)
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(gymLeaderName, rematchDialogue, null, leaderAvatar);
            return;
        }

        if (!isEncounterActive)
        {
            isEncounterActive = true;
            GameObject playerObj = playerInventory != null
                ? playerInventory.gameObject
                : GameObject.FindWithTag("Player");

            if (playerObj != null)
                StartCoroutine(GymLeaderEncounterRoutine(playerObj));
        }
    }

    private IEnumerator GymLeaderEncounterRoutine(GameObject playerObj)
    {
        // 1. Dừng Player
        PlayerMapController playerCtrl = playerObj.GetComponent<PlayerMapController>();
        if (playerCtrl != null) playerCtrl.SetCanMove(false);

        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Hiện dấu chấm than
        if (exclamationMarkObject != null)
        {
            exclamationMarkObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            exclamationMarkObject.SetActive(false);
        }

        // 3. Hội thoại thách đấu
        bool dialogueDone = false;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                gymLeaderName, challengeDialogue,
                () => { dialogueDone = true; }, leaderAvatar);
            yield return new WaitUntil(() => dialogueDone);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 4. Kiểm tra dữ liệu
        if (battleTransferData == null)
        {
            Debug.LogError($"[GymLeaderController] {gymLeaderName}: Chưa gán BattleTransferData!");
            if (playerCtrl != null) playerCtrl.SetCanMove(true);
            isEncounterActive = false;
            yield break;
        }

        if (leaderTeamWithLevel == null || leaderTeamWithLevel.Count == 0)
        {
            Debug.LogError($"[GymLeaderController] {gymLeaderName}: Chưa gán đội hình (leaderTeamWithLevel)!");
            if (playerCtrl != null) playerCtrl.SetCanMove(true);
            isEncounterActive = false;
            yield break;
        }

        // 5. Setup BattleTransferData
        battleTransferData.ResetData();
        battleTransferData.originScene = BattleTransferData.OriginScene.Map;
        // lastPlayerPosition và returnToLastPosition được GameSceneManager.GoToBattle() tự xử lý
        battleTransferData.isTrainerBattle = true;
        battleTransferData.isGymLeaderBattle = true;
        battleTransferData.rewardBadgeId = badgeId;
        battleTransferData.lastEncounteredBeastId = gymLeaderId;

        List<RuntimeBeastData> runtimeTeam = new List<RuntimeBeastData>();
        foreach (var slot in leaderTeamWithLevel)
        {
            if (slot != null && slot.beastData != null)
            {
                int lvl = Mathf.Max(1, slot.beastLevel);
                runtimeTeam.Add(new RuntimeBeastData(slot.beastData, lvl));
            }
        }
        battleTransferData.SetEnemyTeam(runtimeTeam);

        Debug.Log($"[GymLeaderController] {gymLeaderName}: Vào chiến đấu với {runtimeTeam.Count} pet!");
        GameSceneManager.GoToBattle(battleTransferData);
    }

    /// <summary>
    /// Gọi hàm này từ BattleManager sau khi Player thắng Gym Leader.
    /// </summary>
    public void OnPlayerWon()
    {
        if (alreadyDefeated) return;
        alreadyDefeated = true;

        if (playerData != null)
        {
            if (!playerData.defeatedTrainers.Contains(gymLeaderId))
                playerData.defeatedTrainers.Add(gymLeaderId);

            if (!playerData.gymBadges.Contains(badgeId))
                playerData.gymBadges.Add(badgeId);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(PostWinRoutine());
    }

    private IEnumerator PostWinRoutine()
    {
        yield return null; // chờ 1 frame

        // Hội thoại thua
        bool dialogueDone = false;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                gymLeaderName, defeatDialogue,
                () => { dialogueDone = true; }, leaderAvatar);
            yield return new WaitUntil(() => dialogueDone);
        }

        // Trao công cụ
        if (rewardToolItem != null)
        {
            Kinnly.PlayerInventory playerInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
            if (playerInv != null)
            {
                playerInv.AddItem(rewardToolItem, rewardToolAmount);
                Debug.Log($"[GymLeaderController] Đã trao {rewardToolAmount}x {rewardToolItem.name}!");
            }

            if (DialogueManager.Instance != null && !string.IsNullOrEmpty(toolRewardDialogue))
            {
                bool toolDone = false;
                DialogueManager.Instance.StartDialogue(
                    gymLeaderName, toolRewardDialogue,
                    () => { toolDone = true; }, leaderAvatar);
                yield return new WaitUntil(() => toolDone);
            }
        }

        // Thông báo nhận Huy hiệu
        if (DialogueManager.Instance != null)
        {
            bool badgeDone = false;
            DialogueManager.Instance.StartDialogue(
                "Thông báo",
                $"★ Bạn đã nhận được {badgeDisplayName}!",
                () => { badgeDone = true; });
            yield return new WaitUntil(() => badgeDone);
        }

        isEncounterActive = false;
    }
}
