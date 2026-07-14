using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeDebugUI : MonoBehaviour
{
    [Header("Controls")]
    public Button fastForwardButton;
    public Button nextDayButton;
    
    private bool isFastForwarding = false;

    private void Start()
    {
        if (fastForwardButton != null) fastForwardButton.onClick.AddListener(ToggleFastForward);
        if (nextDayButton != null) nextDayButton.onClick.AddListener(SkipToNextDay);
    }

    private void ToggleFastForward()
    {
        if (TimeManager.Instance == null) return;
        
        isFastForwarding = !isFastForwarding;
        TimeManager.Instance.timeSpeedMultiplier = isFastForwarding ? 100f : 1f;
        
        var txt = fastForwardButton.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = isFastForwarding ? "Dừng Tua" : "Tua x100";
            txt.color = isFastForwarding ? Color.red : Color.white;
        }
    }

    private void SkipToNextDay()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SleepToNextDay();
        }
    }
}
