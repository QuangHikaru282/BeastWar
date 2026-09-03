#pragma warning disable 0414
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Quản lý các tương tác trên Main Menu:
/// Play, Continue, Setting, Quit và phát trailer trước khi vào game.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Cấu hình Chuyển Cảnh")]
    [Tooltip("Tên scene sẽ tải sau khi trailer kết thúc")]
    [SerializeField] private string playTargetScene = "MapScene";

    [Tooltip("Thông điệp hiển thị lúc load game")]
    [SerializeField]
    private string playLoadingMessage =
        "Đang bước vào thế giới BeastWar...";

    [Header("UI Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button quitButton;

    [Header("Đăng nhập & Chọn nhân vật")]
    [SerializeField]
    private LoginAndCharSelectManager loginAndCharSelectManager;

    [Header("Dữ liệu Người Chơi")]
    [SerializeField] private PlayerData playerData;

    [Header("Video Trailer")]
    [Tooltip("Panel hoặc Canvas chứa video trailer")]
    [SerializeField] private GameObject trailerPanel;

    [Tooltip("VideoPlayer dùng để phát trailer")]
    [SerializeField] private VideoPlayer trailerVideoPlayer;

    [Tooltip("RawImage hiển thị RenderTexture của trailer")]
    [SerializeField] private RawImage trailerRawImage;

    [Header("Nhạc Nền (BGM)")]
    [Tooltip("File nhạc nền phát tại màn hình MainMenu")]
    [SerializeField] private AudioClip menuBGM;

    private bool isStartingGame;

    private void Start()
    {
        // Phát nhạc nền MainMenu nếu có
        if (menuBGM != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(menuBGM, loop: true, fadeDuration: 1.0f);
        }

        // Gán sự kiện tự động cho các nút.
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayPressed);


        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);

        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettingPressed);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitPressed);

        // Ẩn trailer lúc mới vào Main Menu.
        if (trailerPanel != null)
            trailerPanel.SetActive(false);

        if (trailerRawImage != null)
            trailerRawImage.enabled = false;

        // Thiết lập VideoPlayer.
        if (trailerVideoPlayer != null)
        {
            trailerVideoPlayer.playOnAwake = false;
            trailerVideoPlayer.isLooping = false;

            trailerVideoPlayer.prepareCompleted += OnTrailerPrepared;
            trailerVideoPlayer.loopPointReached += OnTrailerFinished;
            trailerVideoPlayer.errorReceived += OnTrailerError;
        }

        UpdateButtonStates();
    }

    /// <summary>
    /// Khi nhấn Play, xóa toàn bộ dữ liệu cũ và khởi tạo game mới từ đầu.
    /// </summary>
    public void OnPlayPressed()
    {
        if (isStartingGame)
            return;

        Debug.Log("MainMenu: Khởi tạo game mới... Xóa toàn bộ dữ liệu cũ!");

        // Xóa toàn bộ file save chính và file save Nông trại
        SaveLoadSystem.DeleteSave();

        // Reset dữ liệu người chơi về ban đầu
        if (playerData != null)
        {
            playerData.ResetData();
        }

        // Xóa toàn bộ PlayerPrefs (bao gồm flag thoại NPC như "Mom_IntroDialogue_Done")
        // để đảm bảo New Game luôn bắt đầu hoàn toàn từ đầu.
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (loginAndCharSelectManager != null)
        {
            loginAndCharSelectManager.StartCharSelectFlow();
        }
        else
        {
            Debug.LogError(
                "MainMenuController: Chưa gán LoginAndCharSelectManager."
            );
        }
    }


    private Coroutine trailerTimeoutCoroutine;

    private void Update()
    {
        // Bấm phím bất kỳ hoặc click chuột để Bỏ Qua (Skip) Trailer bất cứ lúc nào
        if (trailerPanel != null && trailerPanel.activeSelf)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                Debug.Log("MainMenu: Người chơi bấm Bỏ Qua (Skip) Trailer!");
                SkipTrailerAndStart();
            }
        }
    }

    private void SkipTrailerAndStart()
    {
        if (trailerTimeoutCoroutine != null)
        {
            StopCoroutine(trailerTimeoutCoroutine);
            trailerTimeoutCoroutine = null;
        }

        if (trailerVideoPlayer != null)
        {
            trailerVideoPlayer.Stop();
        }

        if (trailerPanel != null)
        {
            trailerPanel.SetActive(false);
        }

        LoadPlayScene();
    }

    /// <summary>
    /// Được LoginAndCharSelectManager gọi sau khi chọn nhân vật.
    /// </summary>
    public void PlayTrailerAndEnterGame()
    {
        if (isStartingGame)
            return;

        isStartingGame = true;

        // Dừng nhạc Main Menu khi bắt đầu vào trailer/vào game
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fadeDuration: 0.5f);
        }

        if (playButton != null)
            playButton.interactable = false;

        if (trailerVideoPlayer == null || trailerVideoPlayer.clip == null || trailerPanel == null)
        {
            Debug.LogWarning("MainMenuController: Video Player hoặc Panel chưa sẵn sàng -> Vào game ngay.");
            LoadPlayScene();
            return;
        }

        // Hiện Panel video.
        trailerPanel.SetActive(true);

        if (trailerRawImage != null)
            trailerRawImage.enabled = false;


        trailerVideoPlayer.Stop();
        trailerVideoPlayer.time = 0;
        trailerVideoPlayer.Prepare();

        // Tự động chuyển Scene sau 10 giây nếu Video bị kẹt
        if (trailerTimeoutCoroutine != null) StopCoroutine(trailerTimeoutCoroutine);
        trailerTimeoutCoroutine = StartCoroutine(TrailerTimeoutTimer());

        Debug.Log("MainMenu: Đang chuẩn bị trailer...");
    }

    private System.Collections.IEnumerator TrailerTimeoutTimer()
    {
        float duration = 8f;
        if (trailerVideoPlayer != null && trailerVideoPlayer.clip != null && trailerVideoPlayer.clip.length > 0)
        {
            duration = (float)trailerVideoPlayer.clip.length + 1f;
        }

        yield return new WaitForSeconds(duration);

        if (trailerPanel != null && trailerPanel.activeSelf)
        {
            Debug.Log("MainMenu: Trailer hết thời lượng / tự động chuyển vào Game!");
            SkipTrailerAndStart();
        }
    }

    /// <summary>
    /// Video đã chuẩn bị xong thì bắt đầu phát.
    /// </summary>
    private void OnTrailerPrepared(VideoPlayer player)
    {
        Debug.Log("MainMenu: Trailer đã chuẩn bị xong.");

        if (trailerRawImage != null)
            trailerRawImage.enabled = true;

        player.Play();
    }

    /// <summary>
    /// Video kết thúc thì chuyển sang scene game.
    /// </summary>
    private void OnTrailerFinished(VideoPlayer player)
    {
        Debug.Log("MainMenu: Trailer đã kết thúc.");

        LoadPlayScene();
    }

    /// <summary>
    /// Nếu video gặp lỗi, vẫn chuyển scene.
    /// </summary>
    private void OnTrailerError(
        VideoPlayer player,
        string errorMessage
    )
    {
        Debug.LogError(
            "MainMenu: Trailer bị lỗi: " + errorMessage
        );

        LoadPlayScene();
    }

    /// <summary>
    /// Chuyển vào scene game sau trailer.
    /// </summary>
    private void LoadPlayScene()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(
                playTargetScene
            );
        }
        else
        {
            GameObject runnerObj = new GameObject("TempSceneLoader");
            DontDestroyOnLoad(runnerObj);
            var runner = runnerObj.AddComponent<TempSceneLoaderCoroutine>();
            runner.StartLoading(playTargetScene);
        }
    }

    /// <summary>
    /// Xử lý khi nhấn Continue.
    /// </summary>
    public void OnContinuePressed()
    {
        Debug.Log("MainMenu: Đang tải lại tiến trình cũ...");

        // Dừng nhạc Main Menu khi tiếp tục game
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fadeDuration: 0.5f);
        }

        if (playerData != null)
        {
            playerData.Load();
        }


        string savedScene =
            PlayerPrefs.GetString(
                "SavedScene",
                playTargetScene
            );

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(
                savedScene
            );
        }
        else
        {
            GameObject runnerObj = new GameObject("TempSceneLoader");
            DontDestroyOnLoad(runnerObj);
            var runner = runnerObj.AddComponent<TempSceneLoaderCoroutine>();
            runner.StartLoading(savedScene);
        }
    }

    /// <summary>
    /// Xử lý khi nhấn Setting.
    /// </summary>
    public void OnSettingPressed()
    {
        Debug.Log(
            "MainMenu: Mở bảng cài đặt (Âm thanh, Đồ họa...)"
        );

        // TODO: settingPanel.SetActive(true);
    }

    /// <summary>
    /// Xử lý khi nhấn Quit.
    /// </summary>
    public void OnQuitPressed()
    {
        Debug.Log("MainMenu: Đang thoát trò chơi...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Khóa hoặc mở nút Continue dựa trên dữ liệu lưu.
    /// </summary>
    private void UpdateButtonStates()
    {
        if (continueButton != null)
        {
            bool hasSaveData =
                PlayerPrefs.HasKey("PlayerDataSave");

            continueButton.interactable = hasSaveData;
        }
    }

    private void OnDestroy()
    {
        if (trailerVideoPlayer != null)
        {
            trailerVideoPlayer.prepareCompleted -=
                OnTrailerPrepared;

            trailerVideoPlayer.loopPointReached -=
                OnTrailerFinished;

            trailerVideoPlayer.errorReceived -=
                OnTrailerError;
        }
    }
}