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
            SceneTransitionManager.Instance.TransitionToScene(SCENE_MAP, "Đang vào làng...");
        else
            SceneManager.LoadScene(SCENE_MAP);
    }

    public static void GoToHubTown()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_HUBTOWN, "Đang vào HubTown...");
        else
            SceneManager.LoadScene(SCENE_HUBTOWN);
    }

    public static void GoToFormation()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_FORMATION, "Đang chuẩn bị đội hình...");
        else
            SceneManager.LoadScene(SCENE_FORMATION);
    }

    public static void GoToBattle()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_BATTLE, "Đang chuẩn bị chiến đấu...");
        else
            SceneManager.LoadScene(SCENE_BATTLE);
    }

    public static void GoToHunting()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_HUNTING, "Đang chuẩn bị săn bắt...");
        else
            SceneManager.LoadScene(SCENE_HUNTING);
    }

    public static void GoToWorldMap()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_WORLDMAP, "Đang mở bản đồ thế giới...");
        else
            SceneManager.LoadScene(SCENE_WORLDMAP);
    }

    public static void GoToCityMap()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(SCENE_CITYMAP, "Đang di chuyển đến Thành Thị...");
        else
            SceneManager.LoadScene(SCENE_CITYMAP);
    }
}
