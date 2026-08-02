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

    [Header("Nút Nhận Thưởng (Kéo thả Nút bấm vào đây)")]
    [Tooltip("Nút bấm Nhận Thưởng bên trong QuestPanel")]
    [SerializeField] private Button claimRewardButton;

    private QuestManager subscribedManager;

    private void Awake()
    {
        AutoBindAllReferences();
    }



    private void AutoBindAllReferences()
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (var txt in texts)
        {
            string n = txt.name;
            string nl = n.ToLower();
            if (questTitleText == null && (n == "QuestTitleText" || nl.Contains("title") || nl.Contains("tieude"))) questTitleText = txt;
            if (questText == null && (n == "QuestText" || nl.Contains("desc") || nl.Contains("noidung"))) questText = txt;
            if (goldRewardText == null && (nl.Contains("gold") || nl.Contains("vang"))) goldRewardText = txt;
            if (expRewardText == null && (nl.Contains("exp") || nl.Contains("kinhnghiem"))) expRewardText = txt;
        }

        Transform itemTr = transform.Find("QuestUI/Item");
        if (itemTr == null) itemTr = transform.Find("Item");
        if (itemTr == null) itemTr = transform.Find("MainContent/Item");
        if (itemTr != null)
        {
            itemObject = itemTr.gameObject;
            if (itemRewardIcon == null)
            {
                Transform iconTr = itemTr.Find("Icon");
                if (iconTr != null) itemRewardIcon = iconTr.GetComponent<Image>();
                if (itemRewardIcon == null) itemRewardIcon = itemTr.GetComponentInChildren<Image>(true);
            }
            if (itemRewardText == null) itemRewardText = itemTr.GetComponentInChildren<TMP_Text>(true);
        }

        // Tự động tắt các Image icon bị trống sprite (tránh hiện ô vuông màu trắng)
        Image[] images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.sprite == null && (img.name.ToLower().Contains("icon") || img.name.ToLower().Contains("gold") || img.name.ToLower().Contains("exp")))
            {
                img.enabled = false;
            }
        }

        AutoFindClaimButton();
    }

    private void AutoFindClaimButton()
    {
        if (claimRewardButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                string n = btn.name.ToLower();
                // Bỏ qua nút ClosePanel
                if (n.Contains("close")) continue;

                if (n.Contains("claim") || n.Contains("reward") || n.Contains("nhan") || n.Contains("thuong") || n.Contains("item") || n.Contains("button"))
                {
                    claimRewardButton = btn;
                    break;
                }
            }
        }

        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.RemoveAllListeners();
            claimRewardButton.onClick.AddListener(OnClaimRewardButtonClicked);
        }
    }

    private void OnClaimRewardButtonClicked()
    {
        if (QuestManager.Instance != null)
        {
            // Tự động đóng QuestPanel để hiển thị Bảng Nhận Thưởng RewardUI
            QuestPanelController controller = FindFirstObjectByType<QuestPanelController>();
            if (controller != null)
            {
                controller.CloseQuestPanel();
            }
            else
            {
                if (transform.parent != null) transform.parent.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }

            QuestManager.Instance.AdvanceQuest();
        }
    }

    private void OnEnable()
    {
        transform.SetAsLastSibling();
        if (transform.parent != null)
        {
            transform.parent.SetAsLastSibling();
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 100;
        }

        InteractHintManager.Instance?.RegisterPanelOpen();

        AutoBindAllReferences();
        RefreshQuestUI();
    }

    private void OnDisable()
    {
        InteractHintManager.Instance?.RegisterPanelClose();
    }

    private void Start()
    {
        AutoBindAllReferences();
        RefreshQuestUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            QuestPanelController controller = FindFirstObjectByType<QuestPanelController>();
            if (controller != null)
            {
                controller.ToggleQuestPanel();
            }
            else
            {
                bool newState = !gameObject.activeSelf;
                gameObject.SetActive(newState);
                if (transform.parent != null && transform.parent != transform.root)
                {
                    transform.parent.gameObject.SetActive(newState);
                }
            }
        }

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
        if (questId < 0 || questId >= manager.GetTotalQuestCount())
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
            bool hasExp = reward != null && reward.exp > 0;
            expRewardText.text = hasExp ? $"{reward.exp} EXP" : string.Empty;
            expRewardText.gameObject.SetActive(hasExp);
        }

        RefreshItemReward(reward);

        if (claimRewardButton != null)
        {
            bool isReady = manager.IsCurrentQuestReadyToClaim();
            claimRewardButton.gameObject.SetActive(isReady);
            claimRewardButton.interactable = isReady;
        }
    }

    private void RefreshItemReward(QuestRewardInfo reward)
    {
        bool hasItem =
            reward != null &&
            !string.IsNullOrWhiteSpace(reward.itemName);

        if (itemObject != null)
        {
            itemObject.SetActive(hasItem);
        }

        // Đảm bảo quét trực tiếp GameObject "Item" dưới QuestUI
        Transform itemTr = itemObject != null ? itemObject.transform : transform.Find("QuestUI/Item");
        if (itemTr == null) itemTr = transform.Find("Item");
        if (itemTr == null)
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Item") { itemTr = t; break; }
            }
        }

        if (itemTr != null)
        {
            itemTr.gameObject.SetActive(hasItem);

            TMP_Text txt = itemRewardText != null ? itemRewardText : itemTr.GetComponentInChildren<TMP_Text>(true);
            if (txt != null)
            {
                txt.text = hasItem ? reward.itemName : string.Empty;
                txt.enabled = hasItem;
            }

            Image img = itemRewardIcon != null ? itemRewardIcon : itemTr.GetComponentInChildren<Image>(true);
            if (img != null)
            {
                img.sprite = hasItem ? reward.itemIcon : null;
                img.enabled = hasItem && reward.itemIcon != null;
            }
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