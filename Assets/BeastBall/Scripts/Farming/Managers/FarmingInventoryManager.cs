using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeastBall.Farming
{
    [Serializable]
    public class InventorySaveData
    {
        public int Amount;
        public string ItemID;
    }

    /// <summary>
    /// Independent manager for the player's farming inventory (tools, seeds, products).
    /// </summary>
    public class FarmingInventoryManager : MonoBehaviour
    {
        public static FarmingInventoryManager Instance { get; private set; }

        public const int InventorySize = 9; // Can be adjusted as needed

        [Serializable]
        public class InventoryEntry
        {
            public Item Item;
            public int StackSize;
        }

        [Header("References")]
        [Tooltip("Required to resolve string IDs to ScriptableObjects when loading saves.")]
        public ItemDatabase ItemDatabase;

        [Header("Runtime Data")]
        public InventoryEntry[] Entries = new InventoryEntry[InventorySize];
        
        public int EquippedItemIdx { get; private set; } = 0;
        public Item EquippedItem => Entries[EquippedItemIdx]?.Item;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Initialize empty slots
            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i] == null) Entries[i] = new InventoryEntry();
            }
        }

        /// <summary>
        /// Checks if there is enough space in the inventory for the specified item and amount.
        /// </summary>
        public bool CanFitItem(Item item, int amount = 1)
        {
            int remaining = amount;
            
            // Check existing stacks
            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item == item && Entries[i].StackSize < item.MaxStackSize)
                {
                    remaining -= (item.MaxStackSize - Entries[i].StackSize);
                    if (remaining <= 0) return true;
                }
            }
            
            // Check empty slots
            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item == null)
                {
                    remaining -= item.MaxStackSize;
                    if (remaining <= 0) return true;
                }
            }
            return remaining <= 0;
        }

        /// <summary>
        /// Attempts to add an item. Returns true if fully added.
        /// </summary>
        public bool AddItem(Item newItem, int amount = 1)
        {
            if (newItem == null) return false;
            int remainingToFit = amount;

            // Step 1: Try to add to existing non-full stacks
            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item == newItem && Entries[i].StackSize < newItem.MaxStackSize)
                {
                    int fit = Mathf.Min(newItem.MaxStackSize - Entries[i].StackSize, remainingToFit);
                    Entries[i].StackSize += fit;
                    remainingToFit -= fit;
                    
                    if (remainingToFit == 0) return true;
                }
            }

            // Step 2: Try to find an empty slot
            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item == null)
                {
                    Entries[i].Item = newItem;
                    int fit = Mathf.Min(newItem.MaxStackSize, remainingToFit);
                    remainingToFit -= fit;
                    Entries[i].StackSize = fit;
                    
                    if (remainingToFit == 0) return true;
                }
            }

            return remainingToFit == 0;
        }

        public void EquipSlot(int index)
        {
            if (index >= 0 && index < InventorySize)
            {
                EquippedItemIdx = index;
                Debug.Log($"[BeastBall Farming] Equipped Slot {index}: {(EquippedItem != null ? EquippedItem.DisplayName : "Empty")}");
            }
        }

        /// <summary>
        /// Executes the action of the currently equipped item on a target cell.
        /// </summary>
        public bool UseEquippedItem(Vector3Int target)
        {
            if (EquippedItem == null || !EquippedItem.CanUse(target))
                return false;

            bool used = EquippedItem.Use(target);

            if (used && EquippedItem.Consumable)
            {
                Entries[EquippedItemIdx].StackSize -= 1;
                if (Entries[EquippedItemIdx].StackSize <= 0)
                {
                    Entries[EquippedItemIdx].Item = null;
                }
            }

            return used;
        }

        // --- SAVE / LOAD SYSTEM ---
        
        public List<InventorySaveData> SaveData()
        {
            var data = new List<InventorySaveData>();
            foreach (var entry in Entries)
            {
                if (entry.Item != null)
                {
                    data.Add(new InventorySaveData { Amount = entry.StackSize, ItemID = entry.Item.Key });
                }
                else
                {
                    data.Add(null); // Empty slot
                }
            }
            return data;
        }

        public void LoadData(List<InventorySaveData> data)
        {
            if (ItemDatabase == null)
            {
                Debug.LogError("[BeastBall Farming] ItemDatabase is missing on FarmingInventoryManager! Cannot load items.");
                return;
            }

            for (int i = 0; i < data.Count && i < InventorySize; ++i)
            {
                if (data[i] != null && !string.IsNullOrEmpty(data[i].ItemID))
                {
                    Entries[i].Item = ItemDatabase.GetFromID(data[i].ItemID);
                    Entries[i].StackSize = data[i].Amount;
                }
                else
                {
                    Entries[i].Item = null;
                    Entries[i].StackSize = 0;
                }
            }
        }
    }
}
