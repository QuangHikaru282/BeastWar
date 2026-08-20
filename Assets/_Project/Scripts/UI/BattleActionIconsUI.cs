using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quan ly 3 icon hanh dong trong tran chien:
///   1. PokeballBtn  - An khi la tran Trainer
///   2. BackpackBtn  - Giu BattleItemMenuUI
///   3. EscapeBtn    - Thoat tran (100%)
///
/// Gan script nay vao mot GameObject chua ca 3 nut (vi du: BattleActionIcons).
/// </summary>
public class BattleActionIconsUI : MonoBehaviour
{
    public static BattleActionIconsUI Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button pokeballBtn;
    [SerializeField] private Button backpackBtn;
    [SerializeField] private Button escapeBtn;

    [Header("Pokeball count text (optional)")]
    [SerializeField] private Text pokeballCountText;
    [SerializeField] private TMPro.TextMeshProUGUI pokeballCountTextTMP;

    [Header("Escape Confirm Dialog")]
    [SerializeField] private GameObject escapeConfirmPanel;
    [SerializeField] private Button escapeConfirmYesBtn;
    [SerializeField] private Button escapeConfirmNoBtn;

    [Header("Data")]
    [SerializeField] private BattleTransferData battleTransferData;

    private BattleItemMenuUI itemMenuUI;

    public bool IsEscapeConfirmOpen => escapeConfirmPanel != null && escapeConfirmPanel.activeInHierarchy;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (battleTransferData == null)
            battleTransferData = Resources.FindObjectsOfTypeAll<BattleTransferData>().Length > 0
                ? Resources.FindObjectsOfTypeAll<BattleTransferData>()[0] : null;
    }

    private void Start()
    {
        // Ẩn nút Pokeball nếu là trận Trainer
        if (battleTransferData != null && battleTransferData.isTrainerBattle)
        {
            if (pokeballBtn != null) pokeballBtn.gameObject.SetActive(false);
        }

        // Ẩn panel xác nhận thoát khi bắt đầu
        if (escapeConfirmPanel != null)
        {
            escapeConfirmPanel.SetActive(false);
        }

        // Kết nối sự kiện cho các nút được gán thủ công trong Inspector
        if (pokeballBtn != null)
        {
            pokeballBtn.onClick.RemoveAllListeners();
            pokeballBtn.onClick.AddListener(OnPokeballClicked);
        }

        if (escapeBtn != null)
        {
            escapeBtn.onClick.RemoveAllListeners();
            escapeBtn.onClick.AddListener(OnEscapeClicked);
        }

        if (escapeConfirmYesBtn != null)
        {
            escapeConfirmYesBtn.onClick.RemoveAllListeners();
            escapeConfirmYesBtn.onClick.AddListener(OnEscapeConfirmed);
        }

        if (escapeConfirmNoBtn != null)
        {
            escapeConfirmNoBtn.onClick.RemoveAllListeners();
            escapeConfirmNoBtn.onClick.AddListener(OnEscapeCancelled);
        }

        // Tim BattleItemMenuUI tu BackpackBtn va gan su kien Click
        if (backpackBtn != null)
        {
            itemMenuUI = backpackBtn.GetComponent<BattleItemMenuUI>();
            backpackBtn.onClick.RemoveAllListeners();
            backpackBtn.onClick.AddListener(OnBackpackClicked);
        }

        // Goi BattleCaptureHandler cap nhat UI Pokeball
        if (BattleCaptureHandler.Instance != null)
        {
            if (pokeballCountText != null)
                BattleCaptureHandler.Instance.GetType()
                    .GetField("pokeballCountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(BattleCaptureHandler.Instance, pokeballCountText);

            if (pokeballCountTextTMP != null)
                BattleCaptureHandler.Instance.GetType()
                    .GetField("pokeballCountTextTMP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(BattleCaptureHandler.Instance, pokeballCountTextTMP);

            BattleCaptureHandler.Instance.RefreshPokeballUI();

            // Lang nghe ket qua bat thu
            BattleCaptureHandler.Instance.OnCaptureSessionEnd += HandleCaptureEnd;
        }

        // Lang nghe dung do tieu luot
        if (BattleItemHandler.Instance != null)
            BattleItemHandler.Instance.OnItemUsed += HandleItemUsed;
    }

    private void OnDestroy()
    {
        if (BattleCaptureHandler.Instance != null)
            BattleCaptureHandler.Instance.OnCaptureSessionEnd -= HandleCaptureEnd;

        if (BattleItemHandler.Instance != null)
            BattleItemHandler.Instance.OnItemUsed -= HandleItemUsed;
    }

    private void Update()
    {
        // Xử lý phím tắt Escape bàn phím
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Nếu Balo đang mở -> Đóng Balo lại trước (đã xử lý trong BattleItemMenuUI)
            if ((itemMenuUI != null && itemMenuUI.IsOpen) || (BattleItemMenuUI.Instance != null && BattleItemMenuUI.Instance.IsOpen))
            {
                return;
            }

            // Nếu Panel xác nhận thoát đang mở -> bấm Esc để hủy/đóng panel
            if (escapeConfirmPanel != null && escapeConfirmPanel.activeSelf)
            {
                OnClickNoEscape();
            }
            else
            {
                // Mở panel xác nhận thoát
                OnEscapeClicked();
            }
        }
    }

    // ─── Pokeball ────────────────────────────────────────────────────

    private void OnPokeballClicked()
    {
        BattleCaptureHandler.Instance?.OnPokeballButtonPressed();
        BattleCaptureHandler.Instance?.RefreshPokeballUI();
    }

    private void HandleCaptureEnd(bool success)
    {
        if (success)
        {
            BattleManager.Instance?.OnCaptureBeastSuccess();
        }
        else
        {
            BattleManager.Instance?.OnWildBeastFled();
        }
    }

    // ─── Item ────────────────────────────────────────────────────────

    private void HandleItemUsed()
    {
        BattleManager.Instance?.OnPlayerUsedItem();
    }

    // ─── Escape ──────────────────────────────────────────────────────

    public void OnEscapeClicked()
    {
        // Nếu Balo đang mở -> Tự động đóng Balo lại trước để không bị chồng chéo
        if (itemMenuUI != null && itemMenuUI.IsOpen)
        {
            itemMenuUI.CloseMenu();
        }
        else if (BattleItemMenuUI.Instance != null && BattleItemMenuUI.Instance.IsOpen)
        {
            BattleItemMenuUI.Instance.CloseMenu();
        }

        if (escapeConfirmPanel != null)
        {
            escapeConfirmPanel.SetActive(true);

            // Đưa khung chọn 4 góc vào nút Yes ngay lập tức
            if (BattleKeyboardNavigationUI.Instance != null)
            {
                BattleKeyboardNavigationUI.Instance.FocusEscapeDialog();
            }
        }
    }

    public void OnClickYesEscape()
    {
        Debug.Log("[BattleActionIconsUI] Bấm nút YES thoát trận!");
        if (escapeConfirmPanel != null)
            escapeConfirmPanel.SetActive(false);

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.TryFlee();
        }
        else
        {
            LoadScenesSafely(PlayerPrefs.GetString("SceneBeforeBattle", "GameCore"));
        }
    }

    public static void LoadScenesSafely(string rawSceneString)
    {
        if (string.IsNullOrEmpty(rawSceneString)) rawSceneString = "GameCore";

        string[] scenes = rawSceneString.Split(',');
        string primaryScene = scenes[0].Trim();

        UnityEngine.SceneManagement.SceneManager.LoadScene(primaryScene, UnityEngine.SceneManagement.LoadSceneMode.Single);

        for (int i = 1; i < scenes.Length; i++)
        {
            string subScene = scenes[i].Trim();
            if (!string.IsNullOrEmpty(subScene))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(subScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
        }
    }

    private void OnEscapeConfirmed()
    {
        OnClickYesEscape();
    }

    public void OnClickNoEscape()
    {
        if (escapeConfirmPanel != null)
            escapeConfirmPanel.SetActive(false);

        // Trả tiêu điểm về thanh chính sau khi đóng bảng thoát
        if (BattleKeyboardNavigationUI.Instance != null)
        {
            BattleKeyboardNavigationUI.Instance.BuildMainBarList();
        }
    }

    private void OnEscapeCancelled()
    {
        OnClickNoEscape();
    }

    // ─── Backpack ────────────────────────────────────────────────────

    public void OnBackpackClicked()
    {
        if (IsEscapeConfirmOpen) return;

        if (itemMenuUI != null)
        {
            itemMenuUI.ToggleMenu();
        }
        else if (BattleItemMenuUI.Instance != null)
        {
            BattleItemMenuUI.Instance.ToggleMenu();
        }
    }

    /// <summary>Tat/bat 3 nut (goi khi khong phai luot nguoi choi).</summary>
    public void SetInteractable(bool interactable)
    {
        if (pokeballBtn != null) pokeballBtn.interactable = interactable;
        if (backpackBtn != null) backpackBtn.interactable = interactable;
        if (escapeBtn != null) escapeBtn.interactable = interactable;

        if (!interactable)
        {
            if (itemMenuUI != null && itemMenuUI.IsOpen)
            {
                itemMenuUI.CloseMenu();
            }
            else if (BattleItemMenuUI.Instance != null && BattleItemMenuUI.Instance.IsOpen)
            {
                BattleItemMenuUI.Instance.CloseMenu();
            }
        }
    }
}
