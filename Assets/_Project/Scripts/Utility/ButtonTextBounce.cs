using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/// <summary>
/// Gắn vào TextMeshPro bên trong Button để tự động dịch chuyển chữ lên/xuống khi rê chuột vào nút.
/// </summary>
public class ButtonTextBounce : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;
    private Vector3 originalScale;
    private Button parentButton;

    [Header("Cấu hình Dịch Chữ")]
    [Tooltip("Khoảng cách chữ dịch lên khi rê chuột vào (tính theo pixel)")]
    [SerializeField] private float bounceAmountY = 6f;
    [Tooltip("Tỉ lệ thu nhỏ chữ khi rê chuột vào (ví dụ: 0.95f để thu nhỏ 5%)")]
    [SerializeField] private float scaleAmount = 0.95f;
    [Tooltip("Thời gian dịch chuyển mượt mà")]
    [SerializeField] private float duration = 0.15f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalScale = rectTransform.localScale;
        }

        // Tìm kiếm Button cha quản lý Text này
        parentButton = GetComponentInParent<Button>();
    }

    private void OnDisable()
    {
        // Trả lại trạng thái cũ nếu đối tượng bị ẩn
        if (rectTransform != null)
        {
            rectTransform.DOKill();
            rectTransform.anchoredPosition = originalAnchoredPosition;
            rectTransform.localScale = originalScale;
        }
    }

    // Khi di chuột vào nút
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Nếu không có RectTransform hoặc Nút cha đang bị Khóa (interactable = false), bỏ qua hiệu ứng
        if (rectTransform == null) return;
        if (parentButton != null && !parentButton.interactable) return;

        rectTransform.DOKill();
        // Dịch chuyển mượt mà chữ lên trên và thu nhỏ lại
        rectTransform.DOAnchorPosY(originalAnchoredPosition.y + bounceAmountY, duration).SetEase(Ease.OutQuad);
        rectTransform.DOScale(originalScale * scaleAmount, duration).SetEase(Ease.OutQuad);
    }

    // Khi di chuột ra khỏi nút
    public void OnPointerExit(PointerEventData eventData)
    {
        if (rectTransform == null) return;
        // Nếu nút cha đang bị khóa, đảm bảo chữ ở trạng thái mặc định đứng yên
        if (parentButton != null && !parentButton.interactable) return;

        rectTransform.DOKill();
        // Trả chữ về vị trí và kích thước ban đầu
        rectTransform.DOAnchorPosY(originalAnchoredPosition.y, duration).SetEase(Ease.OutQuad);
        rectTransform.DOScale(originalScale, duration).SetEase(Ease.OutQuad);
    }
}
