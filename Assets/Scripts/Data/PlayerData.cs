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

    // ─── TIẾN TRÌNH ẢI ───────────────────────────────────────────────

    [Header("Tiến trình ải")]
    [Tooltip("Ải cao nhất đã mở khoá (mặc định là 1 = ải đầu tiên đã mở)")]
    public int highestUnlockedStage = 1;

    [Tooltip("Số vàng hiện có")]
    public int gold = 0;

    /// <summary>Mảng lưu số sao của từng ải (index 0 = ải 1, index 1 = ải 2 ...).
    /// Kích thước 20 (tương ứng 20 ải).</summary>
    [SerializeField] private int[] stageStarsArray = new int[21]; // index 0 bỏ trống, dùng 1-20

    // ─── Beast Methods ───────────────────────────────────────────────

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

    // ─── Stage Methods ───────────────────────────────────────────────

    /// <summary>Lấy số sao của ải stageId (1-based). Trả về 0 nếu chưa qua.</summary>
    public int GetStageStars(int stageId)
    {
        if (stageId <= 0 || stageId >= stageStarsArray.Length) return 0;
        return stageStarsArray[stageId];
    }

    /// <summary>
    /// Ghi kết quả sau khi thắng một ải.
    /// Tự động mở khoá ải tiếp theo và cộng vàng thưởng.
    /// </summary>
    public void SetStageResult(int stageId, int stars, int rewardGold = 0)
    {
        if (stageId <= 0) return;

        // Chỉ ghi nếu số sao mới tốt hơn
        if (stageId < stageStarsArray.Length && stars > stageStarsArray[stageId])
        {
            stageStarsArray[stageId] = Mathf.Clamp(stars, 0, 3);
        }

        // Mở ải tiếp theo
        if (stageId >= highestUnlockedStage)
        {
            highestUnlockedStage = stageId + 1;
            Debug.Log($"[PlayerData] Mở ải {highestUnlockedStage}!");
        }

        // Cộng vàng (chỉ cộng lần đầu thắng ải)
        if (stageId < stageStarsArray.Length && stageStarsArray[stageId] == 0)
        {
            gold += rewardGold;
            Debug.Log($"[PlayerData] Nhận {rewardGold} vàng. Tổng: {gold}");
        }
    }

    // ─── Reset ───────────────────────────────────────────────────────

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        ownedBeasts.Clear();
        currentFormation.Clear();
        highestUnlockedStage = 1;
        gold = 0;
        stageStarsArray = new int[21];
    }
}
