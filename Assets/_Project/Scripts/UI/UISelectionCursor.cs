using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Khung viền chọn (Cursor/Highlight Frame) duy nhất trên Canvas.
/// Tự động bay đến và co giãn ôm khít kích thước của bất kỳ nút UI nào được hover/chọn.
/// Tự động ẩn đi khi nút mục tiêu bị tắt hoặc bị hủy.
/// </summary>
public class UISelectionCursor : MonoBehaviour
{
    public static UISelectionCursor Instance { get; private set; }

    [Header("1. UI Reference")]
    [Tooltip("RectTransform của chính khung viền (để trống sẽ tự lấy RectTransform của GameObject này)")]
    [SerializeField] private RectTransform cursorRect;

    [Header("2. Kích thước & Khoảng cách")]
    [Tooltip("Khoảng cách mở rộng thêm ra ngoài viền nút (X = ngang, Y = dọc)")]
    [SerializeField] private Vector2 padding = new Vector2(8f, 8f);

    [Header("3. Hiệu ứng di chuyển")]
    [Tooltip("Bật/tắt hiệu ứng lướt mượt mà")]
    [SerializeField] private bool useAnimation = true;

    [Tooltip("Thời gian lướt đến mục tiêu (giây)")]
    [SerializeField] private float moveDuration = 0.12f;

    [Tooltip("Hiệu ứng Ease của DOTween")]
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    [Header("4. Tùy chọn khác")]
    [Tooltip("Ẩn khung viền khi bắt đầu game")]
    [SerializeField] private bool hideOnStart = true;

    private Tween moveTween;
    private Tween sizeTween;
    private RectTransform currentTarget;

    public RectTransform CurrentTarget => currentTarget;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (cursorRect == null) cursorRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (hideOnStart)
        {
            Hide();
        }
    }

    /// <summary>
    /// Di chuyển khung viền đến ôm lấy nút mục tiêu target
    /// </summary>
    public void MoveTo(RectTransform target)
    {
        if (target == null || cursorRect == null) return;

        currentTarget = target;

        // Bật khung viền lên
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // Đưa khung lên lớp trên cùng để không bị các UI khác che khuất
        cursorRect.SetAsLastSibling();

        Vector2 targetSize = target.rect.size + padding;
        Vector3 targetWorldPos = target.position;

        if (useAnimation && Application.isPlaying)
        {
            moveTween?.Kill();
            sizeTween?.Kill();

            moveTween = cursorRect.DOMove(targetWorldPos, moveDuration).SetEase(moveEase);
            sizeTween = cursorRect.DOSizeDelta(targetSize, moveDuration).SetEase(moveEase);
        }
        else
        {
            cursorRect.position = targetWorldPos;
            cursorRect.sizeDelta = targetSize;
        }
    }

    /// <summary>
    /// Ẩn khung viền
    /// </summary>
    public void Hide()
    {
        currentTarget = null;
        moveTween?.Kill();
        sizeTween?.Kill();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Ẩn khung viền nếu đang chỉ vào đúng target này
    /// </summary>
    public void HideIfTarget(RectTransform target)
    {
        if (currentTarget == target)
        {
            Hide();
        }
    }

    /// <summary>
    /// Hiện khung viền
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        // Nếu nút mục tiêu bị Disable / Destroy (ví dụ đóng Balo), tự động ẩn khung viền ngay
        if (gameObject.activeSelf && currentTarget != null)
        {
            if (!currentTarget.gameObject.activeInHierarchy)
            {
                Hide();
            }
        }
    }

    private void OnDestroy()
    {
        moveTween?.Kill();
        sizeTween?.Kill();
    }
}
