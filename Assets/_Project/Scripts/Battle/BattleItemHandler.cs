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

    /// <summary>Lay so luong cua mot item trong kho do Kinnly.</summary>
    public int GetItemCount(string itemName)
    {
        var pd = Resources.FindObjectsOfTypeAll<PlayerData>();
        foreach (var data in pd)
        {
            foreach (var saved in data.savedInventoryItems)
            {
                if (saved.itemName == itemName)
                    return saved.amount;
            }
        }
        return 0;
    }

    private void ConsumeItem(string itemName)
    {
        var pd = Resources.FindObjectsOfTypeAll<PlayerData>();
        foreach (var data in pd)
        {
            for (int i = 0; i < data.savedInventoryItems.Count; i++)
            {
                var saved = data.savedInventoryItems[i];
                if (saved.itemName == itemName && saved.amount > 0)
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
        BeastUnit target = BattleManager.Instance?.GetActivePlayerUnit();
        if (target == null || !target.IsAlive)
        {
            Debug.Log("[Item] Khong tim thay thu dang song de dung thuoc!");
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
}
