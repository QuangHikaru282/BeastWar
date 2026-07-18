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

    public static void GoToBattle()
    {
        // Đóng băng trạng thái: Chỉ lưu GameCore và Scene đang hoạt động (Active Scene)
        // Tránh lưu nhầm các Scene đang được tải ngầm (như ForestScene) vì khi quay về nó sẽ bị hiện chồng lên nhau.
        string scenes = "GameCore";
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != "GameCore" && s.name != "DontDestroyOnLoad" && !s.name.Contains("Battle"))
            {
                scenes += "," + s.name;
            }
        }

        // Lưu lại chính xác số lượng kho đồ & thanh công cụ trước khi vào trận đấu
        Kinnly.PlayerInventory currentInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
        if (currentInv != null)
        {
            currentInv.SaveNow();
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
