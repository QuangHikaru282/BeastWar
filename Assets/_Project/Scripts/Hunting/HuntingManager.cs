using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý việc spawn quái thú trong HuntingScene.
/// Đảm bảo số lượng xuất hiện trên map không vượt quá maxActiveBeasts.
/// Khi một thú bị bắt/đánh bại, nó sẽ tự động spawn thêm từ hàng chờ (waitQueue).
/// </summary>
public class HuntingManager : MonoBehaviour
{
    public static HuntingManager Instance { get; private set; }

    [Header("Dữ liệu cấu hình")]
    [SerializeField] private HuntingSessionData sessionData;
    [SerializeField] private BattleTransferData battleTransferData;

    [Header("Prefab & Điểm Spawn")]
    [SerializeField] private GameObject wildBeastPrefab;
    [SerializeField] private List<Transform> spawnPoints;

    private List<BeastData> sessionPool = new List<BeastData>();
    private Queue<BeastData> waitQueue = new Queue<BeastData>();
    
    // Danh sách id của các vị trí spawn đang bận
    private Dictionary<string, Transform> activeBeasts = new Dictionary<string, Transform>();
    // Lưu các điểm spawn chưa có quái
    private List<Transform> availableSpawnPoints = new List<Transform>();

    // ─── LƯU TRỮ TRẠNG THÁI PHIÊN ĐI SĂN (PERSISTENT STATE) ─────────────────
    [System.Serializable]
    public struct SavedBeastState
    {
        public string uniqueId;
        public int spawnPointIndex;
        public BeastData beastData;
    }
    private static List<BeastData> staticWaitQueue = null;
    private static List<SavedBeastState> staticActiveBeasts = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializeHuntingSession();
    }

    private void InitializeHuntingSession()
    {
        if (sessionData == null || sessionData.wildBeastPool.Count == 0)
        {
            Debug.LogWarning("[HuntingManager] sessionData trống hoặc không có quái vật trong pool!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("[HuntingManager] Chưa gán spawnPoints!");
            return;
        }

        // Mặc định chuẩn bị tất cả điểm spawn
        availableSpawnPoints.AddRange(spawnPoints);

        // Kiểm tra xem có phải là quay lại từ BattleScene của Hunting không
        bool isReturningFromHuntingBattle = battleTransferData != null 
            && battleTransferData.originScene == BattleTransferData.OriginScene.Hunting 
            && staticActiveBeasts != null;

        if (isReturningFromHuntingBattle)
        {
            Debug.Log("[HuntingManager] Quay lại từ trận đấu. Đang khôi phục lại trạng thái cũ của phiên đi săn...");
            
            // Khôi phục hàng chờ
            waitQueue = new Queue<BeastData>(staticWaitQueue);

            // Khôi phục các con quái đang active
            foreach (var savedBeast in staticActiveBeasts)
            {
                if (savedBeast.spawnPointIndex >= 0 && savedBeast.spawnPointIndex < spawnPoints.Count)
                {
                    Transform spawnPoint = spawnPoints[savedBeast.spawnPointIndex];
                    
                    // Loại khỏi danh sách điểm spawn trống
                    availableSpawnPoints.Remove(spawnPoint);

                    // Spawn lại con quái với ID và Data cũ
                    GameObject beastGo = Instantiate(wildBeastPrefab, spawnPoint.position, Quaternion.identity);
                    beastGo.name = savedBeast.uniqueId;

                    var encounter = beastGo.GetComponent<HuntingBeastEncounter>();
                    if (encounter != null)
                    {
                        encounter.uniqueId = savedBeast.uniqueId;
                        encounter.enemyTeam = new List<BeastData> { savedBeast.beastData };
                    }

                    var spriteRenderer = beastGo.GetComponent<SpriteRenderer>() ?? beastGo.GetComponentInChildren<SpriteRenderer>();
                    if (spriteRenderer != null && savedBeast.beastData.frontSprite != null)
                    {
                        spriteRenderer.sprite = savedBeast.beastData.frontSprite;
                    }

                    activeBeasts.Add(savedBeast.uniqueId, spawnPoint);
                }
            }
        }
        else
        {
            Debug.Log("[HuntingManager] Bắt đầu một phiên đi săn MỚI.");
            // Reset dữ liệu static
            staticWaitQueue = null;
            staticActiveBeasts = new List<SavedBeastState>();

            // 1. Tạo sessionPool mới
            sessionPool.Clear();
            for (int i = 0; i < sessionData.totalBeastsInSession; i++)
            {
                BeastData randomBeast = sessionData.wildBeastPool[Random.Range(0, sessionData.wildBeastPool.Count)];
                sessionPool.Add(randomBeast);
            }

            // 2. Đưa vào hàng chờ
            waitQueue.Clear();
            foreach (var beast in sessionPool)
            {
                waitQueue.Enqueue(beast);
            }

            // 3. Spawn đợt đầu tiên
            int initialSpawnCount = Mathf.Min(sessionData.maxActiveBeasts, availableSpawnPoints.Count, waitQueue.Count);
            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnNextBeast();
            }

            SaveSessionState();
        }
    }

    private void SpawnNextBeast()
    {
        if (waitQueue.Count == 0 || availableSpawnPoints.Count == 0)
            return;

        BeastData nextBeastData = waitQueue.Dequeue();
        
        // Chọn ngẫu nhiên 1 điểm spawn trống
        int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
        Transform spawnPoint = availableSpawnPoints[spawnIndex];
        availableSpawnPoints.RemoveAt(spawnIndex);

        // Lấy chỉ số index thực tế của điểm spawn trong list gốc
        int spawnPointIndex = spawnPoints.IndexOf(spawnPoint);

        // Tạo unique ID cho quái vật này
        string uniqueId = "HuntingBeast_" + System.Guid.NewGuid().ToString();

        // Spawn
        GameObject beastGo = Instantiate(wildBeastPrefab, spawnPoint.position, Quaternion.identity);
        beastGo.name = uniqueId;

        var encounter = beastGo.GetComponent<HuntingBeastEncounter>();
        if (encounter != null)
        {
            encounter.uniqueId = uniqueId;
            encounter.enemyTeam = new List<BeastData> { nextBeastData };
            
            if (battleTransferData != null)
            {
                battleTransferData.stunnedBeastIds.Remove(uniqueId);
                battleTransferData.caughtBeastIds.Remove(uniqueId);
            }
        }

        var spriteRenderer = beastGo.GetComponent<SpriteRenderer>() ?? beastGo.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && nextBeastData.frontSprite != null)
        {
            spriteRenderer.sprite = nextBeastData.frontSprite;
        }

        // Lưu vào danh sách active run-time
        activeBeasts.Add(uniqueId, spawnPoint);

        // Lưu vào danh sách active static để bảo toàn trạng thái
        if (staticActiveBeasts == null) staticActiveBeasts = new List<SavedBeastState>();
        staticActiveBeasts.Add(new SavedBeastState
        {
            uniqueId = uniqueId,
            spawnPointIndex = spawnPointIndex,
            beastData = nextBeastData
        });

        SaveSessionState();
        
        Debug.Log($"[HuntingManager] Đã spawn {nextBeastData.beastName}. Còn {waitQueue.Count} con trong hàng chờ.");
    }

    private void SaveSessionState()
    {
        staticWaitQueue = new List<BeastData>(waitQueue);
    }

    /// <summary>
    /// Được gọi bởi HuntingBeastEncounter khi thú bị thu phục, hoặc chuyển sang Battle xong.
    /// </summary>
    public void OnBeastRemoved(string uniqueId)
    {
        if (activeBeasts.ContainsKey(uniqueId))
        {
            // Trả lại điểm spawn
            Transform freedPoint = activeBeasts[uniqueId];
            availableSpawnPoints.Add(freedPoint);
            activeBeasts.Remove(uniqueId);

            // Xóa khỏi danh sách static lưu trữ
            if (staticActiveBeasts != null)
            {
                staticActiveBeasts.RemoveAll(x => x.uniqueId == uniqueId);
            }

            Debug.Log($"[HuntingManager] Quái vật {uniqueId} đã bị xóa khỏi session. Spawn điểm mới...");

            // Tự động bù con mới từ hàng chờ
            if (waitQueue.Count > 0)
            {
                SpawnNextBeast();
            }
            else
            {
                SaveSessionState();
                Debug.Log("[HuntingManager] Đã hết quái vật trong hàng chờ của phiên đi săn này!");
            }
        }
    }
}
