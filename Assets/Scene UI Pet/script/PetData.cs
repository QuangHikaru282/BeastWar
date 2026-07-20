using System.Collections.Generic;
using UnityEngine;

public enum PetElement
{
    None = 0,
    Fire = 1,
    Water = 2,
    Wood = 3,
    Metal = 4,
    Light = 5,
    Dark = 6,
    Earth = 7
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
    [Tooltip("Icon nhỏ hiển thị trong danh sách Pet.")]
    [SerializeField] private Sprite listIcon;

    [Tooltip("Ảnh Pet hiện tại hiển thị ở giữa.")]
    [SerializeField] private Sprite displayImage;

    [Tooltip("Image 2: ảnh xem trước Pet sau tiến hóa.")]
    [SerializeField] private Sprite evolutionImage;

    [Tooltip("Icon nguyên tố của Pet.")]
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

    [Header("Kỹ năng của Pet")]
    [Tooltip("Tối đa 4 kỹ năng. Số lượng do bạn tự quyết định.")]
    [SerializeField] private List<PetSkillEntry> skills = new();

    [Header("Tiến hóa")]
    [Tooltip("PetData được chuyển thành sau khi tiến hóa.")]
    [SerializeField] private PetData evolutionTarget;

    [Tooltip("Cấp độ cần để tiến hóa.")]
    [SerializeField, Min(1)] private int evolveLevel = 10;

    [Tooltip("Số vàng cần để tiến hóa.")]
    [SerializeField, Min(0)] private int evolveGoldCost = 1000;

    public string PetId => petId;
    public string PetName => petName;
    public int Level => level;

    public Sprite ListIcon => listIcon;
    public Sprite DisplayImage => displayImage;
    public Sprite EvolutionImage => evolutionImage;
    public Sprite ElementIcon => elementIcon;

    public PetElement Element => element;
    public PetRarity Rarity => rarity;
    public string Personality => personality;

    public int Health => health;
    public int Attack => attack;
    public int Defense => defense;
    public int Speed => speed;
    public int Power => power;

    public IReadOnlyList<PetSkillEntry> Skills => skills;

    public PetData EvolutionTarget => evolutionTarget;
    public int EvolveLevel => evolveLevel;
    public int EvolveGoldCost => evolveGoldCost;

    /// <summary>
    /// AFTER bằng BEFORE tăng thêm 50%.
    /// Ví dụ 100 sẽ thành 150.
    /// </summary>
    public int GetEvolutionStat(int beforeValue)
    {
        return Mathf.RoundToInt(
            Mathf.Max(0, beforeValue) * 1.5f
        );
    }
}