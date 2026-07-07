using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimeBeastData
{
    public BeastData baseBeast;
    public int currentLevel;
    public int currentExp;
    public RuntimeMoveData[] moves;

    public RuntimeBeastData(BeastData data, int level = 1)
    {
        baseBeast = data;
        currentLevel = level;
        currentExp = 0;

        // Clone moves
        if (data != null && data.moves != null)
        {
            moves = new RuntimeMoveData[data.moves.Length];
            for (int i = 0; i < data.moves.Length; i++)
            {
                if (data.moves[i] != null)
                {
                    moves[i] = new RuntimeMoveData(data.moves[i], 1);
                }
            }
        }
        else
        {
            moves = new RuntimeMoveData[0];
        }
    }

    // --- Dynamic Stats ---

    public int MaxHP => baseBeast != null ? baseBeast.maxHP + (currentLevel - 1) * 10 : 0;
    public int Attack => baseBeast != null ? baseBeast.attack + (currentLevel - 1) * 5 : 0;
    public int Defense => baseBeast != null ? baseBeast.defense + (currentLevel - 1) * 3 : 0;
    public int Speed => baseBeast != null ? baseBeast.speed + (currentLevel - 1) * 2 : 0;

    /// <summary>Lực chiến tính tự động từ các chỉ số (Bao gồm cả chỉ số cộng thêm từ level).</summary>
    public int CombatPower => MaxHP + Attack * 2 + Defense + Speed;

    /// <summary>Tính lượng EXP cần để lên cấp tiếp theo.</summary>
    public int GetExpToNextLevel()
    {
        // Công thức cơ bản: Cấp hiện tại * 100 (VD: Lv1 cần 100 EXP, Lv2 cần 200 EXP)
        return currentLevel * 100;
    }
}
