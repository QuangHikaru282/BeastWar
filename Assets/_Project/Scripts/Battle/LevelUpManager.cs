using UnityEngine;
using System.Collections;

/// <summary>
/// Quản lý việc cộng Kinh Nghiệm (EXP) và Tăng Cấp (Level Up) cho thú.
/// </summary>
public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Để tồn tại xuyên suốt các Scene
        }
    }

    /// <summary>
    /// Cộng EXP cho một thú cụ thể và xử lý Level Up nếu đủ.
    /// </summary>
    public void AddExpToBeast(RuntimeBeastData beast, int expAmount)
    {
        if (beast == null) return;

        beast.currentExp += expAmount;
        Debug.Log($"[LevelUpManager] {beast.baseBeast.beastName} nhận được {expAmount} EXP. Hiện tại: {beast.currentExp}/{beast.GetExpToNextLevel()}");

        // Vòng lặp phòng trường hợp nhận được quá nhiều EXP, nhảy nhiều Level cùng lúc
        while (beast.currentExp >= beast.GetExpToNextLevel())
        {
            beast.currentExp -= beast.GetExpToNextLevel();
            LevelUp(beast);
        }
    }

    /// <summary>
    /// Logic Tăng Cấp: Tăng các chỉ số cơ bản của thú.
    /// </summary>
    private void LevelUp(RuntimeBeastData beast)
    {
        beast.currentLevel++;

        Debug.Log($"<color=yellow>[LevelUpManager] CHÚC MỪNG! {beast.baseBeast.beastName} đã thăng cấp lên Level {beast.currentLevel}!</color>");
        
        // TODO: Mở khóa chiêu thức hoặc Tiến hóa có thể kiểm tra ở đây
    }

    /// <summary>
    /// Chia đều lượng EXP cho toàn bộ đội hình hiện tại của Player.
    /// Dùng sau khi thắng trận hoặc bắt được thú.
    /// </summary>
    public void DistributeExpToFormation(int totalExp, PlayerData playerData)
    {
        if (playerData == null || playerData.currentFormation == null) return;

        int beastCount = 0;
        foreach (var beast in playerData.currentFormation)
        {
            if (beast != null) beastCount++;
        }

        if (beastCount == 0) return;

        int expPerBeast = totalExp / beastCount; // Chia đều EXP

        foreach (var beast in playerData.currentFormation)
        {
            if (beast != null)
            {
                AddExpToBeast(beast, expPerBeast);
            }
        }
    }
}
