using UnityEngine;

/// <summary>
/// Gắn script này lên NPC trong Room7 (ví dụ: Idle_0).
/// Khi người chơi nói chuyện xong, sẽ tự động kích hoạt Sảnh Danh Vọng (Hall of Fame).
/// </summary>
public class HallOfFameNPC : NPCDialogue
{
    [Header("Cài đặt Sảnh Danh Vọng")]
    [Tooltip("Kéo HallOfFameUI trong Scene vào đây (hoặc để trống nếu dùng Singleton HallOfFameUI.Instance)")]
    [SerializeField] private HallOfFameUI hallOfFameUI;

    protected override void OnDialogueFinished()
    {
        base.OnDialogueFinished();

        Debug.Log("[HallOfFameNPC] Hội thoại hoàn tất! Đang kích hoạt Sảnh Danh Vọng...");

        if (hallOfFameUI != null)
        {
            hallOfFameUI.ShowHallOfFame();
        }
        else if (HallOfFameUI.Instance != null)
        {
            HallOfFameUI.Instance.ShowHallOfFame();
        }
        else
        {
            Debug.LogWarning("[HallOfFameNPC] Không tìm thấy HallOfFameUI trong Scene! Hãy tạo GameObject HallOfFamePanel có gắn script HallOfFameUI.");
        }
    }
}
