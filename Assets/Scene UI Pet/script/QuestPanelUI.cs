using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanelUI : MonoBehaviour
{
    [Header("Các thành phần trong QuestPanel")]

    [Tooltip("Text hiển thị tiêu đề nhiệm vụ")]
    [SerializeField] private TMP_Text questTitleText;

    [Tooltip("Text hiển thị nội dung và tiến trình nhiệm vụ")]
    [SerializeField] private TMP_Text questText;

    [Tooltip("Text hiển thị phần thưởng vàng")]
    [SerializeField] private TMP_Text goldRewardText;

    [Tooltip("Text hiển thị phần thưởng EXP")]
    [SerializeField] private TMP_Text expRewardText;

    [Header("Phần thưởng Item")]

    [Tooltip("GameObject Item trong QuestPanel")]
    [SerializeField] private GameObject itemObject;

    [Tooltip("Text tên vật phẩm bên trong Item")]
    [SerializeField] private TMP_Text itemRewardText;

    [Tooltip("Image icon vật phẩm bên trong Item")]
    [SerializeField] private Image itemRewardIcon;

    private QuestManager subscribedManager;

    private void OnEnable()
    {
        RefreshQuestUI();
    }

    private void Start()
    {
        RefreshQuestUI();
    }

    private void Update()
    {
        /*
         * Tạm thời cập nhật liên tục để các tiến trình như:
         * 2/3 Beast, 50/100 Vàng... hiển thị ngay.
         *
         * Khi hệ thống hoàn chỉnh, có thể thay bằng event
         * để tối ưu hiệu năng.
         */
        RefreshQuestUI();
    }

    public void RefreshQuestUI()
    {
        QuestManager manager = QuestManager.Instance;

        if (manager == null)
        {
            SetMissingManagerUI();
            return;
        }

        if (manager.playerData == null)
        {
            SetMissingPlayerDataUI();
            return;
        }

        int questId = manager.playerData.currentMainQuestId;

        // Đã hoàn thành tất cả nhiệm vụ.
        if (questId < 0 || questId >= 25)
        {
            SetAllQuestsCompletedUI();
            return;
        }

        // Hiển thị tiêu đề.
        if (questTitleText != null)
        {
            questTitleText.text = manager.GetCurrentQuestTitle();
        }

        // Hiển thị mô tả và tiến trình.
        if (questText != null)
        {
            questText.text = manager.GetCurrentQuestDescription();
        }

        // Lấy phần thưởng của nhiệm vụ hiện tại.
        QuestRewardInfo reward =
            manager.GetQuestRewardInfo(questId);

        if (goldRewardText != null)
        {
            goldRewardText.text = $"{reward.gold} Vàng";
        }

        if (expRewardText != null)
        {
            expRewardText.text = $"{reward.exp} EXP";
        }

        RefreshItemReward(reward);
    }

    private void RefreshItemReward(QuestRewardInfo reward)
    {
        bool hasItem =
            reward != null &&
            !string.IsNullOrWhiteSpace(reward.itemName);

        /*
         * Nếu nhiệm vụ không có vật phẩm thưởng,
         * ẩn cả object Item.
         */
        if (itemObject != null)
        {
            itemObject.SetActive(hasItem);
        }

        if (!hasItem)
        {
            if (itemRewardText != null)
            {
                itemRewardText.text = "";
            }

            if (itemRewardIcon != null)
            {
                itemRewardIcon.sprite = null;
                itemRewardIcon.enabled = false;
            }

            return;
        }

        if (itemRewardText != null)
        {
            itemRewardText.text = reward.itemName;
        }

        if (itemRewardIcon != null)
        {
            itemRewardIcon.sprite = reward.itemIcon;

            // Chỉ bật Image khi nhiệm vụ có icon.
            itemRewardIcon.enabled = reward.itemIcon != null;
        }
    }

    private void SetAllQuestsCompletedUI()
    {
        if (questTitleText != null)
        {
            questTitleText.text = "Hoàn Thành";
        }

        if (questText != null)
        {
            questText.text =
                "Bạn đã hoàn thành tất cả nhiệm vụ hiện tại!";
        }

        if (goldRewardText != null)
        {
            goldRewardText.text = "";
        }

        if (expRewardText != null)
        {
            expRewardText.text = "";
        }

        if (itemObject != null)
        {
            itemObject.SetActive(false);
        }
    }

    private void SetMissingManagerUI()
    {
        if (questTitleText != null)
        {
            questTitleText.text = "Lỗi QuestManager";
        }

        if (questText != null)
        {
            questText.text =
                "Không tìm thấy QuestManager trong game.";
        }
    }

    private void SetMissingPlayerDataUI()
    {
        if (questTitleText != null)
        {
            questTitleText.text = "Lỗi PlayerData";
        }

        if (questText != null)
        {
            questText.text =
                "QuestManager chưa được gán PlayerData.";
        }
    }
}