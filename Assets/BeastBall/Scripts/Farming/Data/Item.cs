using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Abstract base class for all interactable items and tools in the farming system.
    /// </summary>
    public abstract class Item : ScriptableObject, IDatabaseEntry
    {
        public string Key => UniqueID;

        [Tooltip("Unique ID used in the database for lookup and save system")]
        public string UniqueID = "DefaultID";
        
        public string DisplayName;
        public Sprite ItemSprite;
        public int MaxStackSize = 10;
        public bool Consumable = true;
        public int BuyPrice = -1;

        [Tooltip("Prefab instantiated in the player hand when equipped")]
        public GameObject VisualPrefab;
        public string PlayerAnimatorTriggerUse = "GenericToolSwing";
        
        [Tooltip("Sounds triggered randomly when using the item")]
        public AudioClip[] UseSound;

        // Core behavior to be overridden by subclasses (Hoe, WaterCan, etc.)
        public abstract bool CanUse(Vector3Int target);
        public abstract bool Use(Vector3Int target);

        // Override to return false for items that don't need a specific cell target (like edibles)
        public virtual bool NeedTarget()
        {
            return true;
        }
    }
}
