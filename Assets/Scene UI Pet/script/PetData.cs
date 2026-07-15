using UnityEngine;

public enum PetElement
{
    None,
    Fire,
    Water,
    Grass,
    Electric,
    Dark,
    Light
}

public enum PetRarity
{
    Common,
    Rare,
    Epic,
    SSR,
    Legendary
}

[CreateAssetMenu(
    fileName = "NewPetData",
    menuName = "BeastWar/Pet Data"
)]
public class PetData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    [SerializeField] private string petId;
    [SerializeField] private string petName = "New Pet";
    [SerializeField, Min(1)] private int level = 1;

    [Header("Hình ảnh")]
    [SerializeField] private Sprite listIcon;
    [SerializeField] private Sprite displayImage;
    [SerializeField] private Sprite elementIcon;

    [Header("Phân loại")]
    [SerializeField] private PetElement element;
    [SerializeField] private PetRarity rarity;
    [SerializeField] private string personality = "Normal";

    [Header("Chỉ số chiến đấu")]
    [SerializeField, Min(0)] private int health;
    [SerializeField, Min(0)] private int attack;
    [SerializeField, Min(0)] private int defense;
    [SerializeField, Min(0)] private int speed;
    [SerializeField, Min(0)] private int power;

    public string PetId => petId;
    public string PetName => petName;
    public int Level => level;

    public Sprite ListIcon => listIcon;
    public Sprite DisplayImage => displayImage;
    public Sprite ElementIcon => elementIcon;

    public PetElement Element => element;
    public PetRarity Rarity => rarity;
    public string Personality => personality;

    public int Health => health;
    public int Attack => attack;
    public int Defense => defense;
    public int Speed => speed;
    public int Power => power;
}