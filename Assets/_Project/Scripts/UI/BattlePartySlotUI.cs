using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý từng ô hiển thị Pet trong Bảng Đổi Thú Trận Đấu (Battle Party Screen).
/// </summary>
public class BattlePartySlotUI : MonoBehaviour
{
    [Header("1. Loại ô")]
    [Tooltip("Tích chọn nếu đây là Ô Chính (Bên trái - Pet đang xuất trận)")]
    [SerializeField] private bool isMainActiveSlot = false;

    [Header("2. Thành phần UI")]
    [SerializeField] private Image petIconImage;
    [SerializeField] private Image ballIconImage;
    [SerializeField] private Image genderIconImage;

    [Header("Text thông tin (Hỗ trợ cả TMP và Legacy Text)")]
    [SerializeField] private TextMeshProUGUI nameTextTMP;
    [SerializeField] private Text nameTextLegacy;
    [SerializeField] private TextMeshProUGUI levelTextTMP;
    [SerializeField] private Text levelTextLegacy;
    [SerializeField] private TextMeshProUGUI hpTextTMP;
    [SerializeField] private Text hpTextLegacy;

    [Header("Thanh Máu & Trạng Thái")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image hpFillImage;
    [SerializeField] private GameObject faintedOverlay; // Lớp phủ xám khi hết máu
    [SerializeField] private GameObject emptySlotPlaceholder; // Khung khi ô trống
    [SerializeField] private GameObject contentContainer; // Cụm chứa nội dung khi có thú

    [Header("Khung viền chọn (Outline)")]
    [SerializeField] private GameObject selectionOutline;

    [Header("Màu thanh máu theo %")]
    [SerializeField] private Color hpGreen = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color hpYellow = new Color(0.9f, 0.8f, 0.1f);
    [SerializeField] private Color hpRed = new Color(0.9f, 0.2f, 0.2f);

    private RuntimeBeastData _beastData;
    private bool _isCurrentlyOnField = false;

    public bool IsMainActiveSlot => isMainActiveSlot;
    public RuntimeBeastData BeastData => _beastData;
    public bool IsEmpty => _beastData == null;
    public bool IsFainted => _beastData != null && _beastData.currentHP <= 0;
    public bool IsCurrentlyOnField => _isCurrentlyOnField;

    private void Awake()
    {
        // Tự động tìm thành phần con nếu chưa kéo trong Inspector để tránh null
        if (nameTextTMP == null && nameTextLegacy == null)
        {
            var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                string n = t.gameObject.name.ToLower();
                if (n.Contains("name") || n.Contains("ten")) nameTextTMP = t;
                else if (n.Contains("level") || n.Contains("lv")) levelTextTMP = t;
                else if (n.Contains("hp") || n.Contains("mau") || t.text.Contains("/")) hpTextTMP = t;
            }

            var legs = GetComponentsInChildren<Text>(true);
            foreach (var t in legs)
            {
                string n = t.gameObject.name.ToLower();
                if (n.Contains("name") || n.Contains("ten")) nameTextLegacy = t;
                else if (n.Contains("level") || n.Contains("lv")) levelTextLegacy = t;
                else if (n.Contains("hp") || n.Contains("mau") || t.text.Contains("/")) hpTextLegacy = t;
            }
        }

        if (hpSlider == null) hpSlider = GetComponentInChildren<Slider>(true);
        if (hpSlider != null && hpFillImage == null)
        {
            Transform fillTr = hpSlider.transform.Find("Fill Area/Fill") ?? hpSlider.transform.Find("Fill");
            if (fillTr != null) hpFillImage = fillTr.GetComponent<Image>();
        }
    }

    /// <summary>
    /// Cập nhật dữ liệu hiển thị cho Ô Pet
    /// </summary>
    public void Setup(RuntimeBeastData beast, bool isOnField)
    {
        _beastData = beast;
        _isCurrentlyOnField = isOnField;

        if (_beastData == null || _beastData.baseBeast == null)
        {
            ShowEmpty();
            return;
        }

        if (emptySlotPlaceholder != null) emptySlotPlaceholder.SetActive(false);
        if (contentContainer != null) contentContainer.SetActive(true);

        // 1. Icon Pet
        if (petIconImage != null)
        {
            petIconImage.gameObject.SetActive(true);
            if (_beastData.baseBeast.frontSprite != null)
                petIconImage.sprite = _beastData.baseBeast.frontSprite;
        }

        if (ballIconImage != null) ballIconImage.gameObject.SetActive(true);

        // 2. Tên & Level
        SetText(nameTextTMP, nameTextLegacy, _beastData.baseBeast.beastName);
        SetText(levelTextTMP, levelTextLegacy, $"Lv{_beastData.currentLevel}");

        // 3. Giới tính (nếu có Sprite)
        if (genderIconImage != null)
        {
            genderIconImage.gameObject.SetActive(false); // Ẩn nếu không dùng icon riêng
        }

        // 4. Máu (HP)
        int curHP = Mathf.Max(0, _beastData.currentHP);
        int maxHP = Mathf.Max(1, _beastData.MaxHP);
        float hpPercent = (float)curHP / maxHP;

        SetText(hpTextTMP, hpTextLegacy, $"{curHP}/{maxHP}");

        if (hpSlider != null)
        {
            hpSlider.gameObject.SetActive(true);
            hpSlider.minValue = 0;
            hpSlider.maxValue = 1;
            hpSlider.value = hpPercent;
        }

        if (hpFillImage != null)
        {
            if (hpPercent > 0.5f) hpFillImage.color = hpGreen;
            else if (hpPercent > 0.2f) hpFillImage.color = hpYellow;
            else hpFillImage.color = hpRed;
        }

        // 5. Trạng thái Kiệt sức (Fainted)
        bool fainted = (curHP <= 0);
        if (faintedOverlay != null)
        {
            faintedOverlay.SetActive(fainted);
        }

        // 6. Outline
        SetSelected(false);
    }

    /// <summary>
    /// Hiển thị ô trống khi không có thú
    /// </summary>
    public void ShowEmpty()
    {
        _beastData = null;
        _isCurrentlyOnField = false;

        if (contentContainer != null) contentContainer.SetActive(false);
        if (emptySlotPlaceholder != null) emptySlotPlaceholder.SetActive(true);
        if (faintedOverlay != null) faintedOverlay.SetActive(false);

        SetText(nameTextTMP, nameTextLegacy, "---");
        SetText(levelTextTMP, levelTextLegacy, "");
        SetText(hpTextTMP, hpTextLegacy, "");

        if (petIconImage != null) petIconImage.gameObject.SetActive(false);
        if (ballIconImage != null) ballIconImage.gameObject.SetActive(false);
        if (hpSlider != null) hpSlider.gameObject.SetActive(false);
        SetSelected(false);
    }


    public void SetSelected(bool isSelected)
    {
        if (selectionOutline != null)
        {
            selectionOutline.SetActive(isSelected);
        }
    }

    private void SetText(TextMeshProUGUI tmp, Text legacy, string content)
    {
        if (tmp != null) tmp.text = content;
        if (legacy != null) legacy.text = content;
    }
}
