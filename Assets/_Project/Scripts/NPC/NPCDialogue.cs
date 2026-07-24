using UnityEngine;
using Kinnly;

/// <summary>
/// Gắn script này lên bất kỳ NPC nào để hiển thị cuộc hội thoại khi người chơi lại gần và nhấn F.
/// </summary>
public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Header("Thông tin NPC")]
    [Tooltip("Tên hiển thị của NPC trong khung thoại")]
    [SerializeField] private string npcName = "Dân Làng";

    [Tooltip("Hình ảnh đại diện của NPC (Tùy chọn)")]
    [SerializeField] private Sprite npcAvatar;

    [Header("Nội dung cuộc hội thoại")]
    [Tooltip("Danh sách các câu thoại NPC sẽ nói lần lượt")]
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines = new string[]
    {
        "Chào mừng bạn đến với ngôi làng BeastWar!",
        "Hãy thám hiểm vùng đất này và thu phục những chú Pet hùng mạnh nhé."
    };

    public void Interact(PlayerInventory playerInventory)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(npcName, dialogueLines, OnDialogueFinished, npcAvatar);
        }
        else
        {
            Debug.LogWarning($"[NPCDialogue] Chưa có DialogueManager trong Scene! Nội dung thoại: {string.Join(" | ", dialogueLines)}");
        }
    }

    /// <summary>
    /// Phương thức ảo để các NPC khác có thể override hoặc lắng nghe khi hội thoại kết thúc.
    /// </summary>
    protected virtual void OnDialogueFinished()
    {
        Debug.Log($"[NPCDialogue] Đã kết thúc thoại với {npcName}.");
    }
}
