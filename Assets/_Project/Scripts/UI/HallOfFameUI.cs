using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý Màn hình Sảnh Danh Vọng (Hall of Fame) chuẩn Pokémon FireRed.
/// </summary>
public class HallOfFameUI : MonoBehaviour
{
    public static HallOfFameUI Instance { get; private set; }

    [Header("1. Khung Màn Hình Chính")]
    [Tooltip("GameObject chứa toàn bộ Panel Sảnh Danh Vọng")]
    [SerializeField] private GameObject hallOfFamePanel;

    [Header("2. Danh sách 6 Ô Thú Vô Địch")]
    [Tooltip("Kéo lần lượt 6 ô HallOfFameSlotUI vào đây")]
    [SerializeField] private HallOfFameSlotUI[] petSlots = new HallOfFameSlotUI[6];

    [Header("3. Tiêu đề & Lời chúc mừng (TMP / Legacy Text)")]
    [SerializeField] private TextMeshProUGUI titleTextTMP;
    [SerializeField] private Text titleTextLegacy;
    [SerializeField] private TextMeshProUGUI congratsTextTMP;
    [SerializeField] private Text congratsTextLegacy;

    [Header("4. Nút Đóng / Tiếp tục")]
    [SerializeField] private Button closeButton;

    [Header("5. Danh sách UI cần ẩn (Tùy chọn gán thủ công)")]
    [Tooltip("Kéo các Canvas/HUD khác muốn ẩn vào đây (nếu để trống script sẽ tự động ẩn tất cả Canvas khác)")]
    [SerializeField] private GameObject[] manualUiToHide;

    [Header("6. Dữ liệu Player")]
    [SerializeField] private PlayerData playerData;

    private Action _onCompleteCallback;
    private bool _canClose = false;
    private List<GameObject> _hiddenUIObjects = new List<GameObject>();

    public bool IsOpen => hallOfFamePanel != null && hallOfFamePanel.activeInHierarchy;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (hallOfFamePanel != null) hallOfFamePanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseHallOfFame);
        }
    }

    private void Update()
    {
        if (!IsOpen || !_canClose) return;

        // Bấm F, Space, Enter, hoặc ESC để đóng Sảnh Danh Vọng
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.E))
        {
            CloseHallOfFame();
        }
    }

    /// <summary>
    /// Hiển thị Sảnh Danh Vọng với toàn bộ thú trong đội hình.
    /// </summary>
    public void ShowHallOfFame(Action onComplete = null)
    {
        _onCompleteCallback = onComplete;
        _canClose = false;

        // 1. TẠM THỜI ẨN TOÀN BỘ CÁC UI KHÁC (Túi đồ, Huy hiệu, Nút F, Quest, v.v.)
        _hiddenUIObjects.Clear();

        if (manualUiToHide != null)
        {
            foreach (var go in manualUiToHide)
            {
                if (go != null && go.activeSelf)
                {
                    go.SetActive(false);
                    _hiddenUIObjects.Add(go);
                }
            }
        }

        Canvas[] allCanvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in allCanvases)
        {
            if (c == null) continue;
            if (hallOfFamePanel != null && (c.gameObject == hallOfFamePanel || c.transform.IsChildOf(hallOfFamePanel.transform) || hallOfFamePanel.transform.IsChildOf(c.transform)))
                continue;

            if (c.gameObject.activeSelf && !_hiddenUIObjects.Contains(c.gameObject))
            {
                c.gameObject.SetActive(false);
                _hiddenUIObjects.Add(c.gameObject);
            }
        }

        string[] rootUiNames = new string[] { "InteractHintCanvas", "HuyHieu", "PETUI", "DialogueCanvas", "InteractHintManager", "RewardUI_Panel" };
        foreach (string name in rootUiNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null && obj.activeSelf && !_hiddenUIObjects.Contains(obj))
            {
                obj.SetActive(false);
                _hiddenUIObjects.Add(obj);
            }
        }

        if (playerData == null)
            playerData = Resources.Load<PlayerData>("PlayerData");

        if (hallOfFamePanel != null)
        {
            // Đảm bảo nền đen/xanh tím có độ đặc 100% (Alpha = 1) để che kín map
            Image bg = hallOfFamePanel.GetComponent<Image>();
            if (bg != null)
            {
                Color c = bg.color;
                c.a = 1.0f;
                bg.color = c;
            }

            hallOfFamePanel.SetActive(true);
        }

        // 2. Khóa di chuyển người chơi
        var pmc = UnityEngine.Object.FindFirstObjectByType<PlayerMapController>();
        if (pmc != null) pmc.SetCanMove(false);

        // Cập nhật câu chữ vinh danh
        string playerName = (playerData != null && !string.IsNullOrEmpty(playerData.playerName))
            ? playerData.playerName
            : "Nhà Huấn Luyện";

        SetText(titleTextTMP, titleTextLegacy, "SẢNH DANH VỌNG");
        SetText(congratsTextTMP, congratsTextLegacy, $"Chào mừng {playerName} đến sảnh danh vọng!");

        // Điền thú vào các ô slot
        PopulateSlots();

        StartCoroutine(CoEnableCloseAfterDelay());
    }

    public void CloseHallOfFame()
    {
        if (hallOfFamePanel != null)
        {
            hallOfFamePanel.SetActive(false);
        }

        // 1. KHÔI PHỤC LẠI TẤT CẢ CÁC UI ĐÃ ẨN
        foreach (var obj in _hiddenUIObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
        _hiddenUIObjects.Clear();

        // 2. Mở khóa di chuyển người chơi
        var pmc = UnityEngine.Object.FindFirstObjectByType<PlayerMapController>();
        if (pmc != null) pmc.SetCanMove(true);

        _onCompleteCallback?.Invoke();
    }




    private void PopulateSlots()
    {
        if (playerData == null || playerData.currentFormation == null) return;

        var formation = playerData.currentFormation;

        for (int i = 0; i < petSlots.Length; i++)
        {
            if (petSlots[i] == null) continue;

            if (i < formation.Count && formation[i] != null && formation[i].baseBeast != null)
            {
                petSlots[i].Setup(formation[i]);
            }
            else
            {
                petSlots[i].HideSlot();
            }
        }
    }

    private IEnumerator CoEnableCloseAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // Tránh bấm F bị dính từ hội thoại trước
        _canClose = true;
    }

    private void SetText(TextMeshProUGUI tmp, Text legacy, string content)
    {
        if (tmp != null) tmp.text = content;
        if (legacy != null) legacy.text = content;
    }
}
