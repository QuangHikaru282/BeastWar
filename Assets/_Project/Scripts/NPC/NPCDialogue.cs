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

    [Header("Tuỳ chọn Một lần duy nhất (Giống hệ thống Flag của Pokémon)")]
    [Tooltip("Tích vào nếu muốn script này tự TẮT sau khi nói xong lần đầu tiên.\n" +
             "Dùng khi NPC có nhiều script (ví dụ: Mẹ vừa có thoại mở đầu vừa có chức năng chữa trị).")]
    [SerializeField] private bool disableAfterDialogue = false;

    [Tooltip("Khoá lưu trạng thái duy nhất cho NPC này (dùng PlayerPrefs).\n" +
             "PHẢI là chuỗi không trùng với NPC nào khác. Ví dụ: 'Mom_IntroDialogue_Done'\n" +
             "Để trống nếu không cần lưu (thoại sẽ reset mỗi lần tải scene).")]
    [SerializeField] private string saveKey = "";

    private void Awake()
    {
        // Kiểm tra flag lưu trong PlayerPrefs — giống hệ thống Script Flag của Pokémon.
        // Nếu người chơi đã từng nói chuyện với NPC này (flag = 1) → Tắt component ngay khi load scene.
        // Điều này đảm bảo dù bị đánh bại về nhà hay tải lại game bao nhiêu lần,
        // câu thoại mở đầu sẽ KHÔNG bao giờ lặp lại.
        if (disableAfterDialogue && !string.IsNullOrEmpty(saveKey))
        {
            if (PlayerPrefs.GetInt(saveKey, 0) == 1)
            {
                this.enabled = false;
            }
        }
    }

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

        if (disableAfterDialogue)
        {
            // Lưu flag vào PlayerPrefs để tồn tại vĩnh viễn qua mọi lần tải scene / tắt mở game
            if (!string.IsNullOrEmpty(saveKey))
            {
                PlayerPrefs.SetInt(saveKey, 1);
                PlayerPrefs.Save();
                Debug.Log($"[NPCDialogue] Đã lưu flag '{saveKey}' = 1 vào PlayerPrefs.");
            }

            // Tắt chính component này để lần sau nhấn F sẽ gọi script IInteractable tiếp theo
            this.enabled = false;
            Debug.Log($"[NPCDialogue] Đã tắt NPCDialogue trên '{gameObject.name}' sau lần thoại đầu tiên.");
        }
    }
}

