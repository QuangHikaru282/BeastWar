using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Quản lý giao tiếp UI trong Battle (ActionPanel, FruitBuff, Clicks).
/// </summary>
public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    [SerializeField] private ActionPanel actionPanel;
    [SerializeField] private FruitBuffManager fruitBuffManager;
    [SerializeField] private RewardUIManager rewardUIManager;

    public ActionPanel ActionPanel => actionPanel;
    public FruitBuffManager FruitBuffManager => fruitBuffManager;
    public RewardUIManager RewardUIManager => rewardUIManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        // Tự động tìm kiếm các component UI trong Scene nếu chưa gán ở Inspector
        if (actionPanel == null)
        {
            actionPanel = Object.FindFirstObjectByType<ActionPanel>(FindObjectsInactive.Include);
            if (actionPanel == null)
            {
                // Tự động tạo ActionPanel gắn tạm vào BattleManager/BattleUIManager để nó chạy code auto-link UI
                actionPanel = gameObject.AddComponent<ActionPanel>();
                Debug.Log("[BattleUIManager] Tự động tạo và gắn ActionPanel component tại runtime.");
            }
        }
        if (fruitBuffManager == null)
        {
            fruitBuffManager = Object.FindFirstObjectByType<FruitBuffManager>(FindObjectsInactive.Include);
        }
        if (rewardUIManager == null)
        {
            rewardUIManager = Object.FindFirstObjectByType<RewardUIManager>(FindObjectsInactive.Include);
        }
    }

    public void SetupUIHooks()
    {
        // Khởi tạo các event UI chung nếu cần
    }

    public void ShowFruitBuffManager()
    {
        if (fruitBuffManager != null)
        {
            fruitBuffManager.Show();
        }
    }

    public void RefreshFruitBuffBench(List<BeastUnit> playerTeam)
    {
        if (fruitBuffManager == null) return;
        var bench = playerTeam.Where(b => b != null && !b.gameObject.activeSelf).ToList();
        fruitBuffManager.SetBenchUnits(bench);
    }
    
    public void ShowBattleReward(int gold, int exp, System.Action onComplete)
    {
        RewardUIManager rewardUI = rewardUIManager;
        if (rewardUI == null)
        {
            rewardUI = Object.FindFirstObjectByType<RewardUIManager>(FindObjectsInactive.Include);
        }

        if (rewardUI != null)
        {
            rewardUI.gameObject.SetActive(true);
            rewardUI.ShowBattleReward(gold, exp, onComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }
}
