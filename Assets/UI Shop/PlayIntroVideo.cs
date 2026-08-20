using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayIntroVideo : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject introVideoPanel;

    [Header("Character Select Controller mới")]
    [Tooltip("Kéo GameObject chứa CharacterSelectController vào đây")]
    [SerializeField] private CharacterSelectController charSelectController;

    [Header("Dữ liệu Người Chơi")]
    [Tooltip("Kéo PlayerData asset vào đây để reset khi bắt đầu game mới")]
    [SerializeField] private PlayerData playerData;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoRawImage;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "MainGame";

    private bool isStartingGame;

    private void Start()
    {
        if (buttonContainer != null) buttonContainer.SetActive(true);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (introVideoPanel != null) introVideoPanel.SetActive(false);

        if (videoRawImage != null)
            videoRawImage.enabled = false;

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;

            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoError;
        }
    }

    // Gắn vào nút Play
    public void OpenCharacterSelect()
    {
        if (isStartingGame)
            return;

        // Nếu có CharacterSelectController mới, dùng nó
        if (charSelectController != null)
        {
            // Reset toàn bộ dữ liệu người chơi để đảm bảo ElderNPC hiện bảng chọn Pet
            if (playerData != null)
            {
                playerData.ResetData();
                Debug.Log("[PlayIntroVideo] Đã reset PlayerData cho game mới!");
            }

            SaveLoadSystem.DeleteSave();

            if (buttonContainer != null) buttonContainer.SetActive(false);
            charSelectController.StartCharacterSelect();
            return;
        }

        // Fallback: dùng CharSelectPanel cũ
        if (buttonContainer != null) buttonContainer.SetActive(false);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(true);
    }

    // Gắn trực tiếp vào nút Male
    public void SelectMale()
    {
        SelectCharacterAndStart("Male");
    }

    // Gắn trực tiếp vào nút Female
    public void SelectFemale()
    {
        SelectCharacterAndStart("Female");
    }

    private void SelectCharacterAndStart(string characterName)
    {
        if (isStartingGame)
            return;

        isStartingGame = true;

        PlayerPrefs.SetString("SelectedCharacter", characterName);
        PlayerPrefs.Save();

        Debug.Log("Đã chọn nhân vật: " + characterName);

        StartTrailer();
    }

    private void StartTrailer()
    {
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (buttonContainer != null) buttonContainer.SetActive(false);

        // Nếu introVideoPanel bị xóa hoặc không gán VideoPlayer, vào game trực tiếp luôn
        if (introVideoPanel == null || videoPlayer == null)
        {
            Debug.Log("Không có Video Trailer. Vào game trực tiếp...");
            LoadGameScene();
            return;
        }

        introVideoPanel.SetActive(true);

        if (videoRawImage != null)
            videoRawImage.enabled = false;

        videoPlayer.Stop();
        videoPlayer.Prepare();

        Debug.Log("Đang chuẩn bị trailer...");
    }


    private void OnVideoPrepared(VideoPlayer player)
    {
        if (videoRawImage != null)
            videoRawImage.enabled = true;

        player.Play();

        Debug.Log("Trailer bắt đầu.");
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        Debug.Log("Trailer kết thúc.");
        LoadGameScene();
    }

    private void OnVideoError(VideoPlayer player, string errorMessage)
    {
        Debug.LogError("Lỗi video: " + errorMessage);
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("Tên scene game đang để trống.");
            return;
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(gameSceneName);
        }
        else
        {
            // Tạo một GameObject tạm để chạy Coroutine và không bị huỷ khi chuyển cảnh
            GameObject runnerObj = new GameObject("TempSceneLoader");
            DontDestroyOnLoad(runnerObj);
            var runner = runnerObj.AddComponent<TempSceneLoaderCoroutine>();
            runner.StartLoading(gameSceneName);
        }
    }


    public void BackToMainMenu()
    {
        if (isStartingGame)
            return;

        characterSelectPanel.SetActive(false);
        buttonContainer.SetActive(true);
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }
}

public class TempSceneLoaderCoroutine : MonoBehaviour
{
    public void StartLoading(string sceneNames)
    {
        StartCoroutine(LoadScenesSequentially(sceneNames));
    }

    private System.Collections.IEnumerator LoadScenesSequentially(string sceneNames)
    {
        string[] scenesToLoad = sceneNames.Split(',');
        for (int i = 0; i < scenesToLoad.Length; i++)
        {
            string sName = scenesToLoad[i].Trim();
            if (string.IsNullOrEmpty(sName)) continue;

            if (i == 0)
            {
                var asyncLoad = SceneManager.LoadSceneAsync(sName, LoadSceneMode.Single);
                while (asyncLoad != null && !asyncLoad.isDone) yield return null;
            }
            else
            {
                var asyncLoad = SceneManager.LoadSceneAsync(sName, LoadSceneMode.Additive);
                while (asyncLoad != null && !asyncLoad.isDone) yield return null;
            }
        }

        // Đặt scene phụ (ví dụ Lau1) hoặc scene đầu làm Active Scene
        if (scenesToLoad.Length > 1)
        {
            string subScene = scenesToLoad[1].Trim();
            Scene s = SceneManager.GetSceneByName(subScene);
            if (s.IsValid() && s.isLoaded)
            {
                SceneManager.SetActiveScene(s);
            }
        }

        // Đợi 2 frame để toàn bộ các component và SpawnPoint trong các Scene được Start() hoàn tất
        yield return null;
        yield return null;

        // Tìm PlayerData
        PlayerData pData = QuestManager.Instance != null && QuestManager.Instance.playerData != null
            ? QuestManager.Instance.playerData
            : Resources.Load<PlayerData>("PlayerData");

        string targetId = pData != null && !string.IsNullOrEmpty(pData.targetSpawnPointId) 
            ? pData.targetSpawnPointId 
            : PlayerPrefs.GetString("TargetSpawnPointId", "1");

        // Tìm tất cả điểm MapSpawnPoint trong Scene
        MapSpawnPoint[] allSpawns = Object.FindObjectsByType<MapSpawnPoint>(FindObjectsSortMode.None);
        MapSpawnPoint chosenSpawn = null;

        if (allSpawns != null && allSpawns.Length > 0)
        {
            foreach (var sp in allSpawns)
            {
                if (sp != null && sp.spawnId == targetId)
                {
                    chosenSpawn = sp;
                    break;
                }
            }

            // Nếu không khớp ID, lấy spawn point đầu tiên tìm thấy
            if (chosenSpawn == null && allSpawns.Length > 0)
            {
                chosenSpawn = allSpawns[0];
            }
        }

        // Tìm Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerMapController pmc = Object.FindFirstObjectByType<PlayerMapController>();
            if (pmc != null) player = pmc.gameObject;
        }
        if (player == null) player = GameObject.Find("PF Player");

        if (player != null && chosenSpawn != null)
        {
            player.transform.position = chosenSpawn.transform.position;
            Debug.Log($"[TempSceneLoader] Đã đưa Player về điểm Spawn: {chosenSpawn.spawnId} ({chosenSpawn.transform.position})");
        }

        // Snap Camera về Player
        CameraMovement cam = Object.FindFirstObjectByType<CameraMovement>();
        if (cam != null)
        {
            cam.ResetBounds();
            if (player != null) cam.target = player.transform;
            cam.SnapToTarget();
        }

        if (pData != null) pData.targetSpawnPointId = "";
        PlayerPrefs.SetString("TargetSpawnPointId", "");
        PlayerPrefs.Save();

        Destroy(gameObject);
    }
}