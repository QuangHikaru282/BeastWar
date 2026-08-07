using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script điều khiển Quả Cầu Xoay Xoay + Thanh Loading Chạy Theo Tiến Trình Trượt Cảnh (Scene Transition).
/// Gắn vào UI Loading trong SceneTransitionCanvas.
/// </summary>
public class LoadingPokeballUI : MonoBehaviour
{
    [Header("1. Quả Cầu Pokeball Xoay Vòng")]
    [Tooltip("Image UI quả cầu (Ví dụ: Pokeball, BeastBall)")]
    [SerializeField] private RectTransform pokeballIcon;

    [Tooltip("Tốc độ xoay (độ/giây)")]
    [SerializeField] private float rotateSpeed = -360f; // Xoay theo chiều kim đồng hồ

    [Header("2. Thanh Fill Chạy Loading")]
    [Tooltip("Image đại diện cho thanh loading (Chọn Image Type = Filled)")]
    [SerializeField] private Image fillImage;

    [Tooltip("Dành cho thanh phẳng: RectTransform của thanh Fill xanh")]
    [SerializeField] private RectTransform fillRectTransform;
    [SerializeField] private RectTransform parentRectTransform;

    [Header("3. Tự Động Kéo Quả Cầu Theo Đường Fill")]
    [Tooltip("Nếu true: Quả cầu sẽ tự chạy tịnh tiến theo đầu của thanh fill")]
    [SerializeField] private bool movePokeballWithFill = true;

    [Tooltip("Vị trí bắt đầu X của quả cầu (khi fill = 0)")]
    [SerializeField] private float startX = -200f;

    [Tooltip("Vị trí kết thúc X của quả cầu (khi fill = 1)")]
    [SerializeField] private float endX = 200f;

    [Range(0f, 1f)]
    private float currentProgress = 0f;

    private void Update()
    {
        // 1. Xoay quả cầu liên tục
        if (pokeballIcon != null)
        {
            pokeballIcon.Rotate(0f, 0f, rotateSpeed * Time.unscaledDeltaTime);
        }

        // 2. Cập nhật vị trí quả cầu tự động theo tiến trình
        if (movePokeballWithFill && pokeballIcon != null)
        {
            if (fillRectTransform != null && parentRectTransform != null)
            {
                // Tự động đẩy Pokeball theo chiều rộng hiện tại của thanh xanh
                float currentWidth = fillRectTransform.rect.width;
                Vector2 pos = pokeballIcon.anchoredPosition;
                pos.x = fillRectTransform.anchoredPosition.x + currentWidth;
                pokeballIcon.anchoredPosition = pos;
            }
            else if (fillImage != null)
            {
                currentProgress = fillImage.fillAmount;
                Vector2 pos = pokeballIcon.anchoredPosition;
                pos.x = Mathf.Lerp(startX, endX, currentProgress);
                pokeballIcon.anchoredPosition = pos;
            }
        }
    }

    /// <summary>
    /// Cập nhật phần trăm tiến trình Loading (từ 0.0 đến 1.0).
    /// </summary>
    public void SetProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);

        if (fillRectTransform != null && parentRectTransform != null)
        {
            // Điều khiển chiều rộng trực tiếp mà không cần Sprite Filled
            float totalWidth = parentRectTransform.rect.width;
            fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth * currentProgress);
        }
        else if (fillImage != null)
        {
            fillImage.fillAmount = currentProgress;
        }

        if (movePokeballWithFill && pokeballIcon != null)
        {
            Vector2 pos = pokeballIcon.anchoredPosition;
            pos.x = Mathf.Lerp(startX, endX, currentProgress);
            pokeballIcon.anchoredPosition = pos;
        }
    }
}
