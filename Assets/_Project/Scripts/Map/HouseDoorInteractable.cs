using UnityEngine;
using Kinnly;

/// <summary>
/// Gắn script này vào Cửa nhà (GameObject có Collider2D với Option 'Is Trigger' được bật).
/// Tích hợp sẵn với hệ thống InteractHintManager (nút F tự động hiển thị khi lại gần).
/// Khi nhấn phím F, script sẽ chuyển sang Scene nhà/ngoài sân và lưu vị trí SpawnPoint ID.
/// </summary>
public class HouseDoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Cấu hình Chuyển Cảnh")]
    [Tooltip("Tên Scene đích trong Build Settings (Ví dụ: House_Player, HubTown)")]
    [SerializeField] private string targetSceneName;

    [Tooltip("ID của vị trí SpawnPoint tại Scene mới (Ví dụ: Door_Inside, Door_Outside)")]
    [SerializeField] private string targetSpawnPointId;

    [Header("Dữ liệu Người chơi")]
    [Tooltip("Kéo file ScriptableObject PlayerData vào đây để ghi nhớ SpawnPoint ID")]
    [SerializeField] private PlayerData playerData;

    [Header("Điều kiện Khóa Cửa (Tùy chọn)")]
    [Tooltip("Tick chọn nếu muốn kiểm tra điều kiện Nhiệm vụ trước khi cho mở cửa")]
    [SerializeField] private bool requireQuestUnlock = false;

    [Tooltip("ID Nhiệm vụ cần hoàn thành để mở cửa này")]
    [SerializeField] private int requiredQuestId = 0;

    [Tooltip("Thông báo hiển thị khi cửa bị khóa")]
    [SerializeField] private string lockedMessage = "Cửa này đang khóa!";

    [Header("Checkpoint Hồi Sinh Trạm Xá (Tùy chọn)")]
    [Tooltip("Tick chọn nếu ngôi nhà này là Trung Tâm Pokémon / Điểm lưu hồi sinh khi thua trận")]
    [SerializeField] private bool isRespawnCheckpoint = false;

    [Tooltip("ID của SpawnPoint trước cửa trạm xá (dùng để hồi sinh tại đây khi thua trận)")]
    [SerializeField] private string outsideRespawnSpawnPointId = "FromTown";

    public void Interact(PlayerInventory playerInventory)
    {
        // 1. Kiểm tra điều kiện mở khóa (nếu có yêu cầu)
        if (requireQuestUnlock)
        {
            int currentQuestId = 0;
            if (QuestManager.Instance != null && QuestManager.Instance.playerData != null)
            {
                currentQuestId = QuestManager.Instance.playerData.currentMainQuestId;
            }
            else if (playerData != null)
            {
                currentQuestId = playerData.currentMainQuestId;
            }

            if (currentQuestId < requiredQuestId)
            {
                Debug.Log($"<color=yellow>[HouseDoor]</color> {lockedMessage}");
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue("Cửa Khóa", lockedMessage);
                }
                return;
            }
        }

        // 2. Tạm thời khóa chuyển động của người chơi trong lúc chuyển cảnh
        PlayerMapController playerCtrl = playerInventory != null ? playerInventory.GetComponent<PlayerMapController>() : null;
        if (playerCtrl != null)
        {
            playerCtrl.SetCanMove(false);
        }

        // 3. Ghi nhớ SpawnPoint ID vào PlayerData để xuất hiện đúng chỗ ở Map mới
        if (QuestManager.Instance != null && QuestManager.Instance.playerData != null)
        {
            QuestManager.Instance.playerData.targetSpawnPointId = targetSpawnPointId;
        }
        else if (playerData != null)
        {
            playerData.targetSpawnPointId = targetSpawnPointId;
        }
        else
        {
            PlayerData loadedData = Resources.Load<PlayerData>("PlayerData");
            if (loadedData != null)
            {
                loadedData.targetSpawnPointId = targetSpawnPointId;
            }
        }

        // Cập nhật điểm hồi sinh nếu đây là Trung Tâm Pokémon / Trạm xá
        if (isRespawnCheckpoint)
        {
            PlayerData pDataToSave = QuestManager.Instance != null && QuestManager.Instance.playerData != null 
                ? QuestManager.Instance.playerData 
                : (playerData != null ? playerData : Resources.Load<PlayerData>("PlayerData"));
            
            if (pDataToSave != null)
            {
                pDataToSave.respawnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                pDataToSave.respawnSpawnPointId = outsideRespawnSpawnPointId;
                pDataToSave.Save();
                Debug.Log($"<color=green>[Checkpoint]</color> Đã cập nhật Điểm hồi sinh: Scene='{pDataToSave.respawnSceneName}', SpawnId='{outsideRespawnSpawnPointId}'");
            }
        }

        // Xóa cờ vị trí cũ của trận đánh trước đó để ưu tiên đi qua cửa
        BattleTransferData battleData = Resources.Load<BattleTransferData>("BattleTransferData");
        if (battleData != null)
        {
            battleData.returnToLastPosition = false;
        }

        Debug.Log($"<color=cyan>[HouseDoor]</color> Đang chuyển sang Scene '{targetSceneName}' tại điểm Spawn '{targetSpawnPointId}'...");

        // Tự động giữ Scene GameCore nếu đang chạy ở chế độ Multi-Scene
        string finalSceneToLoad = targetSceneName;
        bool isGameCoreLoaded = false;
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name == "GameCore")
            {
                isGameCoreLoaded = true;
                break;
            }
        }

        if (isGameCoreLoaded && !finalSceneToLoad.Contains("GameCore"))
        {
            finalSceneToLoad = "GameCore," + finalSceneToLoad;
        }

        // 4. Kích hoạt hiệu ứng chuyển cảnh mượt
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(finalSceneToLoad);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(finalSceneToLoad);
        }
    }
}
