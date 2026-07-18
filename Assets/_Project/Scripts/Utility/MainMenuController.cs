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

    private bool isStartingGame;

    private void Start()
    {
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
    /// Khi nhấn Play, chỉ mở bảng chọn nhân vật.
    /// </summary>
    public void OnPlayPressed()
    {
        if (isStartingGame)
            return;

        Debug.Log("MainMenu: Khởi tạo game mới...");

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

    /// <summary>
    /// Được LoginAndCharSelectManager gọi sau khi chọn nhân vật.
    /// </summary>
    public void PlayTrailerAndEnterGame()
    {
        if (isStartingGame)
            return;

        isStartingGame = true;

        if (playButton != null)
            playButton.interactable = false;

        // Nếu chưa gán VideoPlayer thì vẫn chuyển scene,
        // tránh làm người chơi bị kẹt.
        if (trailerVideoPlayer == null)
        {
            Debug.LogError(
                "MainMenuController: Chưa gán Trailer Video Player."
            );

            LoadPlayScene();
            return;
        }

        if (trailerVideoPlayer.clip == null)
        {
            Debug.LogError(
                "MainMenuController: VideoPlayer chưa có Video Clip."
            );

            LoadPlayScene();
            return;
        }

        if (trailerPanel == null)
        {
            Debug.LogError(
                "MainMenuController: Chưa gán Trailer Panel."
            );

            LoadPlayScene();
            return;
        }

        // Hiện Panel video.
        trailerPanel.SetActive(true);

        // Tạm ẩn RawImage cho đến khi video chuẩn bị xong.
        if (trailerRawImage != null)
            trailerRawImage.enabled = false;

        trailerVideoPlayer.Stop();
        trailerVideoPlayer.time = 0;
        trailerVideoPlayer.Prepare();

        Debug.Log("MainMenu: Đang chuẩn bị trailer...");
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

        if (playerData != null)
        {
            playerData.Load();
        }

        string savedScene =
            PlayerPrefs.GetString(
                "SavedScene",
                playTargetScene
            );

        string continueMessage =
            "Đang tiếp tục hành trình...";

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