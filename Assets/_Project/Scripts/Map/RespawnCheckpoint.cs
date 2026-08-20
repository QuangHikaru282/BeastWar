using UnityEngine;

/// <summary>
/// Gắn script này vào Cửa/Vùng Trung tâm Pokémon hoặc NPC Y tá Joy.
/// Khi Player bước vào vùng (Trigger) hoặc tương tác (phím F), điểm này sẽ được lưu thành
/// 'Điểm hồi sinh gần nhất' trong PlayerData (chuẩn theo cơ chế Pokémon FireRed).
/// </summary>
public class RespawnCheckpoint : MonoBehaviour, Kinnly.IInteractable
{
    [Header("Cấu hình Checkpoint Trạm Xá")]
    [Tooltip("Tên Scene chứa Trung tâm Pokémon này (Ví dụ: HubTownNew, City, MainRoom)")]
    [SerializeField] private string checkpointSceneName = "HubTownNew";

    [Tooltip("ID của SpawnPoint xuất hiện trước cửa/trong Trung tâm Pokémon đó (Ví dụ: FromTown, HealCenter_Spawn)")]
    [SerializeField] private string checkpointSpawnPointId = "FromTown";

    [Header("Dữ liệu Người Chơi")]
    [SerializeField] private PlayerData playerData;

    [Header("Tùy chọn")]
    [Tooltip("Tự động lưu checkpoint ngay khi Player bước chân vào vùng Trigger")]
    [SerializeField] private bool triggerOnEnter = true;

    [Tooltip("Thông báo hiển thị khi tương tác (tùy chọn)")]
    [SerializeField] private string checkpointMessage = "Điểm hồi sinh của bạn đã được cập nhật tại đây!";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerOnEnter && collision.CompareTag("Player"))
        {
            SetCheckpoint();
        }
    }

    public void Interact(Kinnly.PlayerInventory playerInventory)
    {
        SetCheckpoint();

        if (!string.IsNullOrEmpty(checkpointMessage) && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Trung Tâm Pokémon", checkpointMessage);
        }
    }

    public void SetCheckpoint()
    {
        PlayerData pData = playerData != null 
            ? playerData 
            : (QuestManager.Instance != null ? QuestManager.Instance.playerData : Resources.Load<PlayerData>("PlayerData"));

        if (pData != null)
        {
            pData.respawnSceneName = checkpointSceneName;
            pData.respawnSpawnPointId = checkpointSpawnPointId;
            pData.Save();
            Debug.Log($"<color=green>[RespawnCheckpoint]</color> Đã cập nhật Điểm hồi sinh gần nhất: Scene='{checkpointSceneName}', SpawnId='{checkpointSpawnPointId}'");
        }
    }
}
