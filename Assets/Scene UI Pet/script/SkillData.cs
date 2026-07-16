using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSkillData",
    menuName = "BeastWar/Skill Data"
)]
public class SkillData : ScriptableObject
{
    [Header("Thông tin kỹ năng")]
    [SerializeField] private string skillId;
    [SerializeField] private string skillName = "New Skill";

    [TextArea(2, 4)]
    [SerializeField] private string description;

    [SerializeField] private Sprite skillIcon;

    [Header("Cấp độ")]
    [SerializeField, Min(1)] private int maxLevel = 5;

    [Header("Chi phí nâng cấp")]
    [SerializeField, Min(0)] private int baseUpgradeCost = 100;
    [SerializeField, Min(0)] private int additionalCostPerLevel = 50;

    public string SkillId => skillId;
    public string SkillName => skillName;
    public string Description => description;
    public Sprite SkillIcon => skillIcon;
    public int MaxLevel => maxLevel;

    public int GetUpgradeCost(int currentLevel)
    {
        int safeLevel = Mathf.Max(1, currentLevel);

        return baseUpgradeCost +
               (safeLevel - 1) * additionalCostPerLevel;
    }
}