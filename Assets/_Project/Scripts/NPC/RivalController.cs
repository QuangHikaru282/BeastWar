using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gắn script này vào NPC Rival trong Lab.
/// Rival sẽ tự động dùng Beast khắc chế với Starter người chơi đã chọn.
///
/// Setup trong Inspector:
///   - Gán PlayerData
///   - Gán BattleTransferData
///   - Gán 3 đội hình preset tương ứng với 3 lựa chọn Starter:
///       rivalTeamIfPlayerChoseStarter1 → đội khi player chọn Starter 1 (Cỏ)
///       rivalTeamIfPlayerChoseStarter2 → đội khi player chọn Starter 2 (Lửa)
///       rivalTeamIfPlayerChoseStarter3 → đội khi player chọn Starter 3 (Nước)
/// </summary>
public class RivalController : MonoBehaviour, Kinnly.IInteractable
{
    [Header("References — Kéo thả trong Inspector")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private BattleTransferData battleTransferData;

    [Header("Avatar & Thoại")]
    [SerializeField] private Sprite rivalAvatar;

    [TextArea(2, 4)]
    [SerializeField] private string preBattleDialogue =
        "Mày chọn rồi à? Vừa hay, tao đã chọn đối thủ hoàn hảo để khắc chế mày! Chuẩn bị đi!";

    [TextArea(2, 4)]
    [SerializeField] private string postBattleDialogue =
        "Haah... Không thể nào. Nhưng tao sẽ không dừng lại ở đây đâu. Chờ xem!";

    [Header("3 Preset Đội Rival (Kéo BeastData vào từng ô)")]
    [Tooltip("Đội Rival dùng khi Player chọn Starter 1 (Cỏ) — Rival dùng Lửa")]
    public List<BeastData> rivalTeamIfPlayerChoseStarter1;

    [Tooltip("Đội Rival dùng khi Player chọn Starter 2 (Lửa) — Rival dùng Nước")]
    public List<BeastData> rivalTeamIfPlayerChoseStarter2;

    [Tooltip("Đội Rival dùng khi Player chọn Starter 3 (Nước) — Rival dùng Cỏ")]
    public List<BeastData> rivalTeamIfPlayerChoseStarter3;

    [Header("Trạng thái")]
    [Tooltip("ID duy nhất để lưu đã đánh bại hay chưa")]
    public string uniqueRivalId = "Rival_Lab_Phase1";
    public bool alreadyDefeated = false;

    private void Start()
    {
        // Đọc trạng thái đã đánh bại từ PlayerData
        if (playerData != null &&
            playerData.defeatedTrainers != null &&
            playerData.defeatedTrainers.Contains(uniqueRivalId))
        {
            alreadyDefeated = true;
        }

        // Ẩn Rival nếu giai đoạn 1 đã hoàn thành
        if (alreadyDefeated)
        {
            // Tuỳ chọn: có thể ẩn hẳn hoặc đổi dialogue
            gameObject.SetActive(false);
        }
    }

    public void Interact(Kinnly.PlayerInventory playerInventory)
    {
        // Chưa có Starter → không cho đấu
        if (playerData == null ||
            playerData.ownedBeasts == null ||
            playerData.ownedBeasts.Count == 0)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(
                    "Rival",
                    "Mày chưa chọn Beast à? Đi chọn đi rồi tao sẽ thách đấu!",
                    null,
                    rivalAvatar
                );
            }
            return;
        }

        if (alreadyDefeated)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(
                    "Rival",
                    postBattleDialogue,
                    null,
                    rivalAvatar
                );
            }
            return;
        }

        StartCoroutine(RivalEncounterRoutine(playerInventory?.gameObject));
    }

    private IEnumerator RivalEncounterRoutine(GameObject playerObj)
    {
        // 1. Dừng Player
        PlayerMapController ctrl = playerObj != null
            ? playerObj.GetComponent<PlayerMapController>()
            : null;
        if (ctrl != null) ctrl.SetCanMove(false);

        // 2. Dialogue thách đấu
        bool done = false;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                "Rival",
                preBattleDialogue,
                () => { done = true; },
                rivalAvatar
            );
            yield return new WaitUntil(() => done);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 3. Xác định đội Rival dựa vào Starter người chơi đã chọn
        List<BeastData> chosenTeam = GetRivalTeamBasedOnPlayerStarter();

        if (chosenTeam == null || chosenTeam.Count == 0)
        {
            Debug.LogError("[RivalController] Chưa gán đội Rival phù hợp trong Inspector!");
            if (ctrl != null) ctrl.SetCanMove(true);
            yield break;
        }

        // 4. Vào Battle
        if (battleTransferData == null)
        {
            Debug.LogError("[RivalController] Chưa gán BattleTransferData!");
            if (ctrl != null) ctrl.SetCanMove(true);
            yield break;
        }

        battleTransferData.ResetData();
        battleTransferData.originScene = BattleTransferData.OriginScene.Map;
        battleTransferData.isTrainerBattle = true;
        battleTransferData.isGymLeaderBattle = false;
        battleTransferData.lastEncounteredBeastId = uniqueRivalId;
        battleTransferData.returnToLastPosition = true;

        if (playerObj != null)
            battleTransferData.lastPlayerPosition = playerObj.transform.position;

        List<RuntimeBeastData> runtimeTeam = new List<RuntimeBeastData>();
        foreach (var beast in chosenTeam)
        {
            if (beast != null)
                runtimeTeam.Add(new RuntimeBeastData(beast, 1));
        }
        battleTransferData.SetEnemyTeam(runtimeTeam);

        GameSceneManager.GoToBattle();
    }

    /// <summary>
    /// Xác định đội Rival dựa vào playerData.lastStarterChoiceIndex:
    ///   0 → Player chọn Starter 1 (Cỏ)  → Rival dùng rivalTeamIfPlayerChoseStarter1
    ///   1 → Player chọn Starter 2 (Lửa) → Rival dùng rivalTeamIfPlayerChoseStarter2
    ///   2 → Player chọn Starter 3 (Nước)→ Rival dùng rivalTeamIfPlayerChoseStarter3
    /// </summary>
    private List<BeastData> GetRivalTeamBasedOnPlayerStarter()
    {
        int idx = (playerData != null) ? playerData.lastStarterChoiceIndex : 0;

        switch (idx)
        {
            case 1:  return rivalTeamIfPlayerChoseStarter2;
            case 2:  return rivalTeamIfPlayerChoseStarter3;
            default: return rivalTeamIfPlayerChoseStarter1;
        }
    }
}
