using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chứa toàn bộ danh sách 10 ải của Arena.
/// Tạo bằng: chuột phải trong Project → Create → BeastBall → ArenaData
/// </summary>
[CreateAssetMenu(fileName = "ArenaData", menuName = "BeastBall/ArenaData")]
public class ArenaData : ScriptableObject
{
    [Header("Danh sách 10 ải Arena")]
    public List<StageData> stages = new List<StageData>();

    [Header("Cấu hình mở khóa")]
    [Tooltip("Số ải cần vượt qua để mở khóa Thành Thị")]
    public int stagesToUnlockNewMap = 5;
}
