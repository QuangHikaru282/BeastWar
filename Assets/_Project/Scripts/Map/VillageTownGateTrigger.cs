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
    [Header("References — Kéo thả trong Inspector")]
    [Tooltip("Kéo PlayerData vào đây")]
    [SerializeField] private PlayerData playerData;

    [Tooltip("Kéo NPC Oak / Trưởng Làng vào đây")]
    [SerializeField] private ElderNPC elderNPC;

    [Tooltip("Kéo Sprite avatar của Oak vào đây (dùng trong dialogue)")]
    [SerializeField] private Sprite oakAvatar;

    [Header("Điểm đứng của Oak khi chặn cổng")]
    [Tooltip("Kéo Transform điểm Oak sẽ di chuyển đến khi chặn Player")]
    [SerializeField] private Transform oakBlockPosition;

    [Header("Điểm teleport Player về Lab")]
    [Tooltip("Kéo Transform vị trí Player sẽ được dịch chuyển đến (cửa Lab)")]
    [SerializeField] private Transform labEntrance;

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

        // 2. Di chuyển Oak đến vị trí chặn (nếu có)
        if (elderNPC != null && oakBlockPosition != null)
        {
            elderNPC.transform.position = oakBlockPosition.position;
        }

        // 3. Chạy dialogue Oak
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

        // 4. Teleport Player về cửa Lab
        if (labEntrance != null)
        {
            playerObj.transform.position = labEntrance.position;
        }

        // 5. Mở lại di chuyển
        if (playerCtrl != null) playerCtrl.SetCanMove(true);

        // 6. Kích hoạt Interact với Oak ngay lập tức
        //    để bảng chọn Starter mở tự động
        if (elderNPC != null)
        {
            // Chờ 1 frame để Player spawn xong
            yield return null;

            Kinnly.PlayerInventory inv = playerObj.GetComponent<Kinnly.PlayerInventory>();
            if (inv != null)
            {
                elderNPC.Interact(inv);
            }
        }
    }
}
