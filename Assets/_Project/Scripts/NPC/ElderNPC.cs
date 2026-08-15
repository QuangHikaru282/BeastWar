using UnityEngine;
using Kinnly;

public class ElderNPC : MonoBehaviour, IInteractable
{
    [Header("UI Component")]
    [Tooltip("Kéo GameObject StarterSelectionPanel vào đây")]
    public GameObject starterSelectionUI;

    [Tooltip("Hình ảnh đại diện (Avatar) của Trưởng Làng")]
    public Sprite npcAvatar;

    [Header("Thoại Nhắc Nhở / Thường Ngày")]
    [Tooltip("Các câu thoại ngẫu nhiên khi Trưởng Làng không có nhiệm vụ trực tiếp")]
    [TextArea(2, 4)]
    public string[] defaultDialogueLines = new string[]
    {
        "Chúc cháu lên đường bình an! Hãy chăm sóc tốt cho các bạn Pet của mình nhé.",
        "Nếu cần thêm vật phẩm hay hạt giống, cháu hãy ghé Cửa Hàng của Thương Gia trong làng nhé!",
        "Chăm chỉ rèn luyện và thám hiểm sẽ giúp đội hình của cháu ngày càng mạnh mẽ hơn."
    };

    [Header("State")]
    public bool hasGivenStarter = false;

    private int lastDialogueIndex = 0;

    private void Start()
    {
        // Ẩn bảng chọn Pet khi vừa bắt đầu game
        if (starterSelectionUI != null)
        {
            starterSelectionUI.SetActive(false);
        }

        UpdateHasGivenStarterState();

        // Tự động kiểm tra và chạy hội thoại chúc mừng nếu vừa đánh thắng Rival xong
        StartCoroutine(CheckAutoCongratulateRoutine());
    }

    private System.Collections.IEnumerator CheckAutoCongratulateRoutine()
    {
        // Chờ scene và player khởi tạo ổn định
        yield return new WaitForSeconds(0.5f);

        PlayerData pData = null;
        if (global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null)
        {
            pData = global::QuestManager.Instance.playerData;
        }
        else
        {
            pData = Resources.Load<PlayerData>("PlayerData");
        }

        if (pData != null && pData.defeatedTrainers != null && pData.defeatedTrainers.Contains("Rival_Lab_Phase1"))
        {
            // Kiểm tra xem đã nói câu chúc mừng chưa (nếu quest còn là 0)
            if (pData.currentMainQuestId == 0)
            {
                pData.currentMainQuestId = 1; // Hoàn thành Giai đoạn 1

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(
                        "Trưởng Làng",
                        "Trận đấu vừa rồi tuyệt vời lắm! Cháu đã thể hiện sự gắn kết rất tốt với Beast của mình.\n" +
                        "Bây giờ cổng làng đã mở, chúc cháu có một chuyến phiêu lưu thật vui vẻ và đầy kỳ thú!",
                        null,
                        npcAvatar
                    );
                }
            }
        }
    }

    private void UpdateHasGivenStarterState()
    {
        if (hasGivenStarter) return;

        if (global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null)
        {
            var pData = global::QuestManager.Instance.playerData;
            if (pData.currentMainQuestId >= 1 || (pData.ownedBeasts != null && pData.ownedBeasts.Count > 0))
            {
                hasGivenStarter = true;
            }
        }
    }

    public void Interact(PlayerInventory playerInventory)
    {
        UpdateHasGivenStarterState();

        // 1. Giao Pet khởi đầu
        if (!hasGivenStarter)
        {
            if (starterSelectionUI == null)
            {
                Debug.LogWarning(
                    "Chưa gán giao diện StarterSelectionUI " +
                    "cho NPC Trưởng làng!"
                );

                return;
            }

            // Đảm bảo bảng chọn Pet bị ẩn trong khi Trưởng Làng đang thoại
            if (starterSelectionUI != null)
            {
                starterSelectionUI.SetActive(false);
            }

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(
                    "Trưởng Làng",
                    "Làng của chúng ta đang bị quái vật quấy phá. Cháu hãy nhận lấy một Pet khởi đầu và giúp ta giải quyết chúng nhé!",
                    () => { StartCoroutine(ShowStarterUIDelayed()); },
                    npcAvatar
                );
            }
            else
            {
                StartCoroutine(ShowStarterUIDelayed());
            }

            Debug.Log("Đã bắt đầu thoại chọn Pet khởi đầu.");
            return;
        }

        // 2. Chúc mừng sau khi thắng trận đấu với Rival
        if (hasGivenStarter && global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null)
        {
            var pData = global::QuestManager.Instance.playerData;
            if (pData.defeatedTrainers != null && pData.defeatedTrainers.Contains("Rival_Lab_Phase1"))
            {
                if (pData.currentMainQuestId == 0)
                {
                    pData.currentMainQuestId = 1; // Hoàn thành Giai đoạn 1, mở khóa tiến trình tiếp theo
                }

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(
                        "Trưởng Làng",
                        "Trận đấu vừa rồi tuyệt vời lắm! Cháu đã thể hiện sự gắn kết rất tốt với Beast của mình.\n" +
                        "Bây giờ cổng làng đã mở, cháu có thể tự do ra ngoài khám phá thế giới!",
                        null,
                        npcAvatar
                    );
                    return;
                }
            }
        }

        // 3. Kiểm tra Quest 25: Gặp Trưởng Làng
        if (hasGivenStarter && global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null)
        {
            int qId = global::QuestManager.Instance.playerData.currentMainQuestId;
            if (qId == 25)
            {
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(
                        "Trưởng Làng",
                        "Chào cháu! Cháu đã học được cách câu cá rồi đấy. Ta tặng cháu 100 Vàng và Bóng Thu Phục để chuẩn bị thu phục các loài Thú mới ở Hồ Thần Bí!",
                        () => {
                            global::QuestManager.Instance.MarkCurrentQuestCompleted();
                        },
                        npcAvatar
                    );
                }
                else
                {
                    global::QuestManager.Instance.MarkCurrentQuestCompleted();
                }
                return;
            }
        }

        // 3. Kiểm tra Quest 16: thu thập 1000 vàng
        if (
            hasGivenStarter
            && global::QuestManager.Instance != null
            && global::QuestManager.Instance.playerData != null
            && global::QuestManager.Instance
                .playerData.currentMainQuestId == 16
        )
        {
            var pData =
                global::QuestManager.Instance.playerData;

            if (pData.gold >= 1000)
            {
                pData.gold -= 1000;

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(
                        "Trưởng Làng",
                        "Tuyệt vời! Cháu đã mang về đủ 1000 vàng. Ta sẽ dùng số tiền này để mở rộng Nông Trại cho cháu!",
                        null,
                        npcAvatar
                    );
                }

                global::QuestManager.Instance.AdvanceQuest();
            }
            else
            {
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(
                        "Trưởng Làng",
                        $"Cháu vẫn chưa đủ 1000 vàng. Hiện tại cháu mới có {pData.gold} vàng thôi. Hãy cố gắng lên nhé!",
                        null,
                        npcAvatar
                    );
                }
            }

            return;
        }

        // 3. Thoại bình thường khi không có nhiệm vụ trực tiếp tại Trưởng Làng
        if (hasGivenStarter)
        {
            var dialoguePages = new System.Collections.Generic.List<string>();

            // Trang 1: Lời khuyên thường ngày
            if (defaultDialogueLines != null && defaultDialogueLines.Length > 0)
            {
                dialoguePages.Add(defaultDialogueLines[lastDialogueIndex % defaultDialogueLines.Length]);
                lastDialogueIndex++;
            }
            else
            {
                dialoguePages.Add("Chúc cháu lên đường bình an! Hãy chăm sóc tốt cho các bạn Pet của mình nhé.");
            }

            // Trang 2: Nhắc nhở tiến trình nhiệm vụ hiện tại
            if (global::QuestManager.Instance != null)
            {
                string questDesc = global::QuestManager.Instance.GetCurrentQuestDescription();
                if (!string.IsNullOrEmpty(questDesc))
                {
                    dialoguePages.Add($"Nhiệm vụ hiện tại của cháu:\n{questDesc}");
                }
            }

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(
                    "Trưởng Làng",
                    dialoguePages,
                    null,
                    npcAvatar
                );
            }
        }
    }

    /// <summary>
    /// Chờ Dialogue đóng hoàn toàn rồi mới kích hoạt StarterSelectionPanel.
    /// </summary>
    private System.Collections.IEnumerator ShowStarterUIDelayed()
    {
        // Chờ đến khi Dialogue hoàn tất và đóng hẳn
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            yield return null;
        }

        // Chờ thêm 1 frame để hệ thống Canvas khôi phục trạng thái
        yield return null;

        if (starterSelectionUI != null)
        {
            // Bật Canvas cha trước
            Canvas parentCanvas = starterSelectionUI.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null)
            {
                parentCanvas.gameObject.SetActive(true);
                parentCanvas.sortingOrder = 1000;
            }

            // Bật chính StarterSelectionPanel
            starterSelectionUI.SetActive(true);
            starterSelectionUI.transform.SetAsLastSibling();

            InteractHintManager.Instance?.RegisterPanelOpen();
            Debug.Log("<color=green>[ElderNPC] Đã kích hoạt thành công StarterSelectionPanel!</color>");
        }
        else
        {
            Debug.LogError("[ElderNPC] starterSelectionUI đang bị null! Hãy kéo StarterSelectionPanel vào ô trong Inspector của NPC Trưởng Làng.");
        }
    }
}