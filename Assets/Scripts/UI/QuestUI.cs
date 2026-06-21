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

    private void Update()
    {
        if (playerData == null) return;
        if (questPanel == null || questText == null) return;

        if (playerData.tutorialQuestStage != lastPrintedStage)
        {
            Debug.Log("[QuestUI] Current Stage: " + playerData.tutorialQuestStage);
            lastPrintedStage = playerData.tutorialQuestStage;
        }

        if (playerData.tutorialQuestStage == 0)
        {
            // Ẩn bảng nhiệm vụ an toàn (không dùng SetActive(false) để tránh tắt luôn script)
            questText.text = "";
            Image panelImg = questPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.enabled = false;
        }
        else if (playerData.tutorialQuestStage == 1)
        {
            // Đã nhận Pet, đi thu phục
            Image panelImg = questPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.enabled = true;

            questText.text = "Nhiệm vụ: Vào bãi cỏ, chiến đấu và thu phục 1 con Pet hoang dã.";
            questText.color = Color.black; // Chuyển sang màu đen cho dễ nhìn trên nền cỏ
        }
        else if (playerData.tutorialQuestStage == 2)
        {
            // Đã thu phục xong
            Image panelImg = questPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.enabled = true;

            questText.text = "Nhiệm vụ hoàn thành! Bạn đã sẵn sàng khám phá thế giới.";
            questText.color = Color.blue; // Chuyển sang màu xanh dương
        }
    }
}
