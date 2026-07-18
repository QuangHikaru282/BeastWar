using UnityEngine;

public class SkillContentUI : MonoBehaviour
{
    private const int MaximumSkillSlots = 4;

    [Header("4 ô Skill cố định")]
    [SerializeField]
    private SkillSlotUI[] skillSlots =
        new SkillSlotUI[MaximumSkillSlots];

    private RuntimeBeastData currentPet;
    private PlayerData playerData;

    private System.Action onSkillUpgradedCallback;

    private void Awake()
    {
        HideAllSlots();
    }

    public void Display(RuntimeBeastData pet, PlayerData pData, System.Action onSkillUpgraded)
    {
        currentPet = pet;
        playerData = pData;
        onSkillUpgradedCallback = onSkillUpgraded;

        HideAllSlots();

        if (currentPet == null || currentPet.moves == null)
            return;

        int slotIndex = 0;

        for (int skillIndex = 0;
             skillIndex < currentPet.moves.Length;
             skillIndex++)
        {
            RuntimeMoveData skillEntry =
                currentPet.moves[skillIndex];

            if (skillEntry == null || skillEntry.baseMove == null)
                continue;

            if (slotIndex >= MaximumSkillSlots ||
                slotIndex >= skillSlots.Length)
            {
                Debug.LogWarning(
                    $"{currentPet.baseBeast.beastName} có nhiều hơn 4 kỹ năng. " +
                    "Chỉ 4 kỹ năng đầu tiên được hiển thị."
                );

                break;
            }

            SkillSlotUI slot = skillSlots[slotIndex];

            if (slot != null)
                slot.Setup(skillEntry, playerData, onSkillUpgraded);

            slotIndex++;
        }

        // Những slot còn dư tiếp tục được ẩn.
        for (int i = slotIndex; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
                skillSlots[i].Hide();
        }
    }

    public void Refresh()
    {
        Display(currentPet, playerData, onSkillUpgradedCallback);
    }

    public void Clear()
    {
        currentPet = null;
        HideAllSlots();
    }

    private void HideAllSlots()
    {
        if (skillSlots == null)
            return;

        foreach (SkillSlotUI slot in skillSlots)
        {
            if (slot != null)
                slot.Hide();
        }
    }
}