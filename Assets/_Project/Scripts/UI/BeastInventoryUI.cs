using UnityEngine;

public class BeastInventoryUI : MonoBehaviour
{
    [Header("Data Reference")]
    public PlayerData playerData;

    [Header("UI Components")]
    [Tooltip("Khung chứa các ô Pet (nơi có gắn GridLayoutGroup)")]
    public Transform gridContainer;
    
    [Tooltip("Prefab của 1 ô hiển thị Pet")]
    public GameObject petSlotPrefab;

    public void RefreshPetList()
    {
        // 1. Xóa các ô hiển thị Pet cũ để làm mới danh sách
        foreach (Transform child in gridContainer) 
        { 
            Destroy(child.gameObject); 
        }

        if (playerData == null) 
        {
            Debug.LogError("Chưa gán PlayerData cho BeastInventoryUI!");
            return;
        }

        if (petSlotPrefab == null || gridContainer == null)
        {
            Debug.LogError("Chưa gán gridContainer hoặc petSlotPrefab cho BeastInventoryUI!");
            return;
        }

        // 2. Tạo các ô mới tương ứng với số lượng thú cưng đang sở hữu
        foreach (BeastData beast in playerData.ownedBeasts)
        {
            if (beast != null)
            {
                GameObject slot = Instantiate(petSlotPrefab, gridContainer);
                BeastInventorySlotUI slotUI = slot.GetComponent<BeastInventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.Setup(beast);
                }
            }
        }
    }
}
