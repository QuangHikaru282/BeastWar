using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Một dòng hiển thị trong danh sách Quest Tracker compact.
/// Gồm: Tên nhiệm vụ + trạng thái màu (Đang làm / Xong).
/// </summary>
public class QuestTrackerItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image backgroundImage;

    [Header("Màu trạng thái")]
    [SerializeField] private Color colorDone = new Color(0.4f, 0.8f, 0.4f);       // Xanh lá — đã xong
    [SerializeField] private Color colorCurrent = new Color(1f, 0.85f, 0.2f);     // Vàng — đang làm
    [SerializeField] private Color colorLocked = new Color(0.6f, 0.6f, 0.6f);     // Xám — chưa tới

    [Header("Màu nền")]
    [SerializeField] private Color bgCurrent = new Color(1f, 0.85f, 0.2f, 0.15f);
    [SerializeField] private Color bgNormal = new Color(0f, 0f, 0f, 0.25f);

    private void Awake()
    {
        // Tự tìm nếu chưa kéo vào Inspector
        if (questNameText == null)
        {
            var texts = GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                questNameText = texts[0];
                statusText = texts[1];
            }
            else if (texts.Length == 1)
            {
                questNameText = texts[0];
            }
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    /// <summary>
    /// Cấu hình dòng quest với tiêu đề và trạng thái.
    /// </summary>
    public void Setup(string title, bool isDone, bool isCurrent)
    {
        if (questNameText != null)
        {
            questNameText.text = title;
        }

        if (statusText != null)
        {
            if (isDone)
            {
                statusText.text = "(Xong)";
                statusText.color = colorDone;
            }
            else if (isCurrent)
            {
                statusText.text = "(Đang làm)";
                statusText.color = colorCurrent;
            }
            else
            {
                statusText.text = "";
                statusText.color = colorLocked;
            }
        }

        if (questNameText != null)
        {
            if (isDone) questNameText.color = colorDone;
            else if (isCurrent) questNameText.color = colorCurrent;
            else questNameText.color = colorLocked;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = isCurrent ? bgCurrent : bgNormal;
        }
    }
}
