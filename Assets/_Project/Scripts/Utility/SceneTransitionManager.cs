using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Quản lý hiệu ứng chuyển cảnh mượt mà sử dụng DOTween và Canvas Group.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private LoadingPokeballUI loadingPokeballUI;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [Tooltip("Thời gian tối thiểu màn hình Loading sẽ hiển thị")]
    [SerializeField] private float minShowTime = 0.5f;

    private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);

            // Đảm bảo Canvas cũng không bị destroy khi scene unload
            if (transitionCanvasGroup != null)
            {
                // Tách canvas khỏi parent (nếu có) để DontDestroyOnLoad hoạt động
                if (transitionCanvasGroup.transform.parent != null)
                {
                    transitionCanvasGroup.transform.SetParent(null);
                }
                DontDestroyOnLoad(transitionCanvasGroup.gameObject);

                // Ẩn đi lúc khởi động
                transitionCanvasGroup.alpha = 0f;
                transitionCanvasGroup.blocksRaycasts = false;
                Canvas cv = transitionCanvasGroup.GetComponent<Canvas>();
                if (cv != null) cv.enabled = false;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// Chuyển scene bất tuần tự kèm theo hiệu ứng Fade màn hình.
    /// </summary>
    /// <param name="sceneName">Tên scene cần chuyển tới</param>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning)
        {
            // Aǹ toàn: nếu bị kẹt quá 5 giây, tự reset
            Debug.LogWarning("[SceneTransitionManager] isTransitioning=true, đang tự reset để tránh kẹt!");
            isTransitioning = false;
        }
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        float startTime = Time.time;

        // Bật chặn raycast và force active CanvasGroup để chắc chắn nó hiển thị
        if (transitionCanvasGroup != null)
        {
            Canvas cv = transitionCanvasGroup.GetComponent<Canvas>();
            if (cv != null) cv.enabled = true;

            transitionCanvasGroup.gameObject.SetActive(true);
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
        for (int i = 0; i < scenesToLoad.Length; i++)
        {
            scenesToLoad[i] = scenesToLoad[i].Trim(); // Cực kỳ quan trọng: Xóa khoảng trắng thừa
        }
        string primaryScene = scenesToLoad[0];

        // Lưu lại chính xác số lượng kho đồ hiện tại trước khi chuyển cảnh, trừ khi đang thoát khỏi Battle
        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Battle"))
        {
            Kinnly.PlayerInventory currentInv = FindFirstObjectByType<Kinnly.PlayerInventory>();
            if (currentInv != null)
            {
                currentInv.SaveNow();
            }
        }

        // Kiểm tra xem primaryScene đã được load chưa
        bool isPrimaryLoaded = false;
        for (int j = 0; j < SceneManager.sceneCount; j++)
        {
            if (SceneManager.GetSceneAt(j).name == primaryScene)
            {
                isPrimaryLoaded = true;
                break;
            }
        }

        if (isPrimaryLoaded)
        {
            // Dọn dẹp bằng cách Unload các map cũ không nằm trong danh sách load mới
            for (int j = SceneManager.sceneCount - 1; j >= 0; j--)
            {
                Scene s = SceneManager.GetSceneAt(j);
                if (s.name != primaryScene && System.Array.IndexOf(scenesToLoad, s.name) < 0)
                {
                    yield return SceneManager.UnloadSceneAsync(s);
                }
            }

            // Tải các scene phụ (như HoangDa2, Nha) nếu chưa có
            for (int i = 1; i < scenesToLoad.Length; i++)
            {
                string subName = scenesToLoad[i];
                bool isSubLoaded = false;
                for (int j = 0; j < SceneManager.sceneCount; j++)
                {
                    if (SceneManager.GetSceneAt(j).name == subName)
                    {
                        isSubLoaded = true;
                        break;
                    }
                }

                if (!isSubLoaded)
                {
                    AsyncOperation subLoad = SceneManager.LoadSceneAsync(subName, LoadSceneMode.Additive);
                    if (subLoad != null)
                    {
                        while (!subLoad.isDone) yield return null;
                    }
                }
            }

            float elapsed = Time.time - startTime;
            float remainingTime = minShowTime - elapsed;
            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);
            }
        }
        else
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(primaryScene, LoadSceneMode.Single);
            
            if (asyncLoad == null)
            {
                Debug.LogError($"[LỖI] Không thể tải Scene '{primaryScene}'! Bạn đã QUÊN kéo Scene vào Build Settings.");
                if (transitionCanvasGroup != null) 
                {
                    transitionCanvasGroup.DOFade(0f, 0.2f);
                    transitionCanvasGroup.blocksRaycasts = false;
                }
                isTransitioning = false;
                yield break;
            }

            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                if (loadingPokeballUI != null) loadingPokeballUI.SetProgress(progress);
                yield return null;
            }

            float elapsed = Time.time - startTime;
            float remainingTime = minShowTime - elapsed;
            if (remainingTime > 0)
            {
                float timer = 0f;
                while (timer < remainingTime)
                {
                    timer += Time.deltaTime;
                    float progress = Mathf.Lerp(0.9f, 1.0f, timer / remainingTime);
                    if (loadingPokeballUI != null) loadingPokeballUI.SetProgress(progress);
                    yield return null;
                }
            }

            if (loadingPokeballUI != null) loadingPokeballUI.SetProgress(1.0f);
            asyncLoad.allowSceneActivation = true;

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        // Load các scene phụ (Additive) nếu có
        for (int i = 1; i < scenesToLoad.Length; i++)
        {
            string sceneNameSub = scenesToLoad[i];
            
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
                
                if (subLoad == null)
                {
                    Debug.LogError($"[LỖI] Không thể tải Scene '{sceneNameSub}'. Hãy kiểm tra Build Settings hoặc tên gõ đúng chưa!");
                    continue; // Bỏ qua scene bị lỗi thay vì crash game
                }

                while (!subLoad.isDone)
                {
                    yield return null;
                }
            }
        }

        // Đặt scene chính làm Active Scene
        Scene primarySceneObj = SceneManager.GetSceneByName(primaryScene);
        if (primarySceneObj.IsValid())
        {
            SceneManager.SetActiveScene(primarySceneObj);
        }

        // KHÔI PHỤC VỊ TRÍ NGƯỜI CHƠI SAU TRẬN ĐÁNH (Thay thế cho AutoLoadAdditiveScene)
        BattleTransferData battleData = Resources.Load<BattleTransferData>("BattleTransferData");
        if (battleData != null && battleData.returnToLastPosition)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = battleData.lastPlayerPosition;
                battleData.returnToLastPosition = false;
                Debug.Log($"[SceneTransitionManager] Đã khôi phục vị trí người chơi về: {battleData.lastPlayerPosition}");
            }
        }
        else
        {
            // Kiểm tra điểm SpawnPoint nếu không phải hồi sinh sau trận đánh
            PlayerData pData = global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null 
                ? global::QuestManager.Instance.playerData 
                : Resources.Load<PlayerData>("PlayerData");

            if (pData != null && !string.IsNullOrEmpty(pData.targetSpawnPointId))
            {
                MapSpawnPoint[] allSpawns = Object.FindObjectsByType<MapSpawnPoint>(FindObjectsSortMode.None);
                foreach (var sp in allSpawns)
                {
                    if (sp != null && sp.spawnId == pData.targetSpawnPointId)
                    {
                        GameObject player = GameObject.FindGameObjectWithTag("Player");
                        if (player == null)
                        {
                            PlayerMapController pmc = Object.FindFirstObjectByType<PlayerMapController>();
                            if (pmc != null) player = pmc.gameObject;
                        }
                        if (player == null) player = GameObject.Find("PF Player");

                        if (player != null)
                        {
                            player.transform.position = sp.transform.position;
                            Debug.Log($"[SceneTransitionManager] Đã đưa Player đến SpawnPoint: {sp.spawnId} ({sp.transform.position})");
                        }
                        pData.targetSpawnPointId = "";
                        break;
                    }
                }
            }
        }

        // Đảm bảo mở khóa di chuyển cho người chơi sau khi tải xong Scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerMapController playerCtrl = playerObj.GetComponent<PlayerMapController>();
            if (playerCtrl != null)
            {
                playerCtrl.SetCanMove(true);
            }

            // Đồng bộ Camera ngay lập tức về vị trí Player (tránh camera bị kẹt ở tọa độ của scene cũ)
            CameraMovement camMovement = Object.FindFirstObjectByType<CameraMovement>();
            if (camMovement != null)
            {
                camMovement.ResetBounds(); // Xóa khung giới hạn của map cũ

                // Tìm xem map mới có CameraZoneConfiner không để tự động gán khung mới
                CameraZoneConfiner zone = Object.FindFirstObjectByType<CameraZoneConfiner>();
                if (zone != null)
                {
                    zone.ApplyZoneCameraBounds();
                }

                camMovement.target = playerObj.transform;
                camMovement.SnapToTarget();
            }
        }

        // RESET ngay sau khi scene đã load xong — để không bị kẹt dù fade lỗi
        isTransitioning = false;

        // Fade in (làm sáng dần màn hình game, ẩn màn hình loading)
        if (transitionCanvasGroup != null)
        {
            yield return transitionCanvasGroup.DOFade(0f, fadeDuration).WaitForCompletion();
            transitionCanvasGroup.blocksRaycasts = false;
            transitionCanvasGroup.alpha = 0f;

            Canvas cv = transitionCanvasGroup.GetComponent<Canvas>();
            if (cv != null) cv.enabled = false;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }
    }
}
