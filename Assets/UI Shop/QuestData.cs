using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    InProgress,
    Completed,
    RewardClaimed
}

public enum QuestRewardType
{
    Gold,
    Item
}

[Serializable]
public class QuestRewardData
{
    public QuestRewardType rewardType;

    public string rewardName;

    public Sprite rewardIcon;

    [Min(1)]
    public int amount = 1;

    [Tooltip("Chỉ dùng khi Reward Type là Item")]
    public string rewardItemID;
}

[Serializable]
public class QuestData
{
    [Header("Thông tin nhiệm vụ")]
    public string questID;

    public string questName;

    [TextArea(3, 6)]
    public string description;

    [Header("Mục tiêu mua hàng")]
    [Tooltip(
        "Để trống nếu người chơi chỉ cần mua một item bất kỳ. " +
        "Điền itemID nếu phải mua đúng một item cụ thể."
    )]
    public string targetItemID;

    public string objectiveText;

    [Min(1)]
    public int requiredAmount = 1;

    [Min(0)]
    public int currentAmount;

    public QuestState state = QuestState.InProgress;

    [Header("Phần thưởng")]
    public List<QuestRewardData> rewards =
        new List<QuestRewardData>();

    public bool MatchesPurchasedItem(string purchasedItemID)
    {
        // Target Item ID để trống nghĩa là mua item nào cũng được.
        if (string.IsNullOrWhiteSpace(targetItemID))
        {
            return true;
        }

        return string.Equals(
            targetItemID.Trim(),
            purchasedItemID.Trim(),
            StringComparison.OrdinalIgnoreCase
        );
    }
}