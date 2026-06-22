using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Thanh Nộ (Rage Bar) hiển thị phía trên đầu BeastUnit của Player.
/// Gắn component này vào prefab BeastUnit (bên cạnh HPBarUI) và kéo fillImage vào Inspector.
/// </summary>
public class RageBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;       // Image có Image Type = Filled
    [SerializeField] private Color colorEmpty  = new Color(0.3f, 0.0f, 0.0f);  // Đỏ tối
    [SerializeField] private Color colorFull   = new Color(1.0f, 0.3f, 0.0f);  // Cam lửa rực

    private float maxRage;

    /// <summary>Khởi tạo thanh Nộ với giá trị tối đa.</summary>
    public void Initialize(int max)
    {
        maxRage = Mathf.Max(1, max);
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;          // Bắt đầu trận Nộ = 0
            fillImage.color      = colorEmpty;
        }
    }

    /// <summary>Cập nhật thanh Nộ mượt bằng DOTween.</summary>
    public void UpdateRage(int current)
    {
        float ratio = Mathf.Clamp01(current / maxRage);
        fillImage?.DOFillAmount(ratio, 0.3f).SetEase(Ease.OutCubic);

        if (fillImage != null)
        {
            // Màu chuyển dần từ đỏ tối → cam lửa khi Nộ tăng
            Color target = Color.Lerp(colorEmpty, colorFull, ratio);
            fillImage.DOColor(target, 0.3f);
        }

        // Khi đầy Nộ: thêm hiệu ứng nhịp đập (pulse) để thu hút ánh mắt
        if (ratio >= 1f)
        {
            fillImage?.transform
                .DOScale(Vector3.one * 1.05f, 0.3f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
}
