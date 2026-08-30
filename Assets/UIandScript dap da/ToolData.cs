using UnityEngine;

namespace Kinnly
{
    [CreateAssetMenu(
        fileName = "New Wood Cutting Tool",
        menuName = "Wood Cutting/Tool Data"
    )]
    public class WoodCuttingToolData : ScriptableObject
    {
        [Header("Item cong cu trong Inventory")]
        public Item inventoryItem;

        [Header("Ten hien thi")]
        public string displayName;

        [Header("Toc do thanh chay")]
        [Tooltip("So nho hon 1 se lam thanh chay cham hon")]
        [Range(0.5f, 2f)]
        public float indicatorSpeedMultiplier = 1f;

        [Header("Tang ti le do hiem")]
        [Range(0f, 1f)]
        public float rareDropBonus;
    }
}