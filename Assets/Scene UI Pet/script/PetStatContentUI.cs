using TMPro;
using UnityEngine;

public class PetStatContentUI : MonoBehaviour
{
    [Header("Battle Stats")]
    [SerializeField] private TMP_Text healthValueText;
    [SerializeField] private TMP_Text attackValueText;
    [SerializeField] private TMP_Text defenseValueText;
    [SerializeField] private TMP_Text speedValueText;

    [Header("General Info")]
    [SerializeField] private TMP_Text powerValueText;
    [SerializeField] private TMP_Text typeValueText;
    [SerializeField] private TMP_Text rarityValueText;
    [SerializeField] private TMP_Text personalityValueText;

    public void Display(PetData pet)
    {
        if (pet == null)
        {
            Clear();
            return;
        }

        SetText(healthValueText, FormatNumber(pet.Health));
        SetText(attackValueText, FormatNumber(pet.Attack));
        SetText(defenseValueText, FormatNumber(pet.Defense));
        SetText(speedValueText, FormatNumber(pet.Speed));

        SetText(powerValueText, FormatNumber(pet.Power));
        SetText(typeValueText, GetElementName(pet.Element));
        SetText(rarityValueText, pet.Rarity.ToString());
        SetText(personalityValueText, pet.Personality);
    }

    public void Clear()
    {
        SetText(healthValueText, "-");
        SetText(attackValueText, "-");
        SetText(defenseValueText, "-");
        SetText(speedValueText, "-");

        SetText(powerValueText, "-");
        SetText(typeValueText, "-");
        SetText(rarityValueText, "-");
        SetText(personalityValueText, "-");
    }

    private static string FormatNumber(int value)
    {
        return value.ToString("N0");
    }

    private static string GetElementName(PetElement element)
    {
        return element switch
        {
            PetElement.Fire => "Fire",
            PetElement.Water => "Water",
            PetElement.Grass => "Grass",
            PetElement.Electric => "Electric",
            PetElement.Dark => "Dark",
            PetElement.Light => "Light",
            _ => "None"
        };
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }
}