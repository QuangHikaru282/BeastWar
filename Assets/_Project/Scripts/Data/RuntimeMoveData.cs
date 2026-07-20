using UnityEngine;

/// <summary>
/// Dữ liệu runtime của một kỹ năng.
///
/// MoveData chứa dữ liệu gốc.
/// RuntimeMoveData chứa cấp độ và sức mạnh hiện tại
/// của kỹ năng thuộc về một Pet.
/// </summary>
[System.Serializable]
public class RuntimeMoveData : ISerializationCallbackReceiver
{
    [Header("Dữ liệu kỹ năng gốc")]
    public MoveData baseMove;

    [Header("Dữ liệu runtime")]
    [Min(1)]
    public int currentLevel = 1;

    [Min(0)]
    public int power;

    /// <summary>
    /// Constructor rỗng để Unity có thể serialize dữ liệu.
    /// </summary>
    public RuntimeMoveData()
    {
        currentLevel = 1;
        power = 0;
    }

    /// <summary>
    /// Tạo một kỹ năng runtime từ MoveData.
    /// </summary>
    public RuntimeMoveData(
        MoveData baseData,
        int level = 1
    )
    {
        Initialize(baseData, level);
    }

    /// <summary>
    /// Khởi tạo hoặc thay đổi dữ liệu kỹ năng runtime.
    /// </summary>
    public void Initialize(
        MoveData baseData,
        int level = 1
    )
    {
        baseMove = baseData;
        currentLevel = Mathf.Max(1, level);

        ClampLevel();
        RecalculatePower();
    }

    /// <summary>
    /// Tính lại sức mạnh kỹ năng dựa theo cấp hiện tại.
    ///
    /// Công thức:
    /// Power hiện tại = Power gốc + (Level - 1) × 5.
    /// </summary>
    public void RecalculatePower()
    {
        if (baseMove == null)
        {
            power = 0;
            return;
        }

        currentLevel = Mathf.Max(1, currentLevel);

        power =
            baseMove.power +
            (currentLevel - 1) * 5;

        power = Mathf.Max(0, power);
    }

    /// <summary>
    /// Lấy giá nâng cấp kỹ năng ở cấp hiện tại.
    /// </summary>
    public int GetUpgradeCost()
    {
        if (baseMove == null)
            return 0;

        int baseCost = Mathf.Max(
            0,
            baseMove.baseUpgradeCost
        );

        return baseCost * Mathf.Max(1, currentLevel);
    }

    /// <summary>
    /// Kiểm tra kỹ năng đã đạt cấp tối đa chưa.
    /// </summary>
    public bool IsMaxLevel()
    {
        if (baseMove == null)
            return true;

        int maxLevel = Mathf.Max(
            1,
            baseMove.maxLevel
        );

        return currentLevel >= maxLevel;
    }

    /// <summary>
    /// Kiểm tra người chơi có thể nâng kỹ năng hay không.
    /// </summary>
    public bool CanUpgrade(
        PlayerData playerData,
        out string reason
    )
    {
        if (baseMove == null)
        {
            reason = "Kỹ năng chưa có MoveData.";
            return false;
        }

        if (playerData == null)
        {
            reason = "PlayerData đang bị null.";
            return false;
        }

        if (IsMaxLevel())
        {
            reason =
                $"{baseMove.moveName} đã đạt cấp tối đa.";
            return false;
        }

        int upgradeCost = GetUpgradeCost();

        if (playerData.gold < upgradeCost)
        {
            reason =
                $"Không đủ vàng. Cần {upgradeCost}, " +
                $"hiện có {playerData.gold}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    /// <summary>
    /// Thử nâng cấp kỹ năng.
    /// Trả về true nếu nâng cấp thành công.
    /// </summary>
    public bool TryUpgrade(PlayerData playerData)
    {
        if (!CanUpgrade(playerData, out string reason))
        {
            Debug.LogWarning(
                $"[RuntimeMoveData] {reason}"
            );

            return false;
        }

        int upgradeCost = GetUpgradeCost();

        // Trừ vàng.
        playerData.gold -= upgradeCost;
        playerData.gold = Mathf.Max(0, playerData.gold);

        // Tăng cấp kỹ năng.
        currentLevel++;

        ClampLevel();
        RecalculatePower();

        // Lưu dữ liệu người chơi.
        playerData.Save();

        Debug.Log(
            $"[RuntimeMoveData] Nâng cấp " +
            $"{baseMove.moveName} thành công. " +
            $"Cấp mới: {currentLevel}. " +
            $"Sức mạnh mới: {power}. " +
            $"Đã dùng: {upgradeCost} vàng. " +
            $"Vàng còn lại: {playerData.gold}."
        );

        return true;
    }

    /// <summary>
    /// Giới hạn cấp hiện tại trong khoảng hợp lệ.
    /// </summary>
    private void ClampLevel()
    {
        currentLevel = Mathf.Max(
            1,
            currentLevel
        );

        if (baseMove == null)
            return;

        int maxLevel = Mathf.Max(
            1,
            baseMove.maxLevel
        );

        currentLevel = Mathf.Clamp(
            currentLevel,
            1,
            maxLevel
        );
    }

    /// <summary>
    /// Được Unity gọi trước khi serialize.
    /// </summary>
    public void OnBeforeSerialize()
    {
        ClampLevel();
        RecalculatePower();
    }

    /// <summary>
    /// Được Unity gọi sau khi load dữ liệu.
    /// Tự tính lại power để tránh sai dữ liệu.
    /// </summary>
    public void OnAfterDeserialize()
    {
        ClampLevel();
        RecalculatePower();
    }
}