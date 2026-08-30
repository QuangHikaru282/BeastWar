using UnityEngine;
using TMPro;

/// <summary>
/// Hiển thị danh sách tất cả nhiệm vụ dưới dạng compact tracker ở góc phải màn hình.
/// Mỗi dòng gồm: Tên nhiệm vụ + trạng thái (Đang làm / Xong).
/// </summary>
public class QuestTrackerUI : MonoBehaviour
{
    [Header("Prefab một dòng quest")]
    [Tooltip("Kéo prefab QuestTrackerItem vào đây")]
    [SerializeField] private QuestTrackerItem itemPrefab;

    [Header("Container chứa các dòng quest")]
    [Tooltip("Kéo Content của ScrollRect vào đây")]
    [SerializeField] private Transform listContainer;

    [Header("Cập nhật")]
    [SerializeField] private float refreshInterval = 0.5f;

    private float timer = 0f;
    private int lastQuestId = -1;

    private void OnEnable()
    {
        RefreshList();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= refreshInterval)
        {
            timer = 0f;
            int currentId = GetCurrentQuestId();
            if (currentId != lastQuestId)
            {
                lastQuestId = currentId;
                RefreshList();
            }
        }
    }

    private int GetCurrentQuestId()
    {
        if (QuestManager.Instance == null) return 0;
        var pd = QuestManager.Instance.playerData;
        if (pd == null) return 0;
        return pd.currentMainQuestId;
    }

    public void RefreshList()
    {
        if (listContainer == null || itemPrefab == null) return;
        if (QuestManager.Instance == null) return;

        // Xóa các dòng cũ
        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        var pd = QuestManager.Instance.playerData;
        if (pd == null) return;

        int currentQuestId = pd.currentMainQuestId;
        int totalQuests = QuestManager.Instance.GetTotalQuestCount();

        for (int i = 0; i < totalQuests; i++)
        {
            string title = QuestManager.Instance.GetQuestTitle(i);
            bool isDone = i < currentQuestId;
            bool isCurrent = i == currentQuestId;

            var item = Instantiate(itemPrefab, listContainer);
            item.Setup(title, isDone, isCurrent);
        }
    }
}
