using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Xu ly dung vat pham (binh thuoc) trong tran chien.
/// Doc so luong item tu kho do Kinnly (PlayerData.savedInventoryItems).
/// </summary>
public class BattleItemHandler : MonoBehaviour
{
    public static BattleItemHandler Instance { get; private set; }

    // ─── Dinh nghia cac loai thuoc ──────────────────────────────────
    [System.Serializable]
    public class BattleItem
    {
        public string itemName;      // Ten phai khop voi item trong Kinnly
        public string displayName;   // Ten hien thi tren UI
        public Sprite icon;          // Icon
        public ItemEffect effect;    // Loai hieu ung
        public int healAmount;       // Luong hoi mau (neu la heal)
    }

    public enum ItemEffect
    {
        HealHP,         // Hoi mau
        CurePoison,     // Giai doc
        CureParalysis,  // Giai te liet
        FullCure        // Giai tat ca trang thai xau
    }

    [Header("Danh sach vat pham duoc phep dung trong Battle")]
    [SerializeField] private List<BattleItem> battleItems = new List<BattleItem>
    {
        new BattleItem { itemName = "Potion",       displayName = "Binh Hoi Mau",   effect = ItemEffect.HealHP,       healAmount = 50  },
        new BattleItem { itemName = "Super Potion",  displayName = "Binh Sieu Hoi",  effect = ItemEffect.HealHP,       healAmount = 120 },
        new BattleItem { itemName = "Antidote",      displayName = "Thuoc Giai Doc", effect = ItemEffect.CurePoison                     },
        new BattleItem { itemName = "Paralyze Heal", displayName = "Thuoc Giai Te",  effect = ItemEffect.CureParalysis                  },
        new BattleItem { itemName = "Full Heal",     displayName = "Thuoc Toan Nang",effect = ItemEffect.FullCure                       },
    };

    public IReadOnlyList<BattleItem> BattleItems => battleItems;

    // Callback de BattleManager biet nguoi choi da dung do (tieu mot luot)
    public System.Action OnItemUsed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>Lấy PlayerData đang hoạt động (ưu tiên QuestManager, fallback Resources).</summary>
    private PlayerData GetActivePlayerData()
    {
        if (global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null)
            return global::QuestManager.Instance.playerData;
        return Resources.Load<PlayerData>("PlayerData");
    }

    /// <summary>Kiểm tra xem tên item trong túi đồ đã lưu có khớp với mục tiêu không (hỗ trợ cả tiếng Anh, tiếng Việt, không dấu, có dấu).</summary>
    public static bool IsItemMatch(string savedName, string targetName)
    {
        if (string.IsNullOrEmpty(savedName) || string.IsNullOrEmpty(targetName)) return false;

        string s1 = savedName.Trim().Replace(" ", "").ToLower();
        string s2 = targetName.Trim().Replace(" ", "").ToLower();

        if (s1 == s2 || s1.Contains(s2) || s2.Contains(s1)) return true;

        // Đối chiếu theo loại Potion
        if (s2.Contains("superpotion") || s2.Contains("sieuhoy") || s2.Contains("sieuhồi") || s2.Contains("giaipro"))
        {
            if (s1.Contains("super") || s1.Contains("sieuhoy") || s1.Contains("sieuhồi") || s1.Contains("giaipro")) return true;
        }
        else if (s2.Contains("potion") || s2.Contains("binhmau") || s2.Contains("mau") || s2.Contains("máu"))
        {
            // Đảm bảo không nhầm với Super Potion
            if (!s1.Contains("super") && !s1.Contains("sieu") && !s1.Contains("siêu") && !s1.Contains("pro") &&
                (s1.Contains("potion") || s1.Contains("binhmau") || s1.Contains("mau") || s1.Contains("máu")))
                return true;
        }
        else if (s2.Contains("antidote") || s2.Contains("giaidoc") || s2.Contains("độc") || s2.Contains("doc"))
        {
            if (s1.Contains("antidote") || s1.Contains("giaidoc") || s1.Contains("doc") || s1.Contains("độc")) return true;
        }
        else if (s2.Contains("paralyze") || s2.Contains("giaite") || s2.Contains("te") || s2.Contains("tê"))
        {
            if (s1.Contains("paralyze") || s1.Contains("giaite") || s1.Contains("te") || s1.Contains("tê")) return true;
        }
        else if (s2.Contains("fullheal") || s2.Contains("toannang") || s2.Contains("toànnăng"))
        {
            if (s1.Contains("fullheal") || s1.Contains("toannang") || s1.Contains("toànnăng") || s1.Contains("full")) return true;
        }
        else if (s2.Contains("pokeball") || s2.Contains("ball") || s2.Contains("bong") || s2.Contains("bóng"))
        {
            if (s1.Contains("pokeball") || s1.Contains("ball") || s1.Contains("bong") || s1.Contains("bóng")) return true;
        }

        return false;
    }

    /// <summary>Lay so luong cua mot item trong kho do Kinnly.</summary>
    public int GetItemCount(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return 0;

        // Ưu tiên dùng PlayerData đang chạy thực tế
        var active = GetActivePlayerData();
        if (active != null && active.savedInventoryItems != null)
        {
            foreach (var saved in active.savedInventoryItems)
            {
                if (string.IsNullOrEmpty(saved.itemName)) continue;

                if (IsItemMatch(saved.itemName, itemName))
                {
                    return saved.amount;
                }
            }
            return 0;
        }

        // Fallback: tìm trong tất cả instance đã load
        var pd = Resources.FindObjectsOfTypeAll<PlayerData>();
        foreach (var data in pd)
        {
            if (data.savedInventoryItems == null) continue;
            foreach (var saved in data.savedInventoryItems)
            {
                if (string.IsNullOrEmpty(saved.itemName)) continue;

                if (IsItemMatch(saved.itemName, itemName))
                {
                    return saved.amount;
                }
            }
        }
        return 0;
    }

    /// <summary>In ra Console toàn bộ tên item trong túi đồ để debug. Gọi trong Start hoặc khi mở Balo.</summary>
    [ContextMenu("Debug: In tên item trong túi đồ")]
    public void DebugPrintInventoryItems()
    {
        var active = GetActivePlayerData();
        if (active == null)
        {
            Debug.LogError("[BattleItemHandler] Không tìm thấy PlayerData!");
            return;
        }

        if (active.savedInventoryItems == null || active.savedInventoryItems.Count == 0)
        {
            Debug.LogWarning("[BattleItemHandler] Túi đồ RỖNG! Không có item nào trong savedInventoryItems.");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"[BattleItemHandler] Túi đồ có {active.savedInventoryItems.Count} loại item:");
        foreach (var item in active.savedInventoryItems)
        {
            sb.AppendLine($"  • itemName = \"{item.itemName}\"  |  amount = {item.amount}");
        }
        Debug.Log(sb.ToString());

        // In luôn các tên item mà BattleItemHandler đang tìm
        sb.Clear();
        sb.AppendLine("[BattleItemHandler] Tên item BattleItemHandler đang tìm:");
        foreach (var bi in battleItems)
        {
            int cnt = GetItemCount(bi.itemName);
            sb.AppendLine($"  • \"{bi.itemName}\" → Tìm thấy: {cnt}");
        }
        Debug.Log(sb.ToString());
    }

    private void ConsumeItem(string itemName)
    {
        var data = GetActivePlayerData();
        if (data == null || data.savedInventoryItems == null) return;

        for (int i = 0; i < data.savedInventoryItems.Count; i++)
        {
            var saved = data.savedInventoryItems[i];
            if (string.IsNullOrEmpty(saved.itemName)) continue;

            if (IsItemMatch(saved.itemName, itemName))
            {
                if (saved.amount > 0)
                {
                    var updated = saved;
                    updated.amount--;
                    data.savedInventoryItems[i] = updated;
                    if (updated.amount <= 0)
                        data.savedInventoryItems.RemoveAt(i);
                    data.Save();
                    return;
                }
            }
        }
    }

    /// <summary>Dung 1 vat pham len thu dang chien dau cua nguoi choi.</summary>
    public bool UseItem(BattleItem item)
    {
        if (item == null) return false;

        // Kiem tra so luong
        int count = GetItemCount(item.itemName);
        if (count <= 0)
        {
            Debug.Log($"[Item] Khong con {item.displayName} trong kho!");
            return false;
        }

        // Tim thu dang chien dau
        BattleManager bm = BattleManager.Instance ?? FindFirstObjectByType<BattleManager>();
        BeastUnit target = bm != null ? bm.GetActivePlayerUnit() : null;
        if (target == null || !target.IsAlive)
        {
            Debug.LogWarning("[Item] Khong tim thay thu dang song de dung thuoc!");
            return false;
        }

        // Ap dung hieu ung
        bool success = false;
        switch (item.effect)
        {
            case ItemEffect.HealHP:
                target.Heal(item.healAmount);
                Debug.Log($"[Item] Dung {item.displayName}: hoi {item.healAmount} HP cho {target.Data.baseBeast.beastName}.");
                success = true;
                break;

            case ItemEffect.CurePoison:
                if (target.CurrentStatus == BeastUnit.StatusEffect.Poisoned)
                {
                    target.ClearStatus(BeastUnit.StatusEffect.Poisoned);
                    Debug.Log($"[Item] Dung {item.displayName}: giai doc cho {target.Data.baseBeast.beastName}.");
                    success = true;
                }
                else Debug.Log($"[Item] {target.Data.baseBeast.beastName} khong bi nhiem doc!");
                break;

            case ItemEffect.CureParalysis:
                if (target.CurrentStatus == BeastUnit.StatusEffect.Paralyzed)
                {
                    target.ClearStatus(BeastUnit.StatusEffect.Paralyzed);
                    Debug.Log($"[Item] Dung {item.displayName}: giai te liet cho {target.Data.baseBeast.beastName}.");
                    success = true;
                }
                else Debug.Log($"[Item] {target.Data.baseBeast.beastName} khong bi te liet!");
                break;

            case ItemEffect.FullCure:
                target.ClearStatus();
                Debug.Log($"[Item] Dung {item.displayName}: xoa tat ca trang thai xau cho {target.Data.baseBeast.beastName}.");
                success = true;
                break;
        }

        if (success)
        {
            ConsumeItem(item.itemName);
            OnItemUsed?.Invoke(); // Bao BattleManager tieu 1 luot
        }

        return success;
    }

    /// <summary>Dùng 1 item theo tên (ví dụ Pokeball).</summary>
    public bool UseItemByName(string itemName)
    {
        int count = GetItemCount(itemName);
        if (count > 0)
        {
            ConsumeItem(itemName);
            return true;
        }
        return false;
    }
}
