using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý UI chọn Quả Buff đầu trận.
/// Hiện Panel 3 nút → Player chọn 1 → ẩn Panel → báo BattleManager tiếp tục.
///
/// 3 loại Quả:
///   BatonPass  (0) - Đánh xong tự đổi sang thú dự bị.
///   CritRage   (1) - Crit đủ 5 lần → hồi đầy Nộ.
///   TeamHeal   (2) - Hồi 50% MaxHP cho toàn đội ngay khi chọn.
/// </summary>
public class FruitBuffManager : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────────
    public static FruitBuffManager Instance { get; private set; }

    // ─── Inspector ───────────────────────────────────────────────────
    [Header("UI References")]
    [SerializeField] private GameObject fruitPanel;         // Panel chứa 3 nút
    [SerializeField] private Button     btnBatonPass;       // Nút Quả 1
    [SerializeField] private Button     btnCritRage;        // Nút Quả 2
    [SerializeField] private Button     btnTeamHeal;        // Nút Quả 3

    [Header("Data")]
    [SerializeField] private PlayerData playerData;         // Để đọc currentFormation khi TeamHeal

    // ─── Runtime ─────────────────────────────────────────────────────
    public enum FruitType { BatonPass, CritRage, TeamHeal, None }
    public FruitType ActiveFruit { get; private set; } = FruitType.None;

    private int  critCount       = 0;   // Số lần Crit đã gây trong trận (dùng cho CritRage)
    private bool batonPassActive = false;

    // Danh sách tất cả BeastUnit của Player đang không ở sân (để gọi TeamHeal)
    // BattleManager sẽ inject danh sách này qua SetBenchUnits()
    private List<BeastUnit> benchUnits = new List<BeastUnit>();

    // ─── Unity Lifecycle ─────────────────────────────────────────────

    private void Awake()
    {
        Instance = this;

        // Gắn sự kiện cho 3 nút
        btnBatonPass?.onClick.AddListener(() => OnFruitChosen(FruitType.BatonPass));
        btnCritRage?.onClick .AddListener(() => OnFruitChosen(FruitType.CritRage));
        btnTeamHeal?.onClick .AddListener(() => OnFruitChosen(FruitType.TeamHeal));
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>
    /// BattleManager gọi hàm này để hiện Panel chọn Quả đầu trận.
    /// </summary>
    public void Show()
    {
        // Reset trạng thái từ trận trước
        ActiveFruit    = FruitType.None;
        critCount      = 0;
        batonPassActive = false;

        if (fruitPanel != null) fruitPanel.SetActive(true);
    }

    /// <summary>
    /// Ẩn Panel (tự gọi sau khi player đã chọn).
    /// </summary>
    public void Hide()
    {
        if (fruitPanel != null) fruitPanel.SetActive(false);
    }

    /// <summary>
    /// BattleManager inject danh sách thú đang nghỉ (bench) để FruitBuffManager
    /// biết cần TeamHeal những con nào.
    /// </summary>
    public void SetBenchUnits(List<BeastUnit> bench)
    {
        benchUnits = bench ?? new List<BeastUnit>();
    }

    // ─── Sự kiện BattleManager gọi ───────────────────────────────────

    /// <summary>
    /// Gọi sau mỗi khi Player đánh xong 1 lượt (dùng cho Baton Pass).
    /// BattleManager gọi hàm này trong ExecuteAttack().
    /// </summary>
    public void OnPlayerAttackFinished()
    {
        if (ActiveFruit != FruitType.BatonPass) return;
        if (!batonPassActive) return;

        // Yêu cầu BattleManager đổi thú (nếu còn thú dự bị)
        if (BattleManager.Instance != null)
            BattleManager.Instance.ForceSwapPlayer();
    }

    /// <summary>
    /// BeastUnit gọi hàm này qua event OnCritLanded khi gây chí mạng.
    /// </summary>
    public void OnCritRegistered(BeastUnit attacker)
    {
        if (ActiveFruit != FruitType.CritRage) return;

        critCount++;
        Debug.Log($"[FruitBuff] Crit #{critCount}/5 bởi {attacker.Data.beastName}");

        if (critCount >= 5)
        {
            attacker.AddRage(attacker.MaxRage); // Hồi đầy Nộ
            critCount = 0;                       // Reset để có thể kích hoạt lại
            Debug.Log($"[FruitBuff] RAGE FULL! {attacker.Data.beastName} đã đầy Nộ.");
        }
    }

    // ─── Subscribe / Unsubscribe Crit Event ──────────────────────────

    /// <summary>
    /// Subscribe vào event OnCritLanded của BeastUnit đang trên sân.
    /// BattleManager gọi mỗi khi spawn thú Player mới lên sân.
    /// </summary>
    public void SubscribeToUnit(BeastUnit unit)
    {
        if (unit == null) return;
        unit.OnCritLanded += () => OnCritRegistered(unit);
    }

    // ─── Private ─────────────────────────────────────────────────────

    private void OnFruitChosen(FruitType fruit)
    {
        ActiveFruit = fruit;
        Debug.Log($"[FruitBuff] Đã chọn: {fruit}");

        switch (fruit)
        {
            case FruitType.BatonPass:
                batonPassActive = true;
                // Hiệu ứng sẽ kích hoạt mỗi khi Player tấn công xong
                break;

            case FruitType.CritRage:
                critCount = 0;
                // BattleManager sẽ subscribe OnCritLanded sau khi spawn thú
                break;

            case FruitType.TeamHeal:
                ApplyTeamHeal();
                break;
        }

        Hide();
        // Báo BattleManager tiếp tục luồng trận đấu
        BattleManager.Instance?.OnFruitChosen();
    }

    private void ApplyTeamHeal()
    {
        // Hồi 50% MaxHP cho thú đang trên sân (nếu có)
        // BattleManager sẽ gọi trực tiếp TeamHeal() lên active unit
        // Ở đây chúng ta hồi cho các thú đang ở bench
        foreach (var unit in benchUnits)
        {
            if (unit != null)
                unit.TeamHeal();
        }
        Debug.Log($"[FruitBuff] Team Heal áp dụng cho {benchUnits.Count} thú đang nghỉ.");

        // Thú đang trên sân sẽ được hồi bởi BattleManager.OnFruitChosen()
    }
}
