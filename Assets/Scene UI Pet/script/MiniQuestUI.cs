using UnityEngine;

public class MiniQuestUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private QuestItemUI questPrefab;

    private void Start()
    {
        CreateQuestList();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged += RefreshAll;
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged -= RefreshAll;
        }
    }

    private void CreateQuestList()
    {
        // Xóa các QuestItem cũ
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        if (QuestManager.Instance == null) return;

        int currentQuest = QuestManager.Instance.playerData.currentMainQuestId;
        int totalQuest = QuestManager.Instance.GetQuestCount();

        // Chỉ tạo Quest từ Quest hiện tại trở đi
        for (int i = currentQuest; i < totalQuest; i++)
        {
            QuestItemUI item = Instantiate(questPrefab, content);
            item.Setup(i);
        }
    }

    private void RefreshAll()
    {
        // Khi hoàn thành Quest thì tạo lại danh sách
        CreateQuestList();
    }
}