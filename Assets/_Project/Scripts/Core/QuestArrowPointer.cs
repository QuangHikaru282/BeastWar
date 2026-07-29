using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mũi tên chỉ đường (Quest Arrow Pointer) hướng về phía mục tiêu nhiệm vụ hiện tại.
/// 
/// Tính năng:
/// 1. Tự động xoay mũi tên hướng về phía mục tiêu Quest Target.
/// 2. Khi mục tiêu ở ngoài màn hình: Mũi tên nằm ở mép màn hình và xoay theo hướng mục tiêu.
/// 3. Khi mục tiêu ở trong màn hình: Mũi tên trỏ thẳng xuống đầu mục tiêu (Point Down).
/// </summary>
public class QuestArrowPointer : MonoBehaviour
{
    public static QuestArrowPointer Instance { get; private set; }

    [Header("Cấu hình Mũi Tên")]
    [Tooltip("RectTransform của mũi tên UI (Image)")]
    [SerializeField] private RectTransform arrowUI;

    [Tooltip("Khoảng cách mép màn hình (Pixel padding) khi mục tiêu ở xa")]
    [SerializeField] private float screenEdgePadding = 50f;

    [Tooltip("Độ cao nổi phía trên mục tiêu khi mục tiêu ở trong màn hình")]
    [SerializeField] private float targetOffsetY = 1.2f;

    private Camera mainCamera;
    private Transform currentTargetTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mainCamera == null) mainCamera = Camera.main;

        if (arrowUI != null)
        {
            canvasGroup = arrowUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = arrowUI.gameObject.AddComponent<CanvasGroup>();
        }

        // Tự động vẽ Sprite mũi tên sắc nét nếu ô Source Image chưa gán
        EnsureArrowSprite();
    }

    private void OnEnable()
    {
        QuestManager.OnQuestAdvanced += RefreshTarget;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestAdvanced -= RefreshTarget;
    }

    private void Start()
    {
        RefreshTarget();
    }

    private void LateUpdate()
    {
        if (arrowUI == null) return;

        // Nếu có bất kỳ bảng UI nào (như Bảng Nhiệm Vụ, Shop...) đang mở, tự động ẩn mũi tên đi
        if (InteractHintManager.Instance != null && InteractHintManager.Instance.IsAnyPanelOpen)
        {
            SetVisible(false);
            return;
        }

        // Khi người chơi đứng gần mục tiêu và Nút F gợi ý hiện lên -> Tự động ẩn Mũi tên chỉ đường đi
        if (InteractHintManager.Instance != null && InteractHintManager.Instance.IsHintVisible)
        {
            SetVisible(false);
            return;
        }

        if (currentTargetTransform == null || !currentTargetTransform.gameObject.activeInHierarchy)
        {
            RefreshTarget();
            if (currentTargetTransform == null)
            {
                SetVisible(false);
                return;
            }
        }

        SetVisible(true);
        UpdateArrowPositionAndRotation();
    }

    public void RefreshTarget()
    {
        currentTargetTransform = null;

        if (QuestManager.Instance != null && QuestManager.Instance.playerData != null)
        {
            int activeQuestId = QuestManager.Instance.playerData.currentMainQuestId;

            // Tìm QuestTarget hoặc QuestMarker khớp với activeQuestId
            QuestTarget[] targets = FindObjectsByType<QuestTarget>(FindObjectsSortMode.None);
            foreach (var t in targets)
            {
                if (t != null && t.questId == activeQuestId && t.gameObject.activeInHierarchy)
                {
                    currentTargetTransform = t.transform;
                    break;
                }
            }

            // Nếu không thấy QuestTarget, tìm QuestMarker
            if (currentTargetTransform == null)
            {
                QuestMarker[] markers = FindObjectsByType<QuestMarker>(FindObjectsSortMode.None);
                foreach (var m in markers)
                {
                    if (m != null && m.targetQuestId == activeQuestId && m.gameObject.activeInHierarchy)
                    {
                        currentTargetTransform = m.transform;
                        break;
                    }
                }
            }
        }

        SetVisible(currentTargetTransform != null);
    }

    private void UpdateArrowPositionAndRotation()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || currentTargetTransform == null) return;

        Vector3 targetWorldPos = currentTargetTransform.position + Vector3.up * targetOffsetY;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetWorldPos);

        // Check xem mục tiêu có nằm trước màn hình camera không
        bool isBehind = screenPos.z < 0;
        if (isBehind)
        {
            screenPos *= -1f;
        }

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        bool isOffScreen = isBehind ||
                           screenPos.x < screenEdgePadding ||
                           screenPos.x > screenWidth - screenEdgePadding ||
                           screenPos.y < screenEdgePadding ||
                           screenPos.y > screenHeight - screenEdgePadding;

        if (isOffScreen)
        {
            // Mục tiêu ở ngoài màn hình -> Kéo mũi tên sát mép màn hình & xoay theo hướng mục tiêu
            Vector3 screenCenter = new Vector3(screenWidth / 2f, screenHeight / 2f, 0f);
            Vector3 dir = (screenPos - screenCenter).normalized;

            // Giới hạn trong viền màn hình (Edge clamp)
            float clampedX = Mathf.Clamp(screenPos.x, screenEdgePadding, screenWidth - screenEdgePadding);
            float clampedY = Mathf.Clamp(screenPos.y, screenEdgePadding, screenHeight - screenEdgePadding);

            arrowUI.position = new Vector3(clampedX, clampedY, 0f);

            // Xoay mũi tên chỉ về phía mục tiêu
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrowUI.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
        else
        {
            // Mục tiêu ở trong màn hình -> Đặt mũi tên trỏ xuống đầu mục tiêu
            arrowUI.position = screenPos;
            
            // Xoay mũi tên trỏ thẳng xuống (Point down) + nhún nhẹ
            float bounce = Mathf.Sin(Time.time * 5f) * 6f;
            arrowUI.position = screenPos + new Vector3(0, bounce, 0);
            arrowUI.rotation = Quaternion.Euler(0, 0, 180f);
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
        else if (arrowUI != null)
        {
            arrowUI.gameObject.SetActive(visible);
        }
    }

    private void EnsureArrowSprite()
    {
        if (arrowUI == null) return;
        Image img = arrowUI.GetComponent<Image>();
        if (img != null && img.sprite == null)
        {
            img.sprite = CreateArrowSprite();
            img.color = new Color(1f, 0.85f, 0.1f, 1f); // Màu vàng rực rỡ
        }
    }

    private Sprite CreateArrowSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color white = Color.white;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        // Vẽ mũi tên tam giác trỏ lên phía trên (Upward Arrow)
        for (int y = 8; y < 28; y++)
        {
            int widthAtY = (28 - y) / 2;
            int startX = 16 - widthAtY;
            int endX = 16 + widthAtY;

            for (int x = startX; x <= endX; x++)
            {
                tex.SetPixel(x, y, white);
            }
        }

        // Cán mũi tên
        for (int y = 2; y < 10; y++)
        {
            for (int x = 13; x <= 19; x++)
            {
                tex.SetPixel(x, y, white);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}
