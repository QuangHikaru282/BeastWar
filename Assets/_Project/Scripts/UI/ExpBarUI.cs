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

        UpdateText(currentLevel, currentExp, maxExp);
    }

    public System.Collections.IEnumerator AnimateExpIncrease(int startExp, int expGained, int startLevel, System.Func<int, int> getExpToNextLevelFunc, System.Action<int> onLevelUpCallback = null)
    {
        if (expSlider == null) yield break;

        int currentLvl = startLevel;
        int currentExp = startExp;
        int remainingExpGained = expGained;

        int initialMax = getExpToNextLevelFunc != null ? getExpToNextLevelFunc(currentLvl) : (expSlider.maxValue > 0 ? (int)expSlider.maxValue : 200);
        UpdateText(currentLvl, currentExp, initialMax);

        while (remainingExpGained > 0)
        {
            int maxExpForLvl = getExpToNextLevelFunc != null ? getExpToNextLevelFunc(currentLvl) : (int)expSlider.maxValue;
            if (maxExpForLvl <= 0) maxExpForLvl = 200;

            expSlider.maxValue = maxExpForLvl;

            int neededForLevelUp = maxExpForLvl - currentExp;
            int expThisStep = Mathf.Min(remainingExpGained, neededForLevelUp);
            int targetExp = currentExp + expThisStep;

            float duration = 1.2f * ((float)expThisStep / maxExpForLvl);
            if (duration < 0.4f) duration = 0.4f;

            float elapsedTime = 0f;
            float startVal = currentExp;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                float val = Mathf.Lerp(startVal, targetExp, t);
                expSlider.value = val;
                UpdateText(currentLvl, Mathf.RoundToInt(val), maxExpForLvl);
                yield return null;
            }

            expSlider.value = targetExp;
            remainingExpGained -= expThisStep;
            currentExp = targetExp;

            if (currentExp >= maxExpForLvl)
            {
                currentLvl++;
                currentExp = 0;
                expSlider.value = 0;
                int nextMax = getExpToNextLevelFunc != null ? getExpToNextLevelFunc(currentLvl) : maxExpForLvl;
                UpdateText(currentLvl, 0, nextMax);

                if (onLevelUpCallback != null)
                {
                    onLevelUpCallback.Invoke(currentLvl);
                }

                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    private void UpdateText(int level, int exp, int maxExp)
    {
        string lvlStr = $"Lv.{level}";
        if (levelTextTMP != null) levelTextTMP.text = lvlStr;
        if (levelTextLegacy != null) levelTextLegacy.text = lvlStr;

        string expStr = $"{exp}/{maxExp}";
        if (expTextTMP != null) expTextTMP.text = expStr;
        if (expTextLegacy != null) expTextLegacy.text = expStr;
    }
}
