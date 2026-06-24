using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestListItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Button selectButton;

    private QuestData questData;
    private QuestUIManager questUIManager;

    private void Awake()
    {
        if (selectButton == null)
        {
            selectButton = GetComponent<Button>();
        }
    }

    public void Setup(
        QuestData data,
        QuestUIManager manager
    )
    {
        questData = data;
        questUIManager = manager;

        Refresh();

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(SelectQuest);
        }
    }

    public void Refresh()
    {
        if (questData == null)
        {
            return;
        }

        if (questNameText != null)
        {
            questNameText.text = questData.questName;
        }

        if (progressText != null)
        {
            int required = Mathf.Max(
                1,
                questData.requiredAmount
            );

            progressText.text =
                questData.currentAmount +
                "/" +
                required;
        }

        if (stateText != null)
        {
            switch (questData.state)
            {
                case QuestState.InProgress:
                    stateText.text = "Đang thực hiện";
                    break;

                case QuestState.Completed:
                    stateText.text = "Hoàn thành";
                    break;

                case QuestState.RewardClaimed:
                    stateText.text = "Đã nhận thưởng";
                    break;
            }
        }
    }

    private void SelectQuest()
    {
        if (questData == null || questUIManager == null)
        {
            return;
        }

        questUIManager.ShowQuestDetails(questData);
    }
}