using System.Collections;
using UnityEngine;

/// <summary>
/// Xu ly logic bat thu bang Pokeball trong tran chien.
/// - Quai bi choang 3 luot khi bi quang cau
/// - Sau 3 lan quang (thanh cong hay khong) -> quai tinh day va bo di
/// - Ti le bat = beast.captureRate * hpModifier
/// </summary>
public class BattleCaptureHandler : MonoBehaviour
{
    public static BattleCaptureHandler Instance { get; private set; }

    [Header("Config")]
    [Tooltip("Ten item Pokeball trong kho do Kinnly")]
    [SerializeField] private string pokeballItemName = "Pokeball";

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Text pokeballCountText;
    [SerializeField] private TMPro.TextMeshProUGUI pokeballCountTextTMP;

    private int throwsLeft = 3;
    private bool captureSessionActive = false;
    private BeastUnit targetEnemy;

    // Callback de bao BattleManager ket qua
    public System.Action<bool> OnCaptureSessionEnd; // true = da bat, false = thoat

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        RefreshPokeballUI();
    }

    /// <summary>Tra ve so Pokeball con lai tu kho do Kinnly.</summary>
    public int GetPokeballCount()
    {
        var playerData = FindFirstObjectByType<BattleManager>()?.GetComponent<MonoBehaviour>();
        // Tim trong savedInventoryItems
        var pd = Resources.FindObjectsOfTypeAll<PlayerData>();
        foreach (var data in pd)
        {
            foreach (var item in data.savedInventoryItems)
            {
                if (item.itemName == pokeballItemName)
                    return item.amount;
            }
        }
        return 0;
    }

    private void ConsumeOnePokeball()
    {
        var pd = Resources.FindObjectsOfTypeAll<PlayerData>();
        foreach (var data in pd)
        {
            for (int i = 0; i < data.savedInventoryItems.Count; i++)
            {
                var item = data.savedInventoryItems[i];
                if (item.itemName == pokeballItemName && item.amount > 0)
                {
                    var updated = item;
                    updated.amount--;
                    data.savedInventoryItems[i] = updated;
                    if (updated.amount <= 0)
                        data.savedInventoryItems.RemoveAt(i);
                    data.Save();
                    RefreshPokeballUI();
                    return;
                }
            }
        }
    }

    public void RefreshPokeballUI()
    {
        int count = GetPokeballCount();
        string display = $"x{count}";
        if (pokeballCountText != null) pokeballCountText.text = display;
        if (pokeballCountTextTMP != null) pokeballCountTextTMP.text = display;
    }

    /// <summary>
    /// Nguoi choi bam nut Pokeball. Bat dau hoac tiep tuc phien bat thu.
    /// </summary>
    public void OnPokeballButtonPressed()
    {
        if (BattleManager.Instance == null) return;

        // Chi cho phep khi den luot nguoi choi
        var enemy = BattleManager.Instance.GetActiveEnemyUnit();
        if (enemy == null || !enemy.IsAlive) return;

        if (!captureSessionActive)
        {
            // Bat dau phien moi
            targetEnemy = enemy;
            throwsLeft = 3;
            captureSessionActive = true;
            targetEnemy.ApplyStun();
            Debug.Log($"[Capture] Bat dau quang cau! Quai {targetEnemy.Data.baseBeast.beastName} bi choang 3 luot.");
        }

        // Tieu thu 1 Pokeball
        int count = GetPokeballCount();
        if (count <= 0)
        {
            Debug.Log("[Capture] Het Pokeball!");
            return;
        }

        ConsumeOnePokeball();
        throwsLeft--;

        // Tinh ti le bat
        float hpRatio = (float)targetEnemy.CurrentHP / targetEnemy.Data.MaxHP;
        float hpModifier = hpRatio < 0.25f ? 0.70f
                         : hpRatio < 0.50f ? 0.40f
                         : 0.15f;
        float finalRate = targetEnemy.Data.baseBeast.captureRate * hpModifier;

        float roll = UnityEngine.Random.value;
        Debug.Log($"[Capture] Ti le bat: {finalRate:P0} (captureRate={targetEnemy.Data.baseBeast.captureRate}, hpMod={hpModifier}). Roll: {roll:F2}");

        if (roll <= finalRate)
        {
            // BAT THANH CONG
            captureSessionActive = false;
            targetEnemy.ClearStatus();
            Debug.Log($"[Capture] BAT THANH CONG! {targetEnemy.Data.baseBeast.beastName} da duoc bat!");

            // Them vao kho thu cua nguoi choi
            var pd = Resources.FindObjectsOfTypeAll<PlayerData>();
            foreach (var data in pd)
            {
                if (data.currentFormation.Count > 0 || data.ownedBeasts.Count >= 0)
                {
                    data.AddBeast(targetEnemy.Data);
                    data.Save();
                    break;
                }
            }

            OnCaptureSessionEnd?.Invoke(true);
        }
        else
        {
            // THAT BAI
            Debug.Log($"[Capture] Quang cau that bai! Con {throwsLeft} luot.");

            if (throwsLeft <= 0)
            {
                // Het luot -> quai tinh day, bo di
                captureSessionActive = false;
                targetEnemy.ClearStatus();
                Debug.Log($"[Capture] {targetEnemy.Data.baseBeast.beastName} tinh day va bo di!");
                OnCaptureSessionEnd?.Invoke(false);
            }
        }
    }

    /// <summary>Huy phien bat (goi khi ket thuc tran).</summary>
    public void CancelSession()
    {
        if (captureSessionActive && targetEnemy != null)
            targetEnemy.ClearStatus(BeastUnit.StatusEffect.Stunned);
        captureSessionActive = false;
    }

    public bool IsCaptureSessionActive => captureSessionActive;
    public int ThrowsLeft => throwsLeft;
}
