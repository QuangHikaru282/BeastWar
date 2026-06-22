using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Quản lý hiệu ứng chuyển cảnh mượt mà sử dụng DOTween và Canvas Group.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [Tooltip("Thời gian tối thiểu màn hình Loading sẽ hiển thị (giúp người chơi kịp đọc chữ và ngắm ảnh nền)")]
    [SerializeField] private float minShowTime = 3.0f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Đảm bảo Canvas ẩn đi lúc khởi động game
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = 0f;
                transitionCanvasGroup.blocksRaycasts = false;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Chuyển scene bất tuần tự kèm theo hiệu ứng Fade màn hình và chữ Loading tương ứng.
    /// </summary>
    /// <param name="sceneName">Tên scene cần chuyển tới</param>
    /// <param name="message">Thông điệp hiển thị (ví dụ: "Đang vào làng...")</param>
    public void TransitionToScene(string sceneName, string message)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName, message));
    }

    private IEnumerator TransitionRoutine(string sceneName, string message)
    {
        isTransitioning = true;
        float startTime = Time.time;

        // Cập nhật text loading và hiệu ứng animator nếu có
        if (loadingText != null)
        {
            var animator = loadingText.GetComponent<LoadingTextAnimator>();
            if (animator != null)
            {
                animator.SetBaseText(message);
            }
            else
            {
                loadingText.text = message;
            }
        }

        // Bật chặn raycast để người chơi không click được gì trong lúc chuyển cảnh
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.blocksRaycasts = true;
            // Fade out (làm tối dần màn hình game, hiện màn hình loading)
            yield return transitionCanvasGroup.DOFade(1f, fadeDuration).WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        // Load scene bất tuần tự (Async)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Đợi scene load xong 90% (vì Unity giữ lại 10% cuối để kích hoạt)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Tính toán xem đã hiển thị màn hình loading đủ thời gian tối thiểu chưa
        float elapsed = Time.time - startTime;
        float remainingTime = minShowTime - elapsed;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // Kích hoạt scene mới
        asyncLoad.allowSceneActivation = true;

        // Đợi cho đến khi scene mới được load hoàn toàn và kích hoạt thành công
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Fade in (làm sáng dần màn hình game, ẩn màn hình loading)
        if (transitionCanvasGroup != null)
        {
            yield return transitionCanvasGroup.DOFade(0f, fadeDuration).WaitForCompletion();
            transitionCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        isTransitioning = false;
    }
}
