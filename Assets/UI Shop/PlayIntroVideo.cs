using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayIntroVideo : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject introVideoPanel;
    [SerializeField] private Button playButton;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    private bool isStartingGame;

    private void Awake()
    {
        if (introVideoPanel != null)
        {
            introVideoPanel.SetActive(false);
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
        }
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoError;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }

    public void PlayGameIntro()
    {
        if (isStartingGame)
        {
            return;
        }

        if (videoPlayer == null)
        {
            Debug.LogError("Chưa gán VideoPlayer.");
            return;
        }

        isStartingGame = true;

        if (playButton != null)
        {
            playButton.interactable = false;
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (introVideoPanel != null)
        {
            introVideoPanel.SetActive(true);
        }

        videoPlayer.Stop();
        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        player.Play();
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        LoadGameScene();
    }

    private void OnVideoError(VideoPlayer player, string errorMessage)
    {
        Debug.LogError("Lỗi phát video: " + errorMessage);

        // Nếu video bị lỗi vẫn chuyển vào game.
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("Tên scene game đang để trống.");
            return;
        }

        SceneManager.LoadSceneAsync(gameSceneName);
    }
}