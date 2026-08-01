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
    [SerializeField] private EnemyAI         enemyAI;
    [SerializeField] private StageData        currentStageData;   // Gán từ WorldMapData lúc load scene

    // ─── Runtime ─────────────────────────────────────────────────────

    public static BattleManager Instance { get; private set; }

    private List<BeastUnit> playerTeam = new List<BeastUnit>();
    private List<BeastUnit> enemyTeam  = new List<BeastUnit>();

    private BattleState state = BattleState.PreBattle;
    private bool        waitingForPlayerAction;
    private bool        fruitSelected = false;  // Co cho nguoi choi chon Qua
    private bool        playerFled = false;     // Co nguoi choi bo chay
    private bool        captureSuccess = false; // Co bat thu thanh cong
    private bool        wildBeastFled = false;  // Co quai hoang da bo di sau 3 lan quang
    private bool        itemUsedThisTurn = false; // Nguoi choi da dung do luot nay
    private BeastUnit   chosenAttacker;
    private BeastUnit   chosenTarget;
    private RuntimeMoveData    chosenMove;

    private void Start()
    {
        // ─── CHẾ ĐỘ TRAINER BATTLE ───
        if (battleTransferData != null && battleTransferData.isTrainerBattle)
        {
            Debug.Log("[Battle] Bắt đầu trận đấu với TRAINER! Không được phép Bắt thú hay Bỏ chạy.");
        }

        SetupUIHooks();
        StartCoroutine(RunBattle());
    }

    private IEnumerator RunBattle()
    {
        // --- PRE-BATTLE: Hiện Panel chọn Quả, đợi Player chọn xong ---
        state = BattleState.PreBattle;
        fruitSelected = false;
        if (BattleUIManager.Instance != null && BattleUIManager.Instance.FruitBuffManager != null)
        {
            BattleUIManager.Instance.ShowFruitBuffManager();
            yield return new WaitUntil(() => fruitSelected);
        }
        else
        {
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

    // ─── UI HOOKS ────────────────────────────────────────────────────

    private void SetupUIHooks()
    {
        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.SetupUIHooks();
    }

    // ─── INIT ────────────────────────────────────────────────────────

    // Hàng chờ quái địch dự phòng (đối với trận đấu ải WorldMap)
    private Queue<RuntimeBeastData> pendingEnemyQueue = new Queue<RuntimeBeastData>();
    // Hàng chờ thú của người chơi dự phòng
    private Queue<RuntimeBeastData> pendingPlayerQueue = new Queue<RuntimeBeastData>();

    private IEnumerator InitBattle()
    {
        Debug.Log("--- BƯỚC 1: BẮT ĐẦU KHỞI TẠO TRẬN ĐẤU ---");

        // Lấy đội hình Player
        var pFormation = playerData.currentFormation.Where(b => b != null && b.baseBeast != null).ToList();

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
                Debug.Log($"--- BƯỚC 2: ĐÃ TẠO QUÁI PHE MÌNH ({firstPlayer.baseBeast.beastName}) ---");
            }
        }

        // Spawn đội Enemy (Chế độ lần lượt thế chỗ)
        var eFormation = battleTransferData.wildEnemyTeam.Where(b => b != null && b.baseBeast != null).ToList();
        
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
            
            Debug.Log($"--- BƯỚC 3: ĐÃ TẠO QUÁI ĐỊCH ({firstEnemy.baseBeast.beastName}) ---");
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
        if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
        {
            // ActionPanel tự xử lý trạng thái hiển thị qua hàm Show/Initialize
        }

        if (BattleUIManager.Instance == null || BattleUIManager.Instance.ActionPanel == null)
        {
            Debug.LogError("--- LỖI NGHIÊM TRỌNG: KHÔNG TÌM THẤY BẢNG CHỌN CHIÊU THỨC (ActionPanel)! TRẬN ĐẤU SẼ BỊ KẸT! ---");
        }
        else
        {
            BattleUIManager.Instance.ActionPanel.Initialize(playerTeam, enemyTeam, OnPlayerActionChosen);
            Debug.Log("--- BƯỚC 4: ĐÃ TẢI BẢNG CHỌN CHIÊU THỨC (UI) ---");
        }

        // Inject danh sách thú bench cho FruitBuffManager biết để TeamHeal
        RefreshFruitBuffBench();

        // Nếu đã chọn Quả TeamHeal → hồi luôn cho thú đang trên sân
        if (BattleUIManager.Instance != null && BattleUIManager.Instance.FruitBuffManager != null && BattleUIManager.Instance.FruitBuffManager.ActiveFruit == FruitBuffManager.FruitType.TeamHeal)
        {
            foreach (var unit in playerTeam)
                unit?.TeamHeal();
        }

        // Subscribe event Crit nếu đã chọn Quả CritRage
        if (BattleUIManager.Instance != null && BattleUIManager.Instance.FruitBuffManager != null && BattleUIManager.Instance.FruitBuffManager.ActiveFruit == FruitBuffManager.FruitType.CritRage)
        {
            foreach (var unit in playerTeam)
                BattleUIManager.Instance.FruitBuffManager.SubscribeToUnit(unit);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private BeastUnit SpawnBeastUnit(RuntimeBeastData data, Transform spawnPoint, bool isPlayer)
    {
        GameObject go = beastUnitPrefab != null
            ? Instantiate(beastUnitPrefab, spawnPoint.position, Quaternion.identity)
            : new GameObject($"BeastUnit_{data.baseBeast.beastName}");

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

            // Tìm LevelText
            Transform levelTr = hudObj.transform.Find("LevelText");
            TextMeshProUGUI levelTxtTMP = levelTr != null ? levelTr.GetComponent<TextMeshProUGUI>() : null;
            UnityEngine.UI.Text levelTxtLegacy = levelTr != null ? levelTr.GetComponent<UnityEngine.UI.Text>() : null;

            // Tìm ExpBar
            ExpBarUI expBar = hudObj.GetComponentInChildren<ExpBarUI>();
            if (expBar == null)
            {
                // Thử tìm bất kỳ Slider nào có tên chứa EXP hoặc Exp, hoặc Slider thứ 2
                UnityEngine.UI.Slider[] sliders = hudObj.GetComponentsInChildren<UnityEngine.UI.Slider>(true);
                UnityEngine.UI.Slider targetExpSlider = null;
                foreach (var s in sliders)
                {
                    if (s.name.ToLower().Contains("exp") || s.gameObject.name.ToLower().Contains("exp"))
                    {
                        targetExpSlider = s;
                        break;
                    }
                }
                if (targetExpSlider == null && sliders.Length >= 2)
                {
                    targetExpSlider = sliders[1];
                }

                if (targetExpSlider != null)
                {
                    expBar = targetExpSlider.gameObject.AddComponent<ExpBarUI>();
                    var fExp = typeof(ExpBarUI).GetField("expSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (fExp != null) fExp.SetValue(expBar, targetExpSlider);

                    var tmp = targetExpSlider.GetComponentInChildren<TextMeshProUGUI>(true) ?? targetExpSlider.transform.parent.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (tmp != null)
                    {
                        var fTMP = typeof(ExpBarUI).GetField("expTextTMP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (fTMP != null) fTMP.SetValue(expBar, tmp);
                    }
                    var leg = targetExpSlider.GetComponentInChildren<UnityEngine.UI.Text>(true) ?? targetExpSlider.transform.parent.GetComponentInChildren<UnityEngine.UI.Text>(true);
                    if (leg != null)
                    {
                        var fLeg = typeof(ExpBarUI).GetField("expTextLegacy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (fLeg != null) fLeg.SetValue(expBar, leg);
                    }
                }
            }

            // Tìm StatusIcon (UI trạng thái xấu dưới tên nhân vật)
            StatusEffectUI statusUI = hudObj.GetComponentInChildren<StatusEffectUI>(true);


            unit.SetExternalUI(nameTxtTMP, nameTxtLegacy, hpBar, expBar, levelTxtTMP, levelTxtLegacy, statusUI);
            Debug.Log($"[BattleManager] Đã tự động link UI cho {data.baseBeast.beastName} từ {hudName}");
        }

        unit.Initialize(data, isPlayer);
        return unit;
    }

    // ─── PLAYER TURN ─────────────────────────────────────────────────

    private IEnumerator PlayerTurn()
    {
        var alive = playerTeam.Where(b => b != null && b.IsAlive).ToList();
        if (alive.Count == 0) yield break;

        // Xử lý hiệu ứng Độc / Tê liệt / Choáng ở đầu lượt Player
        foreach (var unit in alive)
        {
            if (unit != null && unit.IsAlive)
            {
                bool skip = unit.ProcessStatusEffectTick();
                if (skip) yield break; // Bị liệt/choáng thì bỏ lượt
            }
        }

        // Reset co dau luot

        playerFled = false;
        captureSuccess = false;
        wildBeastFled = false;
        itemUsedThisTurn = false;

        waitingForPlayerAction = true;
        itemUsedThisTurn = false;
        BattleUIManager.Instance?.ActionPanel?.Show(alive[0]);
        // Bat/tat nut icon hanh dong theo luot
        BattleActionIconsUI.Instance?.SetInteractable(true);
        Debug.Log("--- BUOC 5: DEN LUOT NGUOI CHOI (Dang cho ban chon chieu tren man hinh...) ---");

        // Cho player click chon thu minh -> click thu dich (hoac dung do)
        while (waitingForPlayerAction && !playerFled && !captureSuccess && !wildBeastFled)
            yield return null;

        BattleActionIconsUI.Instance?.SetInteractable(false);

        // Kiem tra cac truong hop dac biet
        if (playerFled)
        {
            state = BattleState.BattleEnd;
            ReturnToMap();
            yield break;
        }

        if (captureSuccess)
        {
            state = BattleState.BattleEnd;
            yield return new WaitForSeconds(1f);
            BattleUIManager.Instance?.ShowBattleReward(0, 0, ReturnToMap);
            yield break;
        }

        if (wildBeastFled)
        {
            state = BattleState.BattleEnd;
            yield return new WaitForSeconds(1f);
            ReturnToMap();
            yield break;
        }

        if (itemUsedThisTurn)
        {
            // Dung do tieu 1 luot -> khong tan cong, nhay thang sang luot dich
            yield break;
        }

        Debug.Log($"--- BƯỚC 6: BẠN ĐÃ CHỌN CHIÊU XONG! Đang tung đòn... ---");

            if (chosenAttacker != null && chosenTarget != null)
            {
                if (BattleActionExecutor.Instance != null)
                {
                    yield return StartCoroutine(BattleActionExecutor.Instance.ExecuteAttack(chosenAttacker, chosenTarget, chosenMove));
                }
                else 
                {
                    Debug.LogError("[BattleManager] Thiếu BattleActionExecutor trong Scene!");
                }
            }
            // Sau khi tấn công xong, báo FruitBuffManager (dùng cho Baton Pass)
        BattleUIManager.Instance?.FruitBuffManager?.OnPlayerAttackFinished();
    }

    private void OnPlayerActionChosen(BeastUnit attacker, BeastUnit target, RuntimeMoveData move, bool isCatch)
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

        // Xử lý hiệu ứng Độc / Tê liệt / Choáng ở đầu lượt Enemy
        var enemyAlive = enemyTeam.Where(b => b != null && b.IsAlive).ToList();
        foreach (var unit in enemyAlive)
        {
            if (unit != null && unit.IsAlive)
            {
                bool skip = unit.ProcessStatusEffectTick();
                if (skip) yield break; // Bị liệt/choáng bỏ lượt
            }
        }

        bool valid = enemyAI.ChooseAction(enemyTeam, playerTeam,

                                          out BeastUnit attacker,
                                          out BeastUnit target,
                                          out RuntimeMoveData move);

        if (!valid) yield break;

                if (BattleActionExecutor.Instance != null)
                {
                    yield return StartCoroutine(BattleActionExecutor.Instance.ExecuteAttack(attacker, target, move));
                }
                else 
                {
                    Debug.LogError("[BattleManager] Thiếu BattleActionExecutor trong Scene!");
                }
    }

    // Đã chuyển ExecuteAttack sang BattleActionExecutor.cs

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
            RuntimeBeastData nextEnemyData = pendingEnemyQueue.Dequeue();
            
            // Spawn ở cùng vị trí điểm xuất hiện đầu tiên của Enemy (enemySpawnPoints[0])
            var newUnit = SpawnBeastUnit(nextEnemyData, enemySpawnPoints[0], false);
            enemyTeam.Add(newUnit);

            Debug.Log($"[BattleManager] {nextEnemyData.baseBeast.beastName} đã xuất kích thế chỗ!");

            // Cập nhật lại UI ActionPanel để người chơi có thể chọn mục tiêu mới
            if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
            {
                BattleUIManager.Instance.ActionPanel.Initialize(playerTeam, enemyTeam, OnPlayerActionChosen);
            }

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
            RuntimeBeastData nextPlayerData = pendingPlayerQueue.Dequeue();
            
            // Spawn ở cùng vị trí điểm xuất hiện đầu tiên của Player (playerSpawnPoints[0])
            var newUnit = SpawnBeastUnit(nextPlayerData, playerSpawnPoints[0], true);
            playerTeam.Add(newUnit);

            Debug.Log($"[BattleManager] Thú {nextPlayerData.baseBeast.beastName} của Player đã xuất kích thế chỗ!");

            // Cập nhật lại UI ActionPanel để người chơi có thể điều khiển con mới
            if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
            {
                BattleUIManager.Instance.ActionPanel.Initialize(playerTeam, enemyTeam, OnPlayerActionChosen);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }


    // ─── BATTLE END ──────────────────────────────────────────────────

    private IEnumerator EndBattle()
    {
        Debug.Log("--- BƯỚC 7: TRẬN ĐẤU KẾT THÚC! Đang tổng hợp kết quả... ---");
        // Player thắng khi tất cả enemy trên sân gục VÀ hàng chờ địch không còn con nào
        bool playerWon = enemyTeam.All(b => b == null || !b.IsAlive) && pendingEnemyQueue.Count == 0;

        int totalExpToGive = 0;
        int totalGoldToGive = 0;

        if (playerWon)
        {
            Debug.Log("--- BƯỚC 8: NGƯỜI CHƠI ĐÃ THẮNG! Phân phát Kinh nghiệm (EXP)... ---");
            
            // Tính tổng Vàng và EXP từ các quái vật địch đã bị đánh bại
            totalGoldToGive = currentStageData != null ? currentStageData.rewardGold : 0; // Vàng cơ bản của màn chơi
            
            foreach (var b in enemyTeam)
            {
                if (b != null && b.Data != null)
                {
                    totalExpToGive += b.Data.baseBeast.rewardExp;
                    totalGoldToGive += b.Data.baseBeast.rewardGold;
                }
            }

            if (totalExpToGive == 0) totalExpToGive = 100; // Mặc định nếu chưa set

            // Hiển thị câu thông báo chiến thắng lên khung chữ
            if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
            {
                BattleUIManager.Instance.ActionPanel.SetGuide($"Chiến thắng! Nhận được {totalExpToGive} EXP và {totalGoldToGive} Vàng!");
            }

            // Chạy hiệu ứng thanh EXP tăng dần mượt mà trên UI
            if (playerTeam != null && playerTeam.Count > 0 && playerTeam[0] != null)
            {
                var unit = playerTeam[0];
                if (unit.Data != null)
                {
                    int startLvl = unit.Data.currentLevel;
                    int startExp = unit.Data.currentExp;

                    if (LevelUpManager.Instance != null)
                    {
                        LevelUpManager.Instance.DistributeExpToFormation(totalExpToGive, playerData);
                    }

                    ExpBarUI expBarUI = unit.GetComponent<ExpBarUI>() ?? unit.GetComponentInChildren<ExpBarUI>();
                    if (expBarUI == null && GameObject.Find("PlayerBattleHud") != null)
                    {
                        expBarUI = GameObject.Find("PlayerBattleHud").GetComponentInChildren<ExpBarUI>();
                    }

                    if (expBarUI != null)
                    {
                        yield return StartCoroutine(expBarUI.AnimateExpIncrease(startExp, totalExpToGive, startLvl, (lvl) => lvl * 100, (newLvl) => {
                            Vector3 spawnPos = unit.transform.position + Vector3.up * 1.5f;
                            DamagePopup.CreateText(spawnPos, "LEVEL UP!", Color.yellow, 4f);
                            unit.UpdateLevelText(newLvl);
                        }));
                    }
                    else
                    {
                        yield return new WaitForSeconds(1.0f);
                    }
                }
            }
            else
            {
                if (LevelUpManager.Instance != null)
                {
                    LevelUpManager.Instance.DistributeExpToFormation(totalExpToGive, playerData);
                }
            }

            // Cộng thêm Vàng khi thắng
            playerData.gold += totalGoldToGive;
            Debug.Log($"[Battle] Nhận được {totalGoldToGive} Vàng! Tổng vàng: {playerData.gold}");

            // Nếu đây là trận đánh quái hoang dã hoặc Trainer thì lưu trạng thái
            if (battleTransferData != null && battleTransferData.isTrainerBattle)
            {
                if (!string.IsNullOrEmpty(battleTransferData.lastEncounteredBeastId) && !playerData.defeatedTrainers.Contains(battleTransferData.lastEncounteredBeastId))
                {
                    playerData.defeatedTrainers.Add(battleTransferData.lastEncounteredBeastId);
                    Debug.Log($"Đã đánh bại Trainer: {battleTransferData.lastEncounteredBeastId}");
                    
                    if (global::QuestManager.Instance != null)
                    {
                        if (battleTransferData.lastEncounteredBeastId.Contains("Rival"))
                            global::QuestManager.Instance.OnRivalDefeated();
                        else if (battleTransferData.lastEncounteredBeastId.Contains("Boss"))
                            global::QuestManager.Instance.OnBossDefeated();
                        else
                            global::QuestManager.Instance.OnTrainerDefeated();
                    }
                }
            }
            else
            {
                if (battleTransferData != null && !string.IsNullOrEmpty(battleTransferData.lastEncounteredBeastId))
                {
                    if (!battleTransferData.stunnedBeastIds.Contains(battleTransferData.lastEncounteredBeastId))
                        battleTransferData.stunnedBeastIds.Add(battleTransferData.lastEncounteredBeastId);
                }

                if (global::QuestManager.Instance != null)
                {
                    global::QuestManager.Instance.OnWildBeastDefeated();
                }
            }
        }
        else
        {
            Debug.Log("--- BƯỚC 8: NGƯỜI CHƠI ĐÃ THUA! Các thú cần nghỉ ngơi. ---");
            // Sau này có thể thêm cơ chế trừ tiền hoặc quay về bệnh viện thú
        }

        playerData.Save();

        yield return new WaitForSeconds(1.5f);

        ReturnToMap();
    }

    private void ReturnToMap()
    {
        Debug.Log("--- BƯỚC 9: Quay về Bản Đồ! ---");

        string sceneToReturn = PlayerPrefs.GetString("SceneBeforeBattle", GameSceneManager.SCENE_MAP);

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(sceneToReturn);
        }
        else 
        {
            BattleActionIconsUI.LoadScenesSafely(sceneToReturn);
        }
    }



    // ─── CLICK HANDLER ───────────────────────────────────────────────

    public void HandleBeastClick(BeastUnit clickedBeast)
    {
        if (state == BattleState.PlayerTurn && waitingForPlayerAction && BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
        {
            BattleUIManager.Instance.ActionPanel.OnBeastClicked(clickedBeast);
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
        Debug.Log($"[BattleManager] Quả đã chọn. Bắt đầu trận!");
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
        RuntimeBeastData nextData = pendingPlayerQueue.Dequeue();
        var newUnit = SpawnBeastUnit(nextData, playerSpawnPoints[0], true);
        playerTeam.Add(newUnit);

        // Subscribe event Crit cho thú mới nếu đang chơi Quả CritRage
        if (BattleUIManager.Instance?.FruitBuffManager?.ActiveFruit == FruitBuffManager.FruitType.CritRage)
            BattleUIManager.Instance.FruitBuffManager.SubscribeToUnit(newUnit);

        // Cập nhật bench list
        RefreshFruitBuffBench();

        BattleUIManager.Instance?.ActionPanel?.Initialize(playerTeam, enemyTeam, OnPlayerActionChosen);
        Debug.Log($"[BattleManager] Baton Pass! {nextData.baseBeast.beastName} xuất kích.");
    }

    /// <summary>Tra ve BeastUnit dang song cua phe dich.</summary>
    public BeastUnit GetActiveEnemyUnit()
        => enemyTeam.FirstOrDefault(b => b != null && b.IsAlive);

    /// <summary>Tra ve BeastUnit dang song tren san cua Player.</summary>
    public BeastUnit GetActivePlayerUnit()
        => playerTeam.FirstOrDefault(b => b != null && b.IsAlive);

    // ─── SPECIAL ACTION API (goi boi BattleActionIconsUI) ────────────────────

    /// <summary>Nguoi choi chon bo chay (100% thanh cong).</summary>
    public void TryFlee()
    {
        playerFled = true;
        waitingForPlayerAction = false;
        state = BattleState.BattleEnd;
        Debug.Log("[BattleManager] Nguoi choi da bo chay!");
        ReturnToMap();
    }

    /// <summary>Bat thu hoang da thanh cong. Ket thuc tran.</summary>
    public void OnCaptureBeastSuccess()
    {
        captureSuccess = true;
        waitingForPlayerAction = false;
        Debug.Log("[BattleManager] Bat thu thanh cong! Tran ket thuc.");
    }

    /// <summary>Quai hoang da tinh day va bo di sau 3 lan quang. Ket thuc tran.</summary>
    public void OnWildBeastFled()
    {
        wildBeastFled = true;
        waitingForPlayerAction = false;
        Debug.Log("[BattleManager] Quai hoang da bo di!");
    }

    /// <summary>Nguoi choi dung do tieu 1 luot. Nhay sang luot dich.</summary>
    public void OnPlayerUsedItem()
    {
        if (state != BattleState.PlayerTurn) return;
        itemUsedThisTurn = true;
        waitingForPlayerAction = false;
        Debug.Log("[BattleManager] Nguoi choi dung do, tieu 1 luot.");
    }

    // ─── HELPER ──────────────────────────────────────────────────────

    /// <summary>Gọi RestTick() lên tất cả thú Player đang ở ngoài sân (còn sống).</summary>
    private void TickRestingPlayerUnits()
    {
        // Thú "đang nghỉ" = thú trong pendingPlayerQueue (chưa được spawn lên sân)
        // Queue không cho foreach trực tiếp → chuyển sang List tạm để duyệt
        var resting = new List<RuntimeBeastData>(pendingPlayerQueue);
        foreach (var data in resting)
        {
            // Tìm BeastUnit tương ứng với BeastData (nếu đã spawn nhưng bị swap ra)
            // Trong trường hợp đơn giản: chỉ log, HP thực sẽ được reset lúc spawn
            Debug.Log($"[RestTick] {data.baseBeast.beastName} đang nghỉ ngơi (+{Mathf.RoundToInt(data.MaxHP * 0.02f)} HP).");
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
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.RefreshFruitBuffBench(playerTeam);
        }
    }
}
