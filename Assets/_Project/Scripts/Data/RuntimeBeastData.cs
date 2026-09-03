using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimeBeastData
{
    public BeastData baseBeast;
    public int currentLevel;
    public int currentExp;
    public int currentHP = -1; // -1 nghĩa là chưa khởi tạo, mặc định bằng MaxHP
    public RuntimeMoveData[] moves;

    [Header("Hồ sơ & Ký ức HLV")]
    public NatureType nature = NatureType.Hardy;
    public AbilityData ability;
    public Kinnly.Item heldItem;
    public string originalTrainer = "Truy";
    public int trainerId = 65375;
    public string caughtLocation = "Rừng Tokiwa";
    public int caughtLevel = 1;

    public RuntimeBeastData(BeastData data, int level = 1)
    {
        baseBeast = data;
        currentLevel = level;
        currentExp = 0;
        currentHP = MaxHP;
        caughtLevel = level;

        // Random tính cách khi tạo thú mới
        nature = NatureUtils.GetRandomNature();

        // Gán đặc tính mặc định của loài
        if (data != null && data.defaultAbility != null)
        {
            ability = data.defaultAbility;
        }

        // Tự động gán chiêu theo chuẩn Pokémon FireRed (lấy tối đa 4 chiêu mới nhất <= level hiện tại)
        if (data != null && data.learnableMoves != null && data.learnableMoves.Count > 0)
        {
            List<LearnableMove> eligible = new List<LearnableMove>();
            foreach (var lm in data.learnableMoves)
            {
                if (lm != null && lm.move != null && lm.levelRequired <= level)
                {
                    eligible.Add(lm);
                }
            }

            // Sắp xếp theo level tăng dần
            eligible.Sort((a, b) => a.levelRequired.CompareTo(b.levelRequired));

            // Lấy tối đa 4 chiêu mới nhất
            List<RuntimeMoveData> selectedMoves = new List<RuntimeMoveData>();
            int startIdx = Mathf.Max(0, eligible.Count - 4);
            for (int i = startIdx; i < eligible.Count; i++)
            {
                var m = eligible[i].move;
                if (!selectedMoves.Exists(x => x.baseMove == m))
                {
                    selectedMoves.Add(new RuntimeMoveData(m, 1));
                }
            }

            moves = selectedMoves.ToArray();
        }
        else if (data != null && data.moves != null)
        {
            // Fallback nếu chưa cài bảng learnableMoves
            List<RuntimeMoveData> fallbackList = new List<RuntimeMoveData>();
            for (int i = 0; i < data.moves.Length; i++)
            {
                if (data.moves[i] != null) fallbackList.Add(new RuntimeMoveData(data.moves[i], 1));
            }
            moves = fallbackList.ToArray();
        }
        else
        {
            moves = new RuntimeMoveData[0];
        }
    }

    // --- Dynamic Stats (Áp dụng hệ số Tính cách) ---

    public int MaxHP => baseBeast != null ? baseBeast.maxHP + (currentLevel - 1) * 10 : 0;
    public int Attack => Mathf.RoundToInt((baseBeast != null ? baseBeast.attack + (currentLevel - 1) * 5 : 0) * NatureUtils.GetMultiplier(nature, StatType.Attack));
    public int Defense => Mathf.RoundToInt((baseBeast != null ? baseBeast.defense + (currentLevel - 1) * 3 : 0) * NatureUtils.GetMultiplier(nature, StatType.Defense));
    public int SpAttack => Mathf.RoundToInt((baseBeast != null ? baseBeast.spAttack + (currentLevel - 1) * 4 : 0) * NatureUtils.GetMultiplier(nature, StatType.SpAttack));
    public int SpDefense => Mathf.RoundToInt((baseBeast != null ? baseBeast.spDefense + (currentLevel - 1) * 3 : 0) * NatureUtils.GetMultiplier(nature, StatType.SpDefense));
    public int Speed => Mathf.RoundToInt((baseBeast != null ? baseBeast.speed + (currentLevel - 1) * 2 : 0) * NatureUtils.GetMultiplier(nature, StatType.Speed));

    /// <summary>Lực chiến tính tự động từ các chỉ số (Bao gồm cả chỉ số cộng thêm từ level).</summary>
    public int CombatPower => MaxHP + Attack * 2 + Defense + Speed;

    /// <summary>Tính lượng EXP cần để lên cấp tiếp theo.</summary>
    public int GetExpToNextLevel()
    {
        // Công thức cơ bản: Cấp hiện tại * 100 (VD: Lv1 cần 100 EXP, Lv2 cần 200 EXP)
        return currentLevel * 100;
    }

    /// <summary>Thực hiện tiến hóa theo evolveTarget mặc định (Level + Gold path).</summary>
    public bool Evolve()
    {
        if (baseBeast == null || baseBeast.evolveTarget == null) return false;
        return Evolve(baseBeast.evolveTarget);
    }

    /// <summary>
    /// Thực hiện tiến hóa thành <paramref name="target"/> bất kỳ.
    /// Dùng cho cả: tiến hóa theo Level và tiến hóa bằng Đá.
    /// Giữ nguyên Level, EXP, 4 chiêu hiện tại; thêm chiêu của dạng mới vào ô trống (nếu có).
    /// </summary>
    public bool Evolve(BeastData target)
    {
        if (target == null) return false;

        baseBeast = target;

        // Merge chiêu: Giữ chiêu cũ, thêm chiêu mới của dạng mới vào ô trống
        if (baseBeast.moves != null && baseBeast.moves.Length > 0)
        {
            var newMovesList = new List<RuntimeMoveData>(moves);
            foreach (var m in baseBeast.moves)
            {
                if (m == null) continue;
                if (newMovesList.Exists(x => x != null && x.baseMove == m)) continue;
                if (newMovesList.Count < 4)
                    newMovesList.Add(new RuntimeMoveData(m, 1));
            }
            moves = newMovesList.ToArray();
        }

        return true;
    }

    /// <summary>Hồi đầy máu cho Beast.</summary>
    public void HealFull()
    {
        currentHP = MaxHP;
    }
}
