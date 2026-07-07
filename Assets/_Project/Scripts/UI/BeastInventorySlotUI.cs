using UnityEngine;
using UnityEngine.UI;
using TMPro; // Đã thêm dòng này

public class BeastInventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image petIcon;
    public TextMeshProUGUI petNameText; // Đã đổi sang TextMeshPro
    public TextMeshProUGUI cpText;      // Đã đổi sang TextMeshPro

    public void Setup(RuntimeBeastData beast)
    {
        if (beast == null) return;
        
        if (petIcon != null) 
            petIcon.sprite = beast.baseBeast.frontSprite;
            
        if (petNameText != null) 
            petNameText.text = beast.baseBeast.beastName;
            
        if (cpText != null) 
            cpText.text = "CP: " + beast.CombatPower;
    }
}
