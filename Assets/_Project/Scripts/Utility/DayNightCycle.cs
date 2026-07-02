using System;
using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [Header("Cấu Hình Thời Hình")]
    [Tooltip("Tổng thời gian 1 chu kỳ (giây). 1800s = 30 phút.")]
    [SerializeField] private float totalCycleTime = 1800f;
    
    [Tooltip("Thời gian hiện tại trong ngày (0 đến totalCycleTime)")]
    [SerializeField] private float currentTime = 0f;

    [Header("Màu Sắc Ánh Sáng (Tint)")]
    [Tooltip("Image phủ toàn màn hình (chỉnh màu đen/trắng) hoặc dùng Multiply material.")]
    [SerializeField] private Image overlayTintImage;
    
    public Gradient lightTintGradient;

    // Events cho các hệ thống khác lắng nghe
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
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= totalCycleTime)
        {
            currentTime = 0f;
        }

        float timePercent = currentTime / totalCycleTime;

        if (overlayTintImage != null)
        {
            overlayTintImage.color = lightTintGradient.Evaluate(timePercent);
        }

        OnTimeChanged?.Invoke(timePercent);
    }

    private void SetupDefaultGradient()
    {
        lightTintGradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[5];
        colorKeys[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.3f, 0.6f), 0.0f); // Night
        colorKeys[1] = new GradientColorKey(new Color(0.8f, 0.5f, 0.3f, 0.3f), 0.25f); // Dawn
        colorKeys[2] = new GradientColorKey(new Color(1f, 1f, 1f, 0f), 0.5f); // Day
        colorKeys[3] = new GradientColorKey(new Color(0.6f, 0.3f, 0.4f, 0.4f), 0.75f); // Dusk
        colorKeys[4] = new GradientColorKey(new Color(0.1f, 0.1f, 0.3f, 0.6f), 1.0f); // Night

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[5];
        alphaKeys[0] = new GradientAlphaKey(0.6f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(0.3f, 0.25f);
        alphaKeys[2] = new GradientAlphaKey(0.0f, 0.5f);
        alphaKeys[3] = new GradientAlphaKey(0.4f, 0.75f);
        alphaKeys[4] = new GradientAlphaKey(0.6f, 1.0f);

        lightTintGradient.SetKeys(colorKeys, alphaKeys);
    }
}
