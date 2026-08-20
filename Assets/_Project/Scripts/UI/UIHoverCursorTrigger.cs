using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Gắn script này vào BẤT KỲ nút bấm nào (Nút kĩ năng, nút Balo, nút Thoát...)
/// để khi di chuột hoặc chọn nút đó, khung viền UISelectionCursor sẽ tự bay đến ôm lấy nó.
/// </summary>
public class UIHoverCursorTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Tùy chọn")]
    [Tooltip("Ẩn khung viền khi chuột rời khỏi nút (Mặc định = false để giữ khung ở nút cuối cùng như Pokemon)")]
    [SerializeField] private bool hideOnPointerExit = false;

    private RectTransform myRectTransform;

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TriggerCursor();
    }

    public void OnSelect(BaseEventData eventData)
    {
        TriggerCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hideOnPointerExit && UISelectionCursor.Instance != null)
        {
            UISelectionCursor.Instance.Hide();
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (hideOnPointerExit && UISelectionCursor.Instance != null)
        {
            UISelectionCursor.Instance.Hide();
        }
    }

    private void TriggerCursor()
    {
        if (UISelectionCursor.Instance != null && myRectTransform != null)
        {
            UISelectionCursor.Instance.MoveTo(myRectTransform);
        }
    }
}
