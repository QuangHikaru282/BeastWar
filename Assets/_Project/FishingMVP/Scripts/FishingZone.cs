using UnityEngine;

public class FishingZone : MonoBehaviour
{
    [Header("Cấu hình")]
    [Tooltip("Kéo GameObject FishingSystem vào đây")]
    public FishingMinigame fishingMinigame;

    [Tooltip("Tag của nhân vật chính để kiểm tra va chạm")]
    public string playerTag = "Player";

    private void Start()
    {
        // Tự động tìm FishingMinigame nếu chưa được gán
        if (fishingMinigame == null)
        {
            fishingMinigame = Object.FindFirstObjectByType<FishingMinigame>(FindObjectsInactive.Include);
        }

        // Đảm bảo minigame luôn bị tắt khi game mới bắt đầu (chưa vào vùng)
        if (fishingMinigame != null)
        {
            fishingMinigame.enabled = false;
        }
        else
        {
            Debug.LogWarning("FishingZone: Không tìm thấy FishingMinigame trong scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi Player bước vào vùng Trigger
        if (collision.CompareTag(playerTag))
        {
            if (fishingMinigame != null)
            {
                fishingMinigame.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Khi Player đi ra khỏi vùng Trigger
        if (collision.CompareTag(playerTag))
        {
            if (fishingMinigame != null)
            {
                // Tắt script đi để không cho bấm Space quăng cần nữa
                fishingMinigame.enabled = false;
                
                // (Mẹo nhỏ cho MVP: Tránh lỗi giao diện bị kẹt nếu người chơi chạy mất lúc đang câu)
                if (fishingMinigame.reelingFish)
                {
                    fishingMinigame.FishCaught(); // Ép kết thúc câu cá
                }
            }
        }
    }
}
