using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.red;
    [SerializeField] private float moveUp = 8f;
    [SerializeField] private float speed = 10f;

    private Vector2 originalPos;
    private Vector2 targetPos;
    private Color targetColor;

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>();

        originalPos = text.rectTransform.anchoredPosition;
        targetPos = originalPos;
        targetColor = normalColor;

        text.color = normalColor;
        text.fontStyle = FontStyles.Normal;
    }

    private void Update()
    {
        text.color = Color.Lerp(text.color, targetColor, Time.deltaTime * speed);

        text.rectTransform.anchoredPosition =
            Vector2.Lerp(text.rectTransform.anchoredPosition, targetPos, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetColor = hoverColor;
        targetPos = originalPos + Vector2.up * moveUp;
        text.fontStyle = FontStyles.Bold;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetColor = normalColor;
        targetPos = originalPos;
        text.fontStyle = FontStyles.Normal;
    }
}