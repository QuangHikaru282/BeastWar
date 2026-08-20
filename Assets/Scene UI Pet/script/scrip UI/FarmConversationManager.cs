using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FarmConversationManager : MonoBehaviour
{
    public static FarmConversationManager Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Text UI")]
    [SerializeField] private TMP_Text interactionPromptText;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Ảnh nhân vật")]
    [SerializeField] private Image npcPortraitImage;
    [SerializeField] private Image playerPortraitImage;

    [Header("Thông tin Player")]
    [SerializeField] private string playerName = "Player";
    [SerializeField] private Sprite playerPortrait;

    [Header("Phím điều khiển")]
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] private KeyCode nextLineKey = KeyCode.Space;

    [Header("Màu chân dung")]
    [SerializeField] private Color speakingColor = Color.white;
    [SerializeField] private Color waitingColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private FarmNPCConversation nearbyNPC;
    private FarmNPCConversation activeNPC;
    private int currentLineIndex;
    private bool isDialogueActive;

    public bool IsDialogueActive => isDialogueActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Trong Scene chỉ được có một FarmConversationManager.", gameObject);
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetActiveSafe(dialoguePanel, false);
        SetActiveSafe(interactionPrompt, false);
        RefreshPlayerPortrait();
    }

    private void Update()
    {
        if (!isDialogueActive)
        {
            if (nearbyNPC != null && Input.GetKeyDown(interactionKey))
                StartDialogue(nearbyNPC);

            return;
        }

        if (Input.GetKeyDown(nextLineKey))
            ShowNextLine();
    }

    public void SetNearbyNPC(FarmNPCConversation npc)
    {
        if (npc == null || isDialogueActive)
            return;

        nearbyNPC = npc;

        if (interactionPromptText != null)
            interactionPromptText.text = $"Nhấn {interactionKey} để trò chuyện";

        SetActiveSafe(interactionPrompt, true);
    }

    public void ClearNearbyNPC(FarmNPCConversation npc)
    {
        if (nearbyNPC != npc)
            return;

        nearbyNPC = null;

        if (!isDialogueActive)
            SetActiveSafe(interactionPrompt, false);
    }

    public void StartDialogue(FarmNPCConversation npc)
    {
        if (npc == null || isDialogueActive)
            return;

        if (npc.DialogueLines == null || npc.DialogueLines.Count == 0)
        {
            Debug.LogWarning($"NPC '{npc.NPCName}' chưa có câu thoại.", npc);
            return;
        }

        activeNPC = npc;
        currentLineIndex = -1;
        isDialogueActive = true;

        if (npcPortraitImage != null)
        {
            npcPortraitImage.sprite = npc.NPCPortrait;
            npcPortraitImage.enabled = npc.NPCPortrait != null;
        }

        RefreshPlayerPortrait();
        SetActiveSafe(interactionPrompt, false);
        SetActiveSafe(dialoguePanel, true);
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (!isDialogueActive || activeNPC == null)
        {
            EndDialogue();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= activeNPC.DialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        FarmConversationLine line = activeNPC.DialogueLines[currentLineIndex];

        if (dialogueText != null)
            dialogueText.text = line.text;

        bool npcIsSpeaking = line.speaker == FarmConversationSpeaker.NPC;

        if (speakerNameText != null)
            speakerNameText.text = npcIsSpeaking ? activeNPC.NPCName : playerName;

        if (npcPortraitImage != null)
            npcPortraitImage.color = npcIsSpeaking ? speakingColor : waitingColor;

        if (playerPortraitImage != null)
            playerPortraitImage.color = npcIsSpeaking ? waitingColor : speakingColor;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        activeNPC = null;
        currentLineIndex = -1;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        SetActiveSafe(dialoguePanel, false);
        SetActiveSafe(interactionPrompt, nearbyNPC != null);
    }

    public void SetPlayerProfile(string newPlayerName, Sprite newPlayerPortrait)
    {
        playerName = newPlayerName;
        playerPortrait = newPlayerPortrait;
        RefreshPlayerPortrait();
    }

    private void RefreshPlayerPortrait()
    {
        if (playerPortraitImage == null)
            return;

        playerPortraitImage.sprite = playerPortrait;
        playerPortraitImage.enabled = playerPortrait != null;
        playerPortraitImage.color = waitingColor;
    }

    private static void SetActiveSafe(GameObject target, bool active)
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
