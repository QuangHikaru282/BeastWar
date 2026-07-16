using System;
using UnityEngine;

[Serializable]
public class PetSkillEntry
{
    [SerializeField] private SkillData skillData;

    [SerializeField, Min(1)]
    private int currentLevel = 1;

    public SkillData SkillData => skillData;

    public int CurrentLevel
    {
        get
        {
            if (skillData == null)
                return 1;

            return Mathf.Clamp(
                currentLevel,
                1,
                skillData.MaxLevel
            );
        }
    }

    public int MaxLevel
    {
        get
        {
            return skillData != null
                ? skillData.MaxLevel
                : 1;
        }
    }

    public bool IsValid => skillData != null;

    public bool IsMaxLevel
    {
        get
        {
            return skillData != null &&
                   CurrentLevel >= skillData.MaxLevel;
        }
    }

    public int UpgradeCost
    {
        get
        {
            if (skillData == null)
                return 0;

            return skillData.GetUpgradeCost(CurrentLevel);
        }
    }

    public bool TryUpgrade()
    {
        if (skillData == null || IsMaxLevel)
            return false;

        currentLevel++;
        return true;
    }
}