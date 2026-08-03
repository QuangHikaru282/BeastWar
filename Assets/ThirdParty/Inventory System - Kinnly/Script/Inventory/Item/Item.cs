using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Kinnly
{
    [CreateAssetMenu(
        fileName = "item",
        menuName = "ScriptableObjects/item"
    )]
    public class Item : ScriptableObject
    {
        [Header("Details")]
        public int id;
        public new string name;
        public string description;
        public int price;

        [Header("Assets")]
        public Sprite image;

        [Header("Toggles")]
        public bool isStackable;

        [Tooltip("Tích chọn nếu đây là vật phẩm đặc biệt (Cuốc, Bình nước...) không thể bán và không xuất hiện trong Shop")]
        public bool isSpecialItem;

        [Space(10)]
        [Header("Consumable")]
        public bool isConsumable;

        [Header("Buildings")]
        public bool isBuildable;
        public GameObject building;

        [Header("Tools")]
        public bool isTools;
        public bool isAxe;
        public bool isPickaxe;

        [Header("Farming Integration")]
        public BeastBall.Farming.Item farmingItemDelegate;

        // PHẦN MỚI: không thay đổi code cũ phía trên
        [Space(10)]
        [Header("Wood Cutting Integration")]
        [Tooltip("Tích chọn nếu vật phẩm này là tài nguyên nhận được từ cây")]
        public bool isWoodResource;

        [Tooltip("Độ hiếm của tài nguyên khi chặt cây")]
        public ResourceRarity woodResourceRarity =
            ResourceRarity.Common;

        [Header("Axe Wood Cutting Stats")]
        [Tooltip("Tốc độ thanh canh. Số càng nhỏ thì thanh chạy càng chậm")]
        [Range(0.5f, 2f)]
        public float axeIndicatorSpeedMultiplier = 1f;

        [Tooltip("Tỉ lệ cộng thêm khi nhận tài nguyên hiếm")]
        [Range(0f, 1f)]
        public float axeRareDropBonus = 0f;
    }
}