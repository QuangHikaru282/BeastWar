using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý UI hội thoại chỉ sử dụng một khung chân dung.
/// Avatar trong khung sẽ tự đổi theo người đang nói.
/// </summary>
public class SinglePortraitDialogueManager : MonoBehaviour
{
    public static SinglePortraitDialogueManager Instance { get; private set; }

    [Header("PANEL")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject interactionPrompt;

    [Header("TEXT UI")]
    [SerializeField] private TMP_Text interactionPromptText;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("MỘT KHUNG AVATAR DUY NHẤT")]
    [Tooltip("Kéo Image hiển thị avatar vào đây. Không kéo ảnh khung trang trí.")]
    [SerializeField] private Image currentSpeakerPortrait;

    [Header("THÔNG TIN PLAYER")]
    [SerializeField] private string playerName = "Player";
    [SerializeField] private Sprite playerPortrait;

    [Header("PHÍM ĐIỀU KHIỂN")]
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] private KeyCode nextLineKey = KeyCode.Space;

    private SinglePortraitNPCConversation nearbyNPC;
    private SinglePortraitNPCConversation talkingNPC;
    private int currentLineIndex = -1;
    private bool dialogueIsOpen;

    public bool DialogueIsOpen => dialogueIsOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Chỉ được có một SinglePortraitDialogueManager trong Scene.", gameObject);
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetObjectActive(dialoguePanel, false);
        SetObjectActive(interactionPrompt, false);
        HidePortrait();
    }

    private void Update()
    {
        if (!dialogueIsOpen)
        {
            if (nearbyNPC != null && Input.GetKeyDown(interactionKey))
                OpenDialogue(nearbyNPC);

            return;
        }

        if (Input.GetKeyDown(nextLineKey))
            ShowNextLine();
    }

    public void RegisterNearbyNPC(SinglePortraitNPCConversation npc)
    {
        if (npc == null || dialogueIsOpen)
            return;

        nearbyNPC = npc;

        if (interactionPromptText != null)
            interactionPromptText.text = $"Nhấn {interactionKey} để trò chuyện";

        SetObjectActive(interactionPrompt, true);
    }

    public void UnregisterNearbyNPC(SinglePortraitNPCConversation npc)
    {
        if (nearbyNPC != npc)
            return;

        nearbyNPC = null;

        if (!dialogueIsOpen)
            SetObjectActive(interactionPrompt, false);
    }

    public void OpenDialogue(SinglePortraitNPCConversation npc)
    {
        if (npc == null || dialogueIsOpen)
            return;

        if (npc.Lines == null || npc.Lines.Count == 0)
        {
            Debug.LogWarning($"NPC '{npc.NPCName}' chưa có câu thoại.", npc);
            return;
        }

        talkingNPC = npc;
        currentLineIndex = -1;
        dialogueIsOpen = true;

        SetObjectActive(interactionPrompt, false);
        SetObjectActive(dialoguePanel, true);
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (!dialogueIsOpen || talkingNPC == null)
        {
            CloseDialogue();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= talkingNPC.Lines.Count)
        {
            CloseDialogue();
            return;
        }

        SinglePortraitDialogueLine line = talkingNPC.Lines[currentLineIndex];
        DisplayLine(line);
    }

    private void DisplayLine(SinglePortraitDialogueLine line)
    {
        bool npcIsSpeaking = line.speaker == SinglePortraitDialogueSpeaker.NPC;

        string displayedName = npcIsSpeaking ? talkingNPC.NPCName : playerName;
        Sprite displayedPortrait = npcIsSpeaking ? talkingNPC.NPCPortrait : playerPortrait;

        if (speakerNameText != null)
            speakerNameText.text = displayedName;

        if (dialogueText != null)
            dialogueText.text = line.text;

        if (currentSpeakerPortrait != null)
        {
            currentSpeakerPortrait.sprite = displayedPortrait;
            currentSpeakerPortrait.color = Color.white;
            currentSpeakerPortrait.enabled = displayedPortrait != null;
        }
    }

    public void CloseDialogue()
    {
        dialogueIsOpen = false;
        talkingNPC = null;
        currentLineIndex = -1;

        if (speakerNameText != null)
            speakerNameText.text = string.Empty;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        HidePortrait();
        SetObjectActive(dialoguePanel, false);
        SetObjectActive(interactionPrompt, nearbyNPC != null);
    }

    public void SetPlayerInformation(string newPlayerName, Sprite newPlayerPortrait)
    {
        playerName = newPlayerName;
        playerPortrait = newPlayerPortrait;
    }

    private void HidePortrait()
    {
        if (currentSpeakerPortrait == null)
            return;

        currentSpeakerPortrait.sprite = null;
        currentSpeakerPortrait.enabled = false;
    }

    private static void SetObjectActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}