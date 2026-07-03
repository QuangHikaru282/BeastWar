using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RewardUIManager : MonoBehaviour
{
    public static RewardUIManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject rewardPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI expText;

    [Header("Hàng Vật Phẩm (Item Row)")]
    public GameObject itemRow;          // Kéo nguyên cả hàng Row_Item vào đây
    public TextMeshProUGUI itemText;    // Kéo Text_Item vào đây
    public Image itemIcon;              // (Tùy chọn) Kéo Icon_Item vào đây nếu muốn đổi ảnh icon vật phẩm

    // Callback để gọi khi bấm nút Tiếp tục (vd: Load lại Scene, hoặc tiếp tục game)
    private Action onContinueCallback;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Chỉ gọi DontDestroyOnLoad nếu là Root GameObject (không có cha) để tránh Warning trong Unity
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Tự động gán nếu chưa kéo trong Inspector
        if (rewardPanel == null)
        {
            rewardPanel = gameObject;
        }

        // Tự động ẩn Panel khi vừa vào game (để người chơi không bị dính bảng mẫu lúc mới bật game)
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Hiển thị bảng thưởng khi thắng trận.
    /// </summary>
    public void ShowBattleReward(int gold, int exp, Action onContinue, string itemRewardName = "", Sprite itemRewardIcon = null)
    {
        if (rewardPanel == null) rewardPanel = gameObject;

        if (titleText != null) titleText.text = "CHIẾN THẮNG!";
        if (goldText != null) goldText.text = $"+{gold} Vàng";
        if (expText != null) expText.text = $"+{exp} EXP";

        SetupItemRow(itemRewardName, itemRewardIcon);

        onContinueCallback = onContinue;
        rewardPanel.SetActive(true);
    }

    /// <summary>
    /// Hiển thị bảng thưởng khi hoàn thành nhiệm vụ.
    /// </summary>
    public void ShowQuestReward(int gold, int exp, string itemRewardName, Action onContinue, Sprite itemRewardIcon = null)
    {
        if (rewardPanel == null) rewardPanel = gameObject;

        if (titleText != null) titleText.text = "HOÀN THÀNH NHIỆM VỤ!";
        if (goldText != null) goldText.text = $"+{gold} Vàng";
        if (expText != null) expText.text = $"+{exp} EXP";

        SetupItemRow(itemRewardName, itemRewardIcon);

        onContinueCallback = onContinue;
        rewardPanel.SetActive(true);
    }

    /// <summary>
    /// Xử lý ẩn/hiện linh hoạt cả Hàng Vật Phẩm tùy theo nhiệm vụ hoặc trận đánh có quà hay không.
    /// </summary>
    private void SetupItemRow(string itemRewardName, Sprite customIcon = null)
    {
        if (itemRow == null && itemText != null)
        {
            itemRow = itemText.transform.parent != null ? itemText.transform.parent.gameObject : itemText.gameObject;
        }

        if (!string.IsNullOrEmpty(itemRewardName))
        {
            if (itemRow != null) itemRow.SetActive(true);
            if (itemText != null) itemText.text = itemRewardName;
            if (itemIcon != null && customIcon != null) itemIcon.sprite = customIcon;
        }
        else
        {
            // Nếu nhiệm vụ không có phần thưởng vật phẩm -> Ẩn nguyên cả hàng Icon + Text đi cho gọn
            if (itemRow != null) itemRow.SetActive(false);
        }
    }

    /// <summary>
    /// Gọi khi bấm nút "Tiếp Tục" trên giao diện.
    /// </summary>
    public void OnContinueClicked()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        if (onContinueCallback != null)
        {
            onContinueCallback.Invoke();
            onContinueCallback = null;
        }
    }
}
