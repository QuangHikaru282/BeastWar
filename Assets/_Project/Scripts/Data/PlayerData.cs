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
    public List<RuntimeBeastData> ownedBeasts = new List<RuntimeBeastData>();

    [Header("Đội hình hiện tại (tối đa 3)")]
    public List<RuntimeBeastData> currentFormation = new List<RuntimeBeastData>();

    public const int MaxFormationSize = 3;

    [Header("Giới tính nhân vật")]
    [Tooltip("Male hoặc Female")]
    public string characterGender = "Male";

    [Header("Tên nhân vật")]
    [Tooltip("Tên do người chơi đặt, hiển thị trong hội thoại")]
    public string playerName = "Trainer";

    [Header("Tiến Trình Nhiệm Vụ Chính")]
    public int currentMainQuestId = 0; 

    [Header("Map đã mở khóa")]
    public List<string> unlockedMaps = new List<string>();
    
    [Tooltip("ID của điểm sinh ra (Spawn Point) sẽ dùng khi tải Map mới")]
    public string targetSpawnPointId = "";

    [Header("Điểm Hồi Sinh (Pokémon Center Checkpoint)")]
    [Tooltip("Tên Scene Trung tâm Pokémon gần nhất đã ghé thăm")]
    public string respawnSceneName = "Lau1";

    [Tooltip("ID của SpawnPoint xuất hiện ở Trung tâm Pokémon đó")]
    public string respawnSpawnPointId = "1";
    
    [Header("Trainer đã đánh bại")]
    public List<string> defeatedTrainers = new List<string>();

    [Header("Huy hiệu Gym (Gym Badges)")]
    [Tooltip("Danh sách ID huy hiệu Gym đã thu thập")]
    public List<string> gymBadges = new List<string>();

    /// <summary>Thêm Huy hiệu Gym mới nếu chưa có.</summary>
    public bool AddGymBadge(string badgeId)
    {
        if (string.IsNullOrEmpty(badgeId)) return false;
        if (!gymBadges.Contains(badgeId))
        {
            gymBadges.Add(badgeId);
            Save();
            Debug.Log($"<color=yellow>[PlayerData]</color> Đã nhận Huy hiệu Gym mới: {badgeId}. Tổng số huy hiệu: {gymBadges.Count}");
            return true;
        }
        return false;
    }

    [Tooltip("Số vàng hiện có")]
    public int gold = 0;

    [Tooltip("Index Starter đã chọn (0=Cỏ, 1=Lửa, 2=Nước). Dùng cho Rival.")]
    public int lastStarterChoiceIndex = 0;

    // ─── Beast Methods ───────────────────────────────────────────────

    /// <summary>Thêm Beast vào bộ sưu tập (sau khi bắt được).</summary>
    public void AddBeast(RuntimeBeastData beast)
    {
        if (beast == null) return;
        if (!ownedBeasts.Contains(beast))
            ownedBeasts.Add(beast);
        Debug.Log($"[PlayerData] Đã thêm {(beast.baseBeast != null ? beast.baseBeast.beastName : "Unknown")} vào bộ sưu tập. Tổng: {ownedBeasts.Count}");
        
        // Báo cho hệ thống Quest biết để cập nhật nhiệm vụ
        if (QuestManager.Instance != null && beast.baseBeast != null)
        {
            QuestManager.Instance.OnBeastCaught(beast);
        }
    }

    /// <summary>Lưu đội hình hiện tại.</summary>
    public void SetFormation(List<RuntimeBeastData> formation)
    {
        currentFormation = new List<RuntimeBeastData>(formation);
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
        gymBadges.Clear();
        savedInventoryItems.Clear(); // Xóa sạch dữ liệu kho đồ đã lưu
        gold = 0;
        characterGender = "Male";
        currentMainQuestId = 0;
        respawnSceneName = "Lau1";
        respawnSpawnPointId = "1";
        Save();
    }

    // ─── Save/Load System ───────────────────────────────────────────────

    [System.Serializable]
    private class SaveData
    {
        public List<SavedRuntimeBeast> ownedBeasts = new List<SavedRuntimeBeast>();
        public List<SavedRuntimeBeast> formationBeasts = new List<SavedRuntimeBeast>();
        public List<int> formationIndices = new List<int>();
        public string characterGender;
        public int currentMainQuestId;
        public int gold;
        public List<string> unlockedMaps;
        public List<string> defeatedTrainers;
        public List<string> gymBadges;
        public List<SavedItem> savedInventoryItems;
        public string respawnSceneName;
        public string respawnSpawnPointId;
    }

    [System.Serializable]
    public class SavedRuntimeBeast
    {
        public string baseBeastName;
        public int currentLevel;
        public int currentExp;
        public int currentHP = -1;
        public List<SavedRuntimeMove> moves = new List<SavedRuntimeMove>();
    }

    [System.Serializable]
    public class SavedRuntimeMove
    {
        public string baseMoveName;
        public int currentLevel;
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
                        // Lấy tên file Asset (ví dụ: "Potion", "Super Potion", "Antidote") thay vì display name
                        string assetName = ((UnityEngine.Object)invItem.Item).name;
                        if (string.IsNullOrEmpty(assetName)) assetName = invItem.Item.name;

                        savedInventoryItems.Add(new SavedItem
                        {
                            itemName = assetName,
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

    private SavedRuntimeBeast SerializeBeast(RuntimeBeastData beast)
    {
        if (beast == null || beast.baseBeast == null) return null;
        var saved = new SavedRuntimeBeast
        {
            baseBeastName = beast.baseBeast.name,
            currentLevel = beast.currentLevel,
            currentExp = beast.currentExp,
            currentHP = beast.currentHP
        };
        if (beast.moves != null)
        {
            foreach (var m in beast.moves)
            {
                if (m != null && m.baseMove != null)
                {
                    saved.moves.Add(new SavedRuntimeMove { baseMoveName = m.baseMove.name, currentLevel = m.currentLevel });
                }
            }
        }
        return saved;
    }

    private RuntimeBeastData DeserializeBeast(SavedRuntimeBeast saved, Dictionary<string, BeastData> beastDict, Dictionary<string, MoveData> moveDict)
    {
        if (saved == null || string.IsNullOrEmpty(saved.baseBeastName)) return null;
        if (!beastDict.TryGetValue(saved.baseBeastName, out BeastData baseBeast)) return null;

        RuntimeBeastData rt = new RuntimeBeastData(baseBeast, saved.currentLevel);
        rt.currentExp = saved.currentExp;
        rt.currentHP = saved.currentHP >= 0 ? saved.currentHP : rt.MaxHP;

        // Restore moves
        if (saved.moves != null && saved.moves.Count > 0)
        {
            rt.moves = new RuntimeMoveData[saved.moves.Count];
            for (int i = 0; i < saved.moves.Count; i++)
            {
                if (moveDict.TryGetValue(saved.moves[i].baseMoveName, out MoveData baseMove))
                {
                    rt.moves[i] = new RuntimeMoveData(baseMove, saved.moves[i].currentLevel);
                }
            }
        }
        return rt;
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
            gymBadges = this.gymBadges,
            savedInventoryItems = this.savedInventoryItems,
            respawnSceneName = this.respawnSceneName,
            respawnSpawnPointId = this.respawnSpawnPointId
        };

        foreach (var b in ownedBeasts) 
        {
            var saved = SerializeBeast(b);
            if (saved != null) data.ownedBeasts.Add(saved);
        }

        foreach (var b in currentFormation) 
        {
            if (b != null) 
            {
                data.formationIndices.Add(ownedBeasts.IndexOf(b));
            }
            else 
            {
                data.formationIndices.Add(-1); // Ô trống
            }
        }

        SaveLoadSystem.SaveData(data);
    }

    public void Load()
    {
        SaveData data = SaveLoadSystem.LoadData<SaveData>();
        if (data == null) return;

        if (data == null) return;

        this.characterGender = data.characterGender;
        this.currentMainQuestId = data.currentMainQuestId;
        this.gold = data.gold;
        if (data.unlockedMaps != null) this.unlockedMaps = data.unlockedMaps;
        if (data.defeatedTrainers != null) this.defeatedTrainers = data.defeatedTrainers;
        if (data.gymBadges != null) this.gymBadges = data.gymBadges;
        if (data.savedInventoryItems != null)
        {
            this.savedInventoryItems = data.savedInventoryItems;
        }
        if (!string.IsNullOrEmpty(data.respawnSceneName))
        {
            this.respawnSceneName = data.respawnSceneName;
        }
        if (!string.IsNullOrEmpty(data.respawnSpawnPointId))
        {
            this.respawnSpawnPointId = data.respawnSpawnPointId;
        }

        Debug.Log($"[PlayerData] Load thành công từ File. Tiền: {gold}, Đội hình: {currentFormation.Count} thú.");

        // Restore Beasts from Resources
        BeastData[] allBeasts = Resources.LoadAll<BeastData>("");
        Dictionary<string, BeastData> beastDict = new Dictionary<string, BeastData>();
        foreach (var b in allBeasts) beastDict[b.name] = b;

        MoveData[] allMoves = Resources.LoadAll<MoveData>("");
        Dictionary<string, MoveData> moveDict = new Dictionary<string, MoveData>();
        foreach (var m in allMoves) moveDict[m.name] = m;

        ownedBeasts.Clear();
        if (data.ownedBeasts != null)
        {
            foreach (var savedB in data.ownedBeasts)
            {
                var rt = DeserializeBeast(savedB, beastDict, moveDict);
                if (rt != null) 
                {
                    // Lọc trùng lặp (nếu file save cũ bị lỗi nhân bản)
                    bool isDuplicate = false;
                    foreach (var existing in ownedBeasts)
                    {
                        if (existing.baseBeast == rt.baseBeast && existing.currentLevel == rt.currentLevel && existing.currentExp == rt.currentExp)
                        {
                            isDuplicate = true;
                            break;
                        }
                    }
                    if (!isDuplicate)
                    {
                        ownedBeasts.Add(rt);
                    }
                }
            }
        }

        currentFormation.Clear();
        if (data.formationIndices != null && data.formationIndices.Count > 0)
        {
            foreach (int idx in data.formationIndices)
            {
                if (idx >= 0 && idx < ownedBeasts.Count)
                {
                    currentFormation.Add(ownedBeasts[idx]);
                }
                else
                {
                    currentFormation.Add(null);
                }
            }
        }
        else if (data.formationBeasts != null && data.formationBeasts.Count > 0)
        {
            // Tương thích ngược: Bỏ qua load formation cũ để tránh nhân bản thú
            // Người chơi sẽ phải tự kéo lại thú vào đội hình ở lần load này.
            for (int i = 0; i < PlayerData.MaxFormationSize; i++)
            {
                currentFormation.Add(null);
            }
        }
    }
}
