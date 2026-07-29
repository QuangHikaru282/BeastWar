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

    public static event Action OnQuestAdvanced;

    [Header("Data References")]
    [Tooltip("Kéo PlayerData vào đây")]
    public PlayerData playerData;

    [Header("Item Rewards (Kéo thả Item từ Assets vào đây)")]
    public Kinnly.Item hoeItem;
    public Kinnly.Item tomatoSeedItem;
    public Kinnly.Item waterCanItem;
    public Kinnly.Item captureBallItem;

    [Header("Danh sách Item đăng ký (Kéo các Item khác vào đây nếu muốn giữ khi load scene)")]
    public List<Kinnly.Item> allGameItems = new List<Kinnly.Item>();

    public Kinnly.Item GetItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;

        string cleanName = itemName.Replace("(Clone)", "").Trim();

        if (hoeItem != null && (hoeItem.name == itemName || hoeItem.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))) return hoeItem;
        if (tomatoSeedItem != null && (tomatoSeedItem.name == itemName || tomatoSeedItem.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))) return tomatoSeedItem;
        if (waterCanItem != null && (waterCanItem.name == itemName || waterCanItem.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))) return waterCanItem;
        if (captureBallItem != null && (captureBallItem.name == itemName || captureBallItem.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))) return captureBallItem;

        if (allGameItems != null)
        {
            foreach (var item in allGameItems)
            {
                if (item != null)
                {
                    string assetName = ((UnityEngine.Object)item).name;
                    if (item.name == itemName || item.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase) ||
                        assetName == itemName || assetName.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))
                        return item;
                }
            }
        }

        // Tự động load TẤT CẢ các Item từ toàn bộ folder Resources
        Kinnly.Item[] resItems = Resources.LoadAll<Kinnly.Item>("");
        if (resItems != null)
        {
            foreach (var item in resItems)
            {
                if (item != null)
                {
                    string assetName = ((UnityEngine.Object)item).name;
                    if (item.name == itemName || item.name.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase) ||
                        assetName == itemName || assetName.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (allGameItems != null && !allGameItems.Contains(item))
                        {
                            allGameItems.Add(item);
                        }
                        return item;
                    }
                }
            }
        }

        return null;
    }

    // Các biến lưu trữ tiến trình tạm thời cho các nhiệm vụ cày cuốc
    [Header("Tiến trình nội bộ")]
    public int beastsDefeatedInForest = 0; // Cho nhiệm vụ đánh 3 con
    public int trainersDefeated = 0;       // Cho nhiệm vụ đánh 3 trainer
    public int goldEarnedFromFish = 0;     // Cho nhiệm vụ bán cá
    public int goldEarnedFromWood = 0;     // Cho nhiệm vụ bán gỗ
    
    // Mảng mô tả cho 20 nhiệm vụ (theo bản thiết kế mở rộng)
    private readonly string[] questDescriptions = new string[]
    {
        /* 0  */ "Gặp Trưởng Làng để nhận bạn đồng hành khởi đầu.",
        /* 1  */ "Tham gia trận đấu và đánh bại 1 con Pet hoang dã.",
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
        /* 17 */ "Hoàn thành giai đoạn Demo! Hãy tiếp tục rèn luyện.",
        /* 18 */ "Bắt đầu sự nghiệp Ngư phủ: Tìm đến vùng nước và câu 1 con cá.",
        /* 19 */ "Mang cá câu được bán cho Shop để kiếm 100 Vàng.",
        /* 20 */ "Đi qua Vùng hoang dã để khám phá vùng đất mới.",
        /* 21 */ "Trở thành Tiều phu: Dùng Rìu chặt đổ một cây gỗ.",
        /* 22 */ "Mang Gỗ thu thập được bán cho Cửa Hàng để kiếm 150 Vàng.",
        /* 23 */ "Thú Cưng Tăng Cấp: Tăng cấp cho bạn đồng hành.",
        /* 24 */ "Khám Phá Hang Động: Đi tới Hang Động.",
        /* 25 */ "Đến gặp Trưởng Làng để nhận hướng dẫn và chuẩn bị thu phục Thú."
    };

    // Mảng tiêu đề cho các nhiệm vụ
    private readonly string[] questTitles = new string[]
    {
        /* 0  */ "Khởi Đầu Hành Trình",
        /* 1  */ "Chiến Đấu Và Chiến Thắng",
        /* 2  */ "Gieo Hạt Đầu Tiên",
        /* 3  */ "Tưới Nước Cho Cây",
        /* 4  */ "Thu Hoạch Nông Sản",
        /* 5  */ "Giao Thương Khởi Nghiệp",
        /* 6  */ "Khám Phá Rừng Xanh",
        /* 7  */ "Xây Dựng Đội Hình",
        /* 8  */ "Thú Hệ Nước",
        /* 9  */ "Chế Tạo Mồi Nhử",
        /* 10 */ "Thú Hiếm Xuất Hiện",
        /* 11 */ "Kẻ Thù Thách Đấu",
        /* 12 */ "Tiến Vào Rừng Sâu",
        /* 13 */ "Chiến Đấu Trainer",
        /* 14 */ "Thách Thức Thú Vương",
        /* 15 */ "Huy Hiệu Đầu Tiên",
        /* 16 */ "Mở Rộng Nông Trại",
        /* 17 */ "Hoàn Thành Bản Demo",
        /* 18 */ "Sự Nghiệp Ngư Phủ",
        /* 19 */ "Bán Cá Kiếm Tiền",
        /* 20 */ "Khám Phá Vùng Đất Mới",
        /* 21 */ "Trở Thành Tiều Phu",
        /* 22 */ "Bán Gỗ Kiếm Tiền",
        /* 23 */ "Thú Cưng Tăng Cấp",
        /* 24 */ "Khám Phá Hang Động",
        /* 25 */ "Gặp Trưởng Làng"
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

    private void Update()
    {
        // Cheat code: Click chuột phải (chuột 1) để hoàn thành nhanh quest hiện tại
        if (Input.GetMouseButtonDown(1))
        {
            if (playerData != null && playerData.currentMainQuestId < questDescriptions.Length)
            {
                Debug.Log($"[Cheat] Đã click chuột phải! Hoàn thành nhanh nhiệm vụ {playerData.currentMainQuestId}.");
                AdvanceQuest();
            }
        }
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

        // Chỉ trao/duy trì Cuốc & Hạt Giống từ Quest 2 trở đi (sau khi đã nhận thưởng Quest 1)
        if (id >= 2)
        {
            if (hoeItem != null && !HasItemInInventory(playerInv, hoeItem))
                playerInv.AddItem(hoeItem, 1);

            if (id == 2 && tomatoSeedItem != null && !HasItemInInventory(playerInv, tomatoSeedItem))
                playerInv.AddItem(tomatoSeedItem, 5);
        }

        // Chỉ trao/duy trì Bình Tưới Nước từ Quest 3 trở đi (sau khi đã nhận thưởng Quest 2)
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

        // Đã hoàn thành và nhận thưởng Quest 1 -> Duy trì Dụng cụ Cuốc vĩnh cửu & Hạt giống cho Quest 2
        if (id >= 2)
        {
            if (hoeItem != null && !HasItemInInventory(playerInv, hoeItem))
            {
                playerInv.AddItem(hoeItem, 1);
            }

            if (id == 2 && tomatoSeedItem != null && !HasItemInInventory(playerInv, tomatoSeedItem))
            {
                playerInv.AddItem(tomatoSeedItem, 5);
            }
        }

        // Đã hoàn thành và nhận thưởng Quest 2 -> Duy trì Dụng cụ Bình Tưới Nước vĩnh cửu từ Quest 3 trở đi
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

    public int GetTotalQuestCount()
    {
        return questDescriptions != null ? questDescriptions.Length : 26;
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
            if (tomatoSeedItem == null && n.Contains("tomato")) tomatoSeedItem = item;
            if (waterCanItem == null && (n.Contains("water") || n.Contains("can"))) waterCanItem = item;
        }
    }

    /// <summary>Trả về tiêu đề của Quest hiện tại.</summary>
    public string GetCurrentQuestTitle()
    {
        if (playerData == null) return "Nhiệm vụ chính";

        int id = playerData.currentMainQuestId;
        
        if (id >= questTitles.Length || id < 0)
        {
            return "Nhiệm vụ chính";
        }

        return questTitles[id];
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
        if (id == 19) desc += $" ({goldEarnedFromFish}/100)";
        if (id == 22) desc += $" ({goldEarnedFromWood}/150)";

        return desc;
    }

    [Header("Trạng thái hoàn thành nhiệm vụ")]
    public bool isCurrentQuestCompleted = false;

    public void MarkCurrentQuestCompleted()
    {
        isCurrentQuestCompleted = true;
        Debug.Log($"<color=cyan>[QuestManager]</color> Nhiệm vụ {playerData.currentMainQuestId} đã thỏa mãn mục tiêu! Nút Nhận Thưởng đã sẵn sàng.");
        OnQuestAdvanced?.Invoke();
    }

    public bool IsCurrentQuestReadyToClaim()
    {
        if (playerData == null) return false;
        int id = playerData.currentMainQuestId;

        // Kiểm tra tự động theo dữ liệu thực tế của game
        if (id == 0 && playerData.ownedBeasts != null && playerData.ownedBeasts.Count > 0) return true;
        if (id == 1 && (isCurrentQuestCompleted || beastsDefeatedInForest >= 1)) return true;
        if (id == 6 && beastsDefeatedInForest >= 3) return true;
        if (id == 7 && playerData.ownedBeasts != null && playerData.ownedBeasts.Count >= 3) return true;
        if (id == 13 && trainersDefeated >= 3) return true;
        if (id == 19 && goldEarnedFromFish >= 100) return true;
        if (id == 22 && goldEarnedFromWood >= 150) return true;

        return isCurrentQuestCompleted;
    }

    public bool AdvanceQuest()
    {
        if (playerData == null) return false;

        // CHỈ PHÉP NHẬN THƯỞNG KHI NHIỆM VỤ ĐÃ THỰC SỰ HOÀN THÀNH
        if (!IsCurrentQuestReadyToClaim())
        {
            Debug.LogWarning($"<color=yellow>[QuestManager]</color> Chưa hoàn thành mục tiêu của Nhiệm vụ {playerData.currentMainQuestId}! Không thể nhận thưởng.");
            return false;
        }

        int completedQuestId = playerData.currentMainQuestId;

        // Trao phần thưởng và chuyển sang nhiệm vụ tiếp theo
        GiveRewardForQuest(completedQuestId);
        isCurrentQuestCompleted = false;
        return true;
    }

    private void GiveRewardForQuest(int questId)
    {
        int rewardGold = 0;
        int rewardExp = 0;
        Sprite rewardIcon = null;

        switch (questId)
        {
            case 0: // Nhiệm vụ 0: Gặp Trưởng Làng
                rewardGold = 50;
                rewardExp = 0;
                break;

            case 1: // Nhiệm vụ 1: Chiến đấu và chiến thắng
                rewardGold = 50;
                rewardExp = 0;
                if (hoeItem != null) rewardIcon = hoeItem.image;
                break;

            case 2: // Nhiệm vụ 2: Cày đất & gieo hạt
                rewardGold = 50;
                rewardExp = 0;
                if (waterCanItem != null) rewardIcon = waterCanItem.image;
                break;

            case 4: // Nhiệm vụ 4: Thu hoạch
                rewardGold = 100;
                rewardExp = 0;
                break;

            case 5: // Nhiệm vụ 5: Bán nông sản
                rewardGold = 150;
                rewardExp = 0;
                break;

            case 9: // Nhiệm vụ 9: Chế tạo mồi nhử tại xưởng
                rewardGold = 100;
                rewardExp = 0;
                break;
                
            case 18: // Nhiệm vụ 18: Câu cá
                rewardGold = 50;
                rewardExp = 0;
                break;
                
            case 19: // Nhiệm vụ 19: Bán cá
                rewardGold = 200;
                rewardExp = 0;
                break;
                
            case 20: // Nhiệm vụ 20: Đi qua Vùng hoang dã
                rewardGold = 300;
                rewardExp = 0;
                break;

            case 21: // Nhiệm vụ 21: Chặt gỗ
                rewardGold = 50;
                rewardExp = 0;
                break;

            case 22: // Nhiệm vụ 22: Bán gỗ
                rewardGold = 250;
                rewardExp = 0;
                break;

            case 23: // Nhiệm vụ 23: Lên cấp
                rewardGold = 100;
                rewardExp = 0;
                break;

            case 24: // Nhiệm vụ 24: Đi tới Hang Động
                rewardGold = 500;
                rewardExp = 0;
                break;

            default:
                rewardGold = 50;
                rewardExp = 0;
                break;
        }

        // 1. Cộng Vàng và EXP lập tức
        if (playerData != null)
        {
            playerData.gold += rewardGold;
            if (LevelUpManager.Instance != null)
            {
                LevelUpManager.Instance.DistributeExpToFormation(rewardExp, playerData);
            }
        }

        // 2. Trao/Duy trì vật phẩm thật vào kho/thanh công cụ lập tức
        Kinnly.PlayerInventory playerInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
        if (playerInv != null)
        {
            AutoFindItems();

            if (questId == 1)
            {
                if (hoeItem != null && !HasItemInInventory(playerInv, hoeItem))
                {
                    playerInv.AddItem(hoeItem, 1); // Trao Cuốc
                }
                if (tomatoSeedItem != null && !HasItemInInventory(playerInv, tomatoSeedItem))
                {
                    playerInv.AddItem(tomatoSeedItem, 5); // Trao 5 gói Hạt giống
                }
            }

            if (questId == 2 && waterCanItem != null && !HasItemInInventory(playerInv, waterCanItem))
            {
                playerInv.AddItem(waterCanItem, 1); // Trao Bình tưới nước
            }

            if (questId == 25 && captureBallItem != null && !HasItemInInventory(playerInv, captureBallItem))
            {
                playerInv.AddItem(captureBallItem, 5); // Trao 5 Bóng Thu Phục
            }

            EnsureUnlockedItemsInInventory(playerInv); // Khôi phục/Trao Dụng cụ vĩnh cửu (Cuốc & Bình nước)
            playerInv.SaveNow();
        }

        // 3. Tiến tới nhiệm vụ tiếp theo và lưu dữ liệu ngay lập tức
        if (playerData != null)
        {
            if (questId == 5)
            {
                playerData.currentMainQuestId = 18; // Sau Quest 5 (Bán Shop) -> Nhảy sang Quest 18 (Câu Cá)
            }
            else if (questId == 18)
            {
                playerData.currentMainQuestId = 25; // Sau Quest 18 (Câu Cá) -> Nhảy sang Quest 25 (Gặp Trưởng Làng)
            }
            else if (questId == 25)
            {
                playerData.currentMainQuestId = 8; // Sau Quest 25 (Gặp Trưởng Làng) -> Nhảy sang Quest 8 (Thu phục Thú hệ Nước)
            }
            else
            {
                playerData.currentMainQuestId++;
            }
            playerData.Save();
            Debug.Log($"<color=green>[QuestManager]</color> Đã nhận phần thưởng Nhiệm vụ {questId}! Thăng cấp lên Quest {playerData.currentMainQuestId}.");
        }

        // 4. Báo cho hệ thống UI / Marker / Chỉ đường cập nhật
        OnQuestAdvanced?.Invoke();

        // 5. Kiểm tra nhiệm vụ tiếp theo lập tức
        CheckQuestImmediate();
    }

    /// <summary>
    /// Kiểm tra tức thì các quest có thể đã thỏa điều kiện mà không cần action mới.
    /// </summary>
    private void CheckQuestImmediate()
    {
        if (playerData == null) return;
        int id = playerData.currentMainQuestId;

        // Bỏ qua nhiệm vụ 9 (chế tạo mồi) theo yêu cầu
        if (id == 9)
        {
            playerData.currentMainQuestId = 10;
            playerData.Save();
            CheckQuestImmediate();
            return;
        }

        // Bỏ qua nhiệm vụ 17 (Demo ending) để đi tiếp sang câu cá
        if (id == 17)
        {
            playerData.currentMainQuestId = 18;
            playerData.Save();
            CheckQuestImmediate();
            return;
        }

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
            MarkCurrentQuestCompleted();
        }
        
        if (playerData.currentMainQuestId == 7 && playerData.ownedBeasts.Count >= 3)
        {
            MarkCurrentQuestCompleted();
        }

        if (playerData.currentMainQuestId == 8 && beast != null && beast.baseBeast.element == BeastElement.Water)
        {
            MarkCurrentQuestCompleted();
        }

        if (playerData.currentMainQuestId == 10 && beast != null && beast.baseBeast.isRare)
        {
            MarkCurrentQuestCompleted();
        }
    }

    /// <summary>Kiểm tra khi người chơi đánh bại 1 thú hoang dã.</summary>
    public void OnWildBeastDefeated()
    {
        beastsDefeatedInForest++;

        if (playerData != null && playerData.currentMainQuestId == 1)
        {
            MarkCurrentQuestCompleted();
        }

        if (playerData != null && playerData.currentMainQuestId == 6)
        {
            if (beastsDefeatedInForest >= 3)
            {
                MarkCurrentQuestCompleted();
            }
        }
    }

    /// <summary>Kiểm tra khi người chơi gieo hạt đầu tiên.</summary>
    public void OnSeedPlanted()
    {
        if (playerData.currentMainQuestId == 2)
        {
            MarkCurrentQuestCompleted();
        }
    }

    /// <summary>Kiểm tra khi người chơi tưới nước.</summary>
    public void OnCropWatered()
    {
        if (playerData.currentMainQuestId == 3)
        {
            MarkCurrentQuestCompleted();
        }
    }

    /// <summary>Kiểm tra khi thu hoạch.</summary>
    public void OnCropHarvested()
    {
        if (playerData.currentMainQuestId == 4) MarkCurrentQuestCompleted();
    }

    /// <summary>Kiểm tra khi bán nông sản.</summary>
    public void OnItemsSold()
    {
        if (playerData.currentMainQuestId == 5) MarkCurrentQuestCompleted();
    }

    /// <summary>Kiểm tra khi chế tạo mồi nhử.</summary>
    public void OnBaitCrafted()
    {
        if (playerData.currentMainQuestId == 9) MarkCurrentQuestCompleted();
    }

    /// <summary>Kiểm tra khi gán thú hệ Nước cho nông trại.</summary>
    public void OnWaterBeastAssigned()
    {
        if (playerData.currentMainQuestId == 12) MarkCurrentQuestCompleted();
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
                MarkCurrentQuestCompleted();
            }
        }
    }

    /// <summary>Kiểm tra khi đánh bại Boss.</summary>
    public void OnBossDefeated()
    {
        if (playerData.currentMainQuestId == 14) 
        {
            MarkCurrentQuestCompleted();
        }
    }

    /// <summary>Kiểm tra khi câu được cá.</summary>
    public void OnFishCaught()
    {
        if (playerData.currentMainQuestId == 18)
        {
            MarkCurrentQuestCompleted();
        }
    }

    /// <summary>Kiểm tra khi bán cá.</summary>
    public void OnFishSold(int goldAmount)
    {
        if (playerData.currentMainQuestId == 19)
        {
            goldEarnedFromFish += goldAmount;
            if (goldEarnedFromFish >= 100)
            {
                MarkCurrentQuestCompleted();
            }
        }
    }

    /// <summary>Kiểm tra khi đi qua Vùng hoang dã.</summary>
    public void OnWildernessPassed()
    {
        if (playerData.currentMainQuestId == 20)
        {
            MarkCurrentQuestCompleted();
        }
    }

    /// <summary>Kiểm tra khi chặt cây.</summary>
    public void OnTreeChopped()
    {
        if (playerData.currentMainQuestId == 21)
        {
            MarkCurrentQuestCompleted();
        }
    }

    /// <summary>Kiểm tra khi bán gỗ.</summary>
    public void OnWoodSold(int goldAmount)
    {
        if (playerData.currentMainQuestId == 22)
        {
            goldEarnedFromWood += goldAmount;
            if (goldEarnedFromWood >= 150)
            {
                MarkCurrentQuestCompleted();
            }
        }
    }

    /// <summary>Kiểm tra khi có thú trong đội hình tăng cấp.</summary>
    public void OnBeastLevelUp()
    {
        if (playerData.currentMainQuestId == 23)
        {
            MarkCurrentQuestCompleted();
        }
    }

    /// <summary>Kiểm tra khi người chơi đi tới Hang Động.</summary>
    public void OnCaveReached()
    {
        if (playerData.currentMainQuestId == 24)
        {
            AdvanceQuest();
        }
    }

    /// <summary>Lấy thông tin phần thưởng cho Quest ID tương ứng.</summary>
    public QuestRewardInfo GetQuestRewardInfo(int questId)
    {
        int rewardGold = 0;
        int rewardExp = 0;
        string rewardItem = "";
        Sprite rewardIcon = null;

        switch (questId)
        {
            case 0:
                rewardGold = 50;
                rewardExp = 0;
                rewardItem = "";
                break;
            case 1:
                rewardGold = 50;
                rewardExp = 0;
                rewardItem = "Cuốc & Hạt Giống";
                if (hoeItem != null) rewardIcon = hoeItem.image;
                break;
            case 2:
                rewardGold = 50;
                rewardExp = 0;
                rewardItem = "Bình Tưới Nước";
                if (waterCanItem != null) rewardIcon = waterCanItem.image;
                break;
            case 4:
                rewardGold = 100;
                rewardExp = 0;
                rewardItem = "";
                break;
            case 5:
                rewardGold = 150;
                rewardExp = 0;
                rewardItem = "Cần Câu";
                break;
            case 9:
                rewardGold = 100;
                rewardExp = 0;
                rewardItem = "";
                break;
            case 18:
                rewardGold = 50;
                rewardExp = 0;
                rewardItem = "";
                break;
            case 19:
                rewardGold = 200;
                rewardExp = 0;
                rewardItem = "Vé Bốc Thăm (Hiếm)";
                break;
            case 20:
                rewardGold = 300;
                rewardExp = 0;
                rewardItem = "Vé Tàu Thủy";
                break;
            case 21:
                rewardGold = 50;
                rewardExp = 0;
                rewardItem = "";
                break;
            case 22:
                rewardGold = 250;
                rewardExp = 0;
                rewardItem = "Bản đồ Kho báu";
                break;
            case 23:
                rewardGold = 100;
                rewardExp = 0;
                rewardItem = "Bánh Mì Ngọt";
                break;
            case 24:
                rewardGold = 500;
                rewardExp = 0;
                rewardItem = "Đèn Pin Siêu Sáng";
                break;
            case 25:
                rewardGold = 100;
                rewardExp = 0;
                rewardItem = captureBallItem != null ? captureBallItem.name : "Bóng Thu Phục";
                if (captureBallItem != null) rewardIcon = captureBallItem.image;
                break;
            default:
                rewardGold = 50;
                rewardExp = 0;
                rewardItem = "";
                break;
        }

        if (rewardIcon == null && !string.IsNullOrEmpty(rewardItem))
        {
            var item = GetItemByName(rewardItem);
            if (item != null)
            {
                rewardIcon = item.image;
            }
        }

        return new QuestRewardInfo
        {
            gold = rewardGold,
            exp = rewardExp,
            itemName = rewardItem,
            itemIcon = rewardIcon
        };
    }
}

/// <summary>
/// Lớp cấu trúc chứa thông tin phần thưởng của nhiệm vụ.
/// </summary>
public class QuestRewardInfo
{
    public int gold;
    public int exp;
    public string itemName;
    public Sprite itemIcon;
}

