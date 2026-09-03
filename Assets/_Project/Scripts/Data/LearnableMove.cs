using UnityEngine;

/// <summary>
/// Một entry trong bảng chiêu học theo level của Beast.
/// Kéo thả trong Inspector của BeastData.
/// </summary>
[System.Serializable]
public class LearnableMove
{
    [Tooltip("Beast phải đạt đúng level này thì mới học được chiêu.")]
    [Min(1)] public int levelRequired = 1;

    [Tooltip("Chiêu thức sẽ học được khi đạt levelRequired.")]
    public MoveData move;
}
