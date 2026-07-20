using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiển thị Image 2 và chỉ số Before/After
/// cho một panel Enhance.
/// </summary>
[DisallowMultipleComponent]
public class EnhancePanelStatsUI : MonoBehaviour
{
    [Header("Ảnh tiến hóa")]
    [Tooltip("Kéo Image lớn hiển thị Pet tiến hóa vào đây.")]
    [SerializeField] private Image evolutionPetImage;

    [Header("BEFORE")]
    [SerializeField] private TMP_Text beforeAttackText;
    [SerializeField] private TMP_Text beforeHealthText;
    [SerializeField] private TMP_Text beforeDefenseText;
    [SerializeField] private TMP_Text beforeSpeedText;

    [Header("AFTER")]
    [SerializeField] private TMP_Text afterAttackText;
    [SerializeField] private TMP_Text afterHealthText;
    [SerializeField] private TMP_Text afterDefenseText;
    [SerializeField] private TMP_Text afterSpeedText;

    [Header("Thiết lập")]
    [Tooltip("1.5 là tăng thêm 50%.")]
    [SerializeField, Min(0f)]
    private float afterMultiplier = 1.5f;

    private RuntimeBeastData currentPet;

    public void Display(RuntimeBeastData pet)
    {
        currentPet = pet;

        if (pet == null || pet.baseBeast == null)
        {
            Clear();
            return;
        }

        BeastData beastData = pet.baseBeast;

        // Luôn hiện Image 2.
        // Không phụ thuộc Evolve Target.
        SetImage(
            evolutionPetImage,
            beastData.image2
        );

        if (beastData.image2 == null)
        {
            Debug.LogWarning(
                $"[EnhancePanelStatsUI] BeastData " +
                $"'{beastData.beastName}' chưa được gán Image 2.",
                beastData
            );
        }

        int currentAttack = beastData.attack;
        int currentHealth = beastData.maxHP;
        int currentDefense = beastData.defense;
        int currentSpeed = beastData.speed;

        // BEFORE.
        SetNumber(beforeAttackText, currentAttack);
        SetNumber(beforeHealthText, currentHealth);
        SetNumber(beforeDefenseText, currentDefense);
        SetNumber(beforeSpeedText, currentSpeed);

        // AFTER = BEFORE × 1.5.
        SetNumber(
            afterAttackText,
            CalculateAfter(currentAttack)
        );

        SetNumber(
            afterHealthText,
            CalculateAfter(currentHealth)
        );

        SetNumber(
            afterDefenseText,
            CalculateAfter(currentDefense)
        );

        SetNumber(
            afterSpeedText,
            CalculateAfter(currentSpeed)
        );
    }

    public void Refresh()
    {
        Display(currentPet);
    }

    public void Clear()
    {
        currentPet = null;

        SetImage(evolutionPetImage, null);

        SetText(beforeAttackText, "-");
        SetText(beforeHealthText, "-");
        SetText(beforeDefenseText, "-");
        SetText(beforeSpeedText, "-");

        SetText(afterAttackText, "-");
        SetText(afterHealthText, "-");
        SetText(afterDefenseText, "-");
        SetText(afterSpeedText, "-");
    }

    private int CalculateAfter(int currentValue)
    {
        return Mathf.RoundToInt(
            Mathf.Max(0, currentValue) *
            Mathf.Max(0f, afterMultiplier)
        );
    }

    private static void SetNumber(
        TMP_Text target,
        int value
    )
    {
        SetText(
            target,
            Mathf.Max(0, value).ToString("N0")
        );
    }

    private static void SetText(
        TMP_Text target,
        string value
    )
    {
        if (target != null)
            target.text = value;
    }

    private static void SetImage(
        Image target,
        Sprite sprite
    )
    {
        if (target == null)
        {
            Debug.LogWarning(
                "[EnhancePanelStatsUI] Chưa gán Evolution Pet Image."
            );
            return;
        }

        target.sprite = sprite;
        target.enabled = sprite != null;
        target.preserveAspect = true;

        Color color = target.color;
        color.a = 1f;
        target.color = color;

        if (sprite != null)
        {
            target.gameObject.SetActive(true);
        }
    }
}