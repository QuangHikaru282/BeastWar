using System.Collections;
using UnityEngine;

/// <summary>
/// Quản lý Nhạc nền (BGM) và Âm thanh (SFX) xuyên suốt toàn bộ Game.
/// Tự động duy trì khi chuyển Scene (DontDestroyOnLoad) và hỗ trợ chuyển bài hát mượt mà (Fade).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("1. Audio Source")]
    [Tooltip("Nguồn phát nhạc nền")]
    [SerializeField] private AudioSource musicSource;

    [Tooltip("Nguồn phát âm thanh hiệu ứng (SFX)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("2. Nhạc nền mặc định")]
    [Tooltip("File nhạc nền chính của thế giới mở / khám phá")]
    [SerializeField] private AudioClip defaultMapBGM;

    [Header("3. Cài đặt âm lượng")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private Coroutine _fadeMusicCoroutine;

    private void Awake()
    {
        // Singleton Pattern + DontDestroyOnLoad: Giúp AudioManager tồn tại vĩnh viễn qua mọi Scene
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Phát nhạc nền mặc định nếu có và chưa phát
        if (defaultMapBGM != null && musicSource != null && !musicSource.isPlaying)
        {
            PlayMusic(defaultMapBGM, loop: true, fadeDuration: 1.0f);
        }
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    #region Public Music API (Phát & Đổi Nhạc Nền)

    /// <summary>
    /// Phát một bài nhạc nền mới. Nếu bài đang phát trùng với bài yêu cầu thì không phát lại từ đầu.
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 1.0f)
    {
        if (clip == null || musicSource == null) return;

        // Nếu đang phát chính bài này rồi thì giữ nguyên
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (_fadeMusicCoroutine != null) StopCoroutine(_fadeMusicCoroutine);

        if (fadeDuration > 0f && musicSource.isPlaying)
        {
            _fadeMusicCoroutine = StartCoroutine(CoFadeChangeMusic(clip, loop, fadeDuration));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Dừng nhạc nền với hiệu ứng nhỏ dần (Fade out).
    /// </summary>
    public void StopMusic(float fadeDuration = 1.0f)
    {
        if (musicSource == null || !musicSource.isPlaying) return;

        if (_fadeMusicCoroutine != null) StopCoroutine(_fadeMusicCoroutine);

        if (fadeDuration > 0f)
        {
            _fadeMusicCoroutine = StartCoroutine(CoFadeOutMusic(fadeDuration));
        }
        else
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Phát hiệu ứng âm thanh (SFX) một lần.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    /// <summary>
    /// Điều chỉnh âm lượng nhạc nền.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    #region Fade Coroutines

    private IEnumerator CoFadeChangeMusic(AudioClip newClip, bool loop, float duration)
    {
        float startVol = musicSource.volume;
        float halfDuration = duration / 2f;

        // 1. Giảm âm lượng bài cũ
        float timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, timer / halfDuration);
            yield return null;
        }

        // 2. Gán bài mới và phát
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        // 3. Tăng âm lượng bài mới lên
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, timer / halfDuration);
            yield return null;
        }

        musicSource.volume = musicVolume;
        _fadeMusicCoroutine = null;
    }

    private IEnumerator CoFadeOutMusic(float duration)
    {
        float startVol = musicSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, timer / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume;
        _fadeMusicCoroutine = null;
    }

    #endregion
}
