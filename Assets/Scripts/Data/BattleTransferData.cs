using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject làm "kênh" truyền dữ liệu giữa Map/Hunting Scene và Battle Scene.
/// WildBeastEncounter / HuntingBeastEncounter ghi vào đây, BattleManager đọc từ đây.
/// </summary>
[CreateAssetMenu(fileName = "BattleTransferData", menuName = "BeastBall/BattleTransferData")]
public class BattleTransferData : ScriptableObject
{
    /// <summary>Scene nào đã kích hoạt trận chiến này.</summary>
    public enum OriginScene { Map, Hunting, WorldMap }

    [Header("Nguồn gốc trận chiến")]
    public OriginScene originScene = OriginScene.Map;

    /// <summary>
    /// Nếu true, BattleManager chỉ dùng 1 Beast đầu tiên trong đội hình của Player (1v1).
    /// Dùng khi đến từ HuntingScene.
    /// </summary>
    public bool isSingleBattle = false;

    [Header("Đội địch (gặp trên Map/Hunting)")]
    public List<BeastData> wildEnemyTeam = new List<BeastData>();

    [Header("Trạng thái quái trên Map/Hunting")]
    public string lastEncounteredBeastId;
    public List<string> stunnedBeastIds = new List<string>();
    public List<string> caughtBeastIds = new List<string>();

    [Header("Ải đang đấu (WorldMap)")]
    [Tooltip("ID của ải đang đấu. -1 nếu không phải ải thường.")]
    public int currentStageId = -1;

    public void SetEnemyTeam(List<BeastData> team)
    {
        wildEnemyTeam = new List<BeastData>(team);
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        lastEncounteredBeastId = "";
        stunnedBeastIds.Clear();
        caughtBeastIds.Clear();
        originScene = OriginScene.Map;
        isSingleBattle = false;
    }
}
