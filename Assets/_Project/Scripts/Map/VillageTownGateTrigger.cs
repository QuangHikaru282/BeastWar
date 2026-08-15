using System.Collections;
using UnityEngine;

/// <summary>
/// Đặt script này vào một GameObject có Collider2D (isTrigger = true)
/// tại cổng ra khỏi thị trấn Pallet.
///
/// Lần đầu Player bước vào (chưa có Beast):
///   1. Dừng Player
///   2. Oak chạy ra nói chuyện
///   3. Chuyển Player về phòng lab để chọn Starter
///
/// Sau khi đã chọn Starter → cổng mở hoàn toàn.
/// </summary>
public class VillageTownGateTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Kéo PlayerData vào đây")]
    [SerializeField] private PlayerData playerData;

    [Tooltip("Ảnh đại diện Trưởng Làng khi thoại")]
    [SerializeField] private Sprite oakAvatar;

    [Header("Chuyển Scene về Lab / Nhà Trưởng Làng")]
    [Tooltip("Tên Scene Lab trong Build Settings")]
    [SerializeField] private string targetSceneName = "TruongLang";

    [Tooltip("ID của SpawnPoint trong Scene Lab")]
    [SerializeField] private string targetSpawnPointId = "1";

    [Header("Cấu hình thoại")]
    [TextArea(2, 4)]
    [SerializeField] private string oakBlockDialogue =
        "Khoan đã! Bên ngoài rất nguy hiểm, không thể đi tay không như vậy được! " +
        "Hãy theo ta về phòng nghiên cứu, ta có thứ này sẽ giúp ích cho cháu!";

    // Đảm bảo chỉ kích hoạt 1 lần mỗi lần vào scene
    private bool _hasTriggered = false;

    private void Start()
    {
        // Nếu đã có Beast (đã qua giai đoạn 1) → tắt trigger này luôn
        if (playerData != null &&
            playerData.ownedBeasts != null &&
            playerData.ownedBeasts.Count > 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        // Nếu đã có Beast → không chặn
        if (playerData != null &&
            playerData.ownedBeasts != null &&
            playerData.ownedBeasts.Count > 0)
        {
            gameObject.SetActive(false);
            return;
        }

        _hasTriggered = true;
        StartCoroutine(OakBlockRoutine(other.gameObject));
    }

    private IEnumerator OakBlockRoutine(GameObject playerObj)
    {
        // 1. Dừng Player
        PlayerMapController playerCtrl = playerObj.GetComponent<PlayerMapController>();
        if (playerCtrl != null) playerCtrl.SetCanMove(false);

        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Chạy dialogue Oak
        bool dialogueDone = false;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                "Trưởng Làng",
                oakBlockDialogue,
                () => { dialogueDone = true; },
                oakAvatar
            );
            yield return new WaitUntil(() => dialogueDone);
        }
        else
        {
            // Fallback nếu không có DialogueManager
            yield return new WaitForSeconds(1f);
            dialogueDone = true;
        }

        // 3. Đưa Player về Lab qua Scene mới
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            // Lưu Spawn ID vào PlayerData để khi sang Scene Lab nhân vật đứng đúng vị trí
            if (playerData != null)
            {
                playerData.targetSpawnPointId = targetSpawnPointId;
            }

            // Gọi chuyển cảnh làm mờ (Fade Transition)
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
            yield break;
        }
        else
        {
            if (playerCtrl != null) playerCtrl.SetCanMove(true);
        }
    }
}
