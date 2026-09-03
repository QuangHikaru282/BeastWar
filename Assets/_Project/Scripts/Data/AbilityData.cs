using UnityEngine;

/// <summary>
/// Dữ liệu Đặc tính / Nội tại (Ability) của thú chuẩn Pokémon.
/// Kéo thả vào BeastData hoặc gán cho RuntimeBeastData.
/// </summary>
[CreateAssetMenu(fileName = "NewAbility", menuName = "BeastBall/AbilityData")]
public class AbilityData : ScriptableObject
{
    [Header("Thông tin Đặc tính")]
    public string abilityName = "Mắt Kép";

    [TextArea(2, 4)]
    public string description = "Tăng độ chính xác cho các chiêu thức.";

    [Header("Hiệu ứng chiến đấu (tùy chọn)")]
    [Tooltip("Tỉ lệ % cộng thêm vào độ chính xác (Ví dụ 30% của Mắt Kép)")]
    [Range(0f, 100f)]
    public float accuracyBonus = 0f;

    [Tooltip("Tỉ lệ % tăng sát thương khi máu dưới 30% (Ví dụ 50% của Rực Lửa / Blaze)")]
    [Range(0f, 100f)]
    public float pinchDamageBonus = 0f;
}
