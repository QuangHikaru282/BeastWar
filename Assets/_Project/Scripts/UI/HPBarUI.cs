using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Thanh máu HP hiển thị phía trên đầu mỗi BeastUnit.
/// Gắn component này vào prefab BeastUnit và kéo các tham chiếu vào Inspector.
/// </summary>
public class HPBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;       // Image có Image Type = Filled
    [SerializeField] private Color colorHigh   = new Color(0.2f, 0.85f, 0.2f);  // Xanh lá
    [SerializeField] private Color colorMedium = new Color(1f,   0.8f,  0f);    // Vàng
    [SerializeField] private Color colorLow    = new Color(0.9f, 0.1f, 0.1f);   // Đỏ

    private float maxHP;

    public void Initialize(int max)
    {
        maxHP = Mathf.Max(1, max);
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.color = colorHigh;
        }
    }

    /// <summary>Cập nhật thanh HP mượt bằng DOTween.</summary>
    public void UpdateHP(int current)
    {
        float ratio = Mathf.Clamp01(current / maxHP);
        fillImage?.DOFillAmount(ratio, 0.35f).SetEase(Ease.OutCubic);

        if (fillImage != null)
        {
            Color target = ratio > 0.5f ? colorHigh
                         : ratio > 0.25f ? colorMedium
                         : colorLow;
            fillImage.DOColor(target, 0.35f);
        }
    }
}
