using UnityEngine;

public class InventoryTabManager : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Khung chứa kho đồ cũ (Kinnly Inventory)")]
    public GameObject itemPanel;
    
    [Tooltip("Khung chứa kho Pet mới")]
    public GameObject petPanel;

    [Header("Scripts")]
    [Tooltip("Kéo script BeastInventoryUI từ PetPanel vào đây")]
    public BeastInventoryUI beastUI;

    private void OnEnable()
    {
        // Khi bật túi đồ lên (bấm phím B), luôn hiển thị Tab Vật Phẩm trước
        ShowItemTab();
    }

    public void ShowItemTab()
    {
        if (itemPanel != null) itemPanel.SetActive(true);
        if (petPanel != null) petPanel.SetActive(false);
    }

    public void ShowPetTab()
    {
        if (itemPanel != null) itemPanel.SetActive(false);
        if (petPanel != null) petPanel.SetActive(true);
        
        // Yêu cầu script Beast UI tải lại danh sách Pet mới nhất để hiển thị
        if (beastUI != null) 
        {
            beastUI.RefreshPetList();
        }
    }
}
