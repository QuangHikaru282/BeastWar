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

    private PetSkillEntry currentSkill;

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

    public void Setup(PetSkillEntry skillEntry)
    {
        if (skillEntry == null || !skillEntry.IsValid)
        {
            Hide();
            return;
        }

        currentSkill = skillEntry;
        gameObject.SetActive(true);

        RefreshUI();
    }

    public void Hide()
    {
        currentSkill = null;
        gameObject.SetActive(false);
    }

    private void RefreshUI()
    {
        if (currentSkill == null || !currentSkill.IsValid)
        {
            Hide();
            return;
        }

        SkillData data = currentSkill.SkillData;

        SetText(skillNameText, data.SkillName);
        SetText(skillDescriptionText, data.Description);

        if (currentSkill.IsMaxLevel)
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
                $"Lv. {currentSkill.CurrentLevel}/{currentSkill.MaxLevel}"
            );

            SetText(
                upgradeCostText,
                currentSkill.UpgradeCost.ToString("N0")
            );

            if (upgradeButton != null)
                upgradeButton.interactable = true;
        }

        SetImage(skillIcon, data.SkillIcon);
    }

    private void HandleUpgradeButton()
    {
        if (currentSkill == null)
            return;

        bool upgraded = currentSkill.TryUpgrade();

        if (!upgraded)
            return;

        RefreshUI();
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