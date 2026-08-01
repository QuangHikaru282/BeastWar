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

        if (questID < current)
        {
            stateText.text = "Hoàn thành";
            stateText.color = Color.green;
        }
        else if (questID == current)
        {
            stateText.text = "Đang làm";
            stateText.color = Color.yellow;
        }
        else
        {
            stateText.text = "Chưa mở";
            stateText.color = Color.gray;
        }
    }
}