using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý Cutscene biến cố trước Cổng Đại Hội:
/// 1. Kiểm tra nếu người chơi có đủ 5 huy hiệu.
/// 2. Kẻ xấu cầm hung khí bất ngờ lao ra đe dọa.
/// 3. Chú Beast đồng hành (con mèo) lao ra đỡ nhát chém thay người chơi.
/// 4. Hiệu ứng chém, chớp đỏ màn hình, Beast ngã gục.
/// 5. Đánh bại kẻ xấu -> Đưa Beast vào cấp cứu.
/// </summary>
public class LeagueAmbushCutscene : MonoBehaviour
{
    [Header("Dữ liệu Người Chơi")]
    [Tooltip("Kéo PlayerData asset vào đây")]
    [SerializeField] private PlayerData playerData;

    [Header("Các Nhân Vật Trong Cutscene (Gán qua Inspector)")]
    [Tooltip("GameObject của Player trên Map")]
    [SerializeField] private PlayerMapController playerController;

    [Tooltip("GameObject của Kẻ Xấu / Tên Cướp")]
    [SerializeField] private GameObject thiefObject;

    [Tooltip("GameObject của Chú Beast đồng hành xuất hiện đỡ đòn")]
    [SerializeField] private GameObject companionBeastObject;

    [Header("Hiệu Ứng Hình Ảnh & Âm Thanh")]
    [Tooltip("Màn che màu đỏ hoặc đen để chớp màn hình khi bị chém")]
    [SerializeField] private Image screenFlashOverlay;

    [Tooltip("AudioSource để phát âm thanh")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Âm thanh vung dao / chém trúng")]
    [SerializeField] private AudioClip slashSFX;

    [Tooltip("Âm thanh Beast bị thương / ngã gục")]
    [SerializeField] private AudioClip beastHurtSFX;

    [Header("Cấu hình Cổng / Spawn Point tiếp theo")]
    [Tooltip("Tên Scene Bệnh Viện / Phòng Cấp Cứu sẽ chuyển đến sau biến cố")]
    [SerializeField] private string emergencySceneName = "BenhVien";

    private bool hasTriggered = false;

    private void Start()
    {
        if (thiefObject != null) thiefObject.SetActive(false);
        if (companionBeastObject != null) companionBeastObject.SetActive(false);
        if (screenFlashOverlay != null)
        {
            Color c = screenFlashOverlay.color;
            c.a = 0f;
            screenFlashOverlay.color = c;
            screenFlashOverlay.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;

        // Chỉ kích hoạt khi Player bước vào và đã có đủ 5 Huy Hiệu
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMapController>() != null)
        {
            if (playerData != null && playerData.gymBadges != null && playerData.gymBadges.Count >= 5)
            {
                hasTriggered = true;
                StartCoroutine(AmbushCutsceneRoutine());
            }
        }
    }

    private IEnumerator AmbushCutsceneRoutine()
    {
        // 1. Khóa di chuyển của người chơi
        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        yield return new WaitForSeconds(0.3f);

        // 2. Kẻ xấu xuất hiện
        if (thiefObject != null)
        {
            thiefObject.SetActive(true);
        }

        // 3. Đoạn thoại đe dọa của kẻ xấu
        bool dialogueFinished = false;
        string[] thiefLines = new string[]
        {
            "Đứng im! Không được nhúc nhích!",
            "Giao toàn bộ 5 Huy Hiệu Hội Quán và đồ đạc quý giá trên người ngươi ra đây mau!",
            "Nếu dám hé răng kêu cứu, nhát dao này sẽ không tha cho ngươi đâu!"
        };

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Kẻ Bịt Mặt", thiefLines, () => dialogueFinished = true);
            yield return new WaitUntil(() => dialogueFinished);
        }
        else
        {
            yield return new WaitForSeconds(2.5f);
        }

        // 4. Kẻ xấu vung hung khí tấn công
        dialogueFinished = false;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Kẻ Bịt Mặt", "Đã bảo là không được nhúc nhích cơ mà! Chết đi!", () => dialogueFinished = true);
            yield return new WaitUntil(() => dialogueFinished);
        }

        // 5. Chú Beast đồng hành bất ngờ lao ra đỡ đòn!
        if (companionBeastObject != null)
        {
            companionBeastObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.2f);

        // Phát âm thanh chém & Beast đau đớn
        if (audioSource != null)
        {
            if (slashSFX != null) audioSource.PlayOneShot(slashSFX);
            if (beastHurtSFX != null) audioSource.PlayOneShot(beastHurtSFX);
        }

        // Chớp đỏ màn hình
        if (screenFlashOverlay != null)
        {
            screenFlashOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(FlashScreenRoutine(Color.red, 0.4f));
        }

        // 6. Beast ngã gục
        dialogueFinished = false;
        string[] reactionLines = new string[]
        {
            "KHÔNGGGGG! Bạn mèo... em làm sao vậy?!",
            "Sao em lại lao ra đỡ nhát dao đó thay tớ?!",
            "Máu... máu chảy nhiều quá... Xin em hãy mở mắt ra nhìn tớ đi!!"
        };

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Bạn", reactionLines, () => dialogueFinished = true);
            yield return new WaitUntil(() => dialogueFinished);
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        // 7. Kẻ xấu hoảng loạn và bị tóm
        dialogueFinished = false;
        string[] thiefDefeatLines = new string[]
        {
            "Khốn khiếp... Con quái thú chết tiệt này ở đâu nhảy ra thế...",
            "Bảo vệ! Bảo vệ Cổng Đại Hội đang chạy tới kìa! Chết tiệt, ta phải chuồn thôi!",
            "(Kẻ xấu bị các bảo vệ và bạn dốc toàn lực khống chế bắt giữ tại chỗ!)"
        };

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Kẻ Bịt Mặt", thiefDefeatLines, () => dialogueFinished = true);
            yield return new WaitUntil(() => dialogueFinished);
        }

        // 8. Chuyển cảnh cấp cứu khẩn cấp
        dialogueFinished = false;
        string[] doctorEmergencyLines = new string[]
        {
            "Bác Sĩ Cấp Cứu: Vết thương rất sâu! Nhịp tim đang yếu dần, mau đưa vào buồng hồi sức đặc biệt ngay!",
            "Bác Sĩ: Cậu bé, bạn đồng hành của cháu đã dùng cả sinh mạng để bảo vệ cháu.",
            "Hãy vững tin! Cháu nhất định phải bước lên sàn đấu Vô Địch để đền đáp lòng quả cảm của người bạn này!"
        };

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Bác Sĩ Trưởng", doctorEmergencyLines, () => dialogueFinished = true);
            yield return new WaitUntil(() => dialogueFinished);
        }

        // Chuyển Scene hoặc đưa người chơi tiếp tục tiến vào đấu trường
        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }

        if (!string.IsNullOrEmpty(emergencySceneName))
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(emergencySceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(emergencySceneName);
            }
        }
    }

    private IEnumerator FlashScreenRoutine(Color flashColor, float duration)
    {
        float elapsed = 0f;
        Color c = flashColor;

        // Fade in
        while (elapsed < duration * 0.3f)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 0.8f, elapsed / (duration * 0.3f));
            screenFlashOverlay.color = c;
            yield return null;
        }

        // Fade out
        elapsed = 0f;
        while (elapsed < duration * 0.7f)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0.8f, 0f, elapsed / (duration * 0.7f));
            screenFlashOverlay.color = c;
            yield return null;
        }

        c.a = 0f;
        screenFlashOverlay.color = c;
        screenFlashOverlay.gameObject.SetActive(false);
    }
}
