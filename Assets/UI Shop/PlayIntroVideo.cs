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

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoRawImage;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "MainGame";

    private bool isStartingGame;

    private void Start()
    {
        buttonContainer.SetActive(true);
        characterSelectPanel.SetActive(false);
        introVideoPanel.SetActive(false);

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

        buttonContainer.SetActive(false);
        characterSelectPanel.SetActive(true);
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
        characterSelectPanel.SetActive(false);
        buttonContainer.SetActive(false);
        introVideoPanel.SetActive(true);

        if (videoRawImage != null)
            videoRawImage.enabled = false;

        if (videoPlayer == null)
        {
            Debug.LogError("Chưa gán VideoPlayer.");
            LoadGameScene();
            return;
        }

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
            if (i == 0)
            {
                var asyncLoad = SceneManager.LoadSceneAsync(sName, LoadSceneMode.Single);
                while (!asyncLoad.isDone) yield return null;
            }
            else
            {
                var asyncLoad = SceneManager.LoadSceneAsync(sName, LoadSceneMode.Additive);
                while (!asyncLoad.isDone) yield return null;
            }
        }
        Destroy(gameObject);
    }
}