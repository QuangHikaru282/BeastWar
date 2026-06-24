using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{
    [Header("Nút")]
    [SerializeField] private Button questButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button claimRewardButton;

    [Header("Panel")]
    [SerializeField] private GameObject questOverlay;
    [SerializeField] private GameObject questDetailPanel;
    [SerializeField] private GameObject emptyQuestText;

    [Header("Danh sách nhiệm vụ")]
    [SerializeField] private RectTransform questListContent;
    [SerializeField] private QuestListItemUI questItemPrefab;
    [SerializeField] private ScrollRect questScrollRect;

    [Header("Thông tin nhiệm vụ")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questStateText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text progressText;

    [Header("Phần thưởng")]
    [SerializeField] private RectTransform rewardContent;
    [SerializeField] private QuestRewardItemUI rewardItemPrefab;

    [Header("Liên kết cửa hàng")]
    [SerializeField] private ShopManager shopManager;

    [Header("Nhiệm vụ đang nhận")]
    [SerializeField]
    private List<QuestData> acceptedQuests =
        new List<QuestData>();

    private readonly List<QuestListItemUI> spawnedQuestItems =
        new List<QuestListItemUI>();

    private QuestData selectedQuest;

    private void Awake()
    {
        if (questButton != null)
        {
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(ToggleQuestPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseQuestPanel);
        }

        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.RemoveAllListeners();
            claimRewardButton.onClick.AddListener(
                ClaimSelectedQuestReward
            );
        }

        if (questOverlay != null)
        {
            questOverlay.SetActive(false);
        }

        HideQuestDetails();
    }

    public void ToggleQuestPanel()
    {
        if (questOverlay == null)
        {
            Debug.LogError(
                "QuestUIManager chưa được gán QuestOverlay."
            );

            return;
        }

        if (questOverlay.activeSelf)
        {
            CloseQuestPanel();
        }
        else
        {
            OpenQuestPanel();
        }
    }

    public void OpenQuestPanel()
    {
        if (questOverlay == null)
        {
            return;
        }

        questOverlay.SetActive(true);
        selectedQuest = null;

        HideQuestDetails();
        RebuildQuestList();
    }

    public void CloseQuestPanel()
    {
        if (questOverlay != null)
        {
            questOverlay.SetActive(false);
        }
    }

    public void RebuildQuestList()
    {
        if (questListContent == null ||
            questItemPrefab == null)
        {
            Debug.LogError(
                "Chưa gán QuestListContent hoặc QuestItemPrefab."
            );

            return;
        }

        spawnedQuestItems.Clear();

        for (int i = questListContent.childCount - 1;
             i >= 0;
             i--)
        {
            GameObject child =
                questListContent.GetChild(i).gameObject;

            child.SetActive(false);
            Destroy(child);
        }

        bool hasVisibleQuest = false;

        foreach (QuestData quest in acceptedQuests)
        {
            if (quest == null)
            {
                continue;
            }

            // Đã nhận thưởng thì không còn nằm trong
            // danh sách nhiệm vụ đang nhận.
            if (quest.state == QuestState.RewardClaimed)
            {
                continue;
            }

            hasVisibleQuest = true;

            QuestListItemUI newItem = Instantiate(
                questItemPrefab,
                questListContent
            );

            newItem.gameObject.SetActive(true);
            newItem.Setup(quest, this);

            spawnedQuestItems.Add(newItem);
        }

        if (emptyQuestText != null)
        {
            emptyQuestText.SetActive(!hasVisibleQuest);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            questListContent
        );

        if (questScrollRect != null)
        {
            questScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public void ShowQuestDetails(QuestData quest)
    {
        if (quest == null)
        {
            return;
        }

        selectedQuest = quest;

        if (questDetailPanel != null)
        {
            questDetailPanel.SetActive(true);
        }

        if (questNameText != null)
        {
            questNameText.text = quest.questName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = quest.description;
        }

        if (objectiveText != null)
        {
            objectiveText.text = quest.objectiveText;
        }

        if (progressText != null)
        {
            progressText.text =
                "Tiến độ: " +
                quest.currentAmount +
                "/" +
                Mathf.Max(1, quest.requiredAmount);
        }

        UpdateStateText(quest);
        CreateRewardList(quest);

        if (claimRewardButton != null)
        {
            claimRewardButton.gameObject.SetActive(
                quest.state == QuestState.Completed
            );
        }
    }

    private void UpdateStateText(QuestData quest)
    {
        if (questStateText == null)
        {
            return;
        }

        switch (quest.state)
        {
            case QuestState.InProgress:
                questStateText.text = "Đang thực hiện";
                break;

            case QuestState.Completed:
                questStateText.text = "Hoàn thành";
                break;

            case QuestState.RewardClaimed:
                questStateText.text = "Đã nhận thưởng";
                break;
        }
    }

    private void CreateRewardList(QuestData quest)
    {
        if (rewardContent == null ||
            rewardItemPrefab == null)
        {
            return;
        }

        for (int i = rewardContent.childCount - 1;
             i >= 0;
             i--)
        {
            GameObject child =
                rewardContent.GetChild(i).gameObject;

            child.SetActive(false);
            Destroy(child);
        }

        foreach (QuestRewardData reward in quest.rewards)
        {
            if (reward == null)
            {
                continue;
            }

            QuestRewardItemUI rewardUI = Instantiate(
                rewardItemPrefab,
                rewardContent
            );

            rewardUI.gameObject.SetActive(true);
            rewardUI.Setup(reward);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            rewardContent
        );
    }

    public void NotifyItemPurchased(string purchasedItemID)
    {
        Debug.Log(
            "QuestUIManager nhận được item đã mua: " +
            purchasedItemID
        );

        bool questChanged = false;

        foreach (QuestData quest in acceptedQuests)
        {
            if (quest == null)
            {
                continue;
            }

            Debug.Log(
                "Kiểm tra nhiệm vụ: " +
                quest.questName +
                " | State: " +
                quest.state +
                " | Target ID: " +
                quest.targetItemID
            );

            if (quest.state != QuestState.InProgress)
            {
                continue;
            }

            // Target Item ID để trống:
            // mua item nào cũng được tính.
            bool matchesItem =
                string.IsNullOrWhiteSpace(quest.targetItemID);

            // Nếu nhiệm vụ yêu cầu item cụ thể
            if (!matchesItem)
            {
                matchesItem = string.Equals(
                    quest.targetItemID.Trim(),
                    purchasedItemID.Trim(),
                    System.StringComparison.OrdinalIgnoreCase
                );
            }

            if (!matchesItem)
            {
                Debug.Log(
                    "Item không khớp với nhiệm vụ: " +
                    quest.questName
                );

                continue;
            }

            int required = Mathf.Max(
                1,
                quest.requiredAmount
            );

            quest.currentAmount = Mathf.Clamp(
                quest.currentAmount + 1,
                0,
                required
            );

            if (quest.currentAmount >= required)
            {
                quest.state = QuestState.Completed;

                Debug.Log(
                    "HOÀN THÀNH NHIỆM VỤ: " +
                    quest.questName
                );
            }

            questChanged = true;
        }

        if (questChanged)
        {
            RefreshVisibleUI();
        }
        else
        {
            Debug.LogWarning(
                "Không có nhiệm vụ mua hàng nào được cập nhật."
            );
        }
    }

    private void ClaimSelectedQuestReward()
    {
        if (selectedQuest == null ||
            selectedQuest.state != QuestState.Completed)
        {
            return;
        }

        foreach (QuestRewardData reward
                 in selectedQuest.rewards)
        {
            if (reward == null)
            {
                continue;
            }

            switch (reward.rewardType)
            {
                case QuestRewardType.Gold:
                    if (shopManager != null)
                    {
                        shopManager.AddGold(reward.amount);
                    }
                    break;

                case QuestRewardType.Item:
                    // Cần nối với Inventory khi bạn làm
                    // hệ thống túi đồ.
                    Debug.Log(
                        "Nhận item thưởng: " +
                        reward.rewardName +
                        " x" +
                        reward.amount
                    );
                    break;
            }
        }

        selectedQuest.state = QuestState.RewardClaimed;

        Debug.Log(
            "Đã nhận thưởng nhiệm vụ: " +
            selectedQuest.questName
        );

        selectedQuest = null;

        HideQuestDetails();
        RebuildQuestList();
    }

    private void RefreshVisibleUI()
    {
        foreach (QuestListItemUI item in spawnedQuestItems)
        {
            if (item != null)
            {
                item.Refresh();
            }
        }

        if (selectedQuest != null)
        {
            ShowQuestDetails(selectedQuest);
        }
    }

    private void HideQuestDetails()
    {
        if (questDetailPanel != null)
        {
            questDetailPanel.SetActive(false);
        }

        if (claimRewardButton != null)
        {
            claimRewardButton.gameObject.SetActive(false);
        }
    }
}