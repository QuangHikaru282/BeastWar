using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý việc Load/Unload các Scene Thế Giới Mở ở dưới nền (Background).
/// Tự động sinh ra khi cần, không cần kéo thả vào Scene.
/// </summary>
public class SeamlessSceneManager : MonoBehaviour
{
    private static SeamlessSceneManager _instance;
    public static SeamlessSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Tự động khởi tạo nếu chưa có
                GameObject go = new GameObject("SeamlessSceneManager");
                _instance = go.AddComponent<SeamlessSceneManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // Danh sách các Chunk (Scene) hiện đang được Load
    private HashSet<string> currentlyLoadedChunks = new HashSet<string>();
    
    // Ngăn chặn gọi Load nhiều lần cùng lúc cho cùng một Scene
    private HashSet<string> scenesCurrentlyLoading = new HashSet<string>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Được gọi bởi SeamlessZone khi Player đi vào.
    /// Nó sẽ Load các hàng xóm mới và Unload các hàng xóm cũ (ở quá xa).
    /// </summary>
    public void UpdateZones(string currentZone, List<string> neighbors)
    {
        // 1. Lập danh sách tất cả các Scene cần phải CÓ MẶT trong bộ nhớ lúc này
        HashSet<string> desiredScenes = new HashSet<string>(neighbors);
        desiredScenes.Add(currentZone);

        // Đảm bảo ghi nhận các Scene đã có sẵn ngay từ đầu (Ví dụ Map đầu tiên khi bật game lên)
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string loadedSceneName = SceneManager.GetSceneAt(i).name;
            if (desiredScenes.Contains(loadedSceneName))
            {
                currentlyLoadedChunks.Add(loadedSceneName);
            }
        }

        // 2. Load các Scene hàng xóm chưa có
        foreach (string sceneToLoad in desiredScenes)
        {
            if (!currentlyLoadedChunks.Contains(sceneToLoad) && !scenesCurrentlyLoading.Contains(sceneToLoad))
            {
                StartCoroutine(LoadSceneAdditiveRoutine(sceneToLoad));
            }
        }

        // 3. Xóa các Scene ở quá xa (nằm trong bộ nhớ nhưng KHÔNG nằm trong desiredScenes)
        List<string> scenesToUnload = new List<string>();
        foreach (string loadedScene in currentlyLoadedChunks)
        {
            if (!desiredScenes.Contains(loadedScene))
            {
                scenesToUnload.Add(loadedScene);
            }
        }

        foreach (string sceneToUnload in scenesToUnload)
        {
            StartCoroutine(UnloadSceneRoutine(sceneToUnload));
        }
    }

    private IEnumerator LoadSceneAdditiveRoutine(string sceneName)
    {
        scenesCurrentlyLoading.Add(sceneName);
        
        Debug.Log($"[Seamless] Đang tải ngầm khu vực: {sceneName}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        if (asyncLoad != null)
        {
            yield return asyncLoad;
            currentlyLoadedChunks.Add(sceneName);
            Debug.Log($"[Seamless] Đã tải xong khu vực: {sceneName}");
        }
        else
        {
            Debug.LogError($"[Seamless] Không tìm thấy Scene {sceneName}! Hãy kiểm tra lại Build Settings.");
        }

        scenesCurrentlyLoading.Remove(sceneName);
    }

    private IEnumerator UnloadSceneRoutine(string sceneName)
    {
        currentlyLoadedChunks.Remove(sceneName);
        Debug.Log($"[Seamless] Đang xóa bộ nhớ khu vực ở xa: {sceneName}");
        
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
        if (asyncUnload != null)
        {
            yield return asyncUnload;
        }
    }
}
