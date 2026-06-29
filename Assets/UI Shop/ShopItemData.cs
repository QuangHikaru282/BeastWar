using System;
using UnityEngine;

public enum ShopItemCategory
{
    Weapon,
    Armor,
    Item,
    Seed
}

[Serializable]
public class ShopItemData
{
    [Header("Mã item")]
    [Tooltip("Ví dụ: iron_sword, health_potion")]
    public string itemID;

    [Header("Thông tin cơ bản")]
    public string itemName;
    public Sprite icon;
    public ShopItemCategory category;

    [Min(0)]
    public int price;

    public string itemTypeText;

    [Header("Chỉ số")]
    [Min(0)]
    public int attack;

    [Min(0)]
    public int durability = 100;

    [TextArea(2, 5)]
    public string description;

    [Header("Số lượng ban đầu")]
    [Min(0)]
    public int startingOwned;

    [NonSerialized]
    public int owned;
}