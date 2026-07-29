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
                    () => {
                        if (starterSelectionUI != null)
                        {
                            starterSelectionUI.SetActive(true);
                            starterSelectionUI.transform.SetAsLastSibling();
                            InteractHintManager.Instance?.RegisterPanelOpen();
                        }
                    },
                    npcAvatar
                );
            }
            else
            {
                starterSelectionUI.SetActive(true);
                starterSelectionUI.transform.SetAsLastSibling();
                InteractHintManager.Instance?.RegisterPanelOpen();
            }

            Debug.Log("Đã mở bảng chọn Pet khởi đầu.");
            return;
        }

        // 2. Kiểm tra Quest 25: Gặp Trưởng Làng
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
}