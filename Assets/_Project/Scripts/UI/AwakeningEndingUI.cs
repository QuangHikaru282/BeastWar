using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý Màn hình Đoạn Kết Xúc Động: Tỉnh Dậy & Trở Về Thực Tại (The Awakening Ending).
/// Kích hoạt sau khi người chơi giành chức Vô Địch và kết thúc Sảnh Danh Vọng.
/// </summary>
public class AwakeningEndingUI : MonoBehaviour
{
    public static AwakeningEndingUI Instance { get; private set; }

    [Header("1. Khung Màn Hình Kết Thúc")]
    [Tooltip("Panel toàn màn hình chứa toàn bộ UI kết thúc")]
    [SerializeField] private GameObject endingPanel;

    [Tooltip("Màn đen chuyển cảnh (Fade CanvasGroup hoặc Image)")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("2. Text Hiển Thị Độc Thoại & Cốt Truyện")]
    [SerializeField] private TextMeshProUGUI monologueTextTMP;
    [SerializeField] private Text legacyMonologueText;

    [Tooltip("Dòng chữ Tri Ân cuối cùng")]
    [SerializeField] private TextMeshProUGUI tributeTextTMP;
    [SerializeField] private Text legacyTributeText;

    [Header("3. Hình Ảnh Minh Họa (Tùy chọn)")]
    [Tooltip("Ảnh phòng khám thú y ngoài đời")]
    [SerializeField] private Image clinicBackgroundImage;

    [Tooltip("Ảnh Bác sĩ thú y")]
    [SerializeField] private Image doctorImage;

    [Tooltip("Ảnh Chú mèo được băng bó an toàn")]
    [SerializeField] private Image rescuedCatImage;

    [Tooltip("Ảnh Cảnh ôm nhau khóc hạnh phúc")]
    [SerializeField] private Image emotionalEndingArt;

    [Header("4. Âm Thanh")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Tiếng bíp bíp của máy đo tim bệnh viện")]
    [SerializeField] private AudioClip heartMonitorBeepSFX;
    [Tooltip("Tiếng mèo kêu nhỏ (Meo...)")]
    [SerializeField] private AudioClip catMeowSFX;
    [Tooltip("Nhạc nền cảm xúc kết game")]
    [SerializeField] private AudioClip endingBGM;

    [Header("5. Nút Quay Về Menu")]
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Tốc độ gõ chữ")]
    [SerializeField] private float textSpeed = 0.04f;

    private bool isEndingPlaying = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (endingPanel != null) endingPanel.SetActive(false);
        if (returnToMenuButton != null)
        {
            returnToMenuButton.gameObject.SetActive(false);
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }
    }

    /// <summary>
    /// Kích hoạt đoạn kết xúc động của game.
    /// </summary>
    public void PlayEndingSequence()
    {
        if (isEndingPlaying) return;
        isEndingPlaying = true;
        StartCoroutine(EndingSequenceRoutine());
    }

    private IEnumerator EndingSequenceRoutine()
    {
        if (endingPanel != null) endingPanel.SetActive(true);

        // Khóa di chuyển người chơi nếu có
        var pmc = UnityEngine.Object.FindFirstObjectByType<PlayerMapController>();
        if (pmc != null) pmc.SetCanMove(false);

        // 1. Mờ dần vào màn hình đen
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
        }

        SetImageAlpha(clinicBackgroundImage, 0f);
        SetImageAlpha(doctorImage, 0f);
        SetImageAlpha(rescuedCatImage, 0f);
        SetImageAlpha(emotionalEndingArt, 0f);
        SetText("");
        SetTributeText("");

        yield return new WaitForSeconds(1.5f);

        // 2. Tiếng bíp bíp của máy đo nhịp tim
        if (audioSource != null && heartMonitorBeepSFX != null)
        {
            audioSource.PlayOneShot(heartMonitorBeepSFX);
        }

        yield return new WaitForSeconds(1.2f);

        if (audioSource != null && heartMonitorBeepSFX != null)
        {
            audioSource.PlayOneShot(heartMonitorBeepSFX);
        }

        yield return new WaitForSeconds(1.0f);

        // 3. Phát nhạc nền kết game cảm xúc
        if (audioSource != null && endingBGM != null)
        {
            audioSource.clip = endingBGM;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 4. Chuỗi lời thoại hồi tưởng ký ức thực tại
        yield return StartCoroutine(TypeTextRoutine("Tiếng reo hò của đấu trường vô địch dần mờ xa..."));
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(TypeTextRoutine("Trước mắt tôi không còn là ánh hào quang sân khấu, mà là ánh đèn trắng lạnh lẽo quen thuộc..."));
        yield return new WaitForSeconds(1.8f);

        // Hiện mờ nền phòng khám
        yield return StartCoroutine(FadeInImageRoutine(clinicBackgroundImage, 1.0f));

        yield return StartCoroutine(TypeTextRoutine("Ký ức kinh hoàng của đêm hôm đó... tôi đã nhớ lại tất cả rồi."));
        yield return new WaitForSeconds(2.0f);

        yield return StartCoroutine(TypeTextRoutine("Một tên cướp mang hung khí đột nhập vào nhà..."));
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(TypeTextRoutine("Khi tôi vừa nhấc máy định gọi cảnh sát, hắn đã phát hiện và muốn dùng tôi làm con tin."));
        yield return new WaitForSeconds(2.0f);

        yield return StartCoroutine(TypeTextRoutine("Trong khoảnh khắc sinh tử ấy... chính chú mèo nhỏ của tôi đã bất chấp nguy hiểm lao vào cào cấu hắn để bảo vệ tôi..."));
        yield return new WaitForSeconds(2.5f);

        yield return StartCoroutine(TypeTextRoutine("Em ấy đã bị nhát chém tàn nhẫn làm trọng thương. Tôi vì quá đau đớn và hoảng loạn nên đã ngất lịm đi..."));
        yield return new WaitForSeconds(2.5f);

        // 5. Bác sĩ bước ra thông báo
        yield return StartCoroutine(FadeInImageRoutine(doctorImage, 1.0f));

        yield return StartCoroutine(TypeTextRoutine("Bác Sĩ Thú Y: 'Cháu tỉnh rồi à? Chú có một tin rất mừng cho cháu đây...'"));
        yield return new WaitForSeconds(2.0f);

        yield return StartCoroutine(TypeTextRoutine("Bác Sĩ: 'Ca phẫu thuật đã thành công mỹ mãn! Chú mèo nhỏ của cháu kiên cường lắm, em ấy đã vượt qua cơn nguy kịch rồi!'"));
        yield return new WaitForSeconds(2.5f);

        // 6. Chú mèo được bế ra, tiếng Meo...
        yield return StartCoroutine(FadeInImageRoutine(rescuedCatImage, 1.0f));

        if (audioSource != null && catMeowSFX != null)
        {
            audioSource.PlayOneShot(catMeowSFX);
        }

        yield return StartCoroutine(TypeTextRoutine("Chú mèo nhỏ với những lớp băng trắng khẽ mở mắt nhìn tôi, kêu lên một tiếng 'Meo...' quen thuộc..."));
        yield return new WaitForSeconds(2.5f);

        // 7. Ôm nhau khóc
        yield return StartCoroutine(FadeInImageRoutine(emotionalEndingArt, 1.5f));

        yield return StartCoroutine(TypeTextRoutine("Tôi ôm chặt lấy người bạn nhỏ bé vào lòng. Những giọt nước mắt nghẹn ngào tuôn rơi... Chúng tôi đã cùng nhau vượt qua giông bão."));
        yield return new WaitForSeconds(3.0f);

        // 8. Dòng chữ tri ân kết game
        SetText("");
        SetTributeText("CẢM ƠN VÌ ĐÃ LUÔN LÀ NGƯỜI HÙNG THẦM LẶNG BẢO VỆ TỚ.");
        yield return new WaitForSeconds(2.0f);

        // 9. Hiện nút quay về Menu
        if (returnToMenuButton != null)
        {
            returnToMenuButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator TypeTextRoutine(string text)
    {
        SetText("");
        string current = "";
        foreach (char c in text)
        {
            current += c;
            SetText(current);
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private IEnumerator FadeInImageRoutine(Image img, float duration)
    {
        if (img == null) yield break;
        float elapsed = 0f;
        Color c = img.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            img.color = c;
            yield return null;
        }
        c.a = 1f;
        img.color = c;
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void SetText(string content)
    {
        if (monologueTextTMP != null) monologueTextTMP.text = content;
        if (legacyMonologueText != null) legacyMonologueText.text = content;
    }

    private void SetTributeText(string content)
    {
        if (tributeTextTMP != null) tributeTextTMP.text = content;
        if (legacyTributeText != null) legacyTributeText.text = content;
    }

    private void OnReturnToMenuClicked()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(mainMenuSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
