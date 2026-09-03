using UnityEngine;

/// <summary>
/// Một nhánh tiến hóa bằng đá (Evolution Stone Entry).
/// Gán vào List trong BeastData qua Inspector.
/// 
/// Ví dụ: Eevee có thể có 3 entries khác nhau:
///   [0] requiredStone = Fire_Evole_Stone  → evolveTarget = Flareon
///   [1] requiredStone = Water_Evole_Stone → evolveTarget = Vaporeon
///   [2] requiredStone = Light_Evole_Stone → evolveTarget = Espeon
/// </summary>
[System.Serializable]
public class EvolutionEntry
{
    [Tooltip("Kéo Item đá (VD: Fire_Evole_Stone) từ Project vào đây.")]
    public Kinnly.Item requiredStone;

    [Tooltip("Kéo BeastData dạng tiến hóa tương ứng vào đây.")]
    public BeastData evolveTarget;
}
