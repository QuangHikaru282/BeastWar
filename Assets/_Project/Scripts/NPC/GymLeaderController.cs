using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ????????????????????????????????????????????????????????????????
///  GymLeaderController — Script dùng chung cho m?i Gym Leader
/// ????????????????????????????????????????????????????????????????
///
/// CÁCH DÙNG:
///   1. T?o GameObject r?ng trong scene (ð?t tên VD: "GymLeader_Earth")
///   2. G?n script này vào
///   3. Thêm Collider2D, b?t isTrigger = true
///   4. Kéo th? các field trong Inspector
///
/// SETUP NHANH 4 GYM:
///   • Gym 1 Ð?t:  gymLeaderId="GymLeader_Earth"  badgeId="Badge_1"  rewardToolItem=Cuoc da
///   • Gym 2 C?:   gymLeaderId="GymLeader_Grass"  badgeId="Badge_2"  rewardToolItem=Liem
///   • Gym 3 Ný?c: gymLeaderId="GymLeader_Water"  badgeId="Badge_3"  rewardToolItem=Binh tuoi
///   • Gym 4 L?a:  gymLeaderId="GymLeader_Fire"   badgeId="Badge_4"   rewardToolItem=Rua
/// </summary>
public class GymLeaderController : MonoBehaviour, Kinnly.IInteractable
{
    [Header("?? Thông tin Gym Leader")]
    [Tooltip("ID duy nh?t ð? lýu tr?ng thái (VD: GymLeader_Earth)")]
    public string gymLeaderId = "GymLeader_Earth";

    [Tooltip("Tên hi?n th? trong h?i tho?i")]
    public string gymLeaderName = "Th? L?nh Ð?t";

    [Tooltip("?nh avatar c?a Gym Leader")]
    public Sprite leaderAvatar;

    [Header("?? H?i Tho?i")]
    [TextArea(2, 5)]
    public string challengeDialogue = "Ð?t ðai này s? nh?n ch?m ngýõi!";

    [TextArea(2, 5)]
    public string defeatDialogue = "Không th? tin ðý?c... H?y nh?n Huy Hi?u này x?ng ðáng.";

    [TextArea(2, 5)]
    public string rematchDialogue = "Ngýõi ð? ch?ng minh b?n thân. Ta không có g? ð? thách ð?u thêm.";

    [Header("?? Ð?i H?nh Chi?n Ð?u")]
    [Tooltip("Kéo BeastData vào ðây (t?i ða 6 Beast)")]
    public List<BeastData> leaderTeam = new List<BeastData>();

    [Header("?? Huy Hi?u & Ph?n Thý?ng")]
    [Tooltip("ID Huy hi?u (VD: EarthBadge, GrassBadge, WaterBadge, FireBadge)")]
    public string badgeId = "Badge_1";

    [Tooltip("Tên Huy hi?u hi?n th? (VD: Huy Hi?u Ð?t)")]
    public string badgeDisplayName = "Huy Hi?u Ð?t";

    [Tooltip("Công c? trao sau khi th?ng (kéo Item asset vào ðây)")]
    public Kinnly.Item rewardToolItem;

    [Tooltip("S? lý?ng v?t ph?m trao")]
    public int rewardToolAmount = 1;

    [TextArea(1, 3)]
    public string toolRewardDialogue = "Ngoài Huy Hi?u, ta t?ng ngýõi thêm m?t v?t ph?m ð?c bi?t!";

    [Header("?? References")]
    [Tooltip("Kéo BattleTransferData asset vào ðây")]
    public BattleTransferData battleTransferData;

    [Tooltip("Kéo PlayerData asset vào ðây")]
    public PlayerData playerData;

    [Tooltip("(Tùy ch?n) D?u ch?m than ! hi?n lên ð?u khi g?p Player")]
    public GameObject exclamationMarkObject;

    [Header("?? Tr?ng Thái (Runtime)")]
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
        // 1. D?ng Player
        PlayerMapController playerCtrl = playerObj.GetComponent<PlayerMapController>();
        if (playerCtrl != null) playerCtrl.SetCanMove(false);

        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Hi?n d?u ch?m than
        if (exclamationMarkObject != null)
        {
            exclamationMarkObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            exclamationMarkObject.SetActive(false);
        }

        // 3. H?i tho?i thách ð?u
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

        // 4. Ki?m tra d? li?u
        if (battleTransferData == null)
        {
            Debug.LogError($"[GymLeaderController] {gymLeaderName}: Chýa gán BattleTransferData!");
            if (playerCtrl != null) playerCtrl.SetCanMove(true);
            isEncounterActive = false;
            yield break;
        }

        if (leaderTeam == null || leaderTeam.Count == 0)
        {
            Debug.LogError($"[GymLeaderController] {gymLeaderName}: Chýa gán leaderTeam!");
            if (playerCtrl != null) playerCtrl.SetCanMove(true);
            isEncounterActive = false;
            yield break;
        }

        // 5. Setup BattleTransferData
        battleTransferData.ResetData();
        battleTransferData.originScene = BattleTransferData.OriginScene.Map;
        battleTransferData.lastPlayerPosition = playerObj.transform.position;
        battleTransferData.returnToLastPosition = true;
        battleTransferData.isTrainerBattle = true;
        battleTransferData.isGymLeaderBattle = true;
        battleTransferData.rewardBadgeId = badgeId;
        battleTransferData.lastEncounteredBeastId = gymLeaderId;

        List<RuntimeBeastData> runtimeTeam = new List<RuntimeBeastData>();
        foreach (var beast in leaderTeam)
        {
            if (beast != null) runtimeTeam.Add(new RuntimeBeastData(beast, 1));
        }
        battleTransferData.SetEnemyTeam(runtimeTeam);

        Debug.Log($"[GymLeaderController] {gymLeaderName}: Vào chi?n ð?u!");
        GameSceneManager.GoToBattle();
    }

    /// <summary>
    /// G?i hàm này t? BattleManager sau khi Player th?ng Gym Leader.
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
        yield return null; // ch? 1 frame

        // H?i tho?i thua
        bool dialogueDone = false;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                gymLeaderName, defeatDialogue,
                () => { dialogueDone = true; }, leaderAvatar);
            yield return new WaitUntil(() => dialogueDone);
        }

        // Trao công c?
        if (rewardToolItem != null)
        {
            Kinnly.PlayerInventory playerInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
            if (playerInv != null)
            {
                playerInv.AddItem(rewardToolItem, rewardToolAmount);
                Debug.Log($"[GymLeaderController] Ð? trao {rewardToolAmount}x {rewardToolItem.name}!");
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

        // Thông báo nh?n Huy hi?u
        if (DialogueManager.Instance != null)
        {
            bool badgeDone = false;
            DialogueManager.Instance.StartDialogue(
                "Thông báo",
                $"? B?n ð? nh?n ðý?c {badgeDisplayName}!",
                () => { badgeDone = true; });
            yield return new WaitUntil(() => badgeDone);
        }

        isEncounterActive = false;
    }
}
