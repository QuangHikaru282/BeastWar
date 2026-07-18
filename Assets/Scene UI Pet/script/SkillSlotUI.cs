using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("Hình ảnh")]
    [SerializeField] private Image skillIcon;

    [Header("Thông tin")]
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillLevelText;
    [SerializeField] private TMP_Text skillDescriptionText;

    [Header("Nâng cấp")]
    [SerializeField] private Button upgradeButton;

    [Tooltip("Không bắt buộc. Có thể để trống nếu UI chưa có text giá.")]
    [SerializeField] private TMP_Text upgradeCostText;

    private RuntimeMoveData currentMove;
    private PlayerData playerData;
    private System.Action onUpgradeCallback;

    private void Awake()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(HandleUpgradeButton);
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(HandleUpgradeButton);
    }

    public void Setup(RuntimeMoveData moveEntry, PlayerData pData, System.Action onUpgrade)
    {
        if (moveEntry == null || moveEntry.baseMove == null)
        {
            Hide();
            return;
        }

        currentMove = moveEntry;
        playerData = pData;
        onUpgradeCallback = onUpgrade;
        
        gameObject.SetActive(true);

        RefreshUI();
    }

    public void Hide()
    {
        currentMove = null;
        gameObject.SetActive(false);
    }

    private void RefreshUI()
    {
        if (currentMove == null || currentMove.baseMove == null)
        {
            Hide();
            return;
        }

        MoveData data = currentMove.baseMove;

        SetText(skillNameText, data.moveName);
        SetText(skillDescriptionText, data.description);

        if (currentMove.currentLevel >= data.maxLevel)
        {
            SetText(skillLevelText, "MAX");
            SetText(upgradeCostText, "MAX");

            if (upgradeButton != null)
                upgradeButton.interactable = false;
        }
        else
        {
            SetText(
                skillLevelText,
                $"Lv. {currentMove.currentLevel}/{data.maxLevel}"
            );

            int cost = currentMove.GetUpgradeCost();
            SetText(
                upgradeCostText,
                cost.ToString("N0")
            );

            if (upgradeButton != null)
                upgradeButton.interactable = (playerData != null && playerData.gold >= cost);
        }

        SetImage(skillIcon, data.icon);
    }

    private void HandleUpgradeButton()
    {
        if (currentMove == null || playerData == null)
            return;

        int cost = currentMove.GetUpgradeCost();
        if (currentMove.currentLevel < currentMove.baseMove.maxLevel && playerData.gold >= cost)
        {
            playerData.gold -= cost;
            currentMove.currentLevel++;
            playerData.Save();
            
            RefreshUI();
            onUpgradeCallback?.Invoke();
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }

    private static void SetImage(Image target, Sprite sprite)
    {
        if (target == null)
            return;

        target.sprite = sprite;
        target.preserveAspect = true;
        target.enabled = sprite != null;
    }
}