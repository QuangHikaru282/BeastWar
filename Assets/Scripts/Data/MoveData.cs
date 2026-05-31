using UnityEngine;

[CreateAssetMenu(fileName = "NewMoveData", menuName = "BeastBall/MoveData")]
public class MoveData : ScriptableObject
{
    [Header("Thông tin")]
    public string moveName = "Tấn công";
    public Sprite icon;
    [TextArea(2, 4)] public string description = "";

    [Header("Chỉ số")]
    [Min(0)] public int power = 40;
    // Công thức tính sát thương: damage = Max(1, attacker.attack * power / 50 - defender.defense)
}
