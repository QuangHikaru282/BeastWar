using UnityEngine;

/// <summary>
/// Đặt kịch bản này vào một GameObject rỗng để làm điểm đánh dấu vị trí xuất hiện của nhân vật.
/// </summary>
public class MapSpawnPoint : MonoBehaviour
{
    [Tooltip("ID của điểm sinh ra (Ví dụ: FromForest, FromCity). ID này phải khớp với cấu hình trong MapPortalTrigger của map cũ.")]
    public string spawnId = "FromForest";

    private void Start()
    {
        // 1. Tìm đọc dữ liệu PlayerData
        PlayerData pData = null;
        if (global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null)
        {
            pData = global::QuestManager.Instance.playerData;
        }
        else
        {
            pData = Resources.Load<PlayerData>("PlayerData");
        }

        if (pData != null)
        {
            // 2. Kiểm tra xem ID của Portal cũ có khớp với điểm này không?
            if (pData.targetSpawnPointId == spawnId)
            {
                // 3. Đưa nhân vật tới đúng tọa độ này
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = transform.position;
                    
                    // 4. Mở khóa cho nhân vật di chuyển lại bình thường
                    PlayerMapController playerCtrl = player.GetComponent<PlayerMapController>();
                    if (playerCtrl != null)
                    {
                        // Gọi trễ 1 chút để màn hình mờ hết màu đen rồi mới đi được
                        Invoke(nameof(UnlockPlayerMove), 0.8f);
                    }
                }

                // 5. Xóa ID để tránh lỗi nếu load lại scene
                pData.targetSpawnPointId = "";
            }
        }
    }

    private void UnlockPlayerMove()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMapController playerCtrl = player.GetComponent<PlayerMapController>();
            if (playerCtrl != null)
            {
                playerCtrl.SetCanMove(true);
            }
        }
    }
}
