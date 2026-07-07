using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Singleton quản lý toàn bộ logic hệ thống Nhiệm Vụ (Quest System).
/// Cung cấp mô tả cho QuestUI và xử lý các điều kiện hoàn thành.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Data References")]
    [Tooltip("Kéo PlayerData vào đây")]
    public PlayerData playerData;

    [Header("Item Rewards (Kéo thả Item từ Assets vào đây)")]
    public Kinnly.Item hoeItem;
    public Kinnly.Item tomatoSeedItem;
    public Kinnly.Item waterCanItem;

    [Header("Danh sách Item đăng ký (Kéo các Item khác vào đây nếu muốn giữ khi load scene)")]
    public List<Kinnly.Item> allGameItems = new List<Kinnly.Item>();

    public Kinnly.Item GetItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;

        string cleanName = itemName.Replace("(Clone)", "").Trim();

        if (hoeItem != null && (hoeItem.name == itemName || hoeItem.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))) return hoeItem;
        if (tomatoSeedItem != null && (tomatoSeedItem.name == itemName || tomatoSeedItem.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))) return tomatoSeedItem;
        if (waterCanItem != null && (waterCanItem.name == itemName || waterCanItem.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))) return waterCanItem;

        if (allGameItems != null)
        {
            foreach (var item in allGameItems)
            {
                if (item != null && (item.name == itemName || item.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))) return item;
            }
        }

        // Tự động load TẤT CẢ các Item từ toàn bộ folder Resources (Bao gồm Nông sản Tomato_Fruit, Hạt giống, Mồi nhử, v.v...)
        Kinnly.Item[] resItems = Resources.LoadAll<Kinnly.Item>("");
        if (resItems != null)
        {
            foreach (var item in resItems)
            {
                if (item != null && (item.name == itemName || item.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase)))
                {
                    if (allGameItems != null && !allGameItems.Contains(item))
                    {
                        allGameItems.Add(item); // Giữ item trong RAM không bao giờ bị unload
                    }
                    return item;
                }
            }
        }

        return null;
    }

    // Các biến lưu trữ tiến trình tạm thời cho các nhiệm vụ cày cuốc
    [Header("Tiến trình nội bộ")]
    public int beastsDefeatedInForest = 0; // Cho nhiệm vụ đánh 3 con
    public int trainersDefeated = 0;       // Cho nhiệm vụ đánh 3 trainer
    
    // Mảng mô tả cho 17 nhiệm vụ (theo bản thiết kế)
    private readonly string[] questDescriptions = new string[]
    {
        /* 0  */ "Gặp Trưởng Làng để nhận bạn đồng hành khởi đầu.",
        /* 1  */ "Vào bãi cỏ, chiến đấu và thu phục 1 con Pet hoang dã.",
        /* 2  */ "Dùng Cuốc cày 3 ô đất và gieo Hạt giống đầu tiên.",
        /* 3  */ "Dùng Bình tưới nước cho cây vừa gieo.",
        /* 4  */ "Chăm sóc và thu hoạch nông sản đầu tiên.",
        /* 5  */ "Mang nông sản bán cho Cửa Hàng (Shop) để mở đường ra Rừng Xanh.",
        /* 6  */ "Khám phá Rừng Xanh: Đánh bại 3 Wild Beast.",
        /* 7  */ "Xây dựng đội hình: Sở hữu 3 loài Thú khác nhau.",
        /* 8  */ "Đến Hồ Thần Bí và bắt một con Thú hệ Nước.",
        /* 9  */ "Đến Xưởng thủ công: Chế tạo Mồi Nhử (Bait) từ nông sản.",
        /* 10 */ "Dùng Mồi Nhử bắt một con Thú Quý Hiếm (Rare Beast).",
        /* 11 */ "Chuẩn bị tinh thần: Kẻ thù truyền kiếp (Rival) thách đấu!",
        /* 12 */ "Tiến vào Rừng Sâu để tìm kiếm các Nhà Huấn Luyện.",
        /* 13 */ "Đánh bại 3 Nhà Huấn Luyện (Trainer) chặn đường trong Rừng Sâu.",
        /* 14 */ "Khiêu chiến Thú Vương đầu tiên cai quản Khu Rừng.",
        /* 15 */ "Lấy Huy Hiệu từ Thú Vương để mở cổng Hang Động Đá Đen.",
        /* 16 */ "Tích lũy 1000 Vàng và trả cho Trưởng làng để mở rộng Nông Trại.",
        /* 17 */ "Hoàn thành giai đoạn Demo! Hãy tiếp tục rèn luyện."
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Delay 1 frame để PlayerInventory.Start() chạy xong trước khi restore
        StartCoroutine(RestoreAfterFrame());
    }

    private System.Collections.IEnumerator RestoreAfterFrame()
    {
        yield return null; // Chờ 1 frame
        yield return null; // Chờ thêm 1 frame nữa cho chắc
        RestoreUnlockedItems();
        CheckQuestImmediate();
    }

    private void Start()
    {
        // Khôi phục các dụng cụ và vật phẩm đã mở khóa khi game vừa khởi động
        StartCoroutine(RestoreAfterFrame());
    }

    /// <summary>
    /// Đảm bảo Dụng cụ vĩnh cửu luôn có trong túi và lưu lại.
    /// PlayerInventory.Start() đã tự restore savedInventoryItems rồi.
    /// </summary>
    private void RestoreUnlockedItems()
    {
        Kinnly.PlayerInventory playerInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
        if (playerInv == null || playerData == null) return;

        // Đảm bảo Dụng cụ vĩnh cửu không bị mất
        playerData.isRestoringInventory = true;
        try { EnsureToolsOnly(playerInv); }
        finally { playerData.isRestoringInventory = false; }

        // Lưu lại sau khi ensure
        playerInv.SaveNow();
    }

    /// <summary>Đảm bảo Cuốc và Bình Nước luôn có trong túi - KHÔNG gọi Save.</summary>
    private void EnsureToolsOnly(Kinnly.PlayerInventory playerInv)
    {
        if (playerInv == null || playerData == null) return;
        AutoFindItems();

        int id = playerData.currentMainQuestId;

        if (id >= 1)
        {
            if (hoeItem != null && !HasItemInInventory(playerInv, hoeItem))
                playerInv.AddItem(hoeItem, 1);

            if (id <= 2 && tomatoSeedItem != null && !HasItemInInventory(playerInv, tomatoSeedItem))
                playerInv.AddItem(tomatoSeedItem, 5);
        }

        if (id >= 3)
        {
            if (waterCanItem != null && !HasItemInInventory(playerInv, waterCanItem))
                playerInv.AddItem(waterCanItem, 1);
        }
    }

    public void EnsureUnlockedItemsInInventory(Kinnly.PlayerInventory playerInv)
    {
        if (playerInv == null || playerData == null) return;
        AutoFindItems();

        int id = playerData.currentMainQuestId;

        // Đã xong Quest 0 (Gặp Trưởng làng) -> Duy trì Dụng cụ Cuốc vĩnh cửu & 5 Hạt giống cho Quest gieo hạt
        if (id >= 1)
        {
            if (hoeItem != null && !HasItemInInventory(playerInv, hoeItem))
            {
                playerInv.AddItem(hoeItem, 1);
            }

            // Nếu đang ở Quest 1 hoặc 2 (chưa hoàn thành gieo hạt) mà bị mất Hạt giống -> Cấp lại 5 gói Hạt giống ngay
            if (id <= 2 && tomatoSeedItem != null && !HasItemInInventory(playerInv, tomatoSeedItem))
            {
                playerInv.AddItem(tomatoSeedItem, 5);
            }
        }

        // Đã xong Quest 2 (Gieo hạt) -> Duy trì Dụng cụ Bình Tưới Nước vĩnh cửu
        if (id >= 3)
        {
            if (waterCanItem != null && !HasItemInInventory(playerInv, waterCanItem))
            {
                playerInv.AddItem(waterCanItem, 1);
            }
        }

        // Luôn cập nhật lại file lưu kho đồ ngay lập tức
        playerInv.SaveNow();
    }

    private void AutoFindItems()
    {
        if (hoeItem != null && tomatoSeedItem != null && waterCanItem != null) return;

        Kinnly.Item[] allItems = Resources.LoadAll<Kinnly.Item>("");
        foreach (var item in allItems)
        {
            if (item == null) continue;
            string n = item.name.ToLower();
            if (hoeItem == null && n.Contains("hoe")) hoeItem = item;
            if (tomatoSeedItem == null && (n.Contains("tomatoseed") || (n.Contains("tomato") && n.Contains("seed")))) tomatoSeedItem = item;
            if (waterCanItem == null && (n.Contains("watercan") || n.Contains("water") || n.Contains("can"))) waterCanItem = item;
        }
    }

    /// <summary>Trả về mô tả của Quest hiện tại.</summary>
    public string GetCurrentQuestDescription()
    {
        if (playerData == null) return "Lỗi: Không tìm thấy PlayerData.";

        int id = playerData.currentMainQuestId;
        
        // Nếu đã vượt quá số quest hiện có
        if (id >= questDescriptions.Length)
        {
            return "Bạn đã hoàn thành tất cả nhiệm vụ hiện tại!";
        }

        string desc = questDescriptions[id];

        // Gắn thêm tiến trình cho những nhiệm vụ cần đếm số lượng
        if (id == 6) desc += $" ({beastsDefeatedInForest}/3)";
        if (id == 7) desc += $" ({playerData.ownedBeasts.Count}/3)";
        if (id == 13) desc += $" ({trainersDefeated}/3)";

        return desc;
    }

    public void AdvanceQuest()
    {
        if (playerData == null) return;
        int completedQuestId = playerData.currentMainQuestId;
        playerData.currentMainQuestId++;
        playerData.Save();
        Debug.Log($"[QuestManager] Nhiệm vụ thăng cấp! Hiện tại là Quest {playerData.currentMainQuestId}.");

        GiveRewardForQuest(completedQuestId);

        // Kiểm tra ngay nếu quest mới cũng đã thỏa điều kiện sẵn
        CheckQuestImmediate();
    }

    private void GiveRewardForQuest(int questId)
    {
        int rewardGold = 0;
        int rewardExp = 0;
        string rewardItem = "";
        Sprite rewardIcon = null;

        switch (questId)
        {
            case 0: // Nhiệm vụ 0: Gặp Trưởng Làng
                rewardGold = 50;
                rewardExp = 100;
                rewardItem = "Cuốc & Hạt Giống";
                if (hoeItem != null) rewardIcon = hoeItem.image;
                break;

            case 2: // Nhiệm vụ 2: Cày đất & gieo hạt
                rewardGold = 50;
                rewardExp = 150;
                rewardItem = "Bình Tưới Nước";
                if (waterCanItem != null) rewardIcon = waterCanItem.image;
                break;

            case 4: // Nhiệm vụ 4: Thu hoạch
                rewardGold = 100;
                rewardExp = 200;
                rewardItem = "";
                break;

            case 5: // Nhiệm vụ 5: Bán nông sản
                rewardGold = 150;
                rewardExp = 250;
                rewardItem = "Thẻ Mở Cổng Rừng";
                break;

            case 9: // Nhiệm vụ 9: Chế tạo mồi nhử tại xưởng
                rewardGold = 100;
                rewardExp = 200;
                rewardItem = "";
                break;

            default:
                rewardGold = 50;
                rewardExp = 100;
                rewardItem = "";
                break;
        }

        // Tìm và hiển thị Bảng Nhận Thưởng (RewardUI_Panel)
        RewardUIManager rewardUI = RewardUIManager.Instance;
        if (rewardUI == null)
        {
            rewardUI = FindFirstObjectByType<RewardUIManager>(FindObjectsInactive.Include);
        }

        if (rewardUI != null)
        {
            rewardUI.gameObject.SetActive(true);
            
            // CHỈ TRAO QUÀ VÀ CỘNG ĐỒ KHI NGƯỜI CHƠI BẤM NÚT "TIẾP TỤC" TRÊN BẢNG
            rewardUI.ShowQuestReward(rewardGold, rewardExp, rewardItem, () => {
                // 1. Cộng Vàng và EXP
                if (playerData != null)
                {
                    playerData.gold += rewardGold;
                    if (LevelUpManager.Instance != null)
                    {
                        LevelUpManager.Instance.DistributeExpToFormation(rewardExp, playerData);
                    }
                }

                // 2. Trao/Duy trì vật phẩm thật vào kho/thanh công cụ
                Kinnly.PlayerInventory playerInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
                if (playerInv != null)
                {
                    if (questId == 0 && tomatoSeedItem != null)
                    {
                        playerInv.AddItem(tomatoSeedItem, 5); // Trao 5 gói Hạt giống (tiêu hao khi gieo)
                    }
                    EnsureUnlockedItemsInInventory(playerInv); // Khôi phục/Trao Dụng cụ vĩnh cửu (Cuốc & Bình nước)
                }

                Debug.Log($"<color=green>[QuestManager]</color> Người chơi đã bấm Tiếp Tục và nhận phần thưởng Nhiệm vụ {questId}!");
            }, rewardIcon);
        }
        else
        {
            // Fallback nếu không có UI thì cộng thẳng
            if (playerData != null) playerData.gold += rewardGold;
        }
    }

    /// <summary>
    /// Kiểm tra tức thì các quest có thể đã thỏa điều kiện mà không cần action mới.
    /// </summary>
    private void CheckQuestImmediate()
    {
        if (playerData == null) return;
        int id = playerData.currentMainQuestId;

        // Quest 7: Sở hữu 3 loài Thú khác nhau — có thể đã đủ từ trước
        if (id == 7 && playerData.ownedBeasts.Count >= 3)
        {
            AdvanceQuest();
            return;
        }

        // Quest 6: Đánh bại 3 Wild Beast — kiểm tra lại bộ đếm
        if (id == 6 && beastsDefeatedInForest >= 3)
        {
            AdvanceQuest();
            return;
        }

        // Quest 13: Đánh bại 3 Trainer — kiểm tra lại bộ đếm
        if (id == 13 && trainersDefeated >= 3)
        {
            AdvanceQuest();
            return;
        }
    }

    private bool HasItemInInventory(Kinnly.PlayerInventory playerInv, Kinnly.Item targetItem)
    {
        if (playerInv == null || targetItem == null) return false;

        List<GameObject> allSlots = new List<GameObject>();
        if (playerInv.InventorySlots != null) allSlots.AddRange(playerInv.InventorySlots);
        if (playerInv.ToolbarSlots != null) allSlots.AddRange(playerInv.ToolbarSlots);

        foreach (var slot in allSlots)
        {
            if (slot != null)
            {
                var invItem = slot.GetComponentInChildren<Kinnly.InventoryItem>(true);
                if (invItem != null && invItem.Item != null)
                {
                    if (invItem.Item == targetItem || invItem.Item.name.Equals(targetItem.name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>Gọi hàm này để nhảy tới một Quest chỉ định.</summary>
    public void SetQuest(int questId)
    {
        if (playerData == null) return;
        playerData.currentMainQuestId = questId;
        playerData.Save();
        Debug.Log($"[QuestManager] Đã chuyển thẳng đến Quest {questId}.");
    }

    // ─── CÁC EVENT KIỂM TRA ĐIỀU KIỆN ────────────────────────────────────

    /// <summary>Kiểm tra khi có thú vừa bị thu phục.</summary>
    public void OnBeastCaught(RuntimeBeastData beast)
    {
        if (playerData.currentMainQuestId == 1)
        {
            AdvanceQuest();
        }
        
        if (playerData.currentMainQuestId == 7 && playerData.ownedBeasts.Count >= 3)
        {
            AdvanceQuest();
        }

        if (playerData.currentMainQuestId == 8 && beast != null && beast.baseBeast.element == BeastElement.Water)
        {
            AdvanceQuest();
        }

        if (playerData.currentMainQuestId == 10 && beast != null && beast.baseBeast.isRare)
        {
            AdvanceQuest();
        }
    }

    /// <summary>Kiểm tra khi người chơi đánh bại 1 thú hoang dã.</summary>
    public void OnWildBeastDefeated()
    {
        if (playerData.currentMainQuestId == 6)
        {
            beastsDefeatedInForest++;
            if (beastsDefeatedInForest >= 3)
            {
                AdvanceQuest();
            }
        }
    }

    /// <summary>Kiểm tra khi người chơi gieo hạt đầu tiên.</summary>
    public void OnSeedPlanted()
    {
        if (playerData.currentMainQuestId == 2)
        {
            AdvanceQuest();
        }
    }

    /// <summary>Kiểm tra khi người chơi tưới nước.</summary>
    public void OnCropWatered()
    {
        if (playerData.currentMainQuestId == 3)
        {
            AdvanceQuest();
        }
    }

    /// <summary>Kiểm tra khi thu hoạch.</summary>
    public void OnCropHarvested()
    {
        if (playerData.currentMainQuestId == 4) AdvanceQuest();
    }

    /// <summary>Kiểm tra khi bán nông sản.</summary>
    public void OnItemsSold()
    {
        if (playerData.currentMainQuestId == 5) AdvanceQuest();
    }

    /// <summary>Kiểm tra khi chế tạo mồi nhử.</summary>
    public void OnBaitCrafted()
    {
        if (playerData.currentMainQuestId == 9) AdvanceQuest();
    }

    /// <summary>Kiểm tra khi gán thú hệ Nước cho nông trại.</summary>
    public void OnWaterBeastAssigned()
    {
        if (playerData.currentMainQuestId == 12) AdvanceQuest();
    }

    /// <summary>Kiểm tra khi đánh bại Rival (đối thủ).</summary>
    public void OnRivalDefeated()
    {
        if (playerData.currentMainQuestId == 11)
        {
            AdvanceQuest(); // Qua Quest 12 (Nhiệm vụ chuyển vùng)
            AdvanceQuest(); // Nhảy thẳng sang Quest 13 (Đánh bại 3 Trainer)
        }
    }

    /// <summary>Kiểm tra khi đánh bại 1 Trainer.</summary>
    public void OnTrainerDefeated()
    {
        if (playerData.currentMainQuestId == 13)
        {
            trainersDefeated++;
            if (trainersDefeated >= 3)
            {
                AdvanceQuest();
            }
        }
    }

    /// <summary>Kiểm tra khi đánh bại Boss.</summary>
    public void OnBossDefeated()
    {
        if (playerData.currentMainQuestId == 14) 
        {
            AdvanceQuest(); // Nhảy sang 15
            AdvanceQuest(); // Cho qua 15 luôn (nhận huy hiệu) để tới 16
        }
    }
}
