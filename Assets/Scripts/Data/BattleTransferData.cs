using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject làm "kênh" truyền dữ liệu giữa Map Scene và Battle Scene.
/// WildBeastEncounter ghi vào đây, BattleManager đọc từ đây.
/// </summary>
[CreateAssetMenu(fileName = "BattleTransferData", menuName = "BeastBall/BattleTransferData")]
public class BattleTransferData : ScriptableObject
{
    [Header("Đội địch (gặp trên Map)")]
    public List<BeastData> wildEnemyTeam = new List<BeastData>();

    [Header("Trạng thái quái trên Map")]
    public string lastEncounteredBeastId;
    public List<string> stunnedBeastIds = new List<string>();
    public List<string> caughtBeastIds = new List<string>();

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
    }
}
