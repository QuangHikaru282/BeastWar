using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Thanh máu HP hiển thị phía trên đầu mỗi BeastUnit.
/// </summary>
public class HPBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;       // Image có Image Type = Filled
    [SerializeField] private Slider hpSlider;       // Hỗ trợ cả Slider mặc định của Unity
    [SerializeField] private Color colorHigh   = new Color(0.2f, 0.85f, 0.2f);  // Xanh lá
    [SerializeField] private Color colorMedium = new Color(1f,   0.8f,  0f);    // Vàng
    [SerializeField] private Color colorLow    = new Color(0.9f, 0.1f, 0.1f);   // Đỏ

    private float maxHP;

    private void Awake()
    {
        if (hpSlider == null) hpSlider = GetComponent<Slider>();
    }

    public void Initialize(int max)
    {
        maxHP = Mathf.Max(1, max);
        if (hpSlider != null)
        {
            hpSlider.maxValue = 1f;
            hpSlider.value = 1f;
        }
        else if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.color = colorHigh;
        }
    }

    /// <summary>Cập nhật thanh HP mượt bằng DOTween.</summary>
    public void UpdateHP(int current)
    {
        float ratio = Mathf.Clamp01(current / maxHP);
        
        if (hpSlider != null)
        {
            hpSlider.DOValue(ratio, 0.35f).SetEase(Ease.OutCubic);
        }
        else if (fillImage != null)
        {
            fillImage.DOFillAmount(ratio, 0.35f).SetEase(Ease.OutCubic);
        }

        // Đổi màu
        Image targetColorImage = null;
        if (fillImage != null) targetColorImage = fillImage;
        else if (hpSlider != null && hpSlider.fillRect != null) targetColorImage = hpSlider.fillRect.GetComponent<Image>();

        if (targetColorImage != null)
        {
            Color target = ratio > 0.5f ? colorHigh
                         : ratio > 0.25f ? colorMedium
                         : colorLow;
            targetColorImage.DOColor(target, 0.35f);
        }
    }
}
