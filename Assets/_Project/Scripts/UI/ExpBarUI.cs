using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpBarUI : MonoBehaviour
{
    [SerializeField] private Slider expSlider;
    [Header("Level Text (Tùy chọn)")]
    [SerializeField] private TextMeshProUGUI levelTextTMP;
    [SerializeField] private Text levelTextLegacy;

    [Header("Exp Text (Tùy chọn)")]
    [SerializeField] private TextMeshProUGUI expTextTMP;
    [SerializeField] private Text expTextLegacy;

    public void Initialize(int maxExp, int currentExp, int currentLevel)
    {
        if (expSlider != null)
        {
            expSlider.maxValue = maxExp;
            expSlider.value = currentExp;
        }

        string lvlStr = $"Lv.{currentLevel}";
        if (levelTextTMP != null)
        {
            levelTextTMP.text = lvlStr;
        }
        if (levelTextLegacy != null)
        {
            levelTextLegacy.text = lvlStr;
        }

        string expStr = $"{currentExp}/{maxExp}";
        if (expTextTMP != null)
        {
            expTextTMP.text = expStr;
        }
        if (expTextLegacy != null)
        {
            expTextLegacy.text = expStr;
        }
    }
}
