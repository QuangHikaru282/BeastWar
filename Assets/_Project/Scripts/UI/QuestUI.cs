using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [Header("Data Reference")]
    public PlayerData playerData;

    [Header("UI Components")]
    public GameObject questPanel;
    public TextMeshProUGUI questText; // Hỗ trợ TextMeshPro luôn cho giống túi đồ

    private int lastPrintedStage = -1;

    private void Start()
    {
        // Luôn đưa bảng nhiệm vụ (questPanel) về lớp nền dưới cùng để không che bất kỳ UI nào khác
        if (questPanel != null)
        {
            questPanel.transform.SetAsFirstSibling();
        }
        else
        {
            transform.SetAsFirstSibling();
        }
    }

    private void Update()
    {
        if (playerData == null) return;
        if (questPanel == null || questText == null) return;

        // Chỉ in ra console nếu ID quest thay đổi
        if (playerData.currentMainQuestId != lastPrintedStage)
        {
            Debug.Log("[QuestUI] Current Quest ID: " + playerData.currentMainQuestId);
            lastPrintedStage = playerData.currentMainQuestId;
        }

        // Bật panel
        Image panelImg = questPanel.GetComponent<Image>();
        if (panelImg != null) panelImg.enabled = true;

        // Lấy text mô tả từ QuestManager
        if (QuestManager.Instance != null)
        {
            questText.text = "Nhiệm vụ: " + QuestManager.Instance.GetCurrentQuestDescription();
        }
        else
        {
            questText.text = "Lỗi: Không tìm thấy QuestManager trong Scene!";
        }

        // Màu chữ mặc định (có thể đổi sang xanh nếu hoàn thành hết quest)
        if (playerData.currentMainQuestId >= 17)
        {
            questText.color = Color.blue;
        }
        else
        {
            questText.color = Color.black;
        }
    }

    // Hàm này sẽ được gọi khi bạn bấm vào nút (Button) trên QuestPanel
    public void OnQuestPanelClicked()
    {
        if (playerData == null) return;

        if (QuestNavigation.Instance != null)
        {
            // Bật/tắt mũi tên chỉ hướng tới mục tiêu của Quest hiện tại
            QuestNavigation.Instance.ToggleNavigationForQuest(playerData.currentMainQuestId);
        }
        else
        {
            Debug.LogWarning("[QuestUI] Không tìm thấy QuestNavigation trong Scene! Hãy tạo một GameObject và gắn script QuestNavigation vào.");
        }
    }
}
