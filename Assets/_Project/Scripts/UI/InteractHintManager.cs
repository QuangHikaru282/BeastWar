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

    [Tooltip("Khoảng cách mặc định (World Space Y) icon F nổi phía trên đối tượng")]
    public float floatOffsetY = 1.2f;

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

    private void HandleEnter(GameObject interactable)
    {
        // Tắt highlight đối tượng cũ
        if (currentOutline != null)
            currentOutline.SetHighlight(false);

        currentTarget = interactable;

        // Bật highlight đối tượng mới
        currentOutline = interactable.GetComponent<SpriteOutline>();
        if (currentOutline == null)
        {
            // Tự động thêm SpriteOutline nếu chưa có
            SpriteRenderer sr = interactable.GetComponent<SpriteRenderer>();
            if (sr != null)
                currentOutline = interactable.AddComponent<SpriteOutline>();
        }

        if (currentOutline != null)
            currentOutline.SetHighlight(true);

        RefreshVisibility();
    }

    private Vector3 lastTargetWorldPos;

    private void HandleExit()
    {
        if (currentOutline != null)
            currentOutline.SetHighlight(false);

        currentTarget = null;
        currentOutline = null;

        // Biến mất ngay lập tức tại vị trí NPC khi rời khỏi phạm vi tương tác
        targetAlpha = 0f;
        if (hintCanvasGroup != null)
        {
            hintCanvasGroup.alpha = 0f;
            hintCanvasGroup.interactable = false;
            hintCanvasGroup.blocksRaycasts = false;
        }

        RefreshVisibility();
    }

    #endregion

    // ────────────────────────────────────────────
    #region Public API

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
        else
        {
            // Tự động khôi phục nếu không có thoại active
            var shopManager = FindFirstObjectByType<ShopManager>();
            if (shopManager == null || !shopManager.IsOpen)
            {
                openPanelCount = 0;
                isAnyPanelOpen = false;
            }
        }

        bool shouldShow = currentTarget != null && !isAnyPanelOpen;
        targetAlpha = shouldShow ? 1f : 0f;
    }

    private void UpdateHintPosition()
    {
        if (hintRoot == null) return;

        if (currentTarget != null)
        {
            float extraOffsetY = 0f;
            if (currentOutline != null)
            {
                extraOffsetY = currentOutline.customHintOffsetY;
            }

            lastTargetWorldPos = currentTarget.transform.position + Vector3.up * (floatOffsetY + extraOffsetY);
        }

        // Chuyển vị trí world-space của đối tượng sang screen-space
        Vector3 screenPos = mainCamera.WorldToScreenPoint(lastTargetWorldPos);

        // Áp vị trí lên RectTransform (dùng cho Canvas Screen Space - Overlay)
        hintRoot.position = screenPos;
    }

    private void UpdateFade()
    {
        if (hintCanvasGroup == null) return;

        hintCanvasGroup.alpha = Mathf.Lerp(
            hintCanvasGroup.alpha,
            targetAlpha,
            Time.deltaTime * fadeSpeed
        );

        // Ẩn hoàn toàn khi alpha gần = 0 để không block raycast
        hintCanvasGroup.interactable  = hintCanvasGroup.alpha > 0.1f;
        hintCanvasGroup.blocksRaycasts = false; // Không bao giờ block raycast của người chơi
    }

    #endregion
}
