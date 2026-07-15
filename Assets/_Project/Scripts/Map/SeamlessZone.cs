using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gắn script này vào một GameObject có BoxCollider2D (IsTrigger = true) bao trọn toàn bộ 1 khu vực Map (Ví dụ: HubTown).
/// Khi người chơi đi vào khu vực này, nó sẽ tự động nạp các Map hàng xóm lên bộ nhớ.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SeamlessZone : MonoBehaviour
{
    [Tooltip("Tên Scene hiện tại của khu vực này (VD: HubTownNew)")]
    public string currentSceneName;

    [Tooltip("Danh sách tên các Scene hàng xóm sát vách cần nạp trước (VD: ForestScene)")]
    public List<string> neighborScenes = new List<string>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có phải Player đi vào không
        if (other.CompareTag("Player"))
        {
            if (string.IsNullOrEmpty(currentSceneName))
            {
                Debug.LogWarning($"[SeamlessZone] Vùng SeamlessZone trên GameObject {gameObject.name} chưa được điền tên currentSceneName!");
                return;
            }

            Debug.Log($"[SeamlessZone] Player vừa bước vào khu vực {currentSceneName}. Bắt đầu nạp Map hàng xóm...");
            
            // Ra lệnh cho SeamlessSceneManager nạp các map lân cận và xóa các map ở xa
            SeamlessSceneManager.Instance.UpdateZones(currentSceneName, neighborScenes);
        }
    }
}
