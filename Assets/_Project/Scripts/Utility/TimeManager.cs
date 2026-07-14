using System;
using UnityEngine;
using UnityEngine.UI;

public enum Season { Spring, Summer, Autumn, Winter }

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Thời gian biểu")]
    public int currentYear = 1;
    public Season currentSeason = Season.Spring;
    public int currentDay = 1;
    
    [Tooltip("Thời gian hiện tại trong ngày (0 đến secondsPerDay). VD: 0=0h, secondsPerDay=24h")]
    public float currentTime = 0f;
    
    [Tooltip("Giây thực tế tương ứng với 1 ngày trong game (VD: 1800s = 30 phút)")]
    public float secondsPerDay = 1800f; 

    [Tooltip("Tốc độ trôi thời gian (có thể x100 để tua nhanh)")]
    public float timeSpeedMultiplier = 1f;

    [Header("Màu Sắc Ngày Đêm")]
    [SerializeField] private Image overlayTintImage;
    public Gradient lightTintGradient;

    // Events
    public event Action OnDayChanged;
    public event Action<Season> OnSeasonChanged;
    public event Action<float> OnTimeChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (lightTintGradient == null || lightTintGradient.colorKeys == null || lightTintGradient.colorKeys.Length == 0)
        {
            SetupDefaultGradient();
        }
        
        // Bắt đầu game là 06:00 sáng
        if (currentTime == 0f) currentTime = secondsPerDay * 0.25f; // 6h là 1/4 ngày
    }

    private void Update()
    {
        currentTime += Time.deltaTime * timeSpeedMultiplier;

        if (currentTime >= secondsPerDay)
        {
            // Trôi qua 24:00 đêm -> Bước sang 00:00 ngày mới
            currentTime -= secondsPerDay;
            AdvanceDay();
        }

        float timePercent = currentTime / secondsPerDay;

        if (overlayTintImage != null)
        {
            overlayTintImage.color = lightTintGradient.Evaluate(timePercent);
        }

        OnTimeChanged?.Invoke(timePercent);
    }

    private void AdvanceDay()
    {
        currentDay++;
        if (currentDay > 28)
        {
            currentDay = 1;
            PassToNextSeason();
        }
        OnDayChanged?.Invoke();
    }

    // Hàm dùng khi người chơi đi ngủ
    public void SleepToNextDay()
    {
        currentTime = secondsPerDay * 0.25f; // Đặt lại 06:00 sáng
        AdvanceDay();
    }

    public void PassToNextSeason()
    {
        int seasonIndex = (int)currentSeason + 1;
        if (seasonIndex > 3)
        {
            seasonIndex = 0;
            currentYear++;
        }
        
        currentSeason = (Season)seasonIndex;
        OnSeasonChanged?.Invoke(currentSeason);
    }

    private void SetupDefaultGradient()
    {
        lightTintGradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[5];
        colorKeys[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.3f, 0.6f), 0.0f); // Night
        colorKeys[1] = new GradientColorKey(new Color(0.8f, 0.5f, 0.3f, 0.3f), 0.25f); // Dawn (6h)
        colorKeys[2] = new GradientColorKey(new Color(1f, 1f, 1f, 0f), 0.5f); // Day (12h)
        colorKeys[3] = new GradientColorKey(new Color(0.6f, 0.3f, 0.4f, 0.4f), 0.75f); // Dusk (18h)
        colorKeys[4] = new GradientColorKey(new Color(0.1f, 0.1f, 0.3f, 0.6f), 1.0f); // Night (24h)

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[5];
        alphaKeys[0] = new GradientAlphaKey(0.6f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(0.3f, 0.25f);
        alphaKeys[2] = new GradientAlphaKey(0.0f, 0.5f);
        alphaKeys[3] = new GradientAlphaKey(0.4f, 0.75f);
        alphaKeys[4] = new GradientAlphaKey(0.6f, 1.0f);

        lightTintGradient.SetKeys(colorKeys, alphaKeys);
    }
}
