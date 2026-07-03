using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject lưu trạng thái người chơi — dùng chung xuyên suốt các Scene.
/// Kéo asset này vào mọi Manager cần đọc/ghi thông tin người chơi.
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "BeastBall/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Bộ sưu tập Beast")]
    public List<BeastData> ownedBeasts = new List<BeastData>();

    [Header("Đội hình hiện tại (tối đa 3)")]
    public List<BeastData> currentFormation = new List<BeastData>();

    public const int MaxFormationSize = 3;

    [Header("Giới tính nhân vật")]
    [Tooltip("Male hoặc Female")]
    public string characterGender = "Male";

    [Header("Tiến Trình Nhiệm Vụ Chính")]
    public int currentMainQuestId = 0; 

    [Header("Map đã mở khóa")]
    public List<string> unlockedMaps = new List<string>();
    
    [Header("Trainer đã đánh bại")]
    public List<string> defeatedTrainers = new List<string>();

    [Tooltip("Số vàng hiện có")]
    public int gold = 0;

    // ─── Beast Methods ───────────────────────────────────────────────

    /// <summary>Thêm Beast vào bộ sưu tập (sau khi bắt được).</summary>
    public void AddBeast(BeastData beast)
    {
        if (beast == null) return;
        if (!ownedBeasts.Contains(beast))
            ownedBeasts.Add(beast);
        Debug.Log($"[PlayerData] Đã thêm {beast.beastName} vào bộ sưu tập. Tổng: {ownedBeasts.Count}");
        
        // Báo cho hệ thống Quest biết để cập nhật nhiệm vụ
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnBeastCaught(beast);
        }
    }

    /// <summary>Lưu đội hình hiện tại.</summary>
    public void SetFormation(List<BeastData> formation)
    {
        currentFormation = new List<BeastData>(formation);
    }

    // ─── Map Methods ───────────────────────────────────────────────

    public bool IsMapUnlocked(string mapName)
    {
        return unlockedMaps.Contains(mapName);
    }

    public void UnlockMap(string mapName)
    {
        if (!unlockedMaps.Contains(mapName))
        {
            unlockedMaps.Add(mapName);
            Save();
        }
    }

    // ─── Reset ───────────────────────────────────────────────────────

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        ownedBeasts.Clear();
        currentFormation.Clear();
        unlockedMaps.Clear();
        defeatedTrainers.Clear();
        savedInventoryItems.Clear(); // Xóa sạch dữ liệu kho đồ đã lưu
        gold = 0;
        characterGender = "Male";
        currentMainQuestId = 0;
        Save();
    }

    // ─── Save/Load System ───────────────────────────────────────────────

    [System.Serializable]
    private class SaveData
    {
        public List<string> ownedBeastNames = new List<string>();
        public List<string> formationBeastNames = new List<string>();
        public string characterGender;
        public int currentMainQuestId;
        public int gold;
        public List<string> unlockedMaps;
        public List<string> defeatedTrainers;
        public List<SavedItem> savedInventoryItems;
    }

    [System.Serializable]
    public struct SavedItem
    {
        public string itemName;
        public int amount;
    }

    [Header("Kho Đồ Lưu (Giữ lại tất cả vật phẩm & số lượng khi chuyển cảnh)")]
    public List<SavedItem> savedInventoryItems = new List<SavedItem>();

    [HideInInspector] public bool isRestoringInventory = false;

    public void SaveInventoryState(Kinnly.PlayerInventory playerInv)
    {
        if (playerInv == null) return;
        if (isRestoringInventory) return; // Đang trong quá trình khôi phục -> Cấm đè dữ liệu!

        savedInventoryItems.Clear();

        if (playerInv.InventorySlots != null)
        {
            foreach (var slot in playerInv.InventorySlots)
            {
                if (slot != null)
                {
                    // includeInactive: true để lấy được cả các ô khi bảng Balo UI đang đóng (inactive)
                    var invItem = slot.GetComponentInChildren<Kinnly.InventoryItem>(true);
                    if (invItem != null && invItem.Item != null && invItem.Amount > 0)
                    {
                        savedInventoryItems.Add(new SavedItem
                        {
                            itemName = invItem.Item.name,
                            amount = invItem.Amount
                        });
                    }
                }
            }
        }
        Save();
    }

    public void RestoreInventoryState(Kinnly.PlayerInventory playerInv)
    {
        if (playerInv == null) return;
        if (currentMainQuestId == 0) return;
        if (savedInventoryItems == null || savedInventoryItems.Count == 0) return;

        // Dọn sạch túi trước khi nạp (tránh nhân đôi)
        ClearInventorySlots(playerInv);

        foreach (var saved in savedInventoryItems)
        {
            Kinnly.Item matching = null;

            if (QuestManager.Instance != null)
                matching = QuestManager.Instance.GetItemByName(saved.itemName);

            if (matching == null)
            {
                Kinnly.Item[] allItems = Resources.LoadAll<Kinnly.Item>("");
                if (allItems != null) matching = System.Array.Find(allItems, x => x != null && x.name == saved.itemName);
            }

            if (matching != null && saved.amount > 0)
                playerInv.AddItem(matching, saved.amount);
        }
    }

    private void ClearInventorySlots(Kinnly.PlayerInventory playerInv)
    {
        if (playerInv == null || playerInv.InventorySlots == null) return;
        foreach (var slot in playerInv.InventorySlots)
        {
            if (slot != null)
            {
                for (int i = slot.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(slot.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public void Save()
    {
        SaveData data = new SaveData
        {
            characterGender = this.characterGender,
            currentMainQuestId = this.currentMainQuestId,
            gold = this.gold,
            unlockedMaps = this.unlockedMaps,
            defeatedTrainers = this.defeatedTrainers,
            savedInventoryItems = this.savedInventoryItems
        };

        foreach (var b in ownedBeasts) 
        {
            if (b != null) data.ownedBeastNames.Add(b.name);
        }
        foreach (var b in currentFormation) 
        {
            if (b != null) data.formationBeastNames.Add(b.name);
            else data.formationBeastNames.Add(""); // Lưu chuỗi rỗng cho ô trống để giữ đúng vị trí
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("PlayerDataSave", json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey("PlayerDataSave")) return;
        string json = PlayerPrefs.GetString("PlayerDataSave");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null) return;

        this.characterGender = data.characterGender;
        this.currentMainQuestId = data.currentMainQuestId;
        this.gold = data.gold;
        if (data.unlockedMaps != null) this.unlockedMaps = data.unlockedMaps;
        if (data.defeatedTrainers != null) this.defeatedTrainers = data.defeatedTrainers;
        if (data.savedInventoryItems != null) this.savedInventoryItems = data.savedInventoryItems;

        // Restore Beasts from Resources
        BeastData[] allBeasts = Resources.LoadAll<BeastData>("");
        Dictionary<string, BeastData> beastDict = new Dictionary<string, BeastData>();
        foreach (var b in allBeasts) beastDict[b.name] = b;

        ownedBeasts.Clear();
        foreach (var bName in data.ownedBeastNames)
        {
            if (!string.IsNullOrEmpty(bName) && beastDict.TryGetValue(bName, out BeastData b)) 
            {
                ownedBeasts.Add(b);
            }
        }

        currentFormation.Clear();
        foreach (var bName in data.formationBeastNames)
        {
            if (!string.IsNullOrEmpty(bName) && beastDict.TryGetValue(bName, out BeastData b)) 
            {
                currentFormation.Add(b);
            }
            else
            {
                currentFormation.Add(null); // Ô trống
            }
        }
    }
}
