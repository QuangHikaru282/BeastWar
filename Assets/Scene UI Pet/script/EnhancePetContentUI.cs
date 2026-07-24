using UnityEngine;

/// <summary>
/// Bật đúng panel dựa theo BeastData.enhanceElement.
/// </summary>
[DisallowMultipleComponent]
public class EnhancePetContentUI : MonoBehaviour
{
    [Header("7 panel nguyên tố")]
    [SerializeField] private GameObject panelMetal;
    [SerializeField] private GameObject panelWater;
    [SerializeField] private GameObject panelWood;
    [SerializeField] private GameObject panelFire;
    [SerializeField] private GameObject panelEarth;
    [SerializeField] private GameObject panelLight;
    [SerializeField] private GameObject panelDark;

    private RuntimeBeastData currentPet;

    private void Awake()
    {
        HideAllPanels();
    }

    private void OnEnable()
    {
        if (currentPet != null)
        {
            Display(currentPet);
        }
    }

    public void Display(RuntimeBeastData pet)
    {
        currentPet = pet;

        HideAllPanels();

        if (pet == null)
        {
            Debug.LogWarning(
                "[EnhancePetContentUI] RuntimeBeastData đang null.",
                this
            );
            return;
        }

        if (pet.baseBeast == null)
        {
            Debug.LogWarning(
                "[EnhancePetContentUI] Pet chưa có Base Beast.",
                this
            );
            return;
        }

        // Dùng Enhance Element, không dùng Element.
        EnhanceElement selectedElement =
            pet.baseBeast.enhanceElement;

        GameObject targetPanel =
            GetPanelByEnhanceElement(selectedElement);

        Debug.Log(
            $"[EnhancePetContentUI] Pet: {pet.baseBeast.beastName} | " +
            $"Enhance Element: {selectedElement}",
            pet.baseBeast
        );

        if (targetPanel == null)
        {
            Debug.LogWarning(
                $"[EnhancePetContentUI] Chưa gán panel cho hệ " +
                $"{selectedElement}.",
                this
            );
            return;
        }

        targetPanel.SetActive(true);

        EnhancePanelStatsUI panelStats =
            targetPanel.GetComponent<EnhancePanelStatsUI>();

        if (panelStats == null)
        {
            // Tìm cả ở object con trong trường hợp script
            // được gắn lên một child của panel.
            panelStats =
                targetPanel.GetComponentInChildren<EnhancePanelStatsUI>(
                    true
                );
        }

        if (panelStats != null)
        {
            panelStats.Display(pet);
        }
        else
        {
            Debug.LogWarning(
                $"Panel '{targetPanel.name}' chưa có " +
                $"EnhancePanelStatsUI.",
                targetPanel
            );
        }
    }

    public void Refresh()
    {
        if (currentPet != null)
            Display(currentPet);
    }

    public void Clear()
    {
        currentPet = null;
        HideAllPanels();
    }

    private GameObject GetPanelByEnhanceElement(
        EnhanceElement element
    )
    {
        switch (element)
        {
            case EnhanceElement.Metal:
                return panelMetal;

            case EnhanceElement.Water:
                return panelWater;

            case EnhanceElement.Wood:
                return panelWood;

            case EnhanceElement.Fire:
                return panelFire;

            case EnhanceElement.Earth:
                return panelEarth;

            case EnhanceElement.Light:
                return panelLight;

            case EnhanceElement.Dark:
                return panelDark;

            default:
                return null;
        }
    }

    private void HideAllPanels()
    {
        SetPanelActive(panelMetal, false);
        SetPanelActive(panelWater, false);
        SetPanelActive(panelWood, false);
        SetPanelActive(panelFire, false);
        SetPanelActive(panelEarth, false);
        SetPanelActive(panelLight, false);
        SetPanelActive(panelDark, false);
    }

    private static void SetPanelActive(
        GameObject panel,
        bool active
    )
    {
        if (panel != null)
            panel.SetActive(active);
    }
}