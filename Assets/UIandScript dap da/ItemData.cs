using UnityEngine;

public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(
    fileName = "New Item",
    menuName = "Wood Cutting/Item Data"
)]
public class ItemData : ScriptableObject
{
    [Header("Thong tin vat pham")]
    public string itemName;
    public Sprite icon;
    public ItemRarity rarity;
}