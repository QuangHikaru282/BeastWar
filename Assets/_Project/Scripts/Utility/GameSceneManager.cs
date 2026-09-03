using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Tiện ích load scene toàn cục.</summary>
public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    // Tên các scene — phải khớp với tên scene trong Build Settings
    public const string SCENE_MAP        = "MapScene";
    public const string SCENE_FORMATION  = "FormationScene";
    public const string SCENE_BATTLE     = "BattleSceneF";
    public const string SCENE_HUNTING    = "HuntingScene";
    public const string SCENE_WORLDMAP   = "WorldMapScene";
    public const string SCENE_CITYMAP    = "City"; // Theo yêu cầu của user, scene của bạn ấy tên là City
    public const string SCENE_HUBTOWN    = "HubTownNew";

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

    public static void GoToMap()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_MAP);
        else
            SceneManager.LoadScene(SCENE_MAP);
    }

    public static void GoToHubTown()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_HUBTOWN);
        else
            SceneManager.LoadScene(SCENE_HUBTOWN);
    }

    public static void GoToFormation()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_FORMATION);
        else
            SceneManager.LoadScene(SCENE_FORMATION);
    }

    /// <summary>
    /// Coordinator trung tâm: Tự lưu vị trí Player, lưu scene hiện tại, rồi chuyển sang Battle.
    /// Mọi trigger (Trainer, Rival, WildBeast...) chỉ cần setup BattleTransferData rồi gọi hàm này.
    /// </summary>
    /// <param name="data">BattleTransferData đã được trigger setup sẵn. Coordinator sẽ tự ghi lastPlayerPosition vào.</param>
    public static void GoToBattle(BattleTransferData data = null)
    {
        // ── COORDINATOR: Tự lưu vị trí Player ──────────────────────────────
        // Không để từng trigger tự lo → tránh bug khi playerObj = null
        if (data != null)
        {
            GameObject player = GameObject.Find("PF Player");
            if (player != null)
            {
                data.lastPlayerPosition = player.transform.position;
                data.returnToLastPosition = true;
                Debug.Log($"[GameSceneManager] ✅ Đã lưu vị trí Player trước trận: {data.lastPlayerPosition}");
            }
            else
            {
                Debug.LogWarning("[GameSceneManager] ⚠️ Không tìm thấy 'PF Player' để lưu vị trí. Kiểm tra tên GameObject trong GameCore.");
            }
        }

        // ── Lưu danh sách scene hiện tại để quay về sau trận ────────────────
        // Chỉ lưu GameCore và Active Scene, tránh lưu scene đang tải ngầm
        string scenes = "GameCore";
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != "GameCore" && s.name != "DontDestroyOnLoad" && !s.name.Contains("Battle"))
            {
                scenes += "," + s.name;
            }
        }

        // ── Lưu inventory trước khi vào trận ────────────────────────────────
        Kinnly.PlayerInventory currentInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
        if (currentInv != null)
        {
            currentInv.SaveNow();

            if (QuestManager.Instance != null && QuestManager.Instance.playerData != null)
            {
                QuestManager.Instance.playerData.SaveInventoryState(currentInv);
                Debug.Log("[GameSceneManager] Đã sync inventory sang PlayerData trước khi vào trận.");
            }
        }

        PlayerPrefs.SetString("SceneBeforeBattle", scenes);
        PlayerPrefs.Save();

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_BATTLE);
        else
            SceneManager.LoadScene(SCENE_BATTLE);
    }

    public static void GoToHunting()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_HUNTING);
        else
            SceneManager.LoadScene(SCENE_HUNTING);
    }

    public static void GoToWorldMap()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_WORLDMAP);
        else
            SceneManager.LoadScene(SCENE_WORLDMAP);
    }

    public static void GoToCityMap()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_CITYMAP);
        else
            SceneManager.LoadScene(SCENE_CITYMAP);
    }
}
