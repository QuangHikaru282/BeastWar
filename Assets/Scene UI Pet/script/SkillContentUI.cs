using UnityEngine;

public class SkillContentUI : MonoBehaviour
{
    private const int MaximumSkillSlots = 4;

    [Header("4 ô Skill cố định")]
    [SerializeField]
    private SkillSlotUI[] skillSlots =
        new SkillSlotUI[MaximumSkillSlots];

    private PetData currentPet;

    private void Awake()
    {
        HideAllSlots();
    }

    public void Display(PetData pet)
    {
        currentPet = pet;

        HideAllSlots();

        if (currentPet == null || currentPet.Skills == null)
            return;

        int slotIndex = 0;

        for (int skillIndex = 0;
             skillIndex < currentPet.Skills.Count;
             skillIndex++)
        {
            PetSkillEntry skillEntry =
                currentPet.Skills[skillIndex];

            if (skillEntry == null || !skillEntry.IsValid)
                continue;

            if (slotIndex >= MaximumSkillSlots ||
                slotIndex >= skillSlots.Length)
            {
                Debug.LogWarning(
                    $"{currentPet.PetName} có nhiều hơn 4 kỹ năng. " +
                    "Chỉ 4 kỹ năng đầu tiên được hiển thị.",
                    currentPet
                );

                break;
            }

            SkillSlotUI slot = skillSlots[slotIndex];

            if (slot != null)
                slot.Setup(skillEntry);

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
        Display(currentPet);
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