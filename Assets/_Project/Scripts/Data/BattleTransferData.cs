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
    public enum OriginScene { Map, Hunting, WorldMap, Arena }

    [Header("Nguồn gốc trận chiến")]
    public OriginScene originScene = OriginScene.Map;

    [Header("Ải Arena đang đấu")]
    [Tooltip("ID của ải Arena đang đấu. -1 nếu không phải.")]
    public int currentArenaStageId = -1;

    [Header("Vị trí người chơi trước trận đấu")]
    public Vector3 lastPlayerPosition;
    public bool returnToLastPosition = false;

    /// <summary>
    /// Nếu true, BattleManager chỉ dùng 1 Beast đầu tiên trong đội hình của Player (1v1).
    /// Dùng khi đến từ HuntingScene.
    /// </summary>
    public bool isSingleBattle = false;

    [Header("Chế độ đấu Trainer & Gym Leader")]
    [Tooltip("Đánh với Trainer thì không được chạy trốn và không được bắt thú")]
    public bool isTrainerBattle = false;
    
    [Tooltip("Nêu đây là trận đấu Gym Leader")]
    public bool isGymLeaderBattle = false;
    
    [Tooltip("ID của Huy hiệu Gym sẽ trao khi đánh bại Gym Leader này")]
    public string rewardBadgeId = "";

    [Tooltip("Số vàng thưởng riêng cho trận này (nếu > 0 sẽ ưu tiên dùng số này)")]
    public int customRewardGold = 0;

    [Header("Đội địch (gặp trên Map/Hunting)")]
    public List<RuntimeBeastData> wildEnemyTeam = new List<RuntimeBeastData>();

    [Header("Trạng thái quái trên Map/Hunting")]
    public string lastEncounteredBeastId;
    public List<string> stunnedBeastIds = new List<string>();
    public List<string> caughtBeastIds = new List<string>();

    [Header("Ải đang đấu (WorldMap)")]
    [Tooltip("ID của ải đang đấu. -1 nếu không phải ải thường.")]
    public int currentStageId = -1;

    public void SetEnemyTeam(List<RuntimeBeastData> team)
    {
        wildEnemyTeam = new List<RuntimeBeastData>(team);
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        lastEncounteredBeastId = "";
        originScene = OriginScene.Map;
        isSingleBattle = false;
        isTrainerBattle = false;
        isGymLeaderBattle = false;   // QUAN TRỌNG: reset để tránh trao badge nhầm
        rewardBadgeId = "";          // QUAN TRỌNG: reset để tránh trao badge cũ
        customRewardGold = 0;
        currentArenaStageId = -1;
    }
}
