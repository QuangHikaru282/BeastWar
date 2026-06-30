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
    public BeastData Data { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image spriteImage;
    [SerializeField] private HPBarUI hpBar;
    [SerializeField] private RageBarUI rageBar;  // Thanh Nộ (gán trong Inspector)
    [SerializeField] private TextMeshProUGUI nameTextTMP;
    [SerializeField] private Text nameTextLegacy;

    // ─── Runtime Stats ────────────────────────────────────────────────
    public int CurrentHP  { get; private set; }
    public bool IsAlive   => CurrentHP > 0;
    public bool IsPlayerTeam { get; private set; }

    public void SetExternalUI(TextMeshProUGUI extNameTextTMP, Text extNameTextLegacy, HPBarUI extHpBar)
    {
        if (extNameTextTMP != null) this.nameTextTMP = extNameTextTMP;
        if (extNameTextLegacy != null) this.nameTextLegacy = extNameTextLegacy;
        if (extHpBar != null) this.hpBar = extHpBar;
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

    public void Initialize(BeastData data, bool isPlayerTeam)
    {
        Data = data;
        IsPlayerTeam = isPlayerTeam;
        CurrentHP = data.maxHP;


        // Hiển thị sprite: Player dùng backSprite (nhìn về phía địch), Enemy dùng frontSprite
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = isPlayerTeam ? (data.backSprite != null ? data.backSprite : data.frontSprite)
                                     : data.frontSprite;
            if (!isPlayerTeam) sr.flipX = true; // Địch nhìn về bên trái
        }

        // Tự động gán Animator Controller hoạt ảnh của thú nếu có
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            if (data.animatorController != null)
            {
                anim.runtimeAnimatorController = data.animatorController;
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
                ? (data.backSprite != null ? data.backSprite : data.frontSprite)
                : data.frontSprite;
        }

        if (nameTextTMP != null) nameTextTMP.text = data.beastName;
        if (nameTextLegacy != null) nameTextLegacy.text = data.beastName;
        
        hpBar?.Initialize(data.maxHP);

        // Reset Rage về 0 mỗi khi thú được khởi tạo vào sân
        CurrentRage = 0;
        rageBar?.Initialize(MaxRage);
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

        hpBar?.UpdateHP(CurrentHP);
        DamagePopup.Create(transform.position + Vector3.up * 0.5f, finalDamage, isCritical);
        transform.DOShakePosition(0.3f, strength: new Vector3(0.15f, 0f, 0f), vibrato: 8)
                 .SetEase(Ease.OutQuad);

        if (isCritical)
            OnCritLanded?.Invoke(); // Thông báo để FruitBuffManager đếm

        if (CurrentHP <= 0)
        {
            Die();
            return true;
        }
        return false;
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
        int healAmount = Mathf.Max(1, Mathf.RoundToInt(Data.maxHP * 0.02f));
        CurrentHP = Mathf.Min(Data.maxHP, CurrentHP + healAmount);
        hpBar?.UpdateHP(CurrentHP);
        Debug.Log($"[RestTick] {Data.beastName} nghỉ ngơi, hồi {healAmount} HP. HP hiện tại: {CurrentHP}/{Data.maxHP}");
    }

    /// <summary>
    /// Hồi máu cho thú. Dùng khi sử dụng chiêu thức buff / hồi máu (MoveType.Self).
    /// </summary>
    public void Heal(int amount)
    {
        if (CurrentHP <= 0) return;
        amount = Mathf.Max(1, amount); // Đảm bảo luôn hồi ít nhất 1 máu
        CurrentHP = Mathf.Min(Data.maxHP, CurrentHP + amount);
        hpBar?.UpdateHP(CurrentHP);

        DamagePopup.Create(transform.position + Vector3.up * 0.5f, amount, false, true);

        Debug.Log($"[Heal] {Data.beastName} tự hồi {amount} HP. HP hiện tại: {CurrentHP}/{Data.maxHP}");
    }

    /// <summary>
    /// Hồi 50% MaxHP cho bản thân (hiệu ứng Quả 3 - Team Heal).
    /// </summary>
    public void TeamHeal()
    {
        if (CurrentHP <= 0) return; // Thú đã chết thì không hồi
        int healAmount = Mathf.RoundToInt(Data.maxHP * 0.5f);
        CurrentHP = Mathf.Min(Data.maxHP, CurrentHP + healAmount);
        hpBar?.UpdateHP(CurrentHP);
        Debug.Log($"[TeamHeal] {Data.beastName} được hồi {healAmount} HP. HP hiện tại: {CurrentHP}/{Data.maxHP}");
    }

    /// <summary>Tính sát thương gây ra cho target theo chiêu thức.</summary>
    public int CalculateDamage(BeastUnit target, MoveData move)
    {
        // Công thức: damage = Max(1, attacker.attack * movePower / 50 - defender.defense)
        int raw = Mathf.RoundToInt(Data.attack * move.power / 50f) - target.Data.defense;
        return Mathf.Max(1, raw);
    }

    /// <summary>Tính sát thương tấn công thường (không dùng chiêu).</summary>
    public int CalculateBaseDamage(BeastUnit target)
    {
        int raw = Data.attack - target.Data.defense;
        // Tăng damage lên tối thiểu 50 để đánh nhanh thắng nhanh (test luồng)
        return Mathf.Max(50, raw);
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
