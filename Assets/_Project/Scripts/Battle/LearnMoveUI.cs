using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Màn hình Học Chiêu / Bỏ Chiêu chuẩn 100% Pokémon FireRed (Ảnh 2, 3, 4, 5).
/// 
/// Luồng chuẩn:
///   1. Hộp thoại: "[Tên Thú] đang cố gắng học [Chiêu Mới]."
///   2. Hộp thoại: "Nhưng, [Tên Thú] ko thể học nhiều hơn bốn chiêu."
///   3. Lựa chọn: "Bỏ đi một chiêu để có chỗ cho [Chiêu Mới]?" -> [Ok / Ko]
///   4. Nếu Ok -> Mở Màn hình Chọn Chiêu (Ảnh 5):
///      - Cột trái: Mini Sprite, Tên thú, Hệ thú, Bảng Sức mạnh / Chính xác / Mô tả chiêu đang chọn.
///      - Cột phải: 4 Chiêu cũ + 1 Chiêu mới ở ô thứ 5.
///      - Click 1 trong 4 chiêu cũ -> Quên chiêu đó và học chiêu mới!
/// </summary>
public class LearnMoveUI : MonoBehaviour
{
    private static LearnMoveUI _instance;
    public static LearnMoveUI Instance => _instance;

    [Header("--- 1. HỘP THOẠI TRONG TRẬN (Ảnh 2, 3, 4) ---")]
    [SerializeField] private GameObject dialoguePanel;
    [Tooltip("Kéo cụm 4 nút CombatButtons vào đây để tự động ẩn khi hiện hội thoại học chiêu")]
    [SerializeField] private GameObject combatButtons;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Text dialogueTextLegacy; // Hỗ trợ Text thường
    [SerializeField] private GameObject choicePanel; // Hộp chọn Ok / Ko
    [SerializeField] private Button choiceOkBtn;
    [SerializeField] private Button choiceNoBtn;

    [Header("--- 2. MÀN HÌNH CHỌN CHIÊU THAY THẾ (Ảnh 5) ---")]
    [SerializeField] private GameObject moveReplaceScreen;

    [Header("Thông tin Thú (Góc trên bên trái)")]
    [SerializeField] private Image beastMiniSprite;
    [SerializeField] private TextMeshProUGUI beastNameText;
    [SerializeField] private Text beastNameTextLegacy;
    [SerializeField] private TextMeshProUGUI type1Text;
    [SerializeField] private Text type1TextLegacy;
    [SerializeField] private GameObject type2Container;
    [SerializeField] private TextMeshProUGUI type2Text;
    [SerializeField] private Text type2TextLegacy;

    [Header("Chi tiết Chiêu đang chọn (Góc dưới bên trái)")]
    [SerializeField] private TextMeshProUGUI detailPowerText;
    [SerializeField] private Text detailPowerTextLegacy;
    [SerializeField] private TextMeshProUGUI detailAccuracyText;
    [SerializeField] private Text detailAccuracyTextLegacy;
    [SerializeField] private TextMeshProUGUI detailDescText;
    [SerializeField] private Text detailDescTextLegacy;

    [Header("4 Ô Chiêu Cũ (Cột phải - Trên)")]
    [SerializeField] private Button[] oldMoveButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI[] oldMoveElementTexts = new TextMeshProUGUI[4];
    [SerializeField] private Text[] oldMoveElementTextsLegacy = new Text[4];
    [SerializeField] private TextMeshProUGUI[] oldMoveNameTexts = new TextMeshProUGUI[4];
    [SerializeField] private Text[] oldMoveNameTextsLegacy = new Text[4];
    [SerializeField] private TextMeshProUGUI[] oldMovePPTexts = new TextMeshProUGUI[4];
    [SerializeField] private Text[] oldMovePPTextsLegacy = new Text[4];
    [SerializeField] private GameObject[] oldMoveOutlines = new GameObject[4];

    [Header("Ô Chiêu Mới Muốn Học (Cột phải - Ô thứ 5 dưới cùng)")]
    [SerializeField] private Button newMoveButton;
    [SerializeField] private TextMeshProUGUI newMoveElementText;
    [SerializeField] private Text newMoveElementTextLegacy;
    [SerializeField] private TextMeshProUGUI newMoveNameText;
    [SerializeField] private Text newMoveNameTextLegacy;
    [SerializeField] private TextMeshProUGUI newMovePPText;
    [SerializeField] private Text newMovePPTextLegacy;
    [SerializeField] private GameObject newMoveOutline;

    [Header("Nút Hủy / Bỏ qua")]
    [SerializeField] private Button cancelButton;

    // ─── Runtime ────────────────────────────────────────────────────────────
    private RuntimeBeastData _currentBeast;
    private MoveData _newMove;
    private Action _onAllDone;
    private bool _dialogueClicked = false;
    private int _userChoice = -1; // 1 = Ok, 0 = Ko

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;

        // Chỉ tắt ChoicePanel và MoveReplaceScreen
        if (choicePanel != null) choicePanel.SetActive(false);
        if (moveReplaceScreen != null) moveReplaceScreen.SetActive(false);

        if (choiceOkBtn != null) choiceOkBtn.onClick.AddListener(() => _userChoice = 1);
        if (choiceNoBtn != null) choiceNoBtn.onClick.AddListener(() => _userChoice = 0);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private int _replaceSlotIndex = 0;


    private void Update()
    {
        // 1. Nhấn F, Space, Enter, Z, E hoặc Click chuột để qua câu thoại
        if (_waitingForDialogueClick)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
                Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.E))
            {
                _dialogueClicked = true;
            }
            return;
        }

        // 2. Điều hướng bàn phím trên Màn hình chọn chiêu thay thế (5 ô chiêu)
        if (moveReplaceScreen != null && moveReplaceScreen.activeInHierarchy)
        {
            // Di chuyển Lên (W / UpArrow)
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                int next = _replaceSlotIndex - 1;
                if (next < 0) next = 4;
                FocusReplaceSlot(next);
            }
            // Di chuyển Xuống (S / DownArrow)
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                int next = _replaceSlotIndex + 1;
                if (next > 4) next = 0;
                FocusReplaceSlot(next);
            }

            // Nhấn F, Space, Enter để CHỌN CHIÊU ĐỔI
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.E))
            {
                if (_replaceSlotIndex < 4)
                {
                    OnOldMoveSelected(_replaceSlotIndex);
                }
                else
                {
                    OnNewMoveSlotSelected();
                }
            }

            // Bấm phím ESC (hoặc B / X) trên bàn phím để Hủy / Quay lại
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.X))
            {
                OnCancelClicked();
            }
        }
    }

    public bool IsChoicePanelActive => choicePanel != null && choicePanel.activeInHierarchy;
    public bool IsMoveReplaceScreenActive => moveReplaceScreen != null && moveReplaceScreen.activeInHierarchy;


    // ─── PUBLIC API ─────────────────────────────────────────────────────────

    public void ProcessQueue(Action onAllDone)
    {
        _onAllDone = onAllDone;
        StartCoroutine(DrainQueueRoutine());
    }

    // ─── MAIN COROUTINE ─────────────────────────────────────────────────────

    private IEnumerator DrainQueueRoutine()
    {
        // Ẩn 4 nút chọn chiêu đánh và các icon Balo/Thoát để nhường chỗ cho hội thoại học chiêu
        if (combatButtons != null) combatButtons.SetActive(false);
        if (BattleActionIconsUI.Instance != null) BattleActionIconsUI.Instance.gameObject.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        while (LearnMoveQueue.TryDequeue(out var request))
        {
            _currentBeast = request.beast;
            _newMove = request.newMove;

            if (_currentBeast == null || _newMove == null) continue;

            int currentMoveCount = 0;
            if (_currentBeast.moves != null)
            {
                foreach (var m in _currentBeast.moves)
                    if (m != null && m.baseMove != null) currentMoveCount++;
            }

            if (currentMoveCount < 4)
            {
                // ── TRƯỜNG HỢP A: Còn chỗ trống -> Tự động học luôn ────────────────
                AutoLearnMove(_currentBeast, _newMove);
                yield return ShowDialogue($"{_currentBeast.baseBeast.beastName} đã học được {_newMove.moveName}!");
            }
            else
            {
                // ── TRƯỜNG HỢP B: Đã đủ 4 chiêu -> Chạy kịch bản FireRed ───────────
                // 1. [Tên] đang cố gắng học [Chiêu]
                yield return ShowDialogue($"{_currentBeast.baseBeast.beastName} đang cố gắng học {_newMove.moveName}.");

                // 2. Nhưng ko thể học nhiều hơn 4 chiêu
                yield return ShowDialogue($"Nhưng, {_currentBeast.baseBeast.beastName} ko thể học nhiều hơn bốn chiêu.");

                // 3. Bỏ đi một chiêu để có chỗ cho [Chiêu]? [Ok / Ko]
                bool openReplaceScreen = false;
                yield return ShowChoiceDialogue($"Bỏ đi một chiêu để có chỗ cho {_newMove.moveName}?", (choseOk) => {
                    openReplaceScreen = choseOk;
                });

                if (openReplaceScreen)
                {
                    // 4. Nếu ấn OK -> Mở màn hình chọn chiêu thay thế 5 ô
                    bool replaceCompleted = false;
                    ShowMoveReplaceScreen(_currentBeast, _newMove, () => replaceCompleted = true);
                    yield return new WaitUntil(() => replaceCompleted);
                }
                else
                {
                    // Nếu ấn KO -> Bỏ qua
                    yield return ShowDialogue($"{_currentBeast.baseBeast.beastName} đã không học {_newMove.moveName}.");
                }
            }
        }

        // Tắt choice panel và move replace screen, dọn text hội thoại
        if (choicePanel != null) choicePanel.SetActive(false);
        if (moveReplaceScreen != null) moveReplaceScreen.SetActive(false);
        SetText(dialogueText, dialogueTextLegacy, "");

        _onAllDone?.Invoke();
    }

    // ─── DIALOGUE HELPERS ───────────────────────────────────────────────────

    private bool _waitingForDialogueClick = false;

    private IEnumerator ShowDialogue(string text)
    {
        // Đảm bảo hộp thoại trắng luôn hiện rõ và ẩn 4 nút Attack + Icon Balo/Thoát
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (combatButtons != null) combatButtons.SetActive(false);
        if (BattleActionIconsUI.Instance != null) BattleActionIconsUI.Instance.gameObject.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        SetText(dialogueText, dialogueTextLegacy, text);

        _dialogueClicked = false;
        _waitingForDialogueClick = true;
        yield return new WaitForSeconds(0.2f); // Chống click nhầm từ frame trước
        yield return new WaitUntil(() => _dialogueClicked);
        _waitingForDialogueClick = false;
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator ShowChoiceDialogue(string question, Action<bool> onResult)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (combatButtons != null) combatButtons.SetActive(false);
        if (BattleActionIconsUI.Instance != null) BattleActionIconsUI.Instance.gameObject.SetActive(false);
        SetText(dialogueText, dialogueTextLegacy, question);

        if (choicePanel != null) choicePanel.SetActive(true);
        _userChoice = -1;
        int currentChoice = 1; // 1 = OK (nút dưới), 0 = KO (nút trên)


        yield return null; // Chờ frame tiếp theo để UI active

        // Đưa khung chọn 4 góc vào nút OK ngay khi hiện bảng
        if (choiceOkBtn != null && UISelectionCursor.Instance != null)
        {
            UISelectionCursor.Instance.MoveTo(choiceOkBtn.GetComponent<RectTransform>());
        }

        while (_userChoice == -1)
        {
            // Chỉ cho phép di chuyển LÊN / XUỐNG giữa đúng 2 ô OK và KO (W / S / Up / Down / A / D)
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentChoice = (currentChoice == 1) ? 0 : 1; // Đổi qua lại giữa OK và KO

                Button focusBtn = (currentChoice == 1) ? choiceOkBtn : choiceNoBtn;
                if (focusBtn != null && UISelectionCursor.Instance != null)
                {
                    UISelectionCursor.Instance.MoveTo(focusBtn.GetComponent<RectTransform>());
                }
            }

            // Nhấn F, Space, Enter để Xác nhận lựa chọn
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.E))
            {
                _userChoice = currentChoice;
            }

            yield return null;
        }

        if (choicePanel != null) choicePanel.SetActive(false);
        onResult?.Invoke(_userChoice == 1);
    }


    // ─── MOVE REPLACE SCREEN (Ảnh 5) ────────────────────────────────────────

    private Action _onReplaceScreenDone;

    private void ShowMoveReplaceScreen(RuntimeBeastData beast, MoveData newMove, Action onDone)
    {
        _onReplaceScreenDone = onDone;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (moveReplaceScreen != null) moveReplaceScreen.SetActive(true);

        // 1. Gán thông tin Thú
        if (beastMiniSprite != null && beast.baseBeast.frontSprite != null)
            beastMiniSprite.sprite = beast.baseBeast.frontSprite;
        SetText(beastNameText, beastNameTextLegacy, beast.baseBeast.beastName);
        SetText(type1Text, type1TextLegacy, beast.baseBeast.element.ToString().ToUpper());
        if (type2Container != null)
        {
            bool hasSecond = beast.baseBeast.secondaryElement != BeastElement.Normal && beast.baseBeast.secondaryElement != beast.baseBeast.element;
            type2Container.SetActive(hasSecond);
            if (hasSecond) SetText(type2Text, type2TextLegacy, beast.baseBeast.secondaryElement.ToString().ToUpper());
        }

        // 2. Gán 4 chiêu cũ
        var moves = beast.moves ?? new RuntimeMoveData[0];
        for (int i = 0; i < 4; i++)
        {
            int slotIdx = i;
            bool hasMove = i < moves.Length && moves[i] != null && moves[i].baseMove != null;
            Button btn = (oldMoveButtons != null && i < oldMoveButtons.Length) ? oldMoveButtons[i] : null;

            if (btn != null)
            {
                btn.interactable = hasMove;
                btn.onClick.RemoveAllListeners();
                if (hasMove)
                {
                    btn.onClick.AddListener(() => OnOldMoveSelected(slotIdx));
                }
            }

            var tmpElem = (oldMoveElementTexts != null && i < oldMoveElementTexts.Length) ? oldMoveElementTexts[i] : null;
            var legElem = (oldMoveElementTextsLegacy != null && i < oldMoveElementTextsLegacy.Length) ? oldMoveElementTextsLegacy[i] : null;
            var tmpName = (oldMoveNameTexts != null && i < oldMoveNameTexts.Length) ? oldMoveNameTexts[i] : null;
            var legName = (oldMoveNameTextsLegacy != null && i < oldMoveNameTextsLegacy.Length) ? oldMoveNameTextsLegacy[i] : null;
            var tmpPP = (oldMovePPTexts != null && i < oldMovePPTexts.Length) ? oldMovePPTexts[i] : null;
            var legPP = (oldMovePPTextsLegacy != null && i < oldMovePPTextsLegacy.Length) ? oldMovePPTextsLegacy[i] : null;

            if (hasMove)
            {
                var m = moves[i];
                PopulateMoveSlotUI(btn, tmpElem, legElem, tmpName, legName, tmpPP, legPP,
                    m.baseMove.moveElement.ToString().ToUpper(),
                    m.baseMove.moveName,
                    $"PP {m.currentPP}/{m.MaxPP}");
            }
            else
            {
                PopulateMoveSlotUI(btn, tmpElem, legElem, tmpName, legName, tmpPP, legPP,
                    "---", "---", "---");
            }
        }

        // 3. Gán chiêu mới ở ô thứ 5
        Button newBtn = newMoveButton;
        if (newBtn == null && oldMoveButtons != null && oldMoveButtons.Length > 0 && oldMoveButtons[0] != null)
        {
            Transform parent = oldMoveButtons[0].transform.parent;
            if (parent != null)
            {
                Transform btn5Tr = parent.Find("Btn_OldMove_0 (4)") ?? parent.Find("Btn_NewMove");
                if (btn5Tr != null) newBtn = btn5Tr.GetComponent<Button>();
            }
        }

        if (newBtn != null)
        {
            newBtn.onClick.RemoveAllListeners();
            newBtn.onClick.AddListener(OnNewMoveSlotSelected);
        }
        PopulateMoveSlotUI(newBtn, newMoveElementText, newMoveElementTextLegacy,
            newMoveNameText, newMoveNameTextLegacy,
            newMovePPText, newMovePPTextLegacy,
            newMove.moveElement.ToString().ToUpper(),
            newMove.moveName,
            $"PP {newMove.maxPP}/{newMove.maxPP}");

        // Mặc định đưa khung chọn vào ô chiêu đầu tiên
        FocusReplaceSlot(0);
    }

    private void FocusReplaceSlot(int index)
    {
        _replaceSlotIndex = Mathf.Clamp(index, 0, 4);

        Button targetBtn = null;
        MoveData targetMove = null;

        if (_replaceSlotIndex < 4)
        {
            targetBtn = (oldMoveButtons != null && _replaceSlotIndex < oldMoveButtons.Length) ? oldMoveButtons[_replaceSlotIndex] : null;
            if (_currentBeast != null && _currentBeast.moves != null && _replaceSlotIndex < _currentBeast.moves.Length && _currentBeast.moves[_replaceSlotIndex] != null)
            {
                targetMove = _currentBeast.moves[_replaceSlotIndex].baseMove;
            }
        }
        else
        {
            targetBtn = newMoveButton;
            if (targetBtn == null && oldMoveButtons != null && oldMoveButtons.Length > 0 && oldMoveButtons[0] != null)
            {
                Transform parent = oldMoveButtons[0].transform.parent;
                if (parent != null)
                {
                    Transform btn5Tr = parent.Find("Btn_OldMove_0 (4)") ?? parent.Find("Btn_NewMove");
                    if (btn5Tr != null) targetBtn = btn5Tr.GetComponent<Button>();
                }
            }
            targetMove = _newMove;
        }

        // Di chuyển khung chọn 4 góc
        if (targetBtn != null && UISelectionCursor.Instance != null)
        {
            UISelectionCursor.Instance.MoveTo(targetBtn.GetComponent<RectTransform>());
        }

        // Cập nhật thông tin chiêu bên trái theo ô đang chọn
        if (targetMove != null)
        {
            HighlightMoveDetail(targetMove);
        }

        // Bật outline của ô đó nếu có
        SetOutlines(_replaceSlotIndex);
    }



    private void PopulateMoveSlotUI(Button btn, TextMeshProUGUI tmpElem, Text legElem,
                                   TextMeshProUGUI tmpName, Text legName,
                                   TextMeshProUGUI tmpPP, Text legPP,
                                   string elemStr, string nameStr, string ppStr)
    {
        bool hasName = (tmpName != null || legName != null);
        bool hasElem = (tmpElem != null || legElem != null);
        bool hasPP = (tmpPP != null || legPP != null);

        if (hasName) SetText(tmpName, legName, nameStr);
        if (hasElem) SetText(tmpElem, legElem, elemStr);
        if (hasPP) SetText(tmpPP, legPP, ppStr);

        // Tự động tìm Text con bên trong Button nếu chưa được kéo vào Inspector
        if (btn != null && (!hasName || !hasElem || !hasPP))
        {
            var tmps = btn.GetComponentsInChildren<TextMeshProUGUI>(true);
            var legs = btn.GetComponentsInChildren<Text>(true);

            if (tmps != null && tmps.Length > 0)
            {
                foreach (var t in tmps)
                {
                    string n = t.gameObject.name.ToLower();
                    if (!hasElem && (n.Contains("elem") || n.Contains("he") || t.text.Contains("Hệ"))) t.text = elemStr;
                    else if (!hasPP && (n.Contains("pp") || t.text.Contains("PP"))) t.text = ppStr;
                    else if (!hasName) t.text = nameStr;
                }
            }

            if (legs != null && legs.Length > 0)
            {
                foreach (var t in legs)
                {
                    string n = t.gameObject.name.ToLower();
                    if (!hasElem && (n.Contains("elem") || n.Contains("he") || t.text.Contains("Hệ"))) t.text = elemStr;
                    else if (!hasPP && (n.Contains("pp") || t.text.Contains("PP"))) t.text = ppStr;
                    else if (!hasName) t.text = nameStr;
                }
            }
        }
    }


    private void HighlightMoveDetail(MoveData move)
    {
        if (move != null)
        {
            SetText(detailPowerText, detailPowerTextLegacy, move.power > 0 ? move.power.ToString() : "---");
            SetText(detailAccuracyText, detailAccuracyTextLegacy, move.accuracy > 0 ? move.accuracy.ToString() : "---");
            SetText(detailDescText, detailDescTextLegacy, move.description);
        }
    }

    private void SetText(TextMeshProUGUI tmp, Text legacy, string content)
    {
        if (tmp != null) tmp.text = content;
        if (legacy != null) legacy.text = content;
    }

    private void OnOldMoveSelected(int slotIndex)
    {
        // Khi click 1 trong 4 chiêu cũ -> Hiển thị chi tiết và xác nhận thay thế
        if (_currentBeast.moves != null && slotIndex < _currentBeast.moves.Length && _currentBeast.moves[slotIndex] != null)
        {
            HighlightMoveDetail(_currentBeast.moves[slotIndex].baseMove);

            // Bật Outline
            SetOutlines(slotIndex);

            // Thực hiện thay chiêu
            string oldMoveName = _currentBeast.moves[slotIndex].baseMove.moveName;
            _currentBeast.moves[slotIndex] = new RuntimeMoveData(_newMove, 1);

            if (moveReplaceScreen != null) moveReplaceScreen.SetActive(false);

            StartCoroutine(ShowForgetSuccessRoutine(oldMoveName, _newMove.moveName));
        }
    }

    private void OnNewMoveSlotSelected()
    {
        // Click ô thứ 5 -> Chỉ xem chi tiết chiêu mới
        HighlightMoveDetail(_newMove);
        SetOutlines(4); // 4 = ô chiêu mới
    }

    private void OnCancelClicked()
    {
        // Người chơi bấm Hủy ở màn hình chọn chiêu
        if (moveReplaceScreen != null) moveReplaceScreen.SetActive(false);
        StartCoroutine(ShowCancelRoutine());
    }

    private IEnumerator ShowForgetSuccessRoutine(string oldName, string newName)
    {
        // Thông báo kinh điển chuẩn Pokemon FireRed
        yield return ShowDialogue($"1, 2 và... bùm!");
        yield return ShowDialogue($"{_currentBeast.baseBeast.beastName} đã quên {oldName}...");
        yield return ShowDialogue($"Và, {_currentBeast.baseBeast.beastName} đã học {newName}!");

        _onReplaceScreenDone?.Invoke();
    }

    private IEnumerator ShowCancelRoutine()
    {
        yield return ShowDialogue($"{_currentBeast.baseBeast.beastName} đã không học {_newMove.moveName}.");
        _onReplaceScreenDone?.Invoke();
    }

    private void SetOutlines(int selectedIndex)
    {
        for (int i = 0; i < oldMoveOutlines.Length; i++)
            if (oldMoveOutlines[i] != null) oldMoveOutlines[i].SetActive(i == selectedIndex);

        if (newMoveOutline != null) newMoveOutline.SetActive(selectedIndex == 4);
    }

    private void AutoLearnMove(RuntimeBeastData beast, MoveData move)
    {
        var list = new List<RuntimeMoveData>(beast.moves ?? new RuntimeMoveData[0]);
        list.Add(new RuntimeMoveData(move, 1));
        beast.moves = list.ToArray();
        Debug.Log($"[LearnMoveUI] {beast.baseBeast.beastName} tự động học {move.moveName}!");
    }
}
