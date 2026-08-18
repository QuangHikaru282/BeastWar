using System.Collections;
using UnityEngine;

/// <summary>
/// Xử lý logic ném bóng bắt thú trong trận chiến.
/// - Không cần kiểm tra số lượng bóng trong túi (ném bóng tự do).
/// - Mỗi lần ném quái sẽ có tỉ lệ bắt dính theo lượng máu.
/// - Mỗi phiên cho phép ném tối đa 3 lần, nếu cả 3 lần trượt thì quái bỏ chạy.
/// </summary>
public class BattleCaptureHandler : MonoBehaviour
{
    public static BattleCaptureHandler Instance { get; private set; }

    [Header("UI (Tùy chọn)")]
    [SerializeField] private UnityEngine.UI.Text pokeballCountText;
    [SerializeField] private TMPro.TextMeshProUGUI pokeballCountTextTMP;

    private int throwsLeft = 3;
    private bool captureSessionActive = false;
    private BeastUnit targetEnemy;

    // Callback để báo BattleManager kết quả
    public System.Action<bool> OnCaptureSessionEnd; // true = đã bắt, false = thoát

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        RefreshPokeballUI();
    }

    /// <summary>
    /// Cập nhật text hiển thị.
    /// </summary>
    public void RefreshPokeballUI()
    {
        string display = captureSessionActive ? $"{throwsLeft}/3" : "∞";
        if (pokeballCountText != null) pokeballCountText.text = display;
        if (pokeballCountTextTMP != null) pokeballCountTextTMP.text = display;
    }

    /// <summary>
    /// Người chơi bấm nút Pokeball -> Quăng bóng bắt thú ngay lập tức!
    /// </summary>
    public void OnPokeballButtonPressed()
    {
        if (BattleManager.Instance == null) return;

        // Chỉ cho phép khi có quái địch hợp lệ
        var enemy = BattleManager.Instance.GetActiveEnemyUnit();
        if (enemy == null || !enemy.IsAlive) return;

        // Kiểm tra xem có phải trận đấu Trainer / Gym không
        BattleTransferData bData = Resources.Load<BattleTransferData>("BattleTransferData");
        if (bData != null && (bData.isTrainerBattle || bData.isGymLeaderBattle))
        {
            Debug.LogWarning("[Capture] Không thể bắt thú cưng của Trainer hoặc Gym Leader!");
            return;
        }

        if (!captureSessionActive)
        {
            // Bắt đầu phiên mới
            targetEnemy = enemy;
            throwsLeft = 3;
            captureSessionActive = true;
            targetEnemy.ApplyStun();
            Debug.Log($"[Capture] Bắt đầu quăng bóng! Quái {targetEnemy.Data.baseBeast.beastName} bị choáng.");
        }

        throwsLeft--;
        RefreshPokeballUI();

        // Tính tỉ lệ bắt dựa trên lượng máu còn lại của quái
        float hpRatio = (float)targetEnemy.CurrentHP / targetEnemy.Data.MaxHP;
        float hpModifier = hpRatio < 0.25f ? 0.85f
                         : hpRatio < 0.50f ? 0.55f
                         : 0.30f;
        float baseRate = (targetEnemy.Data != null && targetEnemy.Data.baseBeast != null) ? targetEnemy.Data.baseBeast.captureRate : 0.5f;
        float finalRate = Mathf.Clamp01(baseRate * hpModifier + 0.15f);

        float roll = UnityEngine.Random.value;
        Debug.Log($"[Capture] Tỉ lệ bắt: {finalRate:P0} (Roll: {roll:F2}). Còn {throwsLeft} lần ném.");

        if (roll <= finalRate)
        {
            // BẮT THÀNH CÔNG
            captureSessionActive = false;
            targetEnemy.ClearStatus();
            Debug.Log($"[Capture] BẮT THÀNH CÔNG! {targetEnemy.Data.baseBeast.beastName} đã được thu phục!");

            // Thêm vào kho thú của người chơi
            PlayerData pd = global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null
                ? global::QuestManager.Instance.playerData
                : Resources.Load<PlayerData>("PlayerData");

            if (pd != null)
            {
                pd.AddBeast(targetEnemy.Data);
                pd.Save();
            }

            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnCaptureBeastSuccess();
            }

            OnCaptureSessionEnd?.Invoke(true);
            RefreshPokeballUI();
        }
        else
        {
            // THẤT BẠI
            Debug.Log($"[Capture] Quăng bóng thất bại! Còn {throwsLeft} lượt ném.");

            if (throwsLeft <= 0)
            {
                // Hết 3 lần ném -> quái tỉnh dậy và bỏ chạy
                captureSessionActive = false;
                targetEnemy.ClearStatus();
                Debug.Log($"[Capture] {targetEnemy.Data.baseBeast.beastName} tỉnh dậy và bỏ chạy mất!");

                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.OnWildBeastFled();
                }

                OnCaptureSessionEnd?.Invoke(false);
                RefreshPokeballUI();
            }
        }
    }

    /// <summary>Hủy phiên bắt (gọi khi kết thúc trận).</summary>
    public void CancelSession()
    {
        if (captureSessionActive && targetEnemy != null)
            targetEnemy.ClearStatus(BeastUnit.StatusEffect.Stunned);
        captureSessionActive = false;
        RefreshPokeballUI();
    }

    public bool IsCaptureSessionActive => captureSessionActive;
    public int ThrowsLeft => throwsLeft;
}
