using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý Màn hình Đổi Pet ra trận (Battle Party Switch Screen) chuẩn phong cách Pokémon FireRed.
/// </summary>
public class BattlePartyUI : MonoBehaviour
{
    public static BattlePartyUI Instance { get; private set; }

    [Header("1. Khung Màn Hình Chính")]
    [Tooltip("GameObject chứa toàn bộ Panel Party")]
    [SerializeField] private GameObject partyPanel;

    [Header("2. Các Ô Thú (1 Ô Chính Trái + 5 Ô Dự Bị Phải)")]
    [Tooltip("Ô lớn bên trái - Thú đang trên sân")]
    [SerializeField] private BattlePartySlotUI activePetSlot;

    [Tooltip("5 Ô bên phải - Thú dự bị")]
    [SerializeField] private BattlePartySlotUI[] benchSlots = new BattlePartySlotUI[5];

    [Header("3. Nút Hủy (Góc dưới phải)")]
    [SerializeField] private Button cancelButton;

    [Header("4. Hộp Thoại Hướng Dẫn (Góc dưới trái)")]
    [SerializeField] private GameObject promptDialoguePanel;
    [SerializeField] private TextMeshProUGUI promptTextTMP;
    [SerializeField] private Text promptTextLegacy;

    [Header("5. Dữ liệu Player")]
    [SerializeField] private PlayerData playerData;

    // Runtime state
    private Action<RuntimeBeastData> _onPetSelectedCallback;
    private Action _onCancelCallback;
    private bool _isForcedSwitch = false; // Bắt buộc phải đổi (khi thú bị hạ gục)
    private int _currentNavigationIndex = 0; // 0 = Active slot trái, 1..5 = Bench slots phải, 6 = Nút Hủy

    public bool IsOpen => partyPanel != null && partyPanel.activeInHierarchy;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (partyPanel != null) partyPanel.SetActive(false);

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }
    }

    private void Update()
    {
        if (!IsOpen) return;

        // Điều hướng chọn thú chính (WASD / Mũi tên)
        HandlePartyNavigation();
    }


    // ─── PUBLIC API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Mở Bảng Party để chọn đổi thú.
    /// </summary>
    /// <param name="onSelected">Callback khi chọn đổi thú thành công</param>
    /// <param name="onCancel">Callback khi người chơi bấm Hủy</param>
    /// <param name="isForced">True nếu bắt buộc phải đổi (khi thú trên sân fainted)</param>
    public void OpenParty(Action<RuntimeBeastData> onSelected, Action onCancel = null, bool isForced = false)
    {
        _onPetSelectedCallback = onSelected;
        _onCancelCallback = onCancel;
        _isForcedSwitch = isForced;

        if (playerData == null)
            playerData = Resources.Load<PlayerData>("PlayerData");

        if (partyPanel != null) partyPanel.SetActive(true);

        // Khóa hoặc ẩn nút Hủy nếu là lượt bắt buộc phải đổi
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(!_isForcedSwitch);
        }


        RefreshSlots();
        SetPrompt("Chọn một POKéMON.");

        // Mặc định focus vào ô đầu tiên của hàng dự bị (Slot 1) hoặc ô chính
        _currentNavigationIndex = 1;
        StartCoroutine(CoFocusInitialSlot());
    }

    public void CloseParty()
    {
        if (partyPanel != null) partyPanel.SetActive(false);
    }


    // ─── REFRESH SLOTS ──────────────────────────────────────────────────────

    private void RefreshSlots()
    {
        if (playerData == null || playerData.currentFormation == null) return;

        var formation = playerData.currentFormation;

        // Ô chính bên trái (Slot 0 - Thú đang trên sân)
        RuntimeBeastData activeBeast = formation.Count > 0 ? formation[0] : null;
        if (activePetSlot != null)
        {
            activePetSlot.Setup(activeBeast, true);
        }

        // 5 Ô phụ bên phải (Slot 1 -> 5)
        for (int i = 0; i < benchSlots.Length; i++)
        {
            int formationIndex = i + 1;
            RuntimeBeastData benchBeast = formationIndex < formation.Count ? formation[formationIndex] : null;

            if (benchSlots[i] != null)
            {
                benchSlots[i].Setup(benchBeast, false);
            }
        }
    }

    private IEnumerator CoFocusInitialSlot()
    {
        yield return null;
        // Nếu slot dự bị 1 có thú, focus slot 1, ngược lại focus slot 0
        if (benchSlots.Length > 0 && benchSlots[0] != null && !benchSlots[0].IsEmpty)
        {
            FocusSlot(1);
        }
        else
        {
            FocusSlot(0);
        }
    }

    // ─── NAVIGATION (WASD + MŨI TÊN + F) ────────────────────────────────────

    private void HandlePartyNavigation()
    {
        // 1. Phím LÊN (W / UpArrow)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_currentNavigationIndex == 6) // Đang ở nút Hủy
            {
                _currentNavigationIndex = GetLastValidBenchSlotIndex();
            }
            else if (_currentNavigationIndex >= 2 && _currentNavigationIndex <= 5)
            {
                _currentNavigationIndex--;
            }
            else if (_currentNavigationIndex == 1)
            {
                _currentNavigationIndex = 0; // Từ slot 1 nhảy sang slot chính bên trái
            }
            FocusSlot(_currentNavigationIndex);
        }
        // 2. Phím XUỐNG (S / DownArrow)
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_currentNavigationIndex == 0)
            {
                _currentNavigationIndex = 1;
            }
            else if (_currentNavigationIndex >= 1 && _currentNavigationIndex < 5)
            {
                _currentNavigationIndex++;
            }
            else if (_currentNavigationIndex == 5 || _currentNavigationIndex == GetLastValidBenchSlotIndex())
            {
                if (!_isForcedSwitch && cancelButton != null && cancelButton.gameObject.activeInHierarchy)
                    _currentNavigationIndex = 6; // Nhảy xuống nút Hủy
            }
            FocusSlot(_currentNavigationIndex);
        }
        // 3. Phím TRÁI (A / LeftArrow)
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _currentNavigationIndex = 0; // Luôn nhảy sang ô chính bên trái
            FocusSlot(_currentNavigationIndex);
        }
        // 4. Phím PHẢI (D / RightArrow)
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_currentNavigationIndex == 0)
            {
                _currentNavigationIndex = 1; // Từ ô chính nhảy sang ô phụ đầu tiên
            }
            else if (_currentNavigationIndex >= 4 && !_isForcedSwitch && cancelButton != null && cancelButton.gameObject.activeInHierarchy)
            {
                _currentNavigationIndex = 6; // Nhảy sang nút Hủy
            }
            FocusSlot(_currentNavigationIndex);
        }

        // 5. Phím CHỌN (F / Space / Enter)
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.E))
        {
            SelectCurrentSlot();
        }

        // 6. Phím HỦY (ESC / B / X)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.X))
        {
            OnCancelButtonClicked();
        }
    }

    private void FocusSlot(int index)
    {
        _currentNavigationIndex = Mathf.Clamp(index, 0, 6);

        // Bỏ chọn tất cả outline
        if (activePetSlot != null) activePetSlot.SetSelected(false);
        foreach (var s in benchSlots) if (s != null) s.SetSelected(false);

        RectTransform targetRect = null;

        if (_currentNavigationIndex == 0)
        {
            if (activePetSlot != null)
            {
                activePetSlot.SetSelected(true);
                targetRect = activePetSlot.GetComponent<RectTransform>();
            }
        }
        else if (_currentNavigationIndex >= 1 && _currentNavigationIndex <= 5)
        {
            int benchIdx = _currentNavigationIndex - 1;
            if (benchIdx < benchSlots.Length && benchSlots[benchIdx] != null)
            {
                benchSlots[benchIdx].SetSelected(true);
                targetRect = benchSlots[benchIdx].GetComponent<RectTransform>();
            }
        }
        else if (_currentNavigationIndex == 6) // Nút Hủy
        {
            if (cancelButton != null)
            {
                targetRect = cancelButton.GetComponent<RectTransform>();
            }
        }

        if (targetRect != null && UISelectionCursor.Instance != null)
        {
            UISelectionCursor.Instance.MoveTo(targetRect);
        }
    }

    private void SelectCurrentSlot()
    {
        if (_currentNavigationIndex == 6)
        {
            OnCancelButtonClicked();
            return;
        }

        BattlePartySlotUI selectedSlot = null;
        if (_currentNavigationIndex == 0) selectedSlot = activePetSlot;
        else if (_currentNavigationIndex >= 1 && _currentNavigationIndex <= 5)
        {
            int idx = _currentNavigationIndex - 1;
            if (idx < benchSlots.Length) selectedSlot = benchSlots[idx];
        }

        if (selectedSlot == null || selectedSlot.IsEmpty)
        {
            SetPrompt("Ô này không có POKéMON!");
            return;
        }

        RuntimeBeastData beast = selectedSlot.BeastData;

        // 1. Nếu là con đang trên sân
        if (selectedSlot.IsCurrentlyOnField)
        {
            SetPrompt($"{beast.baseBeast.beastName} đã ở trên sân rồi!");
            return;
        }

        // 2. Nếu con này đã hết máu
        if (selectedSlot.IsFainted)
        {
            SetPrompt($"{beast.baseBeast.beastName} không còn sức chiến đấu!");
            return;
        }

        // 3. Hợp lệ -> Đổi Pet ngay lập tức
        CloseParty();
        _onPetSelectedCallback?.Invoke(beast);
    }

    private void OnCancelButtonClicked()
    {
        if (_isForcedSwitch)
        {
            SetPrompt("Bạn phải chọn một POKéMON để tiếp tục!");
            return;
        }

        CloseParty();
        _onCancelCallback?.Invoke();
    }

    private int GetLastValidBenchSlotIndex()
    {
        for (int i = benchSlots.Length - 1; i >= 0; i--)
        {
            if (benchSlots[i] != null && !benchSlots[i].IsEmpty)
                return i + 1;
        }
        return 1;
    }

    private void SetPrompt(string msg)
    {
        if (promptTextTMP != null) promptTextTMP.text = msg;
        if (promptTextLegacy != null) promptTextLegacy.text = msg;
    }
}

