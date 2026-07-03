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

    [Header("Hệ thống Nâng Cấp (Tạm thời lưu trên SO)")]
    [Min(1)] public int currentLevel = 1;
    public int maxLevel = 10;
    
    // Vàng cơ bản để nâng từ cấp 1 lên 2, các cấp sau sẽ nhân lên
    public int baseUpgradeCost = 100; 

    public int GetUpgradeCost()
    {
        return baseUpgradeCost * currentLevel;
    }

    /// <summary>
    /// Hàm gọi khi bấm nâng cấp kỹ năng. Trả về true nếu thành công.
    /// </summary>
    public bool TryUpgrade(PlayerData playerData)
    {
        if (currentLevel >= maxLevel)
        {
            Debug.Log($"[MoveData] {moveName} đã đạt cấp tối đa!");
            return false;
        }

        int cost = GetUpgradeCost();
        if (playerData.gold >= cost)
        {
            playerData.gold -= cost;
            currentLevel++;
            power += 5; // Tăng sức mạnh mỗi cấp
            playerData.Save(); // Lưu lại số tiền đã trừ
            
            Debug.Log($"[MoveData] Nâng cấp {moveName} thành công lên cấp {currentLevel}. Sức mạnh mới: {power}. Vàng còn: {playerData.gold}");
            return true;
        }
        else
        {
            Debug.Log($"[MoveData] Không đủ Vàng để nâng cấp {moveName}. Cần {cost}, hiện có {playerData.gold}");
            return false;
        }
    }
}
