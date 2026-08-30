using TMPro;
using UnityEngine;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text stateText;

    private int questID;

    public void Setup(int id)
    {
        questID = id;
        Refresh();
    }

    public void Refresh()
    {
        if (QuestManager.Instance == null)
            return;

        QuestManager qm = QuestManager.Instance;

        titleText.text = qm.GetQuestTitle(questID);
        descriptionText.text = qm.GetQuestDescription(questID);

        int current = qm.playerData.currentMainQuestId;

        bool isReady = qm.IsCurrentQuestReadyToClaim();

        if (questID < current)
        {
            stateText.text = "Đã xong";
            stateText.color = Color.green;
        }
        else if (questID == current)
        {
            if (isReady)
            {
                stateText.text = "Nhận Thưởng";
                stateText.color = Color.cyan;
            }
            else
            {
                stateText.text = "Đang làm";
                stateText.color = Color.yellow;
            }
        }
        else
        {
            stateText.text = "Chưa mở";
            stateText.color = Color.gray;
        }

        UnityEngine.UI.Button btn = GetComponent<UnityEngine.UI.Button>();
        if (btn == null) btn = gameObject.AddComponent<UnityEngine.UI.Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        if (QuestManager.Instance == null) return;

        QuestManager qm = QuestManager.Instance;
        int current = qm.playerData.currentMainQuestId;

        // Khi bấm vào Nhiệm vụ đang làm mà đã hoàn thành mục tiêu (Hoặc bấm Nhận thưởng)
        if (questID == current && qm.IsCurrentQuestReadyToClaim())
        {
            qm.AdvanceQuest();
        }
    }
}