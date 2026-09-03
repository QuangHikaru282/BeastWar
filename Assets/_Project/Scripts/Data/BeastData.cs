using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nguyên tố dùng cho hệ thống chiến đấu.
/// Giữ nguyên thứ tự để không ảnh hưởng dữ liệu cũ.
/// </summary>
public enum BeastElement
{
    Normal,
    Fire,
    Water,
    Grass,
    Electric,
    Ice,
    Ground,
    Rock,
    Fighting,
    Poison,
    Flying,
    Psychic,
    Dark,
    Steel,
    Dragon
}

/// <summary>
/// Nguyên tố dùng riêng để chọn panel Enhance.
/// </summary>
public enum EnhanceElement
{
    None = 0,
    Metal = 1,
    Water = 2,
    Wood = 3,
    Fire = 4,
    Earth = 5,
    Light = 6,
    Dark = 7
}

[CreateAssetMenu(
    fileName = "NewBeastData",
    menuName = "BeastBall/BeastData"
)]
public class BeastData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    [Tooltip("Số thứ tự trong Pokédex/BeastDex (VD: 1, 4, 7, 12...)")]
    [Min(1)] public int pokedexNumber = 1;
    public string beastName = "Unknown Beast";

    [Tooltip("Nguyên tố chính.")]
    public BeastElement element = BeastElement.Normal;

    [Tooltip("Nguyên tố phụ (để None nếu là thú đơn hệ).")]
    public BeastElement secondaryElement = BeastElement.Normal;

    [Tooltip("Đặc tính / Nội tại mặc định của loài.")]
    public AbilityData defaultAbility;

    [Tooltip("Nguyên tố dùng để chọn panel Enhance.")]
    public EnhanceElement enhanceElement = EnhanceElement.None;

    public bool isRare = false;

    [Tooltip("Mô tả tập tính, thông tin loài hiển thị trong BeastDex")]
    [TextArea(2, 5)]
    public string speciesDescription = "Một loài Beast bí ẩn trong thế giới Verdania.";

    [Header("Hình ảnh")]
    [Tooltip("Ảnh Pet nhìn về phía người chơi.")]
    public Sprite frontSprite;

    [Tooltip("Ảnh Pet nhìn về phía đối thủ.")]
    public Sprite backSprite;

    [Tooltip("Image 2: ảnh xem trước Pet sau tiến hóa.")]
    public Sprite image2;

    public RuntimeAnimatorController animatorController;

    [Header("Chỉ số chiến đấu")]
    [Min(1)] public int maxHP = 100;
    [Min(1)] public int attack = 50;
    [Min(1)] public int defense = 30;
    [Min(1)] public int spAttack = 50;
    [Min(1)] public int spDefense = 50;
    [Min(1)] public int speed = 40;

    [Header("Thu phục")]
    [Range(0f, 1f)]
    public float captureRate = 1f;

    [Header("Chiêu thức ban đầu - tối đa 4")]
    [Tooltip("Chiêu thức trang bị sẵn khi bắt được hoặc tạo thú ở level 1.")]
    public MoveData[] moves = new MoveData[0];

    [Header("Bảng chiêu học theo Level")]
    [Tooltip("Danh sách chiêu thức thú sẽ học khi đạt đúng cấp độ yêu cầu. Sắp xếp theo level tăng dần.")]
    public List<LearnableMove> learnableMoves = new List<LearnableMove>();

    public int CombatPower =>
        maxHP +
        attack * 2 +
        defense +
        speed;

    [Header("Tiến hóa theo Level + Vàng (cũ)")]
    [Tooltip("Pet sẽ tiến hóa thành khi đủ Level và Vàng. Để trống nếu chỉ dùng Đá.")]
    public BeastData evolveTarget;

    [Min(0)]
    public int evolveLevel;

    [Min(0)]
    public int evolveGoldCost = 1000;

    [Header("Tiến hóa bằng Đá (chuẩn Pokémon)")]
    [Tooltip("Mỗi entry: kéo Đá (Kinnly.Item) + BeastData dạng tiến hóa tương ứng vào. " +
             "Một thú có thể có nhiều nhánh tiến hóa (VD: Eevee 3 đá khác nhau).")]
    public List<EvolutionEntry> evolutionStones = new List<EvolutionEntry>();

    [Header("Phần thưởng khi bị tiêu diệt")]
    [Min(0)] public int rewardGold = 10;
    [Min(0)] public int rewardExp = 50;

    public Sprite EvolutionImage => image2;

    public int GetAfterEvolutionStat(int currentValue)
    {
        return Mathf.RoundToInt(
            Mathf.Max(0, currentValue) * 1.5f
        );
    }
}