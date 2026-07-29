using UnityEngine;

public class QuestPanelController : MonoBehaviour
{
    [Header("Panel nhiệm vụ")]
    [SerializeField] private GameObject questPanel;

    private void Start()
    {
        // Ẩn bảng nhiệm vụ khi bắt đầu game.
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Gắn hàm này vào nút mở bảng nhiệm vụ.
    /// </summary>
    public void OpenQuestPanel()
    {
        if (questPanel == null)
        {
            Debug.LogWarning(
                "[QuestPanelController] Chưa kéo QuestPanel vào Inspector."
            );

            return;
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
    }

    /// <summary>
    /// Gắn hàm này vào nút đóng bảng nhiệm vụ.
    /// </summary>
    public void CloseQuestPanel()
    {
        if (questPanel == null)
        {
            Debug.LogWarning(
                "[QuestPanelController] Chưa kéo QuestPanel vào Inspector."
            );

            return;
        }

        questPanel.SetActive(false);
        InteractHintManager.Instance?.RegisterPanelClose();
    }
}