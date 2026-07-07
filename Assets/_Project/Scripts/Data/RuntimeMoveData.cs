using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimeMoveData
{
    public MoveData baseMove;
    public int currentLevel;
    public int power;

    public RuntimeMoveData(MoveData baseData, int level = 1)
    {
        baseMove = baseData;
        currentLevel = level;
        
        // Sức mạnh cơ bản + thưởng thêm mỗi cấp
        if (baseData != null)
        {
            power = baseData.power + (currentLevel - 1) * 5;
        }
    }

    public int GetUpgradeCost()
    {
        if (baseMove == null) return 0;
        return baseMove.baseUpgradeCost * currentLevel;
    }

    public bool TryUpgrade(PlayerData playerData)
    {
        if (baseMove == null) return false;

        if (currentLevel >= baseMove.maxLevel)
        {
            Debug.Log($"[RuntimeMoveData] {baseMove.moveName} đã đạt cấp tối đa!");
            return false;
        }

        int cost = GetUpgradeCost();
        if (playerData.gold >= cost)
        {
            playerData.gold -= cost;
            currentLevel++;
            power += 5; // Tăng sức mạnh mỗi cấp
            playerData.Save(); // Lưu lại số tiền đã trừ
            
            Debug.Log($"[RuntimeMoveData] Nâng cấp {baseMove.moveName} thành công lên cấp {currentLevel}. Sức mạnh mới: {power}. Vàng còn: {playerData.gold}");
            return true;
        }
        else
        {
            Debug.Log($"[RuntimeMoveData] Không đủ Vàng để nâng cấp {baseMove.moveName}. Cần {cost}, hiện có {playerData.gold}");
            return false;
        }
    }
}
