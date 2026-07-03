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

        // Hỗ trợ Multi-Scene Editing (chuỗi sceneName có dạng "Scene1,Scene2")
        string[] scenesToLoad = sceneName.Split(',');
        string primaryScene = scenesToLoad[0];

        // Lưu lại chính xác số lượng kho đồ hiện tại trước khi chuyển cảnh
        Kinnly.PlayerInventory currentInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
        if (currentInv != null)
        {
            currentInv.SaveNow();
        }

        // Load scene chính (Single) để dọn dẹp các scene cũ
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(primaryScene, LoadSceneMode.Single);
        
        // --- SỬA LỖI Ở ĐÂY: Nếu scene không tồn tại trong Build Settings, asyncLoad sẽ bị null ---
        if (asyncLoad == null)
        {
            Debug.LogError($"[LỖI NGHIÊM TRỌNG] Không thể tải Scene '{primaryScene}'! Bạn đã QUÊN kéo Scene này vào File -> Build Settings -> Scenes In Build rồi. Hãy làm ngay nhé!");
            
            // Tắt màn hình đen đi để game không bị treo
            if (transitionCanvasGroup != null) 
            {
                transitionCanvasGroup.DOFade(0f, 0.2f);
                transitionCanvasGroup.blocksRaycasts = false;
            }
            yield break;
        }

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

        // Kích hoạt scene chính
        asyncLoad.allowSceneActivation = true;

        // Đợi cho đến khi scene chính load hoàn toàn
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Load các scene phụ (Additive) nếu có
        for (int i = 1; i < scenesToLoad.Length; i++)
        {
            string sceneNameSub = scenesToLoad[i];
            
            // Kiểm tra xem scene đã được load chưa (do AutoLoadAdditiveScene có thể đã load nó ngay trong Start của HubTownNew)
            bool isLoaded = false;
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                if (SceneManager.GetSceneAt(j).name == sceneNameSub)
                {
                    isLoaded = true;
                    break;
                }
            }

            if (!isLoaded)
            {
                AsyncOperation subLoad = SceneManager.LoadSceneAsync(sceneNameSub, LoadSceneMode.Additive);
                while (!subLoad.isDone)
                {
                    yield return null;
                }
            }
        }

        // Đặt scene chính làm Active Scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(primaryScene));

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
