using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// AI đơn giản cho đội địch.
/// Chọn Beast tấn công và mục tiêu theo chiến thuật cơ bản.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    public enum AIStrategy
    {
        Random,         // Hoàn toàn ngẫu nhiên
        TargetWeakest,  // Ưu tiên đánh Beast Player yếu máu nhất
        TargetStrongest // Ưu tiên đánh Beast Player mạnh nhất (phá khiên)
    }

    [SerializeField] private AIStrategy strategy = AIStrategy.TargetWeakest;

    /// <summary>
    /// Chọn Beast địch sẽ tấn công, mục tiêu Player, và chiêu thức sử dụng.
    /// Trả về true nếu chọn được hành động hợp lệ.
    /// </summary>
    public bool ChooseAction(
        List<BeastUnit> enemyTeam,
        List<BeastUnit> playerTeam,
        out BeastUnit attacker,
        out BeastUnit target,
        out MoveData move)
    {
        attacker = null;
        target   = null;
        move     = null;

        var aliveEnemies = enemyTeam.Where(b => b != null && b.IsAlive).ToList();
        var aliveTargets = playerTeam.Where(b => b != null && b.IsAlive).ToList();

        if (aliveEnemies.Count == 0 || aliveTargets.Count == 0) return false;

        // Chọn Beast tấn công ngẫu nhiên từ đội địch còn sống
        attacker = aliveEnemies[Random.Range(0, aliveEnemies.Count)];

        // Chọn mục tiêu theo chiến thuật
        target = strategy switch
        {
            AIStrategy.TargetWeakest   => aliveTargets.OrderBy(b => b.CurrentHP).First(),
            AIStrategy.TargetStrongest => aliveTargets.OrderByDescending(b => b.CurrentHP).First(),
            _                          => aliveTargets[Random.Range(0, aliveTargets.Count)]
        };

        // Chọn chiêu thức: nếu Beast có move thì dùng move đầu tiên, không thì đánh thường (move = null)
        if (attacker.Data.moves != null && attacker.Data.moves.Length > 0)
            move = attacker.Data.moves[Random.Range(0, attacker.Data.moves.Length)];

        return true;
    }
}
