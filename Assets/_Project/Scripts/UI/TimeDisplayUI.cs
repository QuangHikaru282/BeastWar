using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeDisplayUI : MonoBehaviour
{
    [Header("UI Elements (TMP)")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI seasonText;

    [Header("UI Elements (Legacy Text)")]
    public Text timeTextLegacy;
    public Text dateTextLegacy;
    public Text seasonTextLegacy;

    public Image seasonIcon;

    [Header("Season Icons: Spring, Summer, Autumn, Winter")]
    public Sprite[] seasonSprites = new Sprite[4];

    [Header("Controls")]
    public Button nextSeasonButton;

    private void Awake()
    {
        AutoFindTextReferences();
    }

    private void AutoFindTextReferences()
    {
        if (timeText == null && timeTextLegacy == null)
        {
            var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            var texts = GetComponentsInChildren<Text>(true);

            foreach (var t in tmps)
            {
                string n = t.name.ToLower();
                if (n.Contains("time") || n.Contains("gio") || n.Contains("hour")) timeText = t;
                else if (n.Contains("date") || n.Contains("day") || n.Contains("ngay")) dateText = t;
                else if (n.Contains("season") || n.Contains("mua")) seasonText = t;
            }

            foreach (var t in texts)
            {
                string n = t.name.ToLower();
                if (n.Contains("time") || n.Contains("gio") || n.Contains("hour")) timeTextLegacy = t;
                else if (n.Contains("date") || n.Contains("day") || n.Contains("ngay")) dateTextLegacy = t;
                else if (n.Contains("season") || n.Contains("mua")) seasonTextLegacy = t;
            }

            if (timeText == null && dateText == null && tmps.Length >= 3)
            {
                timeText = tmps[0];
                dateText = tmps[1];
                seasonText = tmps[2];
            }
            if (timeTextLegacy == null && dateTextLegacy == null && texts.Length >= 3)
            {
                timeTextLegacy = texts[0];
                dateTextLegacy = texts[1];
                seasonTextLegacy = texts[2];
            }
        }
    }

    private void Start()
    {
        AutoFindTextReferences();

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= UpdateTimeDisplay;
            TimeManager.Instance.OnDayChanged -= UpdateDateDisplay;
            TimeManager.Instance.OnSeasonChanged -= UpdateSeasonDisplay;

            TimeManager.Instance.OnTimeChanged += UpdateTimeDisplay;
            TimeManager.Instance.OnDayChanged += UpdateDateDisplay;
            TimeManager.Instance.OnSeasonChanged += UpdateSeasonDisplay;
        }

        if (nextSeasonButton != null)
        {
            nextSeasonButton.onClick.RemoveAllListeners();
            nextSeasonButton.onClick.AddListener(OnNextSeasonClicked);
        }

        RefreshAllDisplay();
    }

    private void Update()
    {
        RefreshAllDisplay();
    }

    private void OnEnable()
    {
        RefreshAllDisplay();
    }

    public void RefreshAllDisplay()
    {
        if (TimeManager.Instance != null)
        {
            float percent = TimeManager.Instance.secondsPerDay > 0 ? (TimeManager.Instance.currentTime / TimeManager.Instance.secondsPerDay) : 0.25f;
            UpdateTimeDisplay(percent);
            UpdateDateDisplay();
            UpdateSeasonDisplay(TimeManager.Instance.currentSeason);
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

        string str = $"{hours:00}:{minutes:00} {ampm}";

        if (timeText != null) timeText.text = str;
        if (timeTextLegacy != null) timeTextLegacy.text = str;
    }

    private void UpdateDateDisplay()
    {
        if (TimeManager.Instance == null) return;
        string str = $"Năm {TimeManager.Instance.currentYear}, Ngày {TimeManager.Instance.currentDay}";

        if (dateText != null) dateText.text = str;
        if (dateTextLegacy != null) dateTextLegacy.text = str;
    }

    private void UpdateSeasonDisplay(Season currentSeason)
    {
        string str = "Mùa Xuân";
        switch (currentSeason)
        {
            case Season.Spring: str = "Mùa Xuân"; break;
            case Season.Summer: str = "Mùa Hạ"; break;
            case Season.Autumn: str = "Mùa Thu"; break;
            case Season.Winter: str = "Mùa Đông"; break;
        }

        if (seasonText != null) seasonText.text = str;
        if (seasonTextLegacy != null) seasonTextLegacy.text = str;

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
            TimeManager.Instance.currentDay = 1;
            UpdateDateDisplay();
        }
    }
}
