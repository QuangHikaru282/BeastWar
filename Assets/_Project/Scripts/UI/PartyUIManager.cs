using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyUIManager : MonoBehaviour
{
    [Header("Dữ liệu")]
    public PlayerData playerData;

    [Header("Giao diện")]
    [Tooltip("Kéo 3 ô PartySlot (trên cùng) vào đây")]
    public PartySlotUI[] partySlots; 
    [Tooltip("Kéo Content của ScrollView Kho Thú vào đây")]
    public Transform storageContainer; 
    
    [Header("Prefabs")]
    [Tooltip("Prefab của con thú kéo thả (DraggablePet)")]
    public GameObject draggablePetPrefab; 
    [Tooltip("Prefab của ô chứa trong Kho (PartySlotUI loại Storage)")]
    public GameObject storageSlotPrefab; 

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (playerData == null) return;

        // --- 1. Làm sạch giao diện cũ ---
        foreach (var slot in partySlots)
        {
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }
        foreach (Transform child in storageContainer)
        {
            Destroy(child.gameObject);
        }

        // --- 2. Khởi tạo 3 ô Party ---
        // Đảm bảo list currentFormation luôn có 3 phần tử (có thể là null nếu ô trống)
        while (playerData.currentFormation.Count < PlayerData.MaxFormationSize)
        {
            playerData.currentFormation.Add(null);
        }

        for (int i = 0; i < partySlots.Length; i++)
        {
            partySlots[i].slotType = PartySlotUI.SlotType.Party;
            partySlots[i].partyIndex = i;
            partySlots[i].SetupManager(this);

            BeastData beastInSlot = playerData.currentFormation[i];
            if (beastInSlot != null)
            {
                SpawnPetIcon(beastInSlot, partySlots[i].transform, partySlots[i]);
            }
        }

        // --- 3. Khởi tạo Kho (Storage) ---
        foreach (var beast in playerData.ownedBeasts)
        {
            if (beast == null) continue;
            
            // Nếu con thú này đang nằm trong Đội hình (Party), thì không hiện nó ở Kho nữa
            if (playerData.currentFormation.Contains(beast)) continue;

            // Tạo 1 ô chứa (Storage Slot)
            GameObject sSlotObj = Instantiate(storageSlotPrefab, storageContainer);
            PartySlotUI sSlot = sSlotObj.GetComponent<PartySlotUI>();
            sSlot.slotType = PartySlotUI.SlotType.Storage;
            sSlot.SetupManager(this);

            // Tạo icon con thú (DraggablePet) đặt vào trong ô chứa đó
            SpawnPetIcon(beast, sSlotObj.transform, sSlot);
        }
    }

    private void SpawnPetIcon(BeastData beast, Transform parent, PartySlotUI slot)
    {
        GameObject petObj = Instantiate(draggablePetPrefab, parent);
        DraggablePet dragComp = petObj.GetComponent<DraggablePet>();
        if (dragComp != null)
        {
            dragComp.Setup(beast, slot);
        }
    }

    /// <summary>
    /// Xử lý logic chèn/đổi chỗ theo đúng yêu cầu
    /// </summary>
    public void OnPetDropped(DraggablePet draggedPet, PartySlotUI targetSlot)
    {
        PartySlotUI originSlot = draggedPet.myCurrentSlot;
        if (originSlot == targetSlot) return; // Rớt lại đúng chỗ cũ -> Không làm gì

        BeastData draggedBeast = draggedPet.myBeast;

        // TỪ STORAGE KÉO VÀO PARTY
        if (originSlot.slotType == PartySlotUI.SlotType.Storage && targetSlot.slotType == PartySlotUI.SlotType.Party)
        {
            int targetIdx = targetSlot.partyIndex;
            BeastData existingBeast = playerData.currentFormation[targetIdx];

            if (existingBeast == null)
            {
                // Nếu ô đó đang trống -> Vào thẳng ô đó
                playerData.currentFormation[targetIdx] = draggedBeast;
            }
            else
            {
                // Nếu ô đó đã có thú -> Kiểm tra xem 2 ô kia có ô nào trống không
                int emptyIdx = -1;
                for (int i = 0; i < PlayerData.MaxFormationSize; i++)
                {
                    if (playerData.currentFormation[i] == null)
                    {
                        emptyIdx = i;
                        break;
                    }
                }

                if (emptyIdx != -1)
                {
                    // Có ô trống -> Chèn con đang có sẵn vào ô trống đó, nhường chỗ cho con mới
                    playerData.currentFormation[emptyIdx] = existingBeast;
                    playerData.currentFormation[targetIdx] = draggedBeast;
                }
                else
                {
                    // Đội hình đã đầy kín 3 ô -> Swap (Đổi chỗ: con đang có sẵn bị tống xuống Storage)
                    playerData.currentFormation[targetIdx] = draggedBeast;
                    // Con existingBeast sẽ tự động xuất hiện ở Storage trong hàm RefreshUI (vì nó mất tên trong currentFormation)
                }
            }
        }
        // ĐỔI VỊ TRÍ GIỮA 2 Ô TRONG PARTY (Kéo số 1 sang số 2)
        else if (originSlot.slotType == PartySlotUI.SlotType.Party && targetSlot.slotType == PartySlotUI.SlotType.Party)
        {
            int originIdx = originSlot.partyIndex;
            int targetIdx = targetSlot.partyIndex;

            BeastData temp = playerData.currentFormation[targetIdx];
            playerData.currentFormation[targetIdx] = playerData.currentFormation[originIdx];
            playerData.currentFormation[originIdx] = temp;
        }
        // TỪ PARTY KÉO XUỐNG STORAGE (Tháo ra khỏi đội hình)
        else if (originSlot.slotType == PartySlotUI.SlotType.Party && targetSlot.slotType == PartySlotUI.SlotType.Storage)
        {
            playerData.currentFormation[originSlot.partyIndex] = null;
        }
        // TỪ STORAGE KÉO SANG STORAGE (Chỉ là đổi chỗ trong kho, không cần xử lý)

        // Lưu dữ liệu và vẽ lại toàn bộ
        playerData.Save();
        
        // CỰC KỲ QUAN TRỌNG: Lúc đang kéo, con thú bị nhấc ra khỏi ô (ra hẳn Canvas ngoài cùng)
        // Nên vòng lặp dọn dẹp ở trên của RefreshUI sẽ không quét trúng nó.
        // Ta phải tự tay tiêu diệt con thú đang kéo này, vì RefreshUI sẽ đẻ ra con mới ngay sau đây.
        Destroy(draggedPet.gameObject);

        RefreshUI();
    }

    [Header("Liên kết giao diện khác")]
    public GameObject petInfoPanel; // Kéo PetInfoUI_Panel vào đây
    public PetInfoUIManager petInfoManager; // Kéo script PetInfoUIManager vào đây

    public void OpenPetInfo(BeastData beast)
    {
        if (petInfoPanel != null)
        {
            petInfoPanel.SetActive(true);
        }
        if (petInfoManager != null)
        {
            petInfoManager.SelectBeast(beast);
        }
    }
}
