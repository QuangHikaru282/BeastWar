using UnityEngine;

public enum MoveType
{
    Melee,  // Đánh gần (lao vào mục tiêu)
    Ranged, // Đánh xa (đứng tại chỗ niệm chú)
    Self    // Bản thân (dùng cho buff / hồi máu)
}

public enum VfxSpawnType
{
    SpawnAtTarget,       // Hiện ra ngay trên vị trí địch (ví dụ: Sét đánh từ trên xuống)
    ShootFromAttacker,   // Bay từ người tấn công đến kẻ địch (ví dụ: Cầu lửa, Khí công)
    RainFromSky,         // Rơi từ trên trời xuống kẻ địch (ví dụ: Thiên thạch, mưa băng)
    SpawnAtSelf          // Hiện ra ngay trên bản thân (ví dụ: Hồi máu, Buff giáp)
}

[CreateAssetMenu(fileName = "NewMoveData", menuName = "BeastBall/MoveData")]
public class MoveData : ScriptableObject
{
    [Header("Thông tin")]
    public string moveName = "Tấn công";
    public MoveType moveType = MoveType.Melee;
    public Sprite icon;
    [TextArea(2, 4)] public string description = "";

    [Header("Chỉ số")]
    [Min(0)] public int power = 40;
    // Công thức tính sát thương: damage = Max(1, attacker.attack * power / 50 - defender.defense)

    [Header("Hiệu ứng (VFX)")]
    public GameObject vfxPrefab; // Prefab hiệu ứng sẽ tạo ra khi dùng chiêu
    public VfxSpawnType vfxSpawnType = VfxSpawnType.SpawnAtTarget;
}
