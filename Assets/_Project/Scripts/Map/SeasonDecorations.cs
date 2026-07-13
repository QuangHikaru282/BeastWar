using UnityEngine;
using System.Collections.Generic;

public class SeasonDecorations : MonoBehaviour
{
    [Header("Seasonal Objects (Turned ON during season)")]
    public List<GameObject> springObjects;
    public List<GameObject> summerObjects;
    public List<GameObject> autumnObjects;
    public List<GameObject> winterObjects;

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnSeasonChanged += HandleSeasonChanged;
            // Áp dụng ngay khi bắt đầu
            HandleSeasonChanged(TimeManager.Instance.currentSeason);
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnSeasonChanged -= HandleSeasonChanged;
        }
    }

    private void HandleSeasonChanged(Season newSeason)
    {
        SetObjectsActive(springObjects, newSeason == Season.Spring);
        SetObjectsActive(summerObjects, newSeason == Season.Summer);
        SetObjectsActive(autumnObjects, newSeason == Season.Autumn);
        SetObjectsActive(winterObjects, newSeason == Season.Winter);
    }

    private void SetObjectsActive(List<GameObject> objects, bool isActive)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            if (obj != null) obj.SetActive(isActive);
        }
    }
}
