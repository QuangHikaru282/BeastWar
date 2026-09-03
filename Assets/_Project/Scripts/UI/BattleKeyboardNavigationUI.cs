using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hệ thống điều khiển UI trận đấu 100% bằng bàn phím (WASD + F).
/// - Hàng ngang (Kĩ năng, Balo, Thoát): Dùng phím A / D để chuyển qua lại.
/// - Cột dọc (Danh sách vật phẩm trong Balo): Dùng phím W / S để chuyển lên / xuống.
/// - Phím F: Chọn / Sử dụng.
/// - Phím Esc / B: Hủy / Đóng menu.
/// </summary>
public class BattleKeyboardNavigationUI : MonoBehaviour
{
    public static BattleKeyboardNavigationUI Instance { get; private set; }

    [Header("1. Nút trên thanh chính (Hàng ngang - Dùng A / D)")]
    [Tooltip("Kéo nút Balo vào đây")]
    [SerializeField] private Button backpackButton;

    [Tooltip("Kéo nút Thoát vào đây")]
    [SerializeField] private Button escapeButton;

    [Tooltip("Kéo nút Đổi Pet / Đội hình vào đây")]
    [SerializeField] private Button partyButton;

    [Tooltip("Kéo các nút Kĩ năng vào đây theo thứ tự từ trái qua phải")]
    [SerializeField] private Button[] skillButtons;

    [Header("2. Balo (Cột dọc - Dùng W / S)")]
    [SerializeField] private BattleItemMenuUI itemMenuUI;

    [Header("3. Panel Xác nhận Thoát")]
    [SerializeField] private GameObject escapeConfirmPanel;
    [SerializeField] private Button escapeYesBtn;
    [SerializeField] private Button escapeNoBtn;

    [Header("4. Cài đặt Phím bấm")]
    [SerializeField] private KeyCode confirmKey = KeyCode.F;

    private int mainBarIndex = 0;
    private int backpackSlotIndex = 0;
    private int escapeDialogIndex = 0; // 0 = Yes, 1 = No

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        SelectInitialMainButton();
    }

    private List<Button> GetActiveMainBarButtons()
    {
        List<Button> activeList = new List<Button>();

        if (backpackButton != null && backpackButton.gameObject.activeInHierarchy)
            activeList.Add(backpackButton);

        if (partyButton != null && partyButton.gameObject.activeInHierarchy)
            activeList.Add(partyButton);

        if (escapeButton != null && escapeButton.gameObject.activeInHierarchy)
            activeList.Add(escapeButton);

        if (skillButtons != null)
        {
            foreach (var btn in skillButtons)
            {
                if (btn != null && btn.gameObject.activeInHierarchy)
                {
                    activeList.Add(btn);
                }
            }
        }

        return activeList;
    }

    public void BuildMainBarList()
    {
        var activeList = GetActiveMainBarButtons();
        if (activeList.Count > 0)
        {
            mainBarIndex = Mathf.Clamp(mainBarIndex, 0, activeList.Count - 1);
            FocusMainBarButton(mainBarIndex);
        }
    }

    public void FocusFirstSkill()
    {
        StartCoroutine(CoFocusFirstSkill());
    }

    private IEnumerator CoFocusFirstSkill()
    {
        yield return null; // Đợi frame tiếp theo khi các nút skill đã active và bố trí xong
        var activeList = GetActiveMainBarButtons();
        if (activeList.Count == 0) yield break;

        int targetIndex = -1;
        if (skillButtons != null && skillButtons.Length > 0 && skillButtons[0] != null && skillButtons[0].gameObject.activeInHierarchy)
        {
            targetIndex = activeList.IndexOf(skillButtons[0]);
        }

        if (targetIndex < 0)
        {
            targetIndex = activeList.Count > 2 ? 2 : 0;
        }

        mainBarIndex = targetIndex;
        FocusMainBarButton(mainBarIndex);
    }

    private void SelectInitialMainButton()
    {
        FocusFirstSkill();
    }

    private void Update()
    {
        // Phím P để mở nhanh Bảng Party / Đổi Pet
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (BattlePartyUI.Instance != null && !BattlePartyUI.Instance.IsOpen)
            {
                BattleActionIconsUI.Instance?.OnPartyButtonClicked();
                return;
            }
        }

        // 0. NẾU BẢNG PARTY HOẶC BẢNG HỌC CHIÊU ĐANG MỞ -> Nhường quyền điều khiển
        if (BattlePartyUI.Instance != null && BattlePartyUI.Instance.IsOpen)
        {
            return;
        }

        if (LearnMoveUI.Instance != null && (LearnMoveUI.Instance.IsChoicePanelActive || LearnMoveUI.Instance.IsMoveReplaceScreenActive))
        {
            return;
        }

        // 1. NẾU PANEL XÁC NHẬN THOÁT ĐANG MỞ
        if (escapeConfirmPanel != null && escapeConfirmPanel.activeInHierarchy)
        {
            HandleEscapeDialogNavigation();
            return;
        }

        // 2. NẾU BALO ĐANG MỞ (Cột dọc - Dùng W / S)
        if (itemMenuUI != null && itemMenuUI.IsOpen)
        {
            HandleBackpackNavigation();
            return;
        }

        // 3. THANH CHÍNH TRẬN ĐẤU (Hàng ngang - Dùng A / D)
        HandleMainBarNavigation();
    }



    // ─────────────────────────────────────────────────────────────────────────────
    // 1. THANH CHÍNH (HÀNG NGANG: A / D)
    // ─────────────────────────────────────────────────────────────────────────────
    private void HandleMainBarNavigation()
    {
        var activeList = GetActiveMainBarButtons();
        if (activeList.Count == 0) return;

        if (mainBarIndex >= activeList.Count) mainBarIndex = activeList.Count - 1;
        if (mainBarIndex < 0) mainBarIndex = 0;

        // Sang trái (A)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            mainBarIndex--;
            if (mainBarIndex < 0) mainBarIndex = activeList.Count - 1;
            FocusMainBarButton(mainBarIndex);
        }
        // Sang phải (D)
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            mainBarIndex++;
            if (mainBarIndex >= activeList.Count) mainBarIndex = 0;
            FocusMainBarButton(mainBarIndex);
        }

        // Nút F để chọn
        if (Input.GetKeyDown(confirmKey) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (mainBarIndex >= 0 && mainBarIndex < activeList.Count)
            {
                Button selectedBtn = activeList[mainBarIndex];
                if (selectedBtn != null && selectedBtn.interactable)
                {
                    selectedBtn.onClick.Invoke();

                    if (selectedBtn == backpackButton)
                    {
                        FocusFirstBackpackSlot();
                    }
                    else if (selectedBtn == escapeButton)
                    {
                        FocusEscapeDialog();
                    }
                }
            }
        }
    }

    public void FocusMainBarButton(int index)
    {
        var activeList = GetActiveMainBarButtons();
        if (index >= 0 && index < activeList.Count)
        {
            Button btn = activeList[index];
            if (btn != null && UISelectionCursor.Instance != null)
            {
                UISelectionCursor.Instance.MoveTo(btn.GetComponent<RectTransform>());
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 2. BALO VẬT PHẨM (CỘT DỌC: W / S)
    // ─────────────────────────────────────────────────────────────────────────────
    public void FocusFirstBackpackSlot()
    {
        backpackSlotIndex = 0;
        StopAllCoroutines();
        StartCoroutine(CoFocusBackpackSlotNextFrame(backpackSlotIndex));
    }

    private IEnumerator CoFocusBackpackSlotNextFrame(int index)
    {
        yield return null;
        FocusBackpackSlot(index);
    }

    private void HandleBackpackNavigation()
    {
        var slots = itemMenuUI.SpawnedSlots;
        if (slots == null || slots.Count == 0) return;

        // Đi lên (W)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            backpackSlotIndex--;
            if (backpackSlotIndex < 0) backpackSlotIndex = slots.Count - 1;
            FocusBackpackSlot(backpackSlotIndex);
        }
        // Đi xuống (S)
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            backpackSlotIndex++;
            if (backpackSlotIndex >= slots.Count) backpackSlotIndex = 0;
            FocusBackpackSlot(backpackSlotIndex);
        }

        // Chọn vật phẩm (Phím F)
        if (Input.GetKeyDown(confirmKey) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (backpackSlotIndex >= 0 && backpackSlotIndex < slots.Count)
            {
                GameObject slotObj = slots[backpackSlotIndex];
                if (slotObj != null)
                {
                    Button btn = slotObj.GetComponent<Button>() ?? slotObj.GetComponentInChildren<Button>();
                    if (btn != null && btn.interactable)
                    {
                        btn.onClick.Invoke();
                    }
                }
            }
        }
    }

    private void FocusBackpackSlot(int index)
    {
        if (itemMenuUI == null) return;
        var slots = itemMenuUI.SpawnedSlots;
        if (slots == null || slots.Count == 0) return;

        if (index >= 0 && index < slots.Count)
        {
            GameObject slotObj = slots[index];
            if (slotObj != null && UISelectionCursor.Instance != null)
            {
                UISelectionCursor.Instance.MoveTo(slotObj.GetComponent<RectTransform>());
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 3. DIALOG XÁC NHẬN THOÁT (HÀNG NGANG: A / D -> Mặc định chọn YES)
    // ─────────────────────────────────────────────────────────────────────────────
    public void FocusEscapeDialog()
    {
        escapeDialogIndex = 0; // Mặc định đưa khung chọn vào nút YES
        StopAllCoroutines();
        StartCoroutine(CoFocusEscapeDialogNextFrame(escapeDialogIndex));
    }

    private IEnumerator CoFocusEscapeDialogNextFrame(int index)
    {
        yield return null;
        FocusEscapeButton(index);
    }

    private void HandleEscapeDialogNavigation()
    {
        // Đổi qua lại giữa Yes (0) và No (1)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) || 
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            escapeDialogIndex = (escapeDialogIndex == 0) ? 1 : 0;
            FocusEscapeButton(escapeDialogIndex);
        }

        // Bấm F để chọn Yes hoặc No
        if (Input.GetKeyDown(confirmKey) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            Button targetBtn = (escapeDialogIndex == 0) ? escapeYesBtn : escapeNoBtn;
            if (targetBtn != null)
            {
                targetBtn.onClick.Invoke();
            }
        }
    }

    private void FocusEscapeButton(int index)
    {
        Button targetBtn = (index == 0) ? escapeYesBtn : escapeNoBtn;
        if (targetBtn != null && UISelectionCursor.Instance != null)
        {
            UISelectionCursor.Instance.MoveTo(targetBtn.GetComponent<RectTransform>());
        }
    }
}
