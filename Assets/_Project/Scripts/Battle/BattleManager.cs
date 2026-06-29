using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
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
    private enum BattleState { PreBattle, Init, PlayerTurn, EnemyTurn, BattleEnd }

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
    [SerializeField] private ActionPanel     actionPanel;
    [SerializeField] private EnemyAI         enemyAI;
    [SerializeField] private FruitBuffManager fruitBuffManager;  // Panel chọn Quả đầu trận
    [SerializeField] private StageData        currentStageData;   // Gán từ WorldMapData lúc load scene

    // ─── Runtime ─────────────────────────────────────────────────────

    public static BattleManager Instance { get; private set; }

    private List<BeastUnit> playerTeam = new List<BeastUnit>();
    private List<BeastUnit> enemyTeam  = new List<BeastUnit>();

    private BattleState state = BattleState.PreBattle;
    private bool        waitingForPlayerAction;
    private bool        fruitSelected = false;  // Cờ chờ người chơi chọn Quả
    private BeastUnit   chosenAttacker;
    private BeastUnit   chosenTarget;
    private MoveData    chosenMove;

    private void Start()
    {
        // XÓA BỎ BATTLESYSTEM CŨ CỦA TUTORIAL NẾU NGƯỜI DÙNG QUÊN XÓA
        var oldSystem = FindFirstObjectByType<BattleSystem>();
        if (oldSystem != null)
        {
            Debug.LogWarning("--- ĐÃ PHÁT HIỆN BATTLESYSTEM (TUTORIAL CŨ)! HỆ THỐNG ĐÃ TỰ ĐỘNG XÓA ĐỂ TRÁNH XUNG ĐỘT! ---");
            Destroy(oldSystem.gameObject);
        }

        StartCoroutine(RunBattle());
    }

    private IEnumerator RunBattle()
    {
        // --- PRE-BATTLE: Hiện Panel chọn Quả, đợi Player chọn xong ---
        state = BattleState.PreBattle;
        fruitSelected = false;
        if (fruitBuffManager != null)
        {
            fruitBuffManager.Show();
            yield return new WaitUntil(() => fruitSelected);
        }
        else
        {
            // Bỏ qua chọn quả nếu không có FruitBuffManager
            fruitSelected = true;
        }

        // --- INIT: Spawn thú lên sân ---
        state = BattleState.Init;
        yield return StartCoroutine(InitBattle());

        // --- Main loop: Player luôn đi trước ---
        while (true)
        {
            // Lượt Player
            state = BattleState.PlayerTurn;
            yield return StartCoroutine(PlayerTurn());
            
            // Nếu quái địch chết, spawn con tiếp theo thế chỗ trước khi check kết thúc
            yield return StartCoroutine(CheckAndSpawnNextEnemy());
            if (CheckBattleEnd()) break;

            // Lượt Enemy
            state = BattleState.EnemyTurn;
            yield return StartCoroutine(EnemyTurn());

            // Hồi máu cho các thú Player đang nghỉ (2% MaxHP/lượt)
            TickRestingPlayerUnits();

            // Nếu thú của Player chết, spawn con tiếp theo thế chỗ trước khi check kết thúc
            yield return StartCoroutine(CheckAndSpawnNextPlayer());
            if (CheckBattleEnd()) break;

            yield return new WaitForSeconds(0.3f);
        }

        // --- KẾT THÚC ---
        state = BattleState.BattleEnd;
        yield return StartCoroutine(EndBattle());
    }

    // ─── INIT ────────────────────────────────────────────────────────

    // Hàng chờ quái địch dự phòng (đối với trận đấu ải WorldMap)
    private Queue<BeastData> pendingEnemyQueue = new Queue<BeastData>();
    // Hàng chờ thú của người chơi dự phòng
    private Queue<BeastData> pendingPlayerQueue = new Queue<BeastData>();

    private IEnumerator InitBattle()
    {
        Debug.Log("--- BƯỚC 1: BẮT ĐẦU KHỞI TẠO TRẬN ĐẤU ---");

        // Lấy đội hình Player
        var pFormation = playerData.currentFormation.Where(b => b != null).ToList();

        // Tự động lấy Beast trong túi nếu đội hình trống
        if (pFormation.Count == 0 && playerData.ownedBeasts != null && playerData.ownedBeasts.Count > 0)
        {
            pFormation = playerData.ownedBeasts.GetRange(0, Mathf.Min(playerData.ownedBeasts.Count, 3));
            playerData.SetFormation(pFormation);
            Debug.Log($"[BattleManager] Đội hình trống! Tự động gán {pFormation.Count} Beast.");
        }

        // Đưa đội hình Player vào hàng chờ
        pendingPlayerQueue.Clear();
        
        // Nếu là trận 1v1 khi săn bắt (HuntingScene), chỉ dùng 1 Beast đầu tiên, không xếp hàng chờ các con sau
        if (battleTransferData != null && battleTransferData.isSingleBattle)
        {
            if (pFormation.Count > 0)
            {
                var unit = SpawnBeastUnit(pFormation[0], playerSpawnPoints[0], true);
                playerTeam.Add(unit);
            }
        }
        else
        {
            // Trận đấu ải WorldMap: Cho ra sân lần lượt
            foreach (var playerBeast in pFormation)
            {
                pendingPlayerQueue.Enqueue(playerBeast);
            }

            if (pendingPlayerQueue.Count > 0 && playerSpawnPoints.Count > 0)
            {
                var firstPlayer = pendingPlayerQueue.Dequeue();
                var unit = SpawnBeastUnit(firstPlayer, playerSpawnPoints[0], true);
                playerTeam.Add(unit);
                Debug.Log($"--- BƯỚC 2: ĐÃ TẠO QUÁI PHE MÌNH ({firstPlayer.beastName}) ---");
            }
        }

        // Spawn đội Enemy (Chế độ lần lượt thế chỗ)
        var eFormation = battleTransferData.wildEnemyTeam.Where(b => b != null).ToList();
        
        pendingEnemyQueue.Clear();
        foreach (var enemyBeast in eFormation)
        {
            pendingEnemyQueue.Enqueue(enemyBeast);
        }

        // Chỉ spawn con enemy đầu tiên lên sân
        if (pendingEnemyQueue.Count > 0 && enemySpawnPoints.Count > 0)
        {
            var firstEnemy = pendingEnemyQueue.Dequeue();
            var unit = SpawnBeastUnit(firstEnemy, enemySpawnPoints[0], false);
            enemyTeam.Add(unit);
            Debug.Log($"--- BƯỚC 3: ĐÃ TẠO QUÁI ĐỊCH ({firstEnemy.beastName}) ---");
        }

        // Khởi tạo EnemyAI nếu chưa gán
        if (enemyAI == null)
        {
            enemyAI = FindFirstObjectByType<EnemyAI>();
            if (enemyAI == null)
            {
                // Tự động tạo EnemyAI nếu trong Scene chưa có
                GameObject aiObj = new GameObject("EnemyAI");
                enemyAI = aiObj.AddComponent<EnemyAI>();
                Debug.Log("--- TỰ ĐỘNG TẠO ENEMY AI VÌ SCENE BỊ THIẾU ---");
            }
        }

        // Khởi tạo ActionPanel
        if (actionPanel == null)
        {
            actionPanel = FindFirstObjectByType<ActionPanel>();
            if (actionPanel == null)
            {
                // Tự động tạo ActionPanel gắn tạm vào BattleManager để nó chạy code auto-link UI
                actionPanel = gameObject.AddComponent<ActionPanel>();
                Debug.Log("--- TỰ ĐỘNG TẠO SCRIPT ACTION PANEL ĐỂ KẾT NỐI VỚI GIAO DIỆN CỦA BẠN ---");
            }
        }

        if (actionPanel == null)
        {
            Debug.LogError("--- LỖI NGHIÊM TRỌNG: KHÔNG TÌM THẤY BẢNG CHỌN CHIÊU THỨC (ActionPanel)! TRẬN ĐẤU SẼ BỊ KẸT! ---");
        }
        else
        {
            actionPanel.Initialize(playerTeam, enemyTeam, OnPlayerActionChosen);
            Debug.Log("--- BƯỚC 4: ĐÃ TẢI BẢNG CHỌN CHIÊU THỨC (UI) ---");
        }

        // Inject danh sách thú bench cho FruitBuffManager biết để TeamHeal
        RefreshFruitBuffBench();

        // Nếu đã chọn Quả TeamHeal → hồi luôn cho thú đang trên sân
        if (fruitBuffManager != null && fruitBuffManager.ActiveFruit == FruitBuffManager.FruitType.TeamHeal)
        {
            foreach (var unit in playerTeam)
                unit?.TeamHeal();
        }

        // Subscribe event Crit nếu đã chọn Quả CritRage
        if (fruitBuffManager != null && fruitBuffManager.ActiveFruit == FruitBuffManager.FruitType.CritRage)
        {
            foreach (var unit in playerTeam)
                fruitBuffManager.SubscribeToUnit(unit);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private BeastUnit SpawnBeastUnit(BeastData data, Transform spawnPoint, bool isPlayer)
    {
        GameObject go = beastUnitPrefab != null
            ? Instantiate(beastUnitPrefab, spawnPoint.position, Quaternion.identity)
            : new GameObject($"BeastUnit_{data.beastName}");

        go.transform.position = spawnPoint.position;
        var unit = go.GetComponent<BeastUnit>() ?? go.AddComponent<BeastUnit>();

        // Tự động tìm và link UI tĩnh trên màn hình (dành riêng cho BattleSceneF)
        string hudName = isPlayer ? "PlayerBattleHud" : "EnemyBattleHud";
        GameObject hudObj = GameObject.Find(hudName);
        if (hudObj != null)
        {
            // Tìm NameText
            Transform nameTr = hudObj.transform.Find("NameText");
            TextMeshProUGUI nameTxtTMP = nameTr != null ? nameTr.GetComponent<TextMeshProUGUI>() : null;
            UnityEngine.UI.Text nameTxtLegacy = nameTr != null ? nameTr.GetComponent<UnityEngine.UI.Text>() : null;

            // Tìm thanh máu (Slider) một cách linh hoạt
            HPBarUI hpBar = hudObj.GetComponentInChildren<HPBarUI>();
            if (hpBar == null)
            {
                // Thử tìm component Slider (mặc định của Unity)
                UnityEngine.UI.Slider slider = hudObj.GetComponentInChildren<UnityEngine.UI.Slider>();
                if (slider != null)
                {
                    hpBar = slider.gameObject.AddComponent<HPBarUI>();
                    // Tự động gán hpSlider thông qua reflection vì biến hpSlider là private
                    var fieldSlider = typeof(HPBarUI).GetField("hpSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (fieldSlider != null) fieldSlider.SetValue(hpBar, slider);
                }
                else
                {
                    // Thử tìm Transform có chứa chữ HP hoặc Fill
                    Transform hpPanelTr = hudObj.transform.Find("HPPanel") ?? hudObj.transform.Find("HPBar");
                    if (hpPanelTr != null)
                    {
                        hpBar = hpPanelTr.gameObject.AddComponent<HPBarUI>();
                        Transform fillTr = hpPanelTr.Find("Fill Area/Fill") ?? hpPanelTr.Find("Fill");
                        if (fillTr != null)
                        {
                            var img = fillTr.GetComponent<UnityEngine.UI.Image>();
                            if (img != null)
                            {
                                var field = typeof(HPBarUI).GetField("fillImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (field != null) field.SetValue(hpBar, img);
                            }
                        }
                    }
                }
            }

            unit.SetExternalUI(nameTxtTMP, nameTxtLegacy, hpBar);
            Debug.Log($"[BattleManager] Đã tự động link UI cho {data.beastName} từ {hudName}");
        }

        unit.Initialize(data, isPlayer);
        return unit;
    }

    // ─── PLAYER TURN ─────────────────────────────────────────────────

    private IEnumerator PlayerTurn()
    {
        var alive = playerTeam.Where(b => b != null && b.IsAlive).ToList();
        if (alive.Count == 0) yield break;

        waitingForPlayerAction = true;
        actionPanel?.Show(alive[0]);
        Debug.Log("--- BƯỚC 5: ĐẾN LƯỢT NGƯỜI CHƠI (Đang chờ bạn chọn chiêu trên màn hình...) ---");

        // Chờ player click chọn thú mình → click thú địch
        while (waitingForPlayerAction)
            yield return null;

        Debug.Log($"--- BƯỚC 6: BẠN ĐÃ CHỌN CHIÊU XONG! Đang tung đòn... ---");

        // Thực hiện tấn công
        yield return StartCoroutine(ExecuteAttack(chosenAttacker, chosenTarget, chosenMove));

        // Sau khi tấn công xong, báo FruitBuffManager (dùng cho Baton Pass)
        fruitBuffManager?.OnPlayerAttackFinished();
    }

    private void OnPlayerActionChosen(BeastUnit attacker, BeastUnit target, MoveData move, bool isCatch)
    {
        chosenAttacker = attacker;
        chosenTarget   = target;
        chosenMove     = move;
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

        yield return StartCoroutine(ExecuteAttack(attacker, target, move));
    }

    // ─── EXECUTE ATTACK ──────────────────────────────────────────────

    private IEnumerator ExecuteAttack(BeastUnit attacker, BeastUnit target, MoveData move)
    {
        if (attacker == null || !attacker.IsAlive) yield break;
        if (target == null   || !target.IsAlive)   yield break;

        string moveName = move != null ? move.moveName : "Tấn công thường";
        MoveType type = move != null ? move.moveType : MoveType.Melee;
        
        Debug.Log($"[Battle] {attacker.Data.beastName} dùng {moveName} ({type}) tấn công {target.Data.beastName}!");

        Vector3 originalPos = attacker.transform.position;

        // Nếu có Animator xịn xò, ra lệnh chạy hoạt hình Attack
        Animator anim = attacker.GetComponent<Animator>();
        if (anim != null && anim.enabled)
        {
            anim.SetTrigger("Attack");
        }

        if (type == MoveType.Melee)
        {
            // ĐÁNH GẦN: Lao lên tiếp cận mục tiêu
            Vector3 dir = (target.transform.position - originalPos).normalized;
            Vector3 dashPos = target.transform.position - dir * 0.8f;
            yield return attacker.transform.DOMove(dashPos, 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();
        }
        else if (type == MoveType.Ranged)
        {
            // ĐÁNH XA: Đứng tại chỗ nhảy nhẹ lên lấy đà (niệm phép)
            yield return attacker.transform.DOJump(originalPos, 0.5f, 1, 0.3f).WaitForCompletion();
        }
        else if (type == MoveType.Self)
        {
            // BẢN THÂN: Phóng to nhẹ rồi thu nhỏ lại (hiệu ứng nảy chữ hoặc buff)
            yield return attacker.transform.DOJump(originalPos, 0.2f, 1, 0.25f).WaitForCompletion();
        }

        // Gọi hiệu ứng VFX nếu chiêu này có cài đặt hiệu ứng
        if (move != null && move.vfxPrefab != null)
        {
            if (move.vfxSpawnType == VfxSpawnType.SpawnAtTarget)
            {
                // SÉT ĐÁNH: Hiện ngay tại chỗ địch
                GameObject vfx = Instantiate(move.vfxPrefab, target.transform.position, Quaternion.identity);
                // Tự động xóa hiệu ứng đi sau 1.5 giây để tránh đầy bộ nhớ
                Destroy(vfx, 1.5f); 
            }
            else if (move.vfxSpawnType == VfxSpawnType.ShootFromAttacker)
            {
                // PHUN LỬA / ĐẠN BAY: Hiện ở người đánh, xoay hướng về phía địch, rồi bay tới
                GameObject projectile = Instantiate(move.vfxPrefab, attacker.transform.position, Quaternion.identity);
                
                // Tính góc xoay để viên đạn hướng thẳng về địch
                Vector3 dirToTarget = (target.transform.position - attacker.transform.position).normalized;
                float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                // Bay tới đích trong 0.3s rồi tự hủy
                projectile.transform.DOMove(target.transform.position, 0.3f).SetEase(Ease.Linear).OnComplete(() => {
                    Destroy(projectile, 0.5f); // Xóa cục VFX sau khi trúng đích 0.5s để đuôi lửa kịp biến mất
                });

                // Chờ đạn bay tới nơi (0.3s) rồi mới trừ máu
                yield return new WaitForSeconds(0.3f);
            }
            else if (move.vfxSpawnType == VfxSpawnType.RainFromSky)
            {
                // MƯA TỪ TRÊN TRỜI: Hiện cách địch 5 đơn vị Y hướng đi xuống
                Vector3 skyPos = target.transform.position + Vector3.up * 5f;
                GameObject projectile = Instantiate(move.vfxPrefab, skyPos, Quaternion.identity);
                
                // Hướng thẳng xuống dưới
                projectile.transform.rotation = Quaternion.Euler(0, 0, -90f);

                // Bay xuống mục tiêu trong 0.4s rồi tự hủy
                projectile.transform.DOMove(target.transform.position, 0.4f).SetEase(Ease.InQuad).OnComplete(() => {
                    Destroy(projectile, 0.5f);
                });

                // Đợi rơi trúng đích (0.4s)
                yield return new WaitForSeconds(0.4f);
            }
            else if (move.vfxSpawnType == VfxSpawnType.SpawnAtSelf)
            {
                // BẢN THÂN: Hiện VFX trực tiếp trên người thi triển (ví dụ: Hào quang hồi máu)
                GameObject vfx = Instantiate(move.vfxPrefab, attacker.transform.position, Quaternion.identity);
                Destroy(vfx, 1.5f);
            }
        }

        // Tính sát thương dựa trên chiêu thức
        int baseDamage = move != null ? attacker.CalculateDamage(target, move) : attacker.CalculateBaseDamage(target);

        // Roll Crit nếu là đòn của Player (IsPlayerTeam)
        bool isCrit    = false;
        int  finalDamage = baseDamage;
        if (attacker.IsPlayerTeam)
        {
            isCrit = UnityEngine.Random.value < attacker.CritChance;
            if (isCrit)
                finalDamage = Mathf.RoundToInt(baseDamage * attacker.CritMultiplier);
        }

        // Gây sát thương (dùng TakeDamageWithResult để event OnCritLanded được bắn)
        bool died = target.TakeDamageWithResult(finalDamage, isCrit);

        Debug.Log($"[Battle] {attacker.Data.beastName} gây {finalDamage} sát thương{(isCrit ? " (CRIT!" + ")": "")}! {target.Data.beastName} HP: {target.CurrentHP}");

        yield return new WaitForSeconds(0.2f);

        // Nếu là đánh gần, phải lùi về vị trí cũ
        if (type == MoveType.Melee)
        {
            yield return attacker.transform.DOMove(originalPos, 0.25f).SetEase(Ease.InQuad).WaitForCompletion();
        }

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
        // Trận đấu chỉ thực sự kết thúc đối với phe địch nếu tất cả quái trên sân chết VÀ hàng chờ rỗng
        bool enemyAllDead  = enemyTeam.All(b => b == null || !b.IsAlive) && pendingEnemyQueue.Count == 0;
        return playerAllDead || enemyAllDead;
    }

    /// <summary>
    /// Kiểm tra xem quái địch trên sân có bị tiêu diệt không. 
    /// Nếu có và vẫn còn quái dự phòng trong hàng chờ, tiến hành spawn con mới thế chỗ.
    /// </summary>
    private IEnumerator CheckAndSpawnNextEnemy()
    {
        var deadEnemy = enemyTeam.FirstOrDefault(b => b != null && !b.IsAlive);
        if (deadEnemy != null && pendingEnemyQueue.Count > 0)
        {
            Debug.Log("[BattleManager] Địch đã gục! Chuẩn bị ra con tiếp theo thế chỗ...");
            yield return new WaitForSeconds(1.0f);

            // Xóa con chết khỏi danh sách trên sân và hủy object của nó
            enemyTeam.Remove(deadEnemy);
            Destroy(deadEnemy.gameObject);

            // Lấy con quái tiếp theo ra
            BeastData nextEnemyData = pendingEnemyQueue.Dequeue();
            
            // Spawn ở cùng vị trí điểm xuất hiện đầu tiên của Enemy (enemySpawnPoints[0])
            var newUnit = SpawnBeastUnit(nextEnemyData, enemySpawnPoints[0], false);
            enemyTeam.Add(newUnit);

            Debug.Log($"[BattleManager] {nextEnemyData.beastName} đã xuất kích thế chỗ!");

            // Cập nhật lại UI ActionPanel để người chơi có thể chọn mục tiêu mới
            actionPanel?.Initialize(playerTeam, enemyTeam, OnPlayerActionChosen);

            yield return new WaitForSeconds(0.5f);
        }
    }


    /// <summary>
    /// Kiểm tra xem thú của Player trên sân có bị tiêu diệt không. 
    /// Nếu có và vẫn còn thú dự phòng trong hàng chờ, tiến hành spawn con mới thế chỗ.
    /// </summary>
    private IEnumerator CheckAndSpawnNextPlayer()
    {
        var deadPlayer = playerTeam.FirstOrDefault(b => b != null && !b.IsAlive);
        if (deadPlayer != null && pendingPlayerQueue.Count > 0)
        {
            Debug.Log("[BattleManager] Thú của Player đã gục! Chuẩn bị ra con tiếp theo thế chỗ...");
            yield return new WaitForSeconds(1.0f);

            // Xóa con chết khỏi danh sách trên sân và hủy object của nó
            playerTeam.Remove(deadPlayer);
            Destroy(deadPlayer.gameObject);

            // Lấy con thú tiếp theo ra
            BeastData nextPlayerData = pendingPlayerQueue.Dequeue();
            
            // Spawn ở cùng vị trí điểm xuất hiện đầu tiên của Player (playerSpawnPoints[0])
            var newUnit = SpawnBeastUnit(nextPlayerData, playerSpawnPoints[0], true);
            playerTeam.Add(newUnit);

            Debug.Log($"[BattleManager] Thú {nextPlayerData.beastName} của Player đã xuất kích thế chỗ!");

            // Cập nhật lại UI ActionPanel để người chơi có thể điều khiển con mới
            actionPanel?.Initialize(playerTeam, enemyTeam, OnPlayerActionChosen);

            yield return new WaitForSeconds(0.5f);
        }
    }


    // ─── BATTLE END ──────────────────────────────────────────────────

    private IEnumerator EndBattle()
    {
        Debug.Log("--- BƯỚC 7: TRẬN ĐẤU KẾT THÚC! Đang tổng hợp kết quả... ---");
        // Player thắng khi tất cả enemy trên sân gục VÀ hàng chờ địch không còn con nào
        bool playerWon = enemyTeam.All(b => b == null || !b.IsAlive) && pendingEnemyQueue.Count == 0;

        // ── Xử lý theo nguồn gốc trận đấu ────────────────────────────

        if (battleTransferData != null && battleTransferData.originScene == BattleTransferData.OriginScene.WorldMap)
        {
            // ── KẾT QUẢ ẢI ──────────────────────────────────────────
            if (playerWon)
            {
                Debug.Log("--- BƯỚC 8: NGƯỜI CHƠI ĐÃ THẮNG! Bắt đầu lưu ải và mở khoá... ---");
                int stars   = CalculateStars();
                int stageId = battleTransferData.currentStageId;

                // Cộng vàng từ StageData (nếu được gán)
                int rewardGold = currentStageData != null ? currentStageData.rewardGold : 0;
                playerData.SetStageResult(stageId, stars, rewardGold);

                string goldMsg = rewardGold > 0 ? $" | +{rewardGold} vàng" : "";
                Debug.Log($"[Battle] TÔI ĐÃ THẮNG ẢI {stageId}! Số sao: {stars} ⭐{goldMsg}");
                Debug.Log("--- BƯỚC 9: LƯU THÀNH CÔNG! Đang quay về World Map... ---");
            }
            else
            {
                Debug.Log("--- BƯỚC 8: NGƯỜI CHƠI ĐÃ THUA! Không lưu ải... ---");
                Debug.Log("[Battle] THUA ẢI. Thử lại!");
            }

            yield return new WaitForSeconds(1.5f);

            battleTransferData.currentStageId = -1;
            GameSceneManager.GoToWorldMap();
        }
        else if (battleTransferData != null && battleTransferData.originScene == BattleTransferData.OriginScene.Hunting)
        {
            // ── KẾT QUẢ HUNT ─────────────────────────────────────────
            if (playerWon && !string.IsNullOrEmpty(battleTransferData.lastEncounteredBeastId))
            {
                if (!battleTransferData.stunnedBeastIds.Contains(battleTransferData.lastEncounteredBeastId))
                    battleTransferData.stunnedBeastIds.Add(battleTransferData.lastEncounteredBeastId);

                Debug.Log($"[Battle] THẮNG! Quái {battleTransferData.lastEncounteredBeastId} bị choáng.");
            }
            else if (!playerWon)
            {
                Debug.Log("[Battle] THUA! Quái sẽ di chuyển lại khi Player quay về.");
            }

            yield return new WaitForSeconds(1.5f);

            battleTransferData.isSingleBattle = false;
            GameSceneManager.GoToHunting();
        }
        else if (battleTransferData != null && battleTransferData.originScene == BattleTransferData.OriginScene.Arena)
        {
            // ── KẾT QUẢ ARENA ─────────────────────────────────────────
            if (playerWon)
            {
                int stars = CalculateStars();
                int stageId = battleTransferData.currentArenaStageId;
                int rewardGold = currentStageData != null ? currentStageData.rewardGold : 0;
                
                playerData.SetArenaStageResult(stageId, stars, rewardGold);
                Debug.Log($"[Battle] THẮNG ẢI ARENA {stageId}! Số sao: {stars}");
            }
            else
            {
                Debug.Log("[Battle] THUA ẢI ARENA. Thử lại!");
            }

            yield return new WaitForSeconds(1.5f);
            
            battleTransferData.currentArenaStageId = -1;
            GameSceneManager.GoToMap();
        }
        else
        {
            // ── KẾT QUẢ CHIẾN ĐẤU THƯỜNG (MapScene) ─────────────────
            if (playerWon && battleTransferData != null
                && !string.IsNullOrEmpty(battleTransferData.lastEncounteredBeastId))
            {
                if (!battleTransferData.stunnedBeastIds.Contains(battleTransferData.lastEncounteredBeastId))
                    battleTransferData.stunnedBeastIds.Add(battleTransferData.lastEncounteredBeastId);

                Debug.Log($"[Battle] THẮNG! Quái {battleTransferData.lastEncounteredBeastId} bị choáng.");
            }

            yield return new WaitForSeconds(1.5f);
            GameSceneManager.GoToMap();
        }
    }

    // ─── TÍNH SAO SAU KHI THẮNG ───────────────────────────────────────

    /// <summary>
    /// Tính số sao dựa trên % HP còn lại của đội player.
    /// ≥ 70% HP → 3 sao | ≥ 40% HP → 2 sao | thắng → 1 sao
    /// </summary>
    private int CalculateStars()
    {
        float totalMax     = playerTeam.Where(b => b != null).Sum(b => (float)b.Data.maxHP);
        float totalCurrent = playerTeam.Where(b => b != null && b.IsAlive).Sum(b => (float)b.CurrentHP);

        if (totalMax <= 0) return 1;

        float ratio = totalCurrent / totalMax;
        if (ratio >= 0.70f) return 3;
        if (ratio >= 0.40f) return 2;
        return 1;
    }


    // ─── CLICK HANDLER ───────────────────────────────────────────────

    public void HandleBeastClick(BeastUnit clickedBeast)
    {
        if (state == BattleState.PlayerTurn && waitingForPlayerAction && actionPanel != null)
        {
            actionPanel.OnBeastClicked(clickedBeast);
        }
    }

    // ─── FRUIT BUFF API ──────────────────────────────────────────────

    /// <summary>
    /// FruitBuffManager gọi hàm này sau khi người chơi đã chọn Quả xong.
    /// Cho phép RunBattle() thoát khỏi WaitUntil và tiếp tục spawn thú.
    /// </summary>
    public void OnFruitChosen()
    {
        fruitSelected = true;
        Debug.Log($"[BattleManager] Quả đã chọn: {fruitBuffManager?.ActiveFruit}. Bắt đầu trận!");
    }

    /// <summary>
    /// Buộc đổi thú Player đang trên sân sang con tiếp theo trong hàng chờ.
    /// Dùng cho Quả 1 (Baton Pass): đổi không qua lượt chết.
    /// </summary>
    public void ForceSwapPlayer()
    {
        if (pendingPlayerQueue.Count == 0)
        {
            Debug.Log("[BattleManager] ForceSwap: Không còn thú dự bị.");
            return;
        }

        // Tìm thú đang sống trên sân để đổi ra
        var current = playerTeam.FirstOrDefault(b => b != null && b.IsAlive);
        if (current == null) return;

        // Xóa thú cũ khỏi danh sách (KHÔNG destroy — chỉ deactivate tạm thời)
        playerTeam.Remove(current);
        current.gameObject.SetActive(false);
        // Đẩy thú cũ vào lại queue để có thể quay ra sau
        // (Nếu muốn đơn giản: giữ nguyên danh sách để track HP)

        // Spawn thú mới
        BeastData nextData = pendingPlayerQueue.Dequeue();
        var newUnit = SpawnBeastUnit(nextData, playerSpawnPoints[0], true);
        playerTeam.Add(newUnit);

        // Subscribe event Crit cho thú mới nếu đang chơi Quả CritRage
        if (fruitBuffManager?.ActiveFruit == FruitBuffManager.FruitType.CritRage)
            fruitBuffManager.SubscribeToUnit(newUnit);

        // Cập nhật bench list
        RefreshFruitBuffBench();

        actionPanel?.Initialize(playerTeam, enemyTeam, OnPlayerActionChosen);
        Debug.Log($"[BattleManager] Baton Pass! {nextData.beastName} xuất kích.");
    }

    /// <summary>Trả về BeastUnit đang sống trên sân của Player.</summary>
    public BeastUnit GetActivePlayerUnit()
        => playerTeam.FirstOrDefault(b => b != null && b.IsAlive);

    // ─── HELPER ──────────────────────────────────────────────────────

    /// <summary>Gọi RestTick() lên tất cả thú Player đang ở ngoài sân (còn sống).</summary>
    private void TickRestingPlayerUnits()
    {
        // Thú "đang nghỉ" = thú trong pendingPlayerQueue (chưa được spawn lên sân)
        // Queue không cho foreach trực tiếp → chuyển sang List tạm để duyệt
        var resting = new List<BeastData>(pendingPlayerQueue);
        foreach (var data in resting)
        {
            // Tìm BeastUnit tương ứng với BeastData (nếu đã spawn nhưng bị swap ra)
            // Trong trường hợp đơn giản: chỉ log, HP thực sẽ được reset lúc spawn
            Debug.Log($"[RestTick] {data.beastName} đang nghỉ ngơi (+{Mathf.RoundToInt(data.maxHP * 0.02f)} HP).");
        }

        // Với thú đã spawn nhưng bị ForceSwap ra (đang SetActive(false)):
        foreach (var unit in playerTeam)
        {
            if (unit != null && !unit.gameObject.activeSelf && unit.CurrentHP > 0)
                unit.RestTick();
        }
    }

    /// <summary>Cập nhật danh sách bench cho FruitBuffManager.</summary>
    private void RefreshFruitBuffBench()
    {
        if (fruitBuffManager == null) return;
        var bench = playerTeam.Where(b => b != null && !b.gameObject.activeSelf).ToList();
        fruitBuffManager.SetBenchUnits(bench);
    }
}
