using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý màn hình Thông tin Pokémon (Summary Screen 3 Trang) chuẩn Pokémon FireRed.
///
/// Bố cục:
///   - Khung bên trái (Cố định ở cả 3 trang): Sprite, Level, Tên, Giới tính, Bóng.
///   - Trang 1 (Thông tin / Info): Số Dex, Tên, 2 Hệ, Chủ nhân (OT), ID No, Vật phẩm (Item), Ký ức HLV (Tính cách & Nơi bắt).
///   - Trang 2 (Thông số / Stats): 6 Chỉ số (HP Bar, Công, Thủ, TC.ĐB, PT.ĐB, Tốc độ), Thanh EXP, Đặc tính (Ability).
///   - Trang 3 (Chiêu thức / Moves): 4 Ô chiêu (Hệ, Tên, PP) + Bảng chi tiết (Sức mạnh, Chính xác, Mô tả).
/// </summary>
public class SummaryScreenUI : MonoBehaviour
{
    public static SummaryScreenUI Instance { get; private set; }

    [Header("Panel chính")]
    [SerializeField] private GameObject mainPanel;

    [Header("--- KHUNG CỐ ĐỊNH BÊN TRÁI ---")]
    [SerializeField] private Image beastSprite;
    [SerializeField] private Image pokeballIcon;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image genderIcon; // Nam (Xanh), Nữ (Hồng)
    [SerializeField] private Sprite maleGenderSprite;
    [SerializeField] private Sprite femaleGenderSprite;

    [Header("--- ĐIỀU HƯỚNG 3 TRANG ---")]
    [SerializeField] private GameObject page1_Info;
    [SerializeField] private GameObject page2_Stats;
    [SerializeField] private GameObject page3_Moves;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI pageTitleText; // "Thông tin Pokémon" / "Thông số Pokémon" / "Chiêu thức"

    [Header("--- TRANG 1: THÔNG TIN POKÉMON ---")]
    [SerializeField] private TextMeshProUGUI pokedexIdText;
    [SerializeField] private TextMeshProUGUI infoNameText;
    [SerializeField] private TextMeshProUGUI type1Text;
    [SerializeField] private GameObject type2Container;
    [SerializeField] private TextMeshProUGUI type2Text;
    [SerializeField] private TextMeshProUGUI otNameText;
    [SerializeField] private TextMeshProUGUI idNoText;
    [SerializeField] private TextMeshProUGUI heldItemText;
    [SerializeField] private TextMeshProUGUI natureMemoText;
    [SerializeField] private TextMeshProUGUI locationMemoText;

    [Header("--- TRANG 2: THÔNG SỐ & ĐẶC TÍNH ---")]
    [SerializeField] private TextMeshProUGUI hpValueText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI spAtkText;
    [SerializeField] private TextMeshProUGUI spDefText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI currentExpText;
    [SerializeField] private TextMeshProUGUI nextExpText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityDescText;

    [Header("--- TRANG 3: BỘ CHIÊU THỨC ---")]
    [SerializeField] private Button[] moveButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI[] moveElementTexts = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI[] moveNameTexts = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI[] movePPTexts = new TextMeshProUGUI[4];
    [SerializeField] private GameObject[] moveSelectionOutlines = new GameObject[4]; // Viền cam khi chọn chiêu

    [Header("Chi tiết chiêu đang chọn (Trang 3)")]
    [SerializeField] private TextMeshProUGUI detailPowerText;
    [SerializeField] private TextMeshProUGUI detailAccuracyText;
    [SerializeField] private TextMeshProUGUI detailDescText;

    private int currentPage = 1; // 1, 2, 3
    private RuntimeBeastData currentBeast;
    private int selectedMoveIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (nextPageButton != null) nextPageButton.onClick.AddListener(NextPage);
        if (prevPageButton != null) prevPageButton.onClick.AddListener(PrevPage);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);

        // Đăng ký click cho 4 nút chiêu ở Trang 3
        for (int i = 0; i < moveButtons.Length; i++)
        {
            int idx = i;
            if (moveButtons[i] != null)
            {
                moveButtons[i].onClick.AddListener(() => SelectMoveDetail(idx));
            }
        }
    }

    private void Update()
    {
        if (mainPanel == null || !mainPanel.activeInHierarchy) return;

        // Phím điều hướng nhanh (A/D hoặc Mũi tên Trái/Phải để lật trang)
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.E))
        {
            NextPage();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Q))
        {
            PrevPage();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B))
        {
            Hide();
        }
    }

    // ─── PUBLIC API ─────────────────────────────────────────────────────────

    public void Show(RuntimeBeastData beast, int startPage = 1)
    {
        currentBeast = beast;
        if (mainPanel != null) mainPanel.SetActive(true);
        gameObject.SetActive(true);

        UpdateCommonLeftPanel();
        SetPage(startPage);
    }

    public void Hide()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
    }

    public void SetPage(int pageIndex)
    {
        currentPage = Mathf.Clamp(pageIndex, 1, 3);

        if (page1_Info != null) page1_Info.SetActive(currentPage == 1);
        if (page2_Stats != null) page2_Stats.SetActive(currentPage == 2);
        if (page3_Moves != null) page3_Moves.SetActive(currentPage == 3);

        switch (currentPage)
        {
            case 1:
                if (pageTitleText != null) pageTitleText.text = "Thông tin Pokémon";
                UpdatePage1();
                break;
            case 2:
                if (pageTitleText != null) pageTitleText.text = "Thông số Pokémon";
                UpdatePage2();
                break;
            case 3:
                if (pageTitleText != null) pageTitleText.text = "Chiêu thức";
                UpdatePage3();
                break;
        }
    }

    public void NextPage() => SetPage(currentPage >= 3 ? 1 : currentPage + 1);
    public void PrevPage() => SetPage(currentPage <= 1 ? 3 : currentPage - 1);

    // ─── PRIVATE DISPLAY LOGIC ──────────────────────────────────────────────

    private void UpdateCommonLeftPanel()
    {
        if (currentBeast == null || currentBeast.baseBeast == null) return;

        if (beastSprite != null) beastSprite.sprite = currentBeast.baseBeast.frontSprite;
        if (levelText != null) levelText.text = $"Lv{currentBeast.currentLevel}";
        if (nameText != null) nameText.text = currentBeast.baseBeast.beastName;

        // Giới tính mặc định hiển thị nam/nữ
        if (genderIcon != null && maleGenderSprite != null)
        {
            genderIcon.sprite = maleGenderSprite;
            genderIcon.gameObject.SetActive(true);
        }
    }

    private void UpdatePage1()
    {
        if (currentBeast == null || currentBeast.baseBeast == null) return;
        var b = currentBeast.baseBeast;

        if (pokedexIdText != null) pokedexIdText.text = b.pokedexNumber.ToString("D3");
        if (infoNameText != null) infoNameText.text = b.beastName;
        if (type1Text != null) type1Text.text = b.element.ToString().ToUpper();

        if (type2Container != null)
        {
            bool hasSecondType = b.secondaryElement != BeastElement.Normal && b.secondaryElement != b.element;
            type2Container.SetActive(hasSecondType);
            if (hasSecondType && type2Text != null)
            {
                type2Text.text = b.secondaryElement.ToString().ToUpper();
            }
        }

        if (otNameText != null) otNameText.text = string.IsNullOrEmpty(currentBeast.originalTrainer) ? "Truy" : currentBeast.originalTrainer;
        if (idNoText != null) idNoText.text = currentBeast.trainerId.ToString();

        if (heldItemText != null)
        {
            heldItemText.text = currentBeast.heldItem != null ? currentBeast.heldItem.name : "Không có";
        }

        if (natureMemoText != null)
        {
            string vietNature = NatureUtils.GetVietnameseName(currentBeast.nature);
            natureMemoText.text = $"T/cách {vietNature.ToLower()}.";
        }

        if (locationMemoText != null)
        {
            string loc = string.IsNullOrEmpty(currentBeast.caughtLocation) ? "Rừng Tokiwa" : currentBeast.caughtLocation;
            int lvl = currentBeast.caughtLevel > 0 ? currentBeast.caughtLevel : currentBeast.currentLevel;
            locationMemoText.text = $"Gặp tại {loc} ở Lv {lvl}.";
        }
    }

    private void UpdatePage2()
    {
        if (currentBeast == null || currentBeast.baseBeast == null) return;

        // Máu HP
        if (hpValueText != null) hpValueText.text = $"{currentBeast.currentHP}/{currentBeast.MaxHP}";
        if (hpSlider != null)
        {
            float pct = (float)currentBeast.currentHP / Mathf.Max(1, currentBeast.MaxHP);
            hpSlider.value = pct;

            // Đổi màu thanh máu chuẩn Pokemon: Xanh lá (>50%), Vàng (>20%), Đỏ (<=20%)
            if (hpFillImage != null)
            {
                if (pct > 0.5f) hpFillImage.color = new Color32(46, 204, 113, 255);
                else if (pct > 0.2f) hpFillImage.color = new Color32(241, 196, 15, 255);
                else hpFillImage.color = new Color32(231, 76, 60, 255);
            }
        }

        // 6 Stats
        if (attackText != null) attackText.text = currentBeast.Attack.ToString();
        if (defenseText != null) defenseText.text = currentBeast.Defense.ToString();
        if (spAtkText != null) spAtkText.text = currentBeast.SpAttack.ToString();
        if (spDefText != null) spDefText.text = currentBeast.SpDefense.ToString();
        if (speedText != null) speedText.text = currentBeast.Speed.ToString();

        // Kinh nghiệm
        int nextExp = currentBeast.GetExpToNextLevel();
        if (currentExpText != null) currentExpText.text = currentBeast.currentExp.ToString();
        if (nextExpText != null) nextExpText.text = (nextExp - currentBeast.currentExp).ToString();
        if (expSlider != null)
        {
            expSlider.value = (float)currentBeast.currentExp / Mathf.Max(1, nextExp);
        }

        // Đặc tính (Ability)
        if (abilityNameText != null)
        {
            abilityNameText.text = currentBeast.ability != null ? currentBeast.ability.abilityName : "Chưa có";
        }
        if (abilityDescText != null)
        {
            abilityDescText.text = currentBeast.ability != null ? currentBeast.ability.description : "Không có đặc tính nào.";
        }
    }

    private void UpdatePage3()
    {
        if (currentBeast == null) return;

        var moves = currentBeast.moves ?? new RuntimeMoveData[0];
        for (int i = 0; i < moveButtons.Length; i++)
        {
            bool hasMove = i < moves.Length && moves[i] != null && moves[i].baseMove != null;

            if (moveButtons[i] != null) moveButtons[i].gameObject.SetActive(hasMove);

            if (hasMove)
            {
                var m = moves[i];
                if (moveElementTexts[i] != null) moveElementTexts[i].text = m.baseMove.moveElement.ToString().ToUpper();
                if (moveNameTexts[i] != null) moveNameTexts[i].text = m.baseMove.moveName;
                if (movePPTexts[i] != null) movePPTexts[i].text = $"PP {m.currentPP}/{m.MaxPP}";
            }
        }

        // Mặc định chọn chiêu đầu tiên để hiện chi tiết
        SelectMoveDetail(0);
    }

    private void SelectMoveDetail(int index)
    {
        selectedMoveIndex = index;

        // Bật viền chọn (Outline)
        for (int i = 0; i < moveSelectionOutlines.Length; i++)
        {
            if (moveSelectionOutlines[i] != null)
            {
                moveSelectionOutlines[i].SetActive(i == index);
            }
        }

        var moves = currentBeast?.moves;
        if (moves != null && index >= 0 && index < moves.Length && moves[index] != null && moves[index].baseMove != null)
        {
            var move = moves[index].baseMove;
            if (detailPowerText != null) detailPowerText.text = move.power > 0 ? move.power.ToString() : "---";
            if (detailAccuracyText != null) detailAccuracyText.text = move.accuracy > 0 ? move.accuracy.ToString() : "---";
            if (detailDescText != null) detailDescText.text = move.description;
        }
        else
        {
            if (detailPowerText != null) detailPowerText.text = "---";
            if (detailAccuracyText != null) detailAccuracyText.text = "---";
            if (detailDescText != null) detailDescText.text = "";
        }
    }
}
