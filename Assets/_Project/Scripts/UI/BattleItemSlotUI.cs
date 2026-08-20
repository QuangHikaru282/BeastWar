using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Gắn script này vào Prefab ô vật phẩm (Item Slot) để kéo thả UI thủ công trong Inspector.
/// Thiết kế tối giản: Chỉ cần Icon + Số lượng + Button.
/// Hỗ trợ tự động gọi UISelectionCursor khi di chuột vào ô.
/// </summary>
public class BattleItemSlotUI : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [Header("UI References (Kéo thả trong Prefab)")]
    [Tooltip("Image hiển thị icon của vật phẩm")]
    [SerializeField] private Image iconImage;

    [Tooltip("Text / TextMeshPro hiển thị số lượng (ví dụ: x3, ∞, 3/3)")]
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Text countTextLegacy;

    [Tooltip("Component Button của ô vật phẩm")]
    [SerializeField] private Button button;

    [Tooltip("CanvasGroup để làm mờ ô khi hết số lượng (tùy chọn)")]
    [SerializeField] private CanvasGroup canvasGroup;

    // Dữ liệu lưu sẵn để hỗ trợ hiển thị tooltip thông tin khi hover trong tương lai
    [HideInInspector] public string itemName;
    [HideInInspector] public string itemDescription;

    private RectTransform myRectTransform;

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
    }

    public void Setup(Sprite icon, string displayName, string quantityString, bool isInteractable, UnityEngine.Events.UnityAction onClickAction, string description = "")
    {
        this.itemName = displayName;
        this.itemDescription = description;

        // Gán Icon
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        // Gán số lượng
        if (countText != null) countText.text = quantityString;
        if (countTextLegacy != null) countTextLegacy.text = quantityString;

        // Gán sự kiện Click
        if (button != null)
        {
            button.interactable = isInteractable;
            button.onClick.RemoveAllListeners();
            if (onClickAction != null)
            {
                button.onClick.AddListener(onClickAction);
            }
        }

        // Làm mờ khi không thể sử dụng
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isInteractable ? 1f : 0.4f;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UISelectionCursor.Instance != null && myRectTransform != null)
        {
            UISelectionCursor.Instance.MoveTo(myRectTransform);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (UISelectionCursor.Instance != null && myRectTransform != null)
        {
            UISelectionCursor.Instance.MoveTo(myRectTransform);
        }
    }
}
