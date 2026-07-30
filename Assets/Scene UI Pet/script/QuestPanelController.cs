using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuestPanelController : MonoBehaviour
{
    [Header("Panel nhiệm vụ")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private RectTransform panelRect;

    [Header("Nút điều khiển")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Cấu hình Trượt (Slide)")]
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private float closedPositionX = 600f; // Vị trí X khi ẩn ra khỏi mép phải
    [SerializeField] private float openedPositionX = 0f;    // Vị trí X khi trượt ra hiển thị

    private CanvasGroup canvasGroup;
    private Tween currentTween;
    private bool isOpen = false;

    private void Awake()
    {
        if (questPanel != null && panelRect == null)
        {
            panelRect = questPanel.GetComponent<RectTransform>();
        }

        if (questPanel != null)
        {
            canvasGroup = questPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = questPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Start()
    {
        // Đăng ký sự kiện click nút nếu được kéo vào Inspector
        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OpenQuestPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseQuestPanel);
        }

        // Khởi tạo trạng thái ban đầu: Đóng panel và vị trí ẩn ra mép phải
        if (panelRect != null)
        {
            Vector2 anchoredPos = panelRect.anchoredPosition;
            anchoredPos.x = closedPositionX;
            panelRect.anchoredPosition = anchoredPos;
        }

        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        UpdateButtonVisibility(false);
    }

    /// <summary>
    /// Gắn hàm này vào nút mở bảng nhiệm vụ.
    /// </summary>
    public void OpenQuestPanel()
    {
        if (questPanel == null)
        {
            Debug.LogWarning("[QuestPanelController] Chưa kéo QuestPanel vào Inspector.");
            return;
        }

        isOpen = true;

        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }

        questPanel.SetActive(true);
        questPanel.transform.SetAsLastSibling();

        if (questPanel.transform.parent != null)
        {
            questPanel.transform.parent.gameObject.SetActive(true);
            questPanel.transform.parent.SetAsLastSibling();
        }

        Canvas canvas = questPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 100;
        }

        InteractHintManager.Instance?.RegisterPanelOpen();
        UpdateButtonVisibility(true);

        // Hiệu ứng trượt từ bên phải vào
        if (panelRect != null)
        {
            currentTween = panelRect.DOAnchorPosX(openedPositionX, slideDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }
    }

    /// <summary>
    /// Gắn hàm này vào nút đóng bảng nhiệm vụ.
    /// </summary>
    public void CloseQuestPanel()
    {
        if (questPanel == null)
        {
            Debug.LogWarning("[QuestPanelController] Chưa kéo QuestPanel vào Inspector.");
            return;
        }

        isOpen = false;

        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }

        InteractHintManager.Instance?.RegisterPanelClose();
        UpdateButtonVisibility(false);

        // Hiệu ứng trượt ra mép phải rồi ẩn active
        if (panelRect != null)
        {
            currentTween = panelRect.DOAnchorPosX(closedPositionX, slideDuration)
                .SetEase(Ease.InCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (!isOpen)
                    {
                        questPanel.SetActive(false);
                    }
                });
        }
        else
        {
            questPanel.SetActive(false);
        }
    }

    public void ToggleQuestPanel()
    {
        if (isOpen)
        {
            CloseQuestPanel();
        }
        else
        {
            OpenQuestPanel();
        }
    }

    private void UpdateButtonVisibility(bool panelIsOpen)
    {
        if (openButton != null)
        {
            openButton.gameObject.SetActive(!panelIsOpen);
            if (!panelIsOpen)
            {
                openButton.transform.SetAsLastSibling();
            }
        }

        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(panelIsOpen);
            if (panelIsOpen)
            {
                closeButton.transform.SetAsLastSibling();
            }
        }
    }
}