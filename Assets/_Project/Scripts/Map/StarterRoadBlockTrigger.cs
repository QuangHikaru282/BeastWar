using UnityEngine;

/// <summary>
/// Gắn script này vào GameObject có BoxCollider2D (Is Trigger = true) đặt chắn ngang con đường ra khỏi làng (chuẩn sự kiện Giáo Sư Oak trong Pokémon).
/// Khi người chơi chưa có Pet mà cố đi ra khỏi làng:
///   1. Giáo Sư / Trưởng Làng hiện thoại cảnh báo: "Khoan đã! Hoang dã phía trước rất nguy hiểm. Hãy đi theo ta..."
///   2. Sau khi dứt lời thoại -> Tự động chuyển Scene sang nhà Trưởng Làng (TruongLang) để chọn 3 Pet khởi đầu!
/// </summary>
public class StarterRoadBlockTrigger : MonoBehaviour
{
    [Header("Dữ liệu Người Chơi")]
    [Tooltip("Kéo PlayerData từ Assets vào")]
    [SerializeField] private PlayerData playerData;

    [Header("Cấu hình Chuyển Scene đến Nhà Trưởng Làng")]
    [Tooltip("Tên Scene nhà Trưởng Làng")]
    [SerializeField] private string targetSceneName = "TruongLang";

    [Tooltip("ID điểm xuất hiện trong nhà Trưởng Làng (mặc định là Lab)")]
    [SerializeField] private string targetSpawnPointId = "Lab";

    [Header("Thông tin Trưởng Làng")]
    [SerializeField] private string elderName = "Trưởng Làng";
    [SerializeField] private Sprite elderAvatar;

    [Header("Lời thoại cảnh báo")]
    [TextArea(2, 4)]
    [SerializeField] private string[] warningDialogue = new string[]
    {
        "Khoan đã! Đừng đi ra khu rừng hoang dã vội!",
        "Phía trước có rất nhiều Thú hoang dã nguy hiểm. Cháu không thể đi tay không được, cần có một chú Pet đồng hành để tự bảo vệ mình!",
        "Hãy đi theo ta về Viện Nghiên Cứu để nhận một chú Pet khởi đầu trước nhé!"
    };

    private bool isTriggering = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggering) return;
        if (!other.CompareTag("Player") && other.GetComponent<PlayerMapController>() == null) return;

        // Tự động tìm PlayerData nếu chưa gán
        if (playerData == null)
        {
            if (QuestManager.Instance != null && QuestManager.Instance.playerData != null)
                playerData = QuestManager.Instance.playerData;
            else
                playerData = Resources.Load<PlayerData>("PlayerData");
        }

        // Kiểm tra xem người chơi đã sở hữu Beast nào chưa
        bool hasPet = playerData != null && playerData.ownedBeasts != null && playerData.ownedBeasts.Count > 0;

        if (hasPet)
        {
            // Đã có Pet -> Cho phép đi qua con đường bình thường
            return;
        }

        // Chưa có Pet -> Dừng người chơi lại và mở hội thoại
        isTriggering = true;

        var playerCtrl = other.GetComponent<PlayerMapController>();
        if (playerCtrl != null)
        {
            playerCtrl.SetCanMove(false);
        }

        var rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(elderName, warningDialogue, () =>
            {
                // Sau khi kết thúc thoại -> Chuyển sang Scene TruongLang
                TeleportToLab();
            }, elderAvatar);
        }
        else
        {
            TeleportToLab();
        }
    }

    private void TeleportToLab()
    {
        if (playerData != null && !string.IsNullOrEmpty(targetSpawnPointId))
        {
            playerData.targetSpawnPointId = targetSpawnPointId;
            playerData.Save();
        }

        Debug.Log($"<color=green>[StarterRoadBlock]</color> Đang chuyển sang Scene '{targetSceneName}' để nhận Pet...");

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

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(finalSceneToLoad);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(finalSceneToLoad);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
