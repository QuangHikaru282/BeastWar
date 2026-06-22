using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chứa toàn bộ danh sách ải của game.
/// Tạo bằng: chuột phải trong Project → Create → BeastBall → WorldMapData
/// </summary>
[CreateAssetMenu(fileName = "WorldMapData", menuName = "BeastBall/WorldMapData")]
public class WorldMapData : ScriptableObject
{
    [Header("Danh sách 20 ải (kéo StageData vào theo thứ tự)")]
    public List<StageData> stages = new List<StageData>();
}
