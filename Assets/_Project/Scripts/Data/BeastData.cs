using System.Collections.Generic;
using UnityEngine;

public enum BeastElement
{
    Normal, Fire, Water, Grass,
    Electric, Ice, Ground, Rock,
    Fighting, Poison, Flying, Psychic,
    Dark, Steel, Dragon
}

[CreateAssetMenu(fileName = "NewBeastData", menuName = "BeastBall/BeastData")]
public class BeastData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string beastName = "Unknown Beast";
    public BeastElement element = BeastElement.Normal;
    public bool isRare = false;
    public Sprite frontSprite;   // Sprite hiển thị khi là địch (nhìn về phía player)
    public Sprite backSprite;    // Sprite hiển thị khi là của mình (nhìn về phía địch)
    public RuntimeAnimatorController animatorController; // Hoạt ảnh chiến đấu của thú

    [Header("Chỉ số chiến đấu")]
    [Min(1)] public int maxHP = 100;
    [Min(1)] public int attack = 50;
    [Min(1)] public int defense = 30;
    [Min(1)] public int speed = 40;

    [Header("Thu phục")]
    [Range(0f, 1f)] public float captureRate = 1.0f;
    // captureRate: 1.0 = cực dễ bắt (thú thường), 0.45 = quý hiếm, 0.1 = huyền thoại

    [Header("Chiêu thức (tối đa 4)")]
    public MoveData[] moves = new MoveData[0];

    /// <summary>Lực chiến tính tự động từ các chỉ số.</summary>
    public int CombatPower => maxHP + attack * 2 + defense + speed;

    // ─── TIẾN HÓA ───────────────────────────────
    
    [Header("Tiến hóa")]
    public BeastData evolveTarget = null; // Thú sẽ tiến hóa thành. Bỏ trống nếu không thể tiến hóa.
    public int evolveLevel = 0;           // Cấp độ yêu cầu (vd: 16)
    public int evolveGoldCost = 1000;     // Lượng vàng yêu cầu

    // ─── PHẦN THƯỞNG ──────────────────────────────
    [Header("Phần thưởng khi bị tiêu diệt")]
    public int rewardGold = 10;
    public int rewardExp = 50;

}
