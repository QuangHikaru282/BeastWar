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
    [SerializeField] private TextMeshProUGUI nameText;


    // Runtime stats
    public int CurrentHP { get; private set; }
    public bool IsAlive => CurrentHP > 0;
    public bool IsPlayerTeam { get; private set; }

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

        if (nameText != null) nameText.text = data.beastName;
        hpBar?.Initialize(data.maxHP);
    }

    /// <summary>Nhận sát thương. Trả về true nếu chết sau đòn này.</summary>
    public bool TakeDamage(int damage, bool isCritical = false)
    {
        if (!IsAlive) return false;

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
