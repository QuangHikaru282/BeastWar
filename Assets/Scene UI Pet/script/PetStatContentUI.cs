using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetStatContentUI : MonoBehaviour
{
    [Header("Battle Stats")]
    [SerializeField] private TMP_Text healthValueText;
    [SerializeField] private TMP_Text attackValueText;
    [SerializeField] private TMP_Text defenseValueText;
    [SerializeField] private TMP_Text speedValueText;

    [Header("General Info")]
    [SerializeField] private TMP_Text powerValueText;

    // Type không còn dùng TMP_Text.
    // Thay bằng Image để hiển thị icon nguyên tố.
    [SerializeField] private Image elementValueImage;

    [SerializeField] private TMP_Text rarityValueText;
    [SerializeField] private TMP_Text personalityValueText;

    public void Display(RuntimeBeastData pet)
    {
        if (pet == null || pet.baseBeast == null)
        {
            Clear();
            return;
        }

        SetText(healthValueText, FormatNumber(pet.MaxHP));
        SetText(attackValueText, FormatNumber(pet.Attack));
        SetText(defenseValueText, FormatNumber(pet.Defense));
        SetText(speedValueText, FormatNumber(pet.Speed));

        SetText(powerValueText, FormatNumber(pet.CombatPower));
        SetText(rarityValueText, pet.baseBeast.isRare ? "Hiếm" : "Thường");
        SetText(personalityValueText, pet.baseBeast.element.ToString()); // Dùng Tên Hệ thay cho tính cách

        // Hiển thị hình ảnh nguyên tố thay cho chữ.
        // Tạm ẩn icon nguyên tố vì hệ thống hiện dùng Text/Enum
        if (elementValueImage != null) elementValueImage.gameObject.SetActive(false);
    }

    public void Clear()
    {
        SetText(healthValueText, "-");
        SetText(attackValueText, "-");
        SetText(defenseValueText, "-");
        SetText(speedValueText, "-");

        SetText(powerValueText, "-");
        SetText(rarityValueText, "-");
        SetText(personalityValueText, "-");

        SetImage(elementValueImage, null);
    }

    private static string FormatNumber(int value)
    {
        return value.ToString("N0");
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

        // Không có sprite thì ẩn Image để tránh ô trắng.
        target.enabled = sprite != null;
    }
}