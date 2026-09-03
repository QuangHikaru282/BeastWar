using System;
using UnityEngine;

/// <summary>
/// 25 Tính cách (Nature) chuẩn Pokémon Gen 3 (FireRed/Emerald).
/// Mỗi tính cách tăng 10% một chỉ số và giảm 10% một chỉ số khác.
/// 5 tính cách trung tính (Hardy, Docile, Bashful, Quirky, Serious) giữ nguyên 1.0x.
/// </summary>
public enum NatureType
{
    // --- Tăng Tấn Công (Attack +10%) ---
    Hardy,    // Trung tính (Atk / Atk)
    Lonely,   // +Attack, -Defense (Cô đơn)
    Brave,    // +Attack, -Speed (Dũng cảm)
    Adamant,  // +Attack, -SpAttack (Kiên định)
    Naughty,  // +Attack, -SpDefense (Nghịch ngợm)

    // --- Tăng Phòng Thủ (Defense +10%) ---
    Bold,     // +Defense, -Attack (Táo bạo)
    Docile,   // Trung tính (Def / Def - Ngoan ngoãn)
    Relaxed,  // +Defense, -Speed (Thư thả)
    Impish,   // +Defense, -SpAttack (Tinh quái)
    Lax,      // +Defense, -SpDefense (Lỏng lẻo)

    // --- Tăng Tốc Độ (Speed +10%) ---
    Timid,    // +Speed, -Attack (Nhút nhát)
    Hasty,    // +Speed, -Defense (Hấp tấp)
    Jolly,    // +Speed, -SpAttack (Vui vẻ)
    Naive,    // +Speed, -SpDefense (Ngây thơ)
    Serious,  // Trung tính (Spe / Spe - Nghiêm túc)

    // --- Tăng Tấn Công Đặc Biệt (SpAttack +10%) ---
    Modest,   // +SpAttack, -Attack (Khiêm tốn)
    Mild,     // +SpAttack, -Defense (Ôn hòa)
    Quiet,    // +SpAttack, -Speed (Trầm lặng)
    Bashful,  // Trung tính (SpA / SpA - Nhút nhát)
    Rash,     // +SpAttack, -SpDefense (Hấp tấp)

    // --- Tăng Phòng Thủ Đặc Biệt (SpDefense +10%) ---
    Calm,     // +SpDefense, -Attack (Điềm tĩnh)
    Gentle,   // +SpDefense, -Defense (Dịu dàng)
    Sassy,    // +SpDefense, -Speed (Xấc xược)
    Careful,  // +SpDefense, -SpAttack (Cẩn thận)
    Quirky    // Trung tính (SpD / SpD - Kỳ quặc)
}

public enum StatType
{
    HP,
    Attack,
    Defense,
    Speed,
    SpAttack,
    SpDefense
}

public static class NatureUtils
{
    /// <summary>
    /// Lấy hệ số nhân của Tính cách áp dụng lên 1 chỉ số cụ thể:
    /// +10% -> trả về 1.1f
    /// -10% -> trả về 0.9f
    /// Trung tính -> trả về 1.0f
    /// (HP không bao giờ bị ảnh hưởng bởi Tính cách)
    /// </summary>
    public static float GetMultiplier(NatureType nature, StatType stat)
    {
        if (stat == StatType.HP) return 1.0f;

        StatType boosted = GetBoostedStat(nature);
        StatType reduced = GetReducedStat(nature);

        if (boosted == reduced) return 1.0f; // Trung tính

        if (stat == boosted) return 1.1f;
        if (stat == reduced) return 0.9f;

        return 1.0f;
    }

    public static StatType GetBoostedStat(NatureType nature)
    {
        switch (nature)
        {
            case NatureType.Hardy:
            case NatureType.Lonely:
            case NatureType.Brave:
            case NatureType.Adamant:
            case NatureType.Naughty:
                return StatType.Attack;

            case NatureType.Bold:
            case NatureType.Docile:
            case NatureType.Relaxed:
            case NatureType.Impish:
            case NatureType.Lax:
                return StatType.Defense;

            case NatureType.Timid:
            case NatureType.Hasty:
            case NatureType.Jolly:
            case NatureType.Naive:
            case NatureType.Serious:
                return StatType.Speed;

            case NatureType.Modest:
            case NatureType.Mild:
            case NatureType.Quiet:
            case NatureType.Bashful:
            case NatureType.Rash:
                return StatType.SpAttack;

            case NatureType.Calm:
            case NatureType.Gentle:
            case NatureType.Sassy:
            case NatureType.Careful:
            case NatureType.Quirky:
                return StatType.SpDefense;

            default:
                return StatType.Attack;
        }
    }

    public static StatType GetReducedStat(NatureType nature)
    {
        switch (nature)
        {
            case NatureType.Hardy:
            case NatureType.Bold:
            case NatureType.Timid:
            case NatureType.Modest:
            case NatureType.Calm:
                return StatType.Attack;

            case NatureType.Lonely:
            case NatureType.Docile:
            case NatureType.Hasty:
            case NatureType.Mild:
            case NatureType.Gentle:
                return StatType.Defense;

            case NatureType.Brave:
            case NatureType.Relaxed:
            case NatureType.Serious:
            case NatureType.Quiet:
            case NatureType.Sassy:
                return StatType.Speed;

            case NatureType.Adamant:
            case NatureType.Impish:
            case NatureType.Jolly:
            case NatureType.Bashful:
            case NatureType.Careful:
                return StatType.SpAttack;

            case NatureType.Naughty:
            case NatureType.Lax:
            case NatureType.Naive:
            case NatureType.Rash:
            case NatureType.Quirky:
                return StatType.SpDefense;

            default:
                return StatType.Attack;
        }
    }

    /// <summary>Trả về tên tiếng Việt hiển thị trên UI (như bản dịch FireRed Việt)</summary>
    public static string GetVietnameseName(NatureType nature)
    {
        switch (nature)
        {
            case NatureType.Lonely:  return "Cô đơn";
            case NatureType.Docile:  return "Ngoan ngoãn";
            case NatureType.Adamant: return "Kiên định";
            case NatureType.Jolly:   return "Vui vẻ";
            case NatureType.Modest:  return "Khiêm tốn";
            case NatureType.Timid:   return "Nhút nhát";
            case NatureType.Brave:   return "Dũng cảm";
            case NatureType.Calm:    return "Điềm tĩnh";
            case NatureType.Careful: return "Cẩn thận";
            case NatureType.Gentle:  return "Dịu dàng";
            case NatureType.Hardy:   return "Kiên cường";
            case NatureType.Hasty:   return "Hấp tấp";
            case NatureType.Impish:  return "Tinh quái";
            case NatureType.Lax:     return "Lỏng lẻo";
            case NatureType.Mild:    return "Ôn hòa";
            case NatureType.Naive:   return "Ngây thơ";
            case NatureType.Naughty: return "Nghịch ngợm";
            case NatureType.Quiet:   return "Trầm lặng";
            case NatureType.Quirky:  return "Kỳ quặc";
            case NatureType.Rash:    return "Bồng bột";
            case NatureType.Relaxed: return "Thư thả";
            case NatureType.Sassy:   return "Xấc xược";
            case NatureType.Serious: return "Nghiêm túc";
            case NatureType.Bold:    return "Táo bạo";
            case NatureType.Bashful: return "Rụt rè";
            default:                 return nature.ToString();
        }
    }

    /// <summary>Random ngẫu nhiên 1 trong 25 tính cách khi bắt thú mới</summary>
    public static NatureType GetRandomNature()
    {
        Array values = Enum.GetValues(typeof(NatureType));
        return (NatureType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
}
