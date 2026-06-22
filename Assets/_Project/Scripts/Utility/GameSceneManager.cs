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

    public static void GoToMap()        => SceneManager.LoadScene(SCENE_MAP);
    public static void GoToFormation()  => SceneManager.LoadScene(SCENE_FORMATION);
    public static void GoToBattle()     => SceneManager.LoadScene(SCENE_BATTLE);
    public static void GoToHunting()    => SceneManager.LoadScene(SCENE_HUNTING);
    public static void GoToWorldMap()   => SceneManager.LoadScene(SCENE_WORLDMAP);
}
