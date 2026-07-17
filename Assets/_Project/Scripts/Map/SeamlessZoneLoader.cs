using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Đặt ở RÌA của map (biên giới) để chuyển sang map hàng xóm.
/// Không dùng màn đen. Khi chạm vào, Teleport Player sang SpawnPoint bên map kia.
/// Yêu cầu: Map hàng xóm đã được SeamlessZone tải ngầm trước đó.
/// </summary>
public class SeamlessZoneLoader : MonoBehaviour
{
    [Header("Cấu hình Biên Giới")]
    [Tooltip("Tên Scene của Map hàng xóm mà Player sắp đi qua")]
    public string targetSceneName;

    [Tooltip("ID của SpawnPoint bên Map kia (Ví dụ: FromHubTown)")]
    public string targetSpawnId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[Border] Player chạm biên giới đi sang {targetSceneName}...");
            
            // 1. Kiểm tra xem Scene hàng xóm đã được load xong chưa
            Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
            if (!targetScene.isLoaded)
            {
                Debug.LogWarning($"[Border] Khoan đã! Map {targetSceneName} chưa load xong, hoặc chưa được tải ngầm!");
                return;
            }

            // 2. Tìm SpawnPoint bên kia biên giới (Phải tìm cả Object đang bị ẩn vì map mới đang bị tàng hình)
            MapSpawnPoint[] allSpawns = FindObjectsByType<MapSpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            MapSpawnPoint destSpawn = null;
            foreach (var sp in allSpawns)
            {
                // Chỉ lấy SpawnPoint nằm trong Scene mục tiêu để tránh nhầm lẫn
                if (sp.spawnId == targetSpawnId && sp.gameObject.scene.name == targetSceneName)
                {
                    destSpawn = sp;
                    break;
                }
            }

            if (destSpawn != null)
            {
                // Bật Scene mới lên ngay lập tức
                foreach (GameObject go in targetScene.GetRootGameObjects())
                {
                    go.SetActive(true);
                }

                // Tắt Scene cũ đi ngay lập tức (để tránh đè hình trong lúc chờ SeamlessSceneManager Unload)
                Scene currentScene = gameObject.scene;
                foreach (GameObject go in currentScene.GetRootGameObjects())
                {
                    go.SetActive(false);
                }

                // 3. Teleport Player
                other.transform.position = destSpawn.transform.position;
                Debug.Log($"[Border] Đã dịch chuyển Player đến {targetSpawnId}");

                // 4. Đặt Active Scene sang Map mới
                SceneManager.SetActiveScene(targetScene);
                Debug.Log($"[Border] Đã chuyển Active Scene sang {targetSceneName}");
            }
            else
            {
                Debug.LogError($"[Border] KHÔNG TÌM THẤY SpawnPoint có ID = '{targetSpawnId}' bên map {targetSceneName}!");
            }
        }
    }
}
