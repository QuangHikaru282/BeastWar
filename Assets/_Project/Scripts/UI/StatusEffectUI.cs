using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hien thi icon trang thai xau (Doc, Te liet, Choang) ben duoi ten thu trong HUD.
/// Gan vao mot GameObject chua Image lam icon, dat trong PlayerBattleHud / EnemyBattleHud.
/// </summary>
public class StatusEffectUI : MonoBehaviour
{
    [Header("Icon Sprites")]
    [SerializeField] private Sprite iconPoisoned;
    [SerializeField] private Sprite iconParalyzed;
    [SerializeField] private Sprite iconStunned;

    [Header("References")]
    [SerializeField] private Image iconImage;

    private void Awake()
    {
        if (iconImage == null) iconImage = GetComponent<Image>();
        gameObject.SetActive(false);
    }

    /// <summary>Hien thi icon tuong ung voi trang thai.</summary>
    public void Show(BeastUnit.StatusEffect status)
    {
        Sprite target = status switch
        {
            BeastUnit.StatusEffect.Poisoned  => iconPoisoned,
            BeastUnit.StatusEffect.Paralyzed => iconParalyzed,
            BeastUnit.StatusEffect.Stunned   => iconStunned,
            _                                => null
        };

        if (target == null) { Hide(); return; }
        if (iconImage != null) iconImage.sprite = target;
        gameObject.SetActive(true);
    }

    /// <summary>An icon trang thai.</summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
