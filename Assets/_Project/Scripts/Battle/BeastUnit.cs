using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Component đại diện cho một Beast trên sân chiến đấu.
/// Gắn vào prefab Beast Unit trong Battle Scene.
/// </summary>
public class BeastUnit : MonoBehaviour
{
    [Header("Dữ liệu")]
    public RuntimeBeastData Data { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image spriteImage;
    [SerializeField] private HPBarUI hpBar;
    [SerializeField] private RageBarUI rageBar;  // Thanh Nộ (gán trong Inspector)
    [SerializeField] private ExpBarUI expBar;    // Thanh EXP (gán trong Inspector)
    [SerializeField] private TextMeshProUGUI nameTextTMP;
    [SerializeField] private Text nameTextLegacy;

    // ─── Runtime Stats ────────────────────────────────────────────────
    public int CurrentHP  { get; private set; }
    public bool IsAlive   => CurrentHP > 0;
    public bool IsPlayerTeam { get; private set; }

    // ─── Status Effects ──────────────────────────────────────────────
    public enum StatusEffect { None, Poisoned, Paralyzed, Stunned, Burned }
    public StatusEffect CurrentStatus { get; private set; } = StatusEffect.None;


    [Header("Status Effect UI (optional)")]
    [SerializeField] private StatusEffectUI statusEffectUI;

    [SerializeField] private TextMeshProUGUI levelTextTMP;
    [SerializeField] private Text levelTextLegacy;

    public void SetExternalUI(TextMeshProUGUI extNameTextTMP, Text extNameTextLegacy, HPBarUI extHpBar, ExpBarUI extExpBar = null, TextMeshProUGUI extLevelTextTMP = null, Text extLevelTextLegacy = null, StatusEffectUI extStatusUI = null)
    {
        if (extNameTextTMP != null) this.nameTextTMP = extNameTextTMP;
        if (extNameTextLegacy != null) this.nameTextLegacy = extNameTextLegacy;
        if (extHpBar != null) this.hpBar = extHpBar;
        if (extExpBar != null) this.expBar = extExpBar;
        if (extLevelTextTMP != null) this.levelTextTMP = extLevelTextTMP;
        if (extLevelTextLegacy != null) this.levelTextLegacy = extLevelTextLegacy;
        if (extStatusUI != null) this.statusEffectUI = extStatusUI;
    }

    // ─── Crit ────────────────────────────────────────────────────────
    /// <summary>Tỉ lệ chí mạng (0.0 → 1.0). Mặc định 20%.</summary>
    [Header("Crit & Rage")]
    [Range(0f, 1f)] public float CritChance     = 0.20f;
    /// <summary>Hệ số nhân sát thương khi chí mạng.</summary>
    public float CritMultiplier = 1.5f;

    // ─── Rage ────────────────────────────────────────────────────────
    public int MaxRage     = 100;
    public int CurrentRage { get; private set; } = 0;

    // ─── Events ──────────────────────────────────────────────────────
    /// <summary>Bắn ra khi gây chí mạng. FruitBuffManager lắng nghe để đếm.</summary>
    public event Action OnCritLanded;
    /// <summary>Bắn ra khi CurrentRage thay đổi. RageBarUI lắng nghe để cập nhật.</summary>
    public event Action<int, int> OnRageChanged; // (currentRage, maxRage)

    private SpriteRenderer sr;

    public void Initialize(RuntimeBeastData data, bool isPlayerTeam)
    {
        Data = data;
        IsPlayerTeam = isPlayerTeam;
        if (data.currentHP >= 0)
        {
            CurrentHP = data.currentHP;
        }
        else
        {
            CurrentHP = data.MaxHP;
            data.currentHP = CurrentHP;
        }


        // Hiển thị sprite: Player dùng backSprite (nhìn về phía địch), Enemy dùng frontSprite
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = isPlayerTeam ? (data.baseBeast.backSprite != null ? data.baseBeast.backSprite : data.baseBeast.frontSprite)
                                     : data.baseBeast.frontSprite;
            // Đã xóa sr.flipX = true; để tôn trọng hướng gốc của ảnh do user vẽ
        }

        // Tự động gán Animator Controller hoạt ảnh của thú nếu có
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            if (data.baseBeast.animatorController != null)
            {
                anim.runtimeAnimatorController = data.baseBeast.animatorController;
                anim.enabled = true;
            }
            else
            {
                anim.enabled = false;
            }
        }

        // UI
        if (spriteImage != null)
        {
            spriteImage.sprite = isPlayerTeam
                ? (data.baseBeast.backSprite != null ? data.baseBeast.backSprite : data.baseBeast.frontSprite)
                : data.baseBeast.frontSprite;
        }

        if (nameTextTMP != null) nameTextTMP.text = data.baseBeast.beastName;
        if (nameTextLegacy != null) nameTextLegacy.text = data.baseBeast.beastName;

        if (levelTextTMP != null) levelTextTMP.text = $"Lv.{data.currentLevel}";
        if (levelTextLegacy != null) levelTextLegacy.text = $"Lv.{data.currentLevel}";
        
        hpBar?.Initialize(data.MaxHP);
        hpBar?.UpdateHP(CurrentHP);

        // Reset Rage về 0 mỗi khi thú được khởi tạo vào sân
        CurrentRage = 0;
        rageBar?.Initialize(MaxRage);

        // Khởi tạo thanh EXP tĩnh (chỉ hiển thị tiến độ của Level hiện tại)
        UpdateExpBar();

        // Reset trạng thái khi vừa lên sân
        CurrentStatus = StatusEffect.None;
        statusEffectUI?.Hide();
    }

    public void UpdateExpBar()
    {
        if (Data != null && expBar != null)
        {
            expBar.Initialize(Data.GetExpToNextLevel(), Data.currentExp, Data.currentLevel);
        }
    }

    public void UpdateLevelText(int level)
    {
        string lvlStr = $"Lv.{level}";
        if (levelTextTMP != null) levelTextTMP.text = lvlStr;
        if (levelTextLegacy != null) levelTextLegacy.text = lvlStr;

        GameObject hud = GameObject.Find("PlayerBattleHud");
        if (hud != null)
        {
            var tmps = hud.GetComponentsInChildren<TMP_Text>(true);
            var texts = hud.GetComponentsInChildren<Text>(true);
            foreach (var t in tmps)
            {
                if (t.name.ToLower().Contains("level") || t.text.StartsWith("Lv.")) t.text = lvlStr;
            }
            foreach (var t in texts)
            {
                if (t.name.ToLower().Contains("level") || t.text.StartsWith("Lv.")) t.text = lvlStr;
            }
        }
    }

    /// <summary>
    /// Nhận sát thương. Tự roll xúc xắc Crit nếu đây là đòn của Player.
    /// Trả về true nếu Beast chết sau đòn này.
    /// </summary>
    /// <param name="damage">Sát thương gốc (trước khi nhân Crit).</param>
    /// <param name="rollCrit">Nếu true, hàm tự tính xác suất chí mạng dựa trên CritChance của attacker.</param>
    /// <param name="attackerCritChance">CritChance của kẻ tấn công. Chỉ dùng khi rollCrit = true.</param>
    public bool TakeDamage(int damage, bool rollCrit = false, float attackerCritChance = 0f)
    {
        if (!IsAlive) return false;

        // ── Roll Crit ────────────────────────────────────────────────
        bool isCritical = false;
        if (rollCrit && UnityEngine.Random.value < attackerCritChance)
        {
            isCritical = true;
            // Crit sẽ được nhân ở phía attacker trước khi truyền vào đây
            // (BattleManager tính damage rồi mới gọi TakeDamage)
        }

        damage = Mathf.Max(1, damage);
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        if (Data != null) Data.currentHP = CurrentHP;

        // Cập nhật HP bar
        hpBar?.UpdateHP(CurrentHP);

        // Spawn damage popup
        DamagePopup.Create(transform.position + Vector3.up * 0.5f, damage, isCritical);

        // Hit animation (rung lắc)
        transform.DOShakePosition(0.3f, strength: new Vector3(0.15f, 0f, 0f), vibrato: 8)
                 .SetEase(Ease.OutQuad);

        if (CurrentHP <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Nhận sát thương từ bên ngoài đã tính Crit sẵn.
    /// Dùng khi BattleManager tự roll Crit rồi truyền kết quả vào.
    /// </summary>
    public bool TakeDamageWithResult(int finalDamage, bool isCritical)
    {
        if (!IsAlive) return false;

        finalDamage = Mathf.Max(1, finalDamage);
        CurrentHP   = Mathf.Max(0, CurrentHP - finalDamage);
        if (Data != null) Data.currentHP = CurrentHP;

        hpBar?.UpdateHP(CurrentHP);
        DamagePopup.Create(transform.position + Vector3.up * 0.5f, finalDamage, isCritical);
        transform.DOShakePosition(0.3f, strength: new Vector3(0.15f, 0f, 0f), vibrato: 8)
                 .SetEase(Ease.OutQuad);

        // Hiệu ứng nhấp nháy Pokémon chớp tắt
        StartCoroutine(FlashPokemonBlink());

        if (isCritical)

            OnCritLanded?.Invoke(); // Thông báo để FruitBuffManager đếm

        if (CurrentHP <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    private System.Collections.IEnumerator FlashPokemonBlink()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(0.06f);
            sr.enabled = true;
            yield return new WaitForSeconds(0.06f);
        }
    }


    private void Die()
    {
        // Fade out và deactivate
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
            r.DOFade(0f, 0.5f);

        if (spriteImage != null)
            spriteImage.DOFade(0f, 0.5f);

        DOVirtual.DelayedCall(0.6f, () => gameObject.SetActive(false));
    }

    // ─── Rage ────────────────────────────────────────────────────────

    /// <summary>Cộng thêm Nộ. Tự clamp trong khoảng [0, MaxRage].</summary>
    public void AddRage(int amount)
    {
        CurrentRage = Mathf.Clamp(CurrentRage + amount, 0, MaxRage);
        rageBar?.UpdateRage(CurrentRage);
        OnRageChanged?.Invoke(CurrentRage, MaxRage);
    }

    /// <summary>Reset Rage về 0 (dùng sau khi xả chiêu cuối).</summary>
    public void ResetRage()
    {
        CurrentRage = 0;
        rageBar?.UpdateRage(0);
        OnRageChanged?.Invoke(0, MaxRage);
    }

    /// <summary>
    /// Hồi 2% MaxHP mỗi lượt khi thú đang ở ngoài sân (đang nghỉ).
    /// Được BattleManager gọi cuối mỗi lượt địch.
    /// </summary>
    public void RestTick()
    {
        if (CurrentHP <= 0) return; // Thú đã chết thì không hồi
        int healAmount = Mathf.Max(1, Mathf.RoundToInt(Data.MaxHP * 0.02f));
        CurrentHP = Mathf.Min(Data.MaxHP, CurrentHP + healAmount);
        if (Data != null) Data.currentHP = CurrentHP;
        hpBar?.UpdateHP(CurrentHP);
        Debug.Log($"[RestTick] {Data.baseBeast.beastName} nghỉ ngơi, hồi {healAmount} HP. HP hiện tại: {CurrentHP}/{Data.MaxHP}");
    }

    // ─── Status Effect API ───────────────────────────────────────────

    /// <summary>Áp trạng thái xấu lên thú này (nếu chưa có trạng thái).</summary>
    public void ApplyStatus(StatusEffect status)
    {
        if (CurrentStatus != StatusEffect.None) return; // Không chồng trạng thái
        CurrentStatus = status;
        statusEffectUI?.Show(status);
        Debug.Log($"[Status] {Data.baseBeast.beastName} bị {status}!");
    }

    /// <summary>Xóa 1 trạng thái xấu cụ thể (hoặc tất cả nếu status = None).</summary>
    public void ClearStatus(StatusEffect status = StatusEffect.None)
    {
        if (status == StatusEffect.None || CurrentStatus == status)
        {
            CurrentStatus = StatusEffect.None;
            statusEffectUI?.Hide();
            Debug.Log($"[Status] {Data.baseBeast.beastName} đã khỏi trạng thái xấu.");
        }
    }

    /// <summary>Áp trạng thái Choáng (Stun). Không override trạng thái khác.</summary>
    public void ApplyStun()
    {
        // Stun là trạng thái tạm, ghi đè lên trạng thái None để biết đang bị choáng
        CurrentStatus = StatusEffect.Stunned;
        statusEffectUI?.Show(StatusEffect.Stunned);
        Debug.Log($"[Status] {Data.baseBeast.beastName} bị Choáng!");
    }

    /// <summary>Xử lý hiệu ứng trạng thái ở đầu lượt. Trả về true nếu thú bị bỏ lượt.</summary>
    public bool ProcessStatusEffectTick()
    {
        switch (CurrentStatus)
        {
            case StatusEffect.Burned:
                int burnDmg = Mathf.Max(1, Mathf.RoundToInt(Data.MaxHP * 0.10f));
                CurrentHP = Mathf.Max(0, CurrentHP - burnDmg);
                if (Data != null) Data.currentHP = CurrentHP;
                hpBar?.UpdateHP(CurrentHP);
                DamagePopup.Create(transform.position + Vector3.up * 0.5f, burnDmg, false);
                Debug.Log($"[Status] {Data.baseBeast.beastName} mất {burnDmg} HP vì bị thiêu đốt!");
                if (CurrentHP <= 0) Die();
                return false; // Thiêu đốt không bỏ lượt

            case StatusEffect.Poisoned:
                int poisonDmg = Mathf.Max(1, Mathf.RoundToInt(Data.MaxHP * 0.08f));
                CurrentHP = Mathf.Max(0, CurrentHP - poisonDmg);
                if (Data != null) Data.currentHP = CurrentHP;
                hpBar?.UpdateHP(CurrentHP);
                DamagePopup.Create(transform.position + Vector3.up * 0.5f, poisonDmg, false);
                Debug.Log($"[Status] {Data.baseBeast.beastName} mất {poisonDmg} HP vì độc.");
                if (CurrentHP <= 0) Die();
                return false; // Độc không bỏ lượt


            case StatusEffect.Paralyzed:
                bool skipTurn = UnityEngine.Random.value < 0.3f; // 30% bỏ lượt
                if (skipTurn) Debug.Log($"[Status] {Data.baseBeast.beastName} bị tê liệt, bỏ lượt!");
                return skipTurn;

            case StatusEffect.Stunned:
                // Stun được quản lý bởi BattleCaptureHandler (đếm số lượt)
                return true; // Luôn bỏ lượt khi bị choáng

            default:
                return false;
        }
    }

    /// <summary>
    /// Hồi máu cho thú. Dùng khi sử dụng chiêu thức buff / hồi máu (MoveType.Self).
    /// </summary>
    public void Heal(int amount)
    {
        if (CurrentHP <= 0) return;
        amount = Mathf.Max(1, amount); // Đảm bảo luôn hồi ít nhất 1 máu
        CurrentHP = Mathf.Min(Data.MaxHP, CurrentHP + amount);
        if (Data != null) Data.currentHP = CurrentHP;
        hpBar?.UpdateHP(CurrentHP);

        DamagePopup.Create(transform.position + Vector3.up * 0.5f, amount, false, true);

        Debug.Log($"[Heal] {Data.baseBeast.beastName} tự hồi {amount} HP. HP hiện tại: {CurrentHP}/{Data.MaxHP}");
    }

    /// <summary>
    /// Hồi 50% MaxHP cho bản thân (hiệu ứng Quả 3 - Team Heal).
    /// </summary>
    public void TeamHeal()
    {
        if (CurrentHP <= 0) return; // Thú đã chết thì không hồi
        int healAmount = Mathf.RoundToInt(Data.MaxHP * 0.5f);
        CurrentHP = Mathf.Min(Data.MaxHP, CurrentHP + healAmount);
        if (Data != null) Data.currentHP = CurrentHP;
        hpBar?.UpdateHP(CurrentHP);
        Debug.Log($"[TeamHeal] {Data.baseBeast.beastName} được hồi {healAmount} HP. HP hiện tại: {CurrentHP}/{Data.MaxHP}");
    }

    /// <summary>Tính sát thương gây ra cho target theo chiêu thức.</summary>
    public int CalculateDamage(BeastUnit target, RuntimeMoveData move, out string effectivenessMsg)
    {
        effectivenessMsg = "";
        int raw = Mathf.RoundToInt(Data.Attack * move.power / 50f) - target.Data.Defense;
        raw = Mathf.Max(1, raw);

        // Áp dụng hệ số khắc hệ
        var typeChart = Resources.Load<ElementTypeChart>("ElementTypeChart");
        if (typeChart == null) typeChart = Resources.Load<ElementTypeChart>("ElementTypeChart/ElementTypeChart");

        if (typeChart != null)
        {
            float multiplier = typeChart.GetMultiplier(move.baseMove.moveElement, target.Data.baseBeast.element);
            raw = Mathf.RoundToInt(raw * multiplier);
            
            if (multiplier > 1.1f) effectivenessMsg = $"Đòn đánh SIÊU HIỆU QUẢ! (x{multiplier})";
            else if (multiplier < 0.9f) effectivenessMsg = $"Đòn đánh KHÔNG HIỆU QUẢ LẮM... (x{multiplier})";

        }

        // Nếu bản thân kẻ tấn công bị Thiêu Đốt (Burned) -> Giảm 25% sát thương gây ra
        if (CurrentStatus == StatusEffect.Burned)
        {
            raw = Mathf.RoundToInt(raw * 0.75f);
            Debug.Log($"[Burned] Sát thương của {Data.baseBeast.beastName} bị giảm 25% vì đang bị Thiêu Đốt!");
        }

        return Mathf.Max(1, raw);
    }


    public int CalculateDamage(BeastUnit target, RuntimeMoveData move)
    {
        return CalculateDamage(target, move, out _);
    }


    /// <summary>Tính sát thương tấn công thường (không dùng chiêu).</summary>
    public int CalculateBaseDamage(BeastUnit target)
    {
        int raw = Data.Attack - target.Data.Defense;
        raw = Mathf.Max(50, raw); // Tăng damage lên tối thiểu 50 để đánh nhanh thắng nhanh (test luồng)

        var typeChart = Resources.Load<ElementTypeChart>("ElementTypeChart");
        if (typeChart != null)
        {
            // Tấn công thường mặc định hệ Normal
            float multiplier = typeChart.GetMultiplier(BeastElement.Normal, target.Data.baseBeast.element);
            raw = Mathf.RoundToInt(raw * multiplier);
        }

        return Mathf.Max(1, raw);
    }

    private void OnMouseDown()
    {
        // Khi người chơi nhấp chuột vào Sprite của thú trên sân
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.HandleBeastClick(this);
        }
    }
}
