using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverScale : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Kích thước")]
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float pressedScale = 0.96f;

    [Header("Tốc độ")]
    [SerializeField] private float animationSpeed = 15f;

    private RectTransform rectTransform;
    private Vector3 normalScale;
    private Vector3 targetScale;
    private bool pointerInside;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        normalScale = rectTransform.localScale;
        targetScale = normalScale;
    }

    private void Update()
    {
        float smoothValue =
            1f - Mathf.Exp(
                -animationSpeed * Time.unscaledDeltaTime
            );

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            smoothValue
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        targetScale = normalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        targetScale = normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pointerInside)
        {
            targetScale = normalScale * pressedScale;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = pointerInside
            ? normalScale * hoverScale
            : normalScale;
    }

    private void OnDisable()
    {
        pointerInside = false;
        targetScale = normalScale;

        if (rectTransform != null)
        {
            rectTransform.localScale = normalScale;
        }
    }
}