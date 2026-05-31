using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject lưu trạng thái người chơi — dùng chung xuyên suốt các Scene.
/// Kéo asset này vào mọi Manager cần đọc/ghi thông tin người chơi.
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "BeastBall/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Bộ sưu tập Beast")]
    public List<BeastData> ownedBeasts = new List<BeastData>();

    [Header("Đội hình hiện tại (tối đa 3)")]
    public List<BeastData> currentFormation = new List<BeastData>();

    public const int MaxFormationSize = 3;

    /// <summary>Thêm Beast vào bộ sưu tập (sau khi bắt được).</summary>
    public void AddBeast(BeastData beast)
    {
        if (beast == null) return;
        if (!ownedBeasts.Contains(beast))
            ownedBeasts.Add(beast);
        Debug.Log($"[PlayerData] Đã thêm {beast.beastName} vào bộ sưu tập. Tổng: {ownedBeasts.Count}");
    }

    /// <summary>Lưu đội hình hiện tại.</summary>
    public void SetFormation(List<BeastData> formation)
    {
        currentFormation = new List<BeastData>(formation);
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        ownedBeasts.Clear();
        currentFormation.Clear();
    }
}
