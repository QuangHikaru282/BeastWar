using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI dùng Đá Tiến Hóa cho Beast (chuẩn Pokémon).
/// 
/// Gắn script này lên một Panel con trong màn hình PetInfoUIManager.
/// Khi người chơi chọn một con thú → gọi ShowFor(beast) từ PetInfoUIManager.
/// 
/// Luồng hoạt động:
///   ShowFor(beast) → duyệt beast.baseBeast.evolutionStones
///     → Tạo nút cho từng entry
///     → Nút sáng nếu có đá trong túi, tối nếu không có
///     → Bấm nút → TryEvolveWithStone() → trừ đá → tiến hóa → callback refresh UI
/// </summary>
public class EvolutionStoneUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [Tooltip("Kéo thả PlayerInventory từ PF Player vào đây.")]
    [SerializeField] private Kinnly.PlayerInventory playerInventory;

    [Header("UI - Danh sách nút đá")]
    [Tooltip("Container chứa các nút đá — xóa và tạo lại mỗi khi gọi ShowFor().")]
    [SerializeField] private Transform stoneButtonContainer;
    [Tooltip("Prefab 1 nút đá: phải có Button, Image (icon đá), TextMeshProUGUI (tên).")]
    [SerializeField] private GameObject stoneButtonPrefab;

    [Header("UI - Kết quả")]
    [Tooltip("Text hiển thị kết quả sau khi dùng đá hoặc thông báo không đủ đá.")]
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("UI - Trạng thái (tuỳ chọn)")]
    [Tooltip("Text hướng dẫn ở đầu panel. Tự động ẩn nếu không có đá tương thích.")]
    [SerializeField] private TextMeshProUGUI guideText;
    [Tooltip("Panel tổng. Ẩn toàn bộ nếu thú không có bất kỳ entry đá nào.")]
    [SerializeField] private GameObject panelRoot;

    // ─── Callback về cho PetInfoUIManager ───────────────────────────────────
    /// <summary>
    /// PetInfoUIManager đăng ký callback này để refresh UI sau khi tiến hóa xong.
    /// VD: petInfoUIManager.OnEvolveComplete += (beast) => { LoadBeastList(); SelectBeast(beast); };
    /// </summary>
    [HideInInspector] public System.Action<RuntimeBeastData> onEvolveComplete;

    // ─── Runtime ────────────────────────────────────────────────────────────
    private RuntimeBeastData _currentBeast;
    private readonly List<GameObject> _activeButtons = new List<GameObject>();

    // ─── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Hiện danh sách các đá tương thích của Beast.
    /// Gọi từ PetInfoUIManager.SelectBeast() hoặc khi người chơi chọn thú.
    /// </summary>
    public void ShowFor(RuntimeBeastData beast)
    {
        _currentBeast = beast;
        ClearButtons();

        if (resultText != null) resultText.text = "";

        // Không có dữ liệu đá → ẩn toàn bộ panel
        bool hasAnyEntry = beast?.baseBeast?.evolutionStones != null
                        && beast.baseBeast.evolutionStones.Count > 0;

        if (panelRoot != null) panelRoot.SetActive(hasAnyEntry);
        if (!hasAnyEntry) return;

        if (guideText != null)
            guideText.text = "Dùng Đá để tiến hóa";

        foreach (var entry in beast.baseBeast.evolutionStones)
        {
            if (entry.requiredStone == null || entry.evolveTarget == null) continue;

            bool hasStone = playerInventory != null
                         && playerInventory.HasItem(entry.requiredStone, 1);

            CreateStoneButton(entry, hasStone);
        }
    }

    // ─── Private ────────────────────────────────────────────────────────────

    private void CreateStoneButton(EvolutionEntry entry, bool hasStone)
    {
        if (stoneButtonPrefab == null || stoneButtonContainer == null) return;

        GameObject btnObj = Instantiate(stoneButtonPrefab, stoneButtonContainer);
        btnObj.SetActive(true);
        _activeButtons.Add(btnObj);

        // ── Icon đá ─────────────────────────────────────────────────────────
        // Tìm Image đầu tiên trong children để hiện icon đá
        Image icon = btnObj.GetComponentInChildren<Image>();
        if (icon != null && entry.requiredStone.image != null)
            icon.sprite = entry.requiredStone.image;

        // ── Text tên đá + tên dạng tiến hóa ─────────────────────────────────
        TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = $"{entry.requiredStone.name}\n→ {entry.evolveTarget.beastName}";

        // ── Nút sáng/tối tuỳ theo có đá không ──────────────────────────────
        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = hasStone;
            EvolutionEntry captured = entry; // capture for closure
            btn.onClick.AddListener(() => TryEvolveWithStone(_currentBeast, captured));
        }
    }

    private void TryEvolveWithStone(RuntimeBeastData beast, EvolutionEntry entry)
    {
        if (beast == null || entry == null) return;

        // ── Kiểm tra lại lần cuối ────────────────────────────────────────────
        if (playerInventory == null || !playerInventory.HasItem(entry.requiredStone, 1))
        {
            ShowResult($"Không đủ {entry.requiredStone.name}!", Color.red);
            return;
        }

        // ── Trừ đá khỏi túi ─────────────────────────────────────────────────
        bool removed = playerInventory.TryRemoveItem(entry.requiredStone, 1);
        if (!removed)
        {
            ShowResult("Lỗi khi trừ đá. Vui lòng thử lại.", Color.red);
            return;
        }

        // ── Kích hoạt Hoạt ảnh Tiến Hóa Cutscene ─────────────────────────────
        string oldName = beast.baseBeast.beastName;
        EvolutionCutsceneManager.Instance.PlayEvolution(beast, entry.evolveTarget, onComplete: () =>
        {
            playerData?.Save();
            ShowResult($"{oldName} đã tiến hóa thành {beast.baseBeast.beastName}!", Color.green);
            Debug.Log($"[EvolutionStoneUI] ✅ {oldName} → {beast.baseBeast.beastName}");

            // Callback về PetInfoUIManager để refresh UI
            onEvolveComplete?.Invoke(beast);

            // Refresh lại panel
            ShowFor(beast);
        });
    }

    private void ShowResult(string message, Color color)
    {
        if (resultText == null) return;
        resultText.text = message;
        resultText.color = color;
    }

    private void ClearButtons()
    {
        foreach (var btn in _activeButtons)
            if (btn != null) Destroy(btn);
        _activeButtons.Clear();
    }
}
