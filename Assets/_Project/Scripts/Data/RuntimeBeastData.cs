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

    /// <summary>Thực hiện tiến hóa cho thú.</summary>
    public bool Evolve()
    {
        if (baseBeast == null || baseBeast.evolveTarget == null) return false;
        
        // Đổi baseBeast thành target tiến hóa
        baseBeast = baseBeast.evolveTarget;

        // Tiến hóa xong sẽ học thêm moves của dạng mới (nếu có)
        // Hiện tại ta có thể hợp nhất chiêu thức hoặc chỉ đơn giản là giữ nguyên / cập nhật chiêu mới.
        // Để đơn giản, ta giữ nguyên chiêu cũ, nếu thú mới có chiêu mới, ta thêm vào chỗ trống.
        if (baseBeast.moves != null && baseBeast.moves.Length > 0)
        {
            var newMovesList = new List<RuntimeMoveData>(moves);
            foreach (var m in baseBeast.moves)
            {
                if (m != null && !newMovesList.Exists(x => x != null && x.baseMove == m))
                {
                    if (newMovesList.Count < 4) // Tối đa 4 chiêu
                    {
                        newMovesList.Add(new RuntimeMoveData(m, 1));
                    }
                }
            }
            moves = newMovesList.ToArray();
        }

        return true;
    }
}
