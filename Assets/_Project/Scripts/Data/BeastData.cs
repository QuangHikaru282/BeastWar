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
    public string beastName = "Unknown Beast";

    [Tooltip("Nguyên tố dùng trong chiến đấu.")]
    public BeastElement element = BeastElement.Normal;

    [Tooltip("Nguyên tố dùng để chọn panel Enhance.")]
    public EnhanceElement enhanceElement = EnhanceElement.None;

    public bool isRare = false;

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
    [Min(1)] public int speed = 40;

    [Header("Thu phục")]
    [Range(0f, 1f)]
    public float captureRate = 1f;

    [Header("Chiêu thức - tối đa 4")]
    public MoveData[] moves = new MoveData[0];

    public int CombatPower =>
        maxHP +
        attack * 2 +
        defense +
        speed;

    [Header("Tiến hóa")]
    [Tooltip("Pet sẽ tiến hóa thành. Có thể để trống khi đang thiết kế UI.")]
    public BeastData evolveTarget;

    [Min(0)]
    public int evolveLevel;

    [Min(0)]
    public int evolveGoldCost = 1000;

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