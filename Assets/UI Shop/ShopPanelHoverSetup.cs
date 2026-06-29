using UnityEngine;
using UnityEngine.UI;

public class ShopPanelHoverSetup : MonoBehaviour
{
    private void Start()
    {
        RefreshHoverEffects();
    }

    public void RefreshHoverEffects()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }

            if (button.GetComponent<UIButtonHoverScale>() == null)
            {
                button.gameObject.AddComponent<UIButtonHoverScale>();
            }
        }
    }
}