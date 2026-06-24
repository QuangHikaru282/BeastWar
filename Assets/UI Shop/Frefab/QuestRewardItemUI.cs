using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRewardItemUI : MonoBehaviour
{
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardNameText;
    [SerializeField] private TMP_Text rewardAmountText;

    public void Setup(QuestRewardData reward)
    {
        if (reward == null)
        {
            return;
        }

        if (rewardIcon != null)
        {
            rewardIcon.sprite = reward.rewardIcon;
            rewardIcon.enabled = reward.rewardIcon != null;
            rewardIcon.preserveAspect = true;
        }

        if (rewardNameText != null)
        {
            rewardNameText.text = reward.rewardName;
        }

        if (rewardAmountText != null)
        {
            rewardAmountText.text = "x" + reward.amount;
        }
    }
}