using UnityEngine;
using TMPro;

/// <summary>
/// Singleton quản lý UI gợi ý tương tác (nút F nổi trên đầu đối tượng).
/// 
/// Cách sử dụng:
/// 1. Tạo một Canvas (World Space hoặc Screen Space - Overlay).
/// 2. Tạo một GameObject con làm "HintRoot" với Image + TextMeshProUGUI hiển thị chữ "F".
/// 3. Kéo HintRoot vào field hintRoot của script này.
/// 4. Gắn script này vào một GameObject trên scene (ví dụ: "GameManagers").
/// </summary>
public class InteractHintManager : MonoBehaviour
{
    public static InteractHintManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("RectTransform của toàn bộ hint UI (chứa icon F + background)")]
    [SerializeField] private RectTransform hintRoot;

    [Tooltip("Camera dùng để convert world position → screen position")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Tốc độ fade in/out (0 = ngay lập tức)")]
    [SerializeField] private float fadeSpeed = 8f;

    private CanvasGroup hintCanvasGroup;
    private GameObject currentTarget;
    private SpriteOutline currentOutline;
    private float targetAlpha = 0f;

    // Đếm số lượng UI panel đang mở (dialogue, shop, ...) để ẩn hint khi có panel mở
    private int openPanelCount = 0;

    // ────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (hintRoot != null)
        {
            hintCanvasGroup = hintRoot.GetComponent<CanvasGroup>();
            if (hintCanvasGroup == null)
                hintCanvasGroup = hintRoot.gameObject.AddComponent<CanvasGroup>();
        }

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        Kinnly.PlayerInteract.OnInteractableEnter += HandleEnter;
        Kinnly.PlayerInteract.OnInteractableExit  += HandleExit;
    }

    private void OnDisable()
    {
        Kinnly.PlayerInteract.OnInteractableEnter -= HandleEnter;
        Kinnly.PlayerInteract.OnInteractableExit  -= HandleExit;
    }

    private void LateUpdate()
    {
        UpdateHintPosition();
        UpdateFade();
    }

    #endregion

    // ────────────────────────────────────────────
    #region Event Handlers

    private float currentPopT = 0f;
    private Transform activeTargetTransform;
    private float lastSideSign = 1f;

    private void HandleEnter(GameObject interactable)
    {
        // Tắt highlight đối tượng cũ
        if (currentOutline != null)
            currentOutline.SetHighlight(false);

        currentTarget = interactable;
        if (interactable != null)
            activeTargetTransform = interactable.transform;

        // Bật highlight đối tượng mới
        currentOutline = interactable.GetComponent<SpriteOutline>();
        if (currentOutline == null && interactable != null)
        {
            // Tự động thêm SpriteOutline nếu chưa có
            SpriteRenderer sr = interactable.GetComponent<SpriteRenderer>();
            if (sr != null)
                currentOutline = interactable.AddComponent<SpriteOutline>();
        }

        if (currentOutline != null)
            currentOutline.SetHighlight(true);

        currentPopT = 0f; // Bắt đầu hiệu ứng đẩy nảy từ tâm NPC
        RefreshVisibility();
    }

    private Vector3 lastTargetWorldPos;

    private void HandleExit()
    {
        if (currentOutline != null)
            currentOutline.SetHighlight(false);

        currentTarget = null;
        currentOutline = null;

        targetAlpha = 0f;
        RefreshVisibility();
    }

    #endregion

    // ────────────────────────────────────────────
    #region Public API

    /// <summary>
    /// Kiểm tra xem có bất kỳ panel UI nào (Dialogue, Shop, QuestPanel...) đang mở hay không.
    /// </summary>
    public bool IsAnyPanelOpen
    {
        get
        {
            if (openPanelCount > 0) return true;
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return true;

            var shopManager = FindFirstObjectByType<ShopManager>();
            if (shopManager != null && shopManager.IsOpen) return true;

            var questPanelController = FindFirstObjectByType<QuestPanelController>();
            if (questPanelController != null)
            {
                var qp = questPanelController.transform.Find("QuestPanel");
                if (qp != null && qp.gameObject.activeInHierarchy) return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Kiểm tra xem nút F gợi ý tương tác có đang hiển thị trên màn hình hay không.
    /// </summary>
    public bool IsHintVisible => currentTarget != null && targetAlpha > 0.1f;

    /// <summary>
    /// Gọi khi một panel UI (dialogue, shop, ...) mở ra.
    /// Nút F sẽ bị ẩn trong thời gian panel mở.
    /// </summary>
    public void RegisterPanelOpen()
    {
        openPanelCount++;
        RefreshVisibility();
    }

    /// <summary>
    /// Gọi khi panel UI đóng lại.
    /// </summary>
    public void RegisterPanelClose()
    {
        openPanelCount = Mathf.Max(0, openPanelCount - 1);
        RefreshVisibility();
    }

    #endregion

    // ────────────────────────────────────────────
    #region Private Helpers

    private void RefreshVisibility()
    {
        bool isAnyPanelOpen = openPanelCount > 0;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            isAnyPanelOpen = true;
        }

        if (!isAnyPanelOpen)
        {
            var shopManager = FindFirstObjectByType<ShopManager>();
            if (shopManager != null && shopManager.IsOpen)
                isAnyPanelOpen = true;
        }

        bool shouldShow = currentTarget != null && !isAnyPanelOpen;
        targetAlpha = shouldShow ? 1f : 0f;
    }

    [Header("Tùy chỉnh Vị Trí Nút F (Chỉnh Real-time trong Inspector)")]
    [Tooltip("Khoảng cách độ cao Y nổi của nút F so với chân NPC (Mặc định: 0.45f)")]
    public float floatOffsetY = 0.45f;

    [Tooltip("Khoảng cách lệch ngang X để nút F nằm sang bên hông NPC (Mặc định: 0.95f)")]
    public float sideOffsetX = 0.95f;

    [Tooltip("Thời gian trượt đẩy nảy từ tâm NPC sang bên hông (giây)")]
    public float popOutDuration = 0.5f;

    private void UpdateHintPosition()
    {
        if (hintRoot == null) return;

        bool shouldShow = currentTarget != null && targetAlpha > 0.05f;
        float targetPop = shouldShow ? 1f : 0f;

        // Kiểm soát thời gian chuyển động chuẩn xác 0.5 giây cho cả 2 chiều đẩy ra & thu vào
        float stepSpeed = Time.deltaTime / Mathf.Max(0.05f, popOutDuration);
        currentPopT = Mathf.MoveTowards(currentPopT, targetPop, stepSpeed);

        float popProgress = EvaluateEaseOutBack(currentPopT);

        Transform targetTr = currentTarget != null ? currentTarget.transform : activeTargetTransform;

        if (targetTr != null)
        {
            float extraOffsetY = 0f;
            if (currentOutline != null)
            {
                extraOffsetY = currentOutline.customHintOffsetY;
            }

            Transform playerTr = Kinnly.Player.Instance != null ? Kinnly.Player.Instance.transform : null;

            if (playerTr != null && currentTarget != null)
            {
                if (playerTr.position.x < targetTr.position.x)
                {
                    lastSideSign = 1f;
                }
                else
                {
                    lastSideSign = -1f;
                }
            }

            float offsetX = sideOffsetX * lastSideSign;

            // Đẩy trượt ra & Thu trượt lùi về tâm NPC (offsetX * popProgress)
            Vector3 baseCenter = targetTr.position + Vector3.up * (floatOffsetY + extraOffsetY);
            lastTargetWorldPos = baseCenter + new Vector3(offsetX * popProgress, 0, 0);
        }

        // Áp dụng scale nảy (0 -> 1)
        hintRoot.localScale = Vector3.one * Mathf.Clamp01(popProgress);

        Vector3 screenPos = mainCamera.WorldToScreenPoint(lastTargetWorldPos);
        hintRoot.position = screenPos;
    }

    private float EvaluateEaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
    }

    private void UpdateFade()
    {
        if (hintCanvasGroup == null) return;

        hintCanvasGroup.alpha = Mathf.Lerp(
            hintCanvasGroup.alpha,
            targetAlpha,
            Time.deltaTime * fadeSpeed
        );

        hintCanvasGroup.interactable  = hintCanvasGroup.alpha > 0.1f;
        hintCanvasGroup.blocksRaycasts = false;
    }

    #endregion
}
