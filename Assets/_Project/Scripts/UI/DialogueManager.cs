using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton quản lý toàn bộ hệ thống giao diện hội thoại (Dialogue UI).
/// 
/// Hỗ trợ:
/// 1. Tự động hiển thị khung thoại ở phía dưới màn hình.
/// 2. Hiệu ứng gõ chữ từng ký tự (Typewriter effect).
/// 3. Bấm F / Space / Click chuột để xem câu thoại tiếp theo hoặc tua nhanh.
/// 4. Tự động ẩn nút Hint phím F trên đầu NPC khi đang nói chuyện.
/// 5. Tự động tạm dừng di chuyển của Player khi đang thoại.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Khung chứa toàn bộ UI hội thoại (bảng thoại ở đáy màn hình)")]
    [SerializeField] private GameObject dialoguePanel;

    [Tooltip("Text hiển thị tên NPC")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Text legacyNameText;

    [Tooltip("Text hiển thị nội dung câu thoại")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Text legacyDialogueText;

    [Tooltip("Icon/Avatar đại diện của NPC (Tùy chọn)")]
    [SerializeField] private Image avatarImage;

    [Tooltip("Biểu tượng/Nút nhắc người chơi bấm F để tiếp tục (Tùy chọn)")]
    [SerializeField] private GameObject continueIndicator;

    [Header("Cấu hình thoại")]
    [Tooltip("Tốc độ gõ chữ (giây / ký tự)")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Tooltip("Tạm dừng di chuyển của Player khi đang nói chuyện")]
    [SerializeField] private bool freezePlayerDuringDialogue = true;

    // Trạng thái nội bộ
    private Queue<string> sentences = new Queue<string>();
    private bool isTyping = false;
    private string currentSentence = "";
    private Action onDialogueCompleteCallback;
    private PlayerMapController playerController;

    public bool IsDialogueActive => dialoguePanel != null && dialoguePanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Tự động tối ưu font chữ và bố cục để không bao giờ bị tràn khung thoại
        if (dialogueText != null)
        {
            dialogueText.enableAutoSizing = true;
            dialogueText.fontSizeMin = 18f;
            dialogueText.fontSizeMax = 28f;
            dialogueText.lineSpacing = -8f;
            dialogueText.margin = new Vector4(10f, 5f, 10f, 25f); // Chừa lề dưới để không chạm nút tiếp tục
        }

        if (nameText != null)
        {
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 18f;
            nameText.fontSizeMax = 32f;
        }

        if (continueIndicator != null)
        {
            // Dời nút "Nhấn [F] để tiếp tục" về góc dưới bên phải khung thoại
            RectTransform rt = continueIndicator.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-40f, 18f);
            }
        }
    }

    private void Start()
    {
        playerController = FindFirstObjectByType<PlayerMapController>();
    }

    private int startFrameCount = -1;

    private void Update()
    {
        if (!IsDialogueActive) return;

        // Bỏ qua input trong đúng 1 frame vừa mở thoại để không bị ăn phím F trùng lặp
        if (Time.frameCount == startFrameCount) return;

        // Bấm F, Space hoặc Chuột trái để chuyển thoại / tua chữ
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
    }

    // ────────────────────────────────────────────
    #region Public API

    /// <summary>
    /// Bắt đầu một cuộc hội thoại với danh sách các câu thoại.
    /// </summary>
    /// <param name="npcName">Tên NPC</param>
    /// <param name="lines">Danh sách các câu thoại</param>
    /// <param name="onComplete">Hành động thực thi sau khi kết thúc thoại</param>
    /// <param name="avatar">Hình đại diện NPC (tùy chọn)</param>
    public void StartDialogue(string npcName, IEnumerable<string> lines, Action onComplete = null, Sprite avatar = null)
    {
        if (lines == null) return;

        startFrameCount = Time.frameCount;
        onDialogueCompleteCallback = onComplete;

        // 1. Cập nhật tên & Avatar NPC
        if (nameText != null) nameText.text = string.IsNullOrEmpty(npcName) ? "???" : npcName;
        if (legacyNameText != null) legacyNameText.text = string.IsNullOrEmpty(npcName) ? "???" : npcName;

        if (avatarImage != null)
        {
            if (avatar != null)
            {
                avatarImage.sprite = avatar;
                avatarImage.gameObject.SetActive(true);
            }
            else
            {
                avatarImage.gameObject.SetActive(false);
            }
        }

        // 2. Đưa các câu thoại vào hàng chờ
        sentences.Clear();
        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                sentences.Enqueue(line);
            }
        }

        if (sentences.Count == 0) return;

        // 3. Mở khung thoại & Thông báo cho InteractHintManager ẩn nút F trên NPC
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialoguePanel.transform.SetAsLastSibling();

            Canvas cv = dialoguePanel.GetComponentInParent<Canvas>();
            if (cv != null) cv.sortingOrder = 9999;
        }
        InteractHintManager.Instance?.RegisterPanelOpen();

        // Ẩn tất cả các UI khác khi đang nói chuyện
        SetOtherUIActive(false);

        // 4. Khóa di chuyển của Player
        SetPlayerMovement(false);

        // 5. Hiển thị câu thoại đầu tiên
        DisplayNextSentence();
    }

    /// <summary>
    /// Bắt đầu hội thoại chỉ có 1 câu duy nhất.
    /// </summary>
    public void StartDialogue(string npcName, string singleLine, Action onComplete = null, Sprite avatar = null)
    {
        StartDialogue(npcName, new string[] { singleLine }, onComplete, avatar);
    }

    /// <summary>
    /// Hiển thị câu thoại tiếp theo hoặc tua nhanh câu thoại hiện tại.
    /// </summary>
    public void DisplayNextSentence()
    {
        // Nếu chữ đang chạy -> Bấm F/Click sẽ tua hiển thị nguyên câu ngay lập tức
        if (isTyping)
        {
            StopAllCoroutines();
            if (dialogueText != null) dialogueText.text = currentSentence;
            if (legacyDialogueText != null) legacyDialogueText.text = currentSentence;
            isTyping = false;

            if (continueIndicator != null)
            {
                continueIndicator.SetActive(true);
            }
            return;
        }

        // Nếu hết câu thoại -> Đóng bảng thoại
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Lấy câu thoại tiếp theo và bắt đầu hiệu ứng gõ chữ
        currentSentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    public float LastEndDialogueTime { get; private set; } = -100f;

    /// <summary>
    /// Đóng cuộc hội thoại.
    /// </summary>
    public void EndDialogue()
    {
        LastEndDialogueTime = Time.time;
        StopAllCoroutines();
        isTyping = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Thông báo cho HintManager để hiện lại phím F trên NPC (nếu player còn đứng gần)
        InteractHintManager.Instance?.RegisterPanelClose();

        // Hiện lại các UI khác đã ẩn khi hết hội thoại
        SetOtherUIActive(true);

        // Mở lại di chuyển của Player
        SetPlayerMovement(true);

        // Thực thi callback kết thúc thoại (nếu có) sau khi UI đã bật lại
        var callback = onDialogueCompleteCallback;
        onDialogueCompleteCallback = null;
        callback?.Invoke();
    }

    #endregion

    // ────────────────────────────────────────────
    #region Private Helpers

    private List<GameObject> hiddenUIElements = new List<GameObject>();

    private void SetOtherUIActive(bool active)
    {
        if (!active)
        {
            hiddenUIElements.Clear();

            // Lấy Canvas chứa Dialogue để làm mốc không ẩn
            Canvas dialogueCanvas = dialoguePanel != null ? dialoguePanel.GetComponentInParent<Canvas>() : null;

            // LearnMovePanel là UI đặc biệt trong Battle — KHÔNG được tắt
            // vì nó cần chạy Coroutine ngay sau khi thoại trong trận kết thúc.
            Canvas learnMoveCanvas = null;
            var learnMoveUI = FindFirstObjectByType<LearnMoveUI>();
            if (learnMoveUI != null)
                learnMoveCanvas = learnMoveUI.GetComponentInParent<Canvas>();

            // Tìm tất cả các Canvas trong scene
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in allCanvases)
            {
                if (canvas == null) continue;
                if (dialogueCanvas != null && canvas == dialogueCanvas) continue;
                if (learnMoveCanvas != null && canvas == learnMoveCanvas) continue; // Bảo vệ LearnMovePanel
                if (canvas.gameObject == gameObject || canvas.transform.IsChildOf(transform)) continue;

                if (canvas.gameObject.activeSelf)
                {
                    canvas.gameObject.SetActive(false);
                    hiddenUIElements.Add(canvas.gameObject);
                }
            }

            // Đồng thời kiểm tra các HUD button lẻ nếu chưa bị ẩn
            string[] hudNames = new string[] { "OpenPetButton", "OpenPanelQuest", "QuestArrow", "TimePanel", "Mở Sảnh Đội", "Toolbar", "PlayerHUD", "PETUI", "QuestPanel" };
            foreach (var name in hudNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null && obj.activeSelf)
                {
                    obj.SetActive(false);
                    if (!hiddenUIElements.Contains(obj)) hiddenUIElements.Add(obj);
                }
            }
        }
        else
        {
            foreach (var obj in hiddenUIElements)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
            hiddenUIElements.Clear();
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";
        if (legacyDialogueText != null) legacyDialogueText.text = "";

        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }

        string currentTyped = "";
        foreach (char letter in sentence.ToCharArray())
        {
            currentTyped += letter;
            if (dialogueText != null) dialogueText.text = currentTyped;
            if (legacyDialogueText != null) legacyDialogueText.text = currentTyped;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        if (continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }
    }

    private void SetPlayerMovement(bool enable)
    {
        if (!freezePlayerDuringDialogue) return;

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerMapController>();
        }

        if (playerController != null)
        {
            playerController.CanMove = enable;
        }
    }

    #endregion
}
