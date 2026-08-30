using UnityEngine;

public class MiniQuestUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private QuestItemUI questPrefab;

    private void Start()
    {
        CreateQuestList();

        QuestManager.OnQuestChanged += RefreshAll;
    }

    private void OnDestroy()
    {
        QuestManager.OnQuestChanged -= RefreshAll;
    }

    private void Update()
    {
        // Nhấn phím M để ẩn / hiện khung MiniQuestPanel
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleVisibility();
        }

        // Cập nhật trạng thái các item liên tục (Đang làm -> Nhận Thưởng / Đã xong)
        RefreshItemStates();
    }

    private void RefreshItemStates()
    {
        if (content == null) return;
        foreach (Transform child in content)
        {
            QuestItemUI item = child.GetComponent<QuestItemUI>();
            if (item != null)
            {
                item.Refresh();
            }
        }
    }

    public void ToggleVisibility()
    {
        if (content != null && content.parent != null)
        {
            GameObject target = content.parent.gameObject;
            target.SetActive(!target.activeSelf);
        }
        else
        {
            gameObject.SetActive(!gameObject.activeSelf);
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