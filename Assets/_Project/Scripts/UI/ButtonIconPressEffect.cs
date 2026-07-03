using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonIconPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Kéo các phần tử (Icon, Text...) muốn lún xuống khi bấm nút vào đây")]
    public RectTransform[] elementsToMove;

    [Tooltip("Khoảng cách lún xuống (thường là số âm, ví dụ -5)")]
    public float pressOffset = -5f;

    private Vector2[] originalPositions;
    private bool isPressed = false;

    private void Start()
    {
        if (elementsToMove != null && elementsToMove.Length > 0)
        {
            originalPositions = new Vector2[elementsToMove.Length];
            for (int i = 0; i < elementsToMove.Length; i++)
            {
                if (elementsToMove[i] != null)
                {
                    originalPositions[i] = elementsToMove[i].anchoredPosition;
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (elementsToMove != null && !isPressed)
        {
            isPressed = true;
            for (int i = 0; i < elementsToMove.Length; i++)
            {
                if (elementsToMove[i] != null)
                {
                    elementsToMove[i].anchoredPosition = originalPositions[i] + new Vector2(0, pressOffset);
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (elementsToMove != null && isPressed)
        {
            isPressed = false;
            for (int i = 0; i < elementsToMove.Length; i++)
            {
                if (elementsToMove[i] != null)
                {
                    elementsToMove[i].anchoredPosition = originalPositions[i];
                }
            }
        }
    }
}
