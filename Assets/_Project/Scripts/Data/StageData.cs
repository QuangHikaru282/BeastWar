using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dữ liệu của một ải (Stage).
/// Tạo bằng: chuột phải trong Project → Create → BeastBall → StageData
/// </summary>
[CreateAssetMenu(fileName = "Stage_01", menuName = "BeastBall/StageData")]
public class StageData : ScriptableObject
{
    [Header("Thông tin ải")]
    public int stageId;           // Số thứ tự ải (1, 2, 3 ... 20)
    public string stageName;       // Tên hiển thị (ví dụ: "Đồng Cỏ Xanh")

    [Header("Đội quái của ải này")]
    public List<BeastData> enemyTeam = new List<BeastData>();

    [Header("Phần thưởng")]
    public int rewardGold = 50;    // Vàng thưởng khi thắng
}
