using System;
using UnityEngine;

public enum ShopItemCategory
{
    Weapon,
    Armor,
    Item
}

[Serializable]
public class ShopItemData
{
    [Header("Thông tin cơ bản")]
    public string itemName;

    public Sprite icon;

    public ShopItemCategory category;

    [Min(0)]
    public int price;

    [Tooltip("Ví dụ: Level 5 Weapon")]
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

    // Số lượng trong lúc chơi game.
    // Không hiện trong Inspector.
    [NonSerialized]
    public int owned;
}