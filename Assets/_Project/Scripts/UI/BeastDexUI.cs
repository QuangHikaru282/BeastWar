using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý Màn hình BeastDex (Pokédex):
/// 1. Hiển thị danh sách tất cả Beast trong game theo số thứ tự Pokédex.
/// 2. Phân loại trạng thái: Chưa gặp (???), Đã nhìn thấy (Bóng đen), Đã thu phục (Đầy đủ thông tin).
/// 3. Hiển thị bảng chi tiết chỉ số, hệ nguyên tố, và tập tính sinh học của từng loài.
/// </summary>
public class BeastDexUI : MonoBehaviour
{
    public static BeastDexUI Instance { get; private set; }

    [Header("1. Khung Giao Diện Chính")]
    [Tooltip("Panel toàn màn hình chứa BeastDex")]
    [SerializeField] private GameObject dexPanel;

    [Tooltip("Nút đóng BeastDex")]
    [SerializeField] private Button closeButton;

    [Header("2. Dữ Liệu & Danh Sách Beast")]
    [Tooltip("Kéo PlayerData asset vào đây")]
    [SerializeField] private PlayerData playerData;

    [Tooltip("Danh sách toàn bộ BeastData có trong game (Kéo thả thủ công hoặc nạp từ Resources)")]
    [SerializeField] private List<BeastData> allBeastsInGame = new List<BeastData>();

    [Header("3. Danh Sách Slot Cuộn (Scroll View)")]
    [Tooltip("Transform Content của Scroll View chứa các ô")]
    [SerializeField] private Transform slotContainer;

    [Tooltip("Prefab ô BeastDexSlotUI")]
    [SerializeField] private GameObject slotPrefab;

    [Header("4. Bảng Chi Tiết Thông Tin Beast (Bên Phải)")]
    [SerializeField] private Image detailBeastImage;
    [SerializeField] private TextMeshProUGUI detailNumberText;
    [SerializeField] private TextMeshProUGUI detailNameText;
    [SerializeField] private TextMeshProUGUI detailElementText;
    [SerializeField] private TextMeshProUGUI detailStatusText;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;

    [Header("Chỉ số chiến đấu trong Detail Panel")]
    [SerializeField] private TextMeshProUGUI hpStatText;
    [SerializeField] private TextMeshProUGUI attackStatText;
    [SerializeField] private TextMeshProUGUI defenseStatText;
    [SerializeField] private TextMeshProUGUI spAtkStatText;
    [SerializeField] private TextMeshProUGUI spDefStatText;
    [SerializeField] private TextMeshProUGUI speedStatText;

    [Header("5. Thống Kê Tổng Quan (Header)")]
    [SerializeField] private TextMeshProUGUI totalSeenCountText;
    [SerializeField] private TextMeshProUGUI totalCaughtCountText;

    [Header("6. Phím Tắt Mở Dex")]
    [SerializeField] private KeyCode toggleHotkey = KeyCode.D;

    private List<BeastDexSlotUI> spawnedSlots = new List<BeastDexSlotUI>();
    private BeastData selectedBeast;

    public bool IsOpen => dexPanel != null && dexPanel.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (dexPanel != null) dexPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseDex);
        }
    }

    private void Update()
    {
        // Bấm phím D để bật/tắt BeastDex
        if (Input.GetKeyDown(toggleHotkey))
        {
            if (IsOpen) CloseDex();
            else OpenDex();
        }

        if (IsOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F)))
        {
            CloseDex();
        }
    }

    public void OpenDex()
    {
        if (dexPanel != null) dexPanel.SetActive(true);

        // Khóa di chuyển người chơi khi mở Dex
        var pmc = UnityEngine.Object.FindFirstObjectByType<PlayerMapController>();
        if (pmc != null) pmc.SetCanMove(false);

        RefreshDexList();
    }

    public void CloseDex()
    {
        if (dexPanel != null) dexPanel.SetActive(false);

        // Mở lại di chuyển người chơi
        var pmc = UnityEngine.Object.FindFirstObjectByType<PlayerMapController>();
        if (pmc != null) pmc.SetCanMove(true);
    }

    private void RefreshDexList()
    {
        if (slotContainer == null || slotPrefab == null) return;

        // Tự động sắp xếp theo số Pokédex Number
        if (allBeastsInGame != null)
        {
            allBeastsInGame.Sort((a, b) => a.pokedexNumber.CompareTo(b.pokedexNumber));
        }

        // Xóa các ô cũ
        for (int i = slotContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(slotContainer.GetChild(i).gameObject);
        }
        spawnedSlots.Clear();

        int seenCount = 0;
        int caughtCount = 0;

        foreach (var beast in allBeastsInGame)
        {
            if (beast == null) continue;

            bool isCaught = playerData != null && playerData.HasCaughtBeast(beast.name);
            bool isSeen = isCaught || (playerData != null && playerData.HasSeenBeast(beast.name));

            if (isCaught) caughtCount++;
            if (isSeen) seenCount++;

            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            BeastDexSlotUI slotUI = slotObj.GetComponent<BeastDexSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(beast, isSeen, isCaught, SelectBeast);
                spawnedSlots.Add(slotUI);
            }
        }

        // Cập nhật Header thống kê
        if (totalSeenCountText != null)
        {
            totalSeenCountText.text = $"ĐÃ THẤY: {seenCount}/{allBeastsInGame.Count}";
        }
        if (totalCaughtCountText != null)
        {
            totalCaughtCountText.text = $"ĐÃ BẮT: {caughtCount}/{allBeastsInGame.Count}";
        }

        // Mặc định chọn con đầu tiên
        if (allBeastsInGame.Count > 0)
        {
            SelectBeast(allBeastsInGame[0]);
        }
    }

    public void SelectBeast(BeastData beast)
    {
        if (beast == null) return;
        selectedBeast = beast;

        bool isCaught = playerData != null && playerData.HasCaughtBeast(beast.name);
        bool isSeen = isCaught || (playerData != null && playerData.HasSeenBeast(beast.name));

        if (detailNumberText != null) detailNumberText.text = $"#{beast.pokedexNumber:D3}";

        if (isCaught)
        {
            // Hiển thị đầy đủ
            if (detailNameText != null) detailNameText.text = beast.beastName;
            if (detailElementText != null) detailElementText.text = $"HỆ: {beast.element}" + (beast.secondaryElement != BeastElement.Normal ? $" / {beast.secondaryElement}" : "");
            if (detailStatusText != null) detailStatusText.text = "<color=green>★ ĐÃ THU PHỤC</color>";
            if (detailDescriptionText != null) detailDescriptionText.text = string.IsNullOrEmpty(beast.speciesDescription) ? "Chưa có mô tả sinh học." : beast.speciesDescription;

            if (detailBeastImage != null)
            {
                detailBeastImage.sprite = beast.frontSprite;
                detailBeastImage.color = Color.white;
                detailBeastImage.gameObject.SetActive(true);
            }

            SetStats(beast.maxHP, beast.attack, beast.defense, beast.spAttack, beast.spDefense, beast.speed);
        }
        else if (isSeen)
        {
            // Chỉ hiển thị tên và bóng đen
            if (detailNameText != null) detailNameText.text = beast.beastName;
            if (detailElementText != null) detailElementText.text = $"HỆ: {beast.element}";
            if (detailStatusText != null) detailStatusText.text = "<color=yellow>ĐÃ NHÌN THẤY</color>";
            if (detailDescriptionText != null) detailDescriptionText.text = "Cần thu phục Beast này để mở khóa dữ liệu nghiên cứu chi tiết.";

            if (detailBeastImage != null)
            {
                detailBeastImage.sprite = beast.frontSprite;
                detailBeastImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Silhouette
                detailBeastImage.gameObject.SetActive(true);
            }

            SetStats(0, 0, 0, 0, 0, 0, true);
        }
        else
        {
            // Chưa gặp
            if (detailNameText != null) detailNameText.text = "???";
            if (detailElementText != null) detailElementText.text = "HỆ: ???";
            if (detailStatusText != null) detailStatusText.text = "<color=grey>CHƯA GẶP</color>";
            if (detailDescriptionText != null) detailDescriptionText.text = "Dữ liệu chưa được ghi nhận. Hãy khám phá các bụi cỏ hoang dã để tìm kiếm loài này.";

            if (detailBeastImage != null)
            {
                detailBeastImage.gameObject.SetActive(false);
            }

            SetStats(0, 0, 0, 0, 0, 0, true);
        }
    }

    private void SetStats(int hp, int atk, int def, int spAtk, int spDef, int spd, bool isUnknown = false)
    {
        string u = "???";
        if (hpStatText != null) hpStatText.text = isUnknown ? u : hp.ToString();
        if (attackStatText != null) attackStatText.text = isUnknown ? u : atk.ToString();
        if (defenseStatText != null) defenseStatText.text = isUnknown ? u : def.ToString();
        if (spAtkStatText != null) spAtkStatText.text = isUnknown ? u : spAtk.ToString();
        if (spDefStatText != null) spDefStatText.text = isUnknown ? u : spDef.ToString();
        if (speedStatText != null) speedStatText.text = isUnknown ? u : spd.ToString();
    }
}
