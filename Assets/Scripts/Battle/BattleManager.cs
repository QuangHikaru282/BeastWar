using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Controller chính của Battle Scene.
/// 
/// LUỒNG ĐƠN GIẢN:
///   INIT → Player đi trước → Click thú mình → Click thú địch → Lao đến đánh
///   → Lượt địch → Lặp lại → 1 bên chết hết → Kết thúc → Về MapScene
/// </summary>
public class BattleManager : MonoBehaviour
{
    private enum BattleState { Init, PlayerTurn, EnemyTurn, BattleEnd }

    // ─── Inspector ───────────────────────────────────────────────────

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private BattleTransferData battleTransferData;

    [Header("Spawn Points — Player (3 vị trí bên trái)")]
    [SerializeField] private List<Transform> playerSpawnPoints;

    [Header("Spawn Points — Enemy (3 vị trí bên phải)")]
    [SerializeField] private List<Transform> enemySpawnPoints;

    [Header("Prefab Beast Unit")]
    [SerializeField] private GameObject beastUnitPrefab;

    [Header("Components")]
    [SerializeField] private ActionPanel actionPanel;
    [SerializeField] private EnemyAI enemyAI;

    // ─── Runtime ─────────────────────────────────────────────────────

    public static BattleManager Instance { get; private set; }

    private List<BeastUnit> playerTeam = new List<BeastUnit>();
    private List<BeastUnit> enemyTeam  = new List<BeastUnit>();

    private BattleState state = BattleState.Init;
    private bool waitingForPlayerAction;
    private BeastUnit chosenAttacker;
    private BeastUnit chosenTarget;

    // ─── Unity Lifecycle ─────────────────────────────────────────────

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(RunBattle());
    }

    // ─── State Machine ───────────────────────────────────────────────

    private IEnumerator RunBattle()
    {
        // --- INIT ---
        state = BattleState.Init;
        yield return StartCoroutine(InitBattle());

        // --- Main loop: Player luôn đi trước ---
        while (true)
        {
            // Lượt Player
            state = BattleState.PlayerTurn;
            yield return StartCoroutine(PlayerTurn());
            if (CheckBattleEnd()) break;

            // Lượt Enemy
            state = BattleState.EnemyTurn;
            yield return StartCoroutine(EnemyTurn());
            if (CheckBattleEnd()) break;

            yield return new WaitForSeconds(0.3f);
        }

        // --- KẾT THÚC ---
        state = BattleState.BattleEnd;
        yield return StartCoroutine(EndBattle());
    }

    // ─── INIT ────────────────────────────────────────────────────────

    private IEnumerator InitBattle()
    {
        Debug.Log("[BattleManager] Trận chiến bắt đầu!");

        // Spawn đội Player
        var pFormation = playerData.currentFormation.Where(b => b != null).ToList();

        // Tự động lấy Beast trong túi nếu đội hình trống
        if (pFormation.Count == 0 && playerData.ownedBeasts != null && playerData.ownedBeasts.Count > 0)
        {
            pFormation = playerData.ownedBeasts.GetRange(0, Mathf.Min(playerData.ownedBeasts.Count, 3));
            playerData.SetFormation(pFormation);
            Debug.Log($"[BattleManager] Đội hình trống! Tự động gán {pFormation.Count} Beast.");
        }

        for (int i = 0; i < pFormation.Count && i < playerSpawnPoints.Count; i++)
        {
            var unit = SpawnBeastUnit(pFormation[i], playerSpawnPoints[i], true);
            playerTeam.Add(unit);
        }

        // Spawn đội Enemy
        var eFormation = battleTransferData.wildEnemyTeam.Where(b => b != null).ToList();
        for (int i = 0; i < eFormation.Count && i < enemySpawnPoints.Count; i++)
        {
            var unit = SpawnBeastUnit(eFormation[i], enemySpawnPoints[i], false);
            enemyTeam.Add(unit);
        }

        // Khởi tạo ActionPanel
        actionPanel?.Initialize(playerTeam, enemyTeam, OnPlayerActionChosen);

        yield return new WaitForSeconds(0.5f);
    }

    private BeastUnit SpawnBeastUnit(BeastData data, Transform spawnPoint, bool isPlayer)
    {
        GameObject go = beastUnitPrefab != null
            ? Instantiate(beastUnitPrefab, spawnPoint.position, Quaternion.identity)
            : new GameObject($"BeastUnit_{data.beastName}");

        go.transform.position = spawnPoint.position;

        var unit = go.GetComponent<BeastUnit>() ?? go.AddComponent<BeastUnit>();
        unit.Initialize(data, isPlayer, null);
        return unit;
    }

    // ─── PLAYER TURN ─────────────────────────────────────────────────

    private IEnumerator PlayerTurn()
    {
        var alive = playerTeam.Where(b => b != null && b.IsAlive).ToList();
        if (alive.Count == 0) yield break;

        waitingForPlayerAction = true;
        actionPanel?.Show();

        // Chờ player click chọn thú mình → click thú địch
        while (waitingForPlayerAction)
            yield return null;

        // Thực hiện tấn công
        yield return StartCoroutine(ExecuteAttack(chosenAttacker, chosenTarget));
    }

    private void OnPlayerActionChosen(BeastUnit attacker, BeastUnit target, MoveData move, bool isCatch)
    {
        chosenAttacker = attacker;
        chosenTarget   = target;
        waitingForPlayerAction = false;
    }

    // ─── ENEMY TURN ──────────────────────────────────────────────────

    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);

        bool valid = enemyAI.ChooseAction(enemyTeam, playerTeam,
                                          out BeastUnit attacker,
                                          out BeastUnit target,
                                          out MoveData move);

        if (!valid) yield break;

        yield return StartCoroutine(ExecuteAttack(attacker, target));
    }

    // ─── EXECUTE ATTACK ──────────────────────────────────────────────

    private IEnumerator ExecuteAttack(BeastUnit attacker, BeastUnit target)
    {
        if (attacker == null || !attacker.IsAlive) yield break;
        if (target == null   || !target.IsAlive)   yield break;

        Debug.Log($"[Battle] {attacker.Data.beastName} tấn công {target.Data.beastName}!");

        // Lao lên
        Vector3 originalPos = attacker.transform.position;
        Vector3 dir = (target.transform.position - originalPos).normalized;
        Vector3 dashPos = target.transform.position - dir * 0.8f;

        yield return attacker.transform.DOMove(dashPos, 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();

        // Tính sát thương (tấn công thường)
        int damage = attacker.CalculateBaseDamage(target);

        // Gây sát thương
        bool died = target.TakeDamage(damage);

        Debug.Log($"[Battle] {attacker.Data.beastName} gây {damage} sát thương! {target.Data.beastName} HP: {target.CurrentHP}");

        yield return new WaitForSeconds(0.2f);

        // Lùi về vị trí cũ
        yield return attacker.transform.DOMove(originalPos, 0.25f).SetEase(Ease.InQuad).WaitForCompletion();

        if (died)
        {
            Debug.Log($"[Battle] {target.Data.beastName} đã chết!");
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ─── CHECK BATTLE END ────────────────────────────────────────────

    private bool CheckBattleEnd()
    {
        bool playerAllDead = playerTeam.All(b => b == null || !b.IsAlive);
        bool enemyAllDead  = enemyTeam.All(b => b == null || !b.IsAlive);
        return playerAllDead || enemyAllDead;
    }

    // ─── BATTLE END ──────────────────────────────────────────────────

    private IEnumerator EndBattle()
    {
        bool playerWon = enemyTeam.All(b => b == null || !b.IsAlive);

        // Lưu quái bị choáng nếu thắng
        if (playerWon && battleTransferData != null 
            && !string.IsNullOrEmpty(battleTransferData.lastEncounteredBeastId))
        {
            if (!battleTransferData.stunnedBeastIds.Contains(battleTransferData.lastEncounteredBeastId))
            {
                battleTransferData.stunnedBeastIds.Add(battleTransferData.lastEncounteredBeastId);
            }
        }

        Debug.Log(playerWon ? "[Battle] THẮNG!" : "[Battle] THUA!");
        yield return new WaitForSeconds(1.5f);

        // Quay về MapScene
        GameSceneManager.GoToMap();
    }

    // ─── CLICK HANDLER ───────────────────────────────────────────────

    public void HandleBeastClick(BeastUnit clickedBeast)
    {
        if (state == BattleState.PlayerTurn && waitingForPlayerAction && actionPanel != null)
        {
            actionPanel.OnBeastClicked(clickedBeast);
        }
    }
}
