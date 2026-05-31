using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBeastData", menuName = "BeastBall/BeastData")]
public class BeastData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string beastName = "Unknown Beast";
    public Sprite frontSprite;   // Sprite hiển thị khi là địch (nhìn về phía player)
    public Sprite backSprite;    // Sprite hiển thị khi là của mình (nhìn về phía địch)
    public RuntimeAnimatorController animatorController; // Hoạt ảnh chiến đấu của thú

    [Header("Chỉ số chiến đấu")]
    [Min(1)] public int maxHP = 100;
    [Min(1)] public int attack = 50;
    [Min(1)] public int defense = 30;
    [Min(1)] public int speed = 40;

    [Header("Thu phục")]
    [Range(0, 255)] public int catchRate = 45;
    // catchRate: 255 = cực dễ bắt, 3 = cực khó bắt (như huyền thoại)

    [Header("Chiêu thức (tối đa 4)")]
    public MoveData[] moves = new MoveData[0];

    /// <summary>Lực chiến tính tự động từ các chỉ số.</summary>
    public int CombatPower => maxHP + attack * 2 + defense + speed;
}
