using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeDisplayUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI seasonText;
    public Image seasonIcon;

    [Header("Season Icons: Spring, Summer, Autumn, Winter")]
    public Sprite[] seasonSprites = new Sprite[4];

    [Header("Controls")]
    public Button nextSeasonButton;

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged += UpdateTimeDisplay;
            TimeManager.Instance.OnDayChanged += UpdateDateDisplay;
            TimeManager.Instance.OnSeasonChanged += UpdateSeasonDisplay;

            // Initial setup
            UpdateDateDisplay();
            UpdateSeasonDisplay(TimeManager.Instance.currentSeason);
        }

        if (nextSeasonButton != null)
        {
            nextSeasonButton.onClick.AddListener(OnNextSeasonClicked);
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= UpdateTimeDisplay;
            TimeManager.Instance.OnDayChanged -= UpdateDateDisplay;
            TimeManager.Instance.OnSeasonChanged -= UpdateSeasonDisplay;
        }
    }

    private void UpdateTimeDisplay(float timePercent)
    {
        if (timeText == null) return;
        
        // Convert timePercent (0-1) to 24h format
        float hoursFloat = timePercent * 24f;
        int hours = Mathf.FloorToInt(hoursFloat);
        int minutes = Mathf.FloorToInt((hoursFloat - hours) * 60f);

        string ampm = "AM";
        if (hours >= 12)
        {
            ampm = "PM";
            if (hours > 12) hours -= 12;
        }
        if (hours == 0) hours = 12;

        timeText.text = $"{hours:00}:{minutes:00} {ampm}";
    }

    private void UpdateDateDisplay()
    {
        if (dateText == null || TimeManager.Instance == null) return;
        dateText.text = $"Năm {TimeManager.Instance.currentYear}, Ngày {TimeManager.Instance.currentDay}";
    }

    private void UpdateSeasonDisplay(Season currentSeason)
    {
        if (seasonText != null)
        {
            switch (currentSeason)
            {
                case Season.Spring: seasonText.text = "Mùa Xuân"; break;
                case Season.Summer: seasonText.text = "Mùa Hạ"; break;
                case Season.Autumn: seasonText.text = "Mùa Thu"; break;
                case Season.Winter: seasonText.text = "Mùa Đông"; break;
            }
        }

        if (seasonIcon != null && seasonSprites.Length == 4 && seasonSprites[(int)currentSeason] != null)
        {
            seasonIcon.sprite = seasonSprites[(int)currentSeason];
        }
    }

    private void OnNextSeasonClicked()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.PassToNextSeason();
            // Reset day to 1 when manually changing season
            TimeManager.Instance.currentDay = 1;
            UpdateDateDisplay();
        }
    }
}
