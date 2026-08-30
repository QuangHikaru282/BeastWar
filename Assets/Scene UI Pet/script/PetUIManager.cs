using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetUIManager : MonoBehaviour
{
    private enum PetTab
    {
        Skill,
        Stats,
        Enhance
    }

    [Header("Dữ liệu người chơi")]
    [Tooltip("Kéo PlayerData của người chơi vào đây.")]
    [SerializeField] private PlayerData playerData;

    [Header("Danh sách Pet bên trái")]
    [SerializeField] private Transform petListContent;
    [SerializeField] private PetSlotUI petSlotPrefab;

    [Header("Pet hiển thị ở giữa")]
    [SerializeField] private Image petDisplayImage;
    [SerializeField] private Image elementIcon;
    [SerializeField] private TMP_Text petNameText;
    [SerializeField] private TMP_Text petLevelText;

    [Header("Background PetDisplayArea")]
    [Tooltip(
        "Kéo object PetDisplayArea có component " +
        "PetDisplayAreaUI vào đây."
    )]
    [SerializeField] private PetDisplayAreaUI petDisplayAreaUI;

    [Header("Nội dung chỉ số Pet")]
    [SerializeField] private PetStatContentUI petStatContentUI;

    [Header("Nội dung Skill")]
    [Tooltip(
        "Kéo SkillContent có component " +
        "SkillContentUI vào đây."
    )]
    [SerializeField] private SkillContentUI skillContentUI;

    [Header("Nội dung Enhance theo nguyên tố")]
    [Tooltip(
        "Kéo EnhancePetContent có component " +
        "EnhancePetContentUI vào đây."
    )]
    [SerializeField] private EnhancePetContentUI enhancePetContentUI;

    [Header("Nút chuyển Pet")]
    [SerializeField] private Button previousPetButton;
    [SerializeField] private Button nextPetButton;

    [Header("Ba nội dung Tab")]
    [SerializeField] private GameObject skillContent;
    [SerializeField] private GameObject petStatContent;
    [SerializeField] private GameObject enhancePetContent;

    [Header("Ba nút Tab")]
    [SerializeField] private Button skillButton;
    [SerializeField] private Button petStatButton;
    [SerializeField] private Button enhancePetButton;

    [Header("Viền Tab đang chọn")]
    [SerializeField] private GameObject skillSelectedIndicator;
    [SerializeField] private GameObject statSelectedIndicator;
    [SerializeField] private GameObject enhanceSelectedIndicator;

    [Header("Thông tin tiến hóa")]
    [Tooltip(
        "Có thể để trống nếu Image 2 được hiển thị " +
        "bởi EnhancePanelStatsUI trong từng panel."
    )]
    [SerializeField] private Image evolveTargetImage;

    [SerializeField] private TMP_Text evolveTargetNameText;
    [SerializeField] private TMP_Text evolveCostText;
    [SerializeField] private TMP_Text evolveLevelReqText;
    [SerializeField] private TMP_Text evolveWarningText;
    [SerializeField] private Button evolveButton;

    [Header("Mở và đóng giao diện")]
    [SerializeField] private Button openPetButton;
    [SerializeField] private Button closeButton;

    [Tooltip(
        "Chỉ kéo PetPanel vào đây. " +
        "Không kéo object chứa PetUIManager hoặc OpenPetButton."
    )]
    [SerializeField] private GameObject petUIRoot;

    [SerializeField] private bool hidePetUIAtStart = true;

    private int originalSortingOrder = 0;

    private readonly List<PetSlotUI> createdSlots =
        new List<PetSlotUI>();

    private int selectedIndex = -1;
    private bool initialized;
    private PetTab currentTab = PetTab.Stats;

    private void Awake()
    {
        AutoBindAllReferences();
        RegisterButtons();
    }

    private void AutoBindAllReferences()
    {
        // PlayerData
        if (playerData == null)
            playerData = Resources.Load<PlayerData>("PlayerData");

        // petUIRoot = PetPanel (con trực tiếp của PetUI)
        if (petUIRoot == null)
        {
            Transform t = transform.Find("PetPanel");
            petUIRoot = (t != null) ? t.gameObject : null;
        }

        Transform root = (petUIRoot != null) ? petUIRoot.transform : transform;

        // Component-based (hoạt động kể cả khi inactive)
        if (petDisplayAreaUI == null)   petDisplayAreaUI   = root.GetComponentInChildren<PetDisplayAreaUI>(true);
        if (petStatContentUI == null)   petStatContentUI   = root.GetComponentInChildren<PetStatContentUI>(true);
        if (skillContentUI == null)     skillContentUI     = root.GetComponentInChildren<SkillContentUI>(true);
        if (enhancePetContentUI == null) enhancePetContentUI = root.GetComponentInChildren<EnhancePetContentUI>(true);

        // Tab GameObjects
        if (skillContent == null && skillContentUI != null)         skillContent     = skillContentUI.gameObject;
        if (petStatContent == null && petStatContentUI != null)     petStatContent   = petStatContentUI.gameObject;
        if (enhancePetContent == null && enhancePetContentUI != null) enhancePetContent = enhancePetContentUI.gameObject;

        // Scroll View -> petListContent
        if (petListContent == null)
        {
            ScrollRect[] srs = root.GetComponentsInChildren<ScrollRect>(true);
            foreach (var sr in srs)
                if (sr.content != null) { petListContent = sr.content; break; }
        }

        // PetSlot prefab từ Resources
        if (petSlotPrefab == null)
        {
            petSlotPrefab = Resources.Load<PetSlotUI>("PetSlot");
            if (petSlotPrefab == null)
            {
                var all = Resources.LoadAll<PetSlotUI>("");
                if (all != null && all.Length > 0) petSlotPrefab = all[0];
            }
        }

        // Buttons theo tên chính xác trong Hierarchy
        Button[] allBtns = root.GetComponentsInChildren<Button>(true);
        foreach (var btn in allBtns)
        {
            string n = btn.name;
            string nl = n.ToLower();
            if (closeButton == null       && (n == "CloseButton"     || nl.Contains("close")))          closeButton       = btn;
            if (previousPetButton == null && (n == "PreviousPetButton"|| nl.Contains("prev") || nl.Contains("left")))  previousPetButton = btn;
            if (nextPetButton == null     && (n == "NextPetButton"    || nl.Contains("next") || nl.Contains("right"))) nextPetButton     = btn;
            if (skillButton == null       && (n == "SkillButton"      || nl.Contains("skill")))          skillButton       = btn;
            if (petStatButton == null     && (n == "PetStatButton"    || nl.Contains("stat") || nl.Contains("info")))  petStatButton     = btn;
            if (enhancePetButton == null  && (n == "EnhancePetButton" || n == "EnhancePetButton (1)" || nl.Contains("enhance"))) enhancePetButton = btn;
            if (evolveButton == null      && (nl.Contains("evolve")   || nl.Contains("tienhoa") || nl.Contains("upgrade"))) evolveButton = btn;
        }

        // openPetButton nằm ngoài PetPanel (anh em của PetUI)
        if (openPetButton == null && transform.parent != null)
        {
            Transform t = transform.parent.Find("OpenPetButton");
            if (t != null) openPetButton = t.GetComponent<Button>();
        }

        // petDisplayImage, petNameText, petLevelText từ PetTopInfo
        Transform dispArea = root.Find("MainContent/PetDisplayArea");
        if (dispArea != null)
        {
            // Luôn tìm đúng PetCharacterArea, sửa cả trường hợp Inspector bị gán nhầm vào PetDisplayArea nền
            Transform charArea = dispArea.Find("PetCharacterArea");
            if (charArea != null)
            {
                Image charImg = charArea.GetComponent<Image>();
                if (charImg != null) petDisplayImage = charImg;
            }

            Transform topInfo = dispArea.Find("PetTopInfo");
            if (topInfo != null)
            {
                TMP_Text[] txts = topInfo.GetComponentsInChildren<TMP_Text>(true);
                foreach (var txt in txts)
                {
                    string nl = txt.name.ToLower();
                    if (petNameText  == null && (nl.Contains("name") || nl.Contains("ten")))  petNameText  = txt;
                    if (petLevelText == null && (nl.Contains("level")|| nl.Contains("lv")))   petLevelText = txt;
                }
            }
        }
    }

    private void Start()
    {
        if (petUIRoot == null)
        {
            Debug.LogError(
                "[PetUIManager] Chưa gán Pet UI Root.",
                this
            );
            return;
        }

        // Đảm bảo gameObject cha luôn BẬT để PetUIManager luôn sẵn sàng
        gameObject.SetActive(true);

        if (hidePetUIAtStart)
        {
            petUIRoot.SetActive(false);
        }
        else
        {
            petUIRoot.SetActive(true);
            Initialize();
        }
    }


    private void OnDestroy()
    {
        UnregisterButtons();
    }

    #region Khởi tạo

    public void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        BuildPetList();

        int firstPetIndex =
            FindNextValidPetIndex(-1, 1);

        if (firstPetIndex >= 0)
        {
            SelectPetByIndex(firstPetIndex);
        }
        else
        {
            ClearDisplay();
        }

        ShowTab(PetTab.Stats);
    }

    /// <summary>
    /// Gọi khi danh sách Pet sở hữu vừa thay đổi.
    /// </summary>
    public void RebuildPetList()
    {
        int oldSelectedIndex = selectedIndex;

        BuildPetList();

        if (IsValidPetIndex(oldSelectedIndex))
        {
            SelectPetByIndex(oldSelectedIndex);
            return;
        }

        int firstPetIndex =
            FindNextValidPetIndex(-1, 1);

        if (firstPetIndex >= 0)
        {
            SelectPetByIndex(firstPetIndex);
        }
        else
        {
            ClearDisplay();
        }
    }

    private void BuildPetList()
    {
        if (petListContent == null)
        {
            Debug.LogError(
                "[PetUIManager] Chưa gán Pet List Content.",
                this
            );
            return;
        }

        if (petSlotPrefab == null)
        {
            Debug.LogError(
                "[PetUIManager] Chưa gán Pet Slot Prefab.",
                this
            );
            return;
        }

        // Xóa những PetSlot cũ.
        for (int i = petListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(
                petListContent.GetChild(i).gameObject
            );
        }

        createdSlots.Clear();

        if (playerData == null)
        {
            Debug.LogError(
                "[PetUIManager] Chưa gán PlayerData.",
                this
            );
            return;
        }

        if (playerData.ownedBeasts == null)
        {
            Debug.LogWarning(
                "[PetUIManager] ownedBeasts đang null.",
                this
            );
            return;
        }

        foreach (RuntimeBeastData pet in playerData.ownedBeasts)
        {
            if (pet == null || pet.baseBeast == null)
                continue;

            PetSlotUI newSlot = Instantiate(
                petSlotPrefab,
                petListContent
            );

            newSlot.name =
                $"PetSlot_{pet.baseBeast.beastName}";

            newSlot.Setup(
                pet,
                SelectPet
            );

            createdSlots.Add(newSlot);
        }
    }

    #endregion

    #region Đăng ký Button

    private void RegisterButtons()
    {
        if (openPetButton != null)
        {
            openPetButton.onClick.AddListener(
                OpenPetUI
            );
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(
                ClosePetUI
            );
        }

        if (previousPetButton != null)
        {
            previousPetButton.onClick.AddListener(
                SelectPreviousPet
            );
        }

        if (nextPetButton != null)
        {
            nextPetButton.onClick.AddListener(
                SelectNextPet
            );
        }

        if (skillButton != null)
        {
            skillButton.onClick.AddListener(
                ShowSkillTab
            );
        }

        if (petStatButton != null)
        {
            petStatButton.onClick.AddListener(
                ShowStatsTab
            );
        }

        if (enhancePetButton != null)
        {
            enhancePetButton.onClick.AddListener(
                ShowEnhanceTab
            );
        }

        if (evolveButton != null)
        {
            evolveButton.onClick.AddListener(
                OnEvolveButtonClicked
            );
        }
    }

    private void UnregisterButtons()
    {
        if (openPetButton != null)
        {
            openPetButton.onClick.RemoveListener(
                OpenPetUI
            );
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                ClosePetUI
            );
        }

        if (previousPetButton != null)
        {
            previousPetButton.onClick.RemoveListener(
                SelectPreviousPet
            );
        }

        if (nextPetButton != null)
        {
            nextPetButton.onClick.RemoveListener(
                SelectNextPet
            );
        }

        if (skillButton != null)
        {
            skillButton.onClick.RemoveListener(
                ShowSkillTab
            );
        }

        if (petStatButton != null)
        {
            petStatButton.onClick.RemoveListener(
                ShowStatsTab
            );
        }

        if (enhancePetButton != null)
        {
            enhancePetButton.onClick.RemoveListener(
                ShowEnhanceTab
            );
        }

        if (evolveButton != null)
        {
            evolveButton.onClick.RemoveListener(
                OnEvolveButtonClicked
            );
        }
    }

    #endregion

    #region Chọn Pet

    private void SelectPet(RuntimeBeastData pet)
    {
        if (pet == null ||
            playerData == null ||
            playerData.ownedBeasts == null)
        {
            return;
        }

        int index =
            playerData.ownedBeasts.IndexOf(pet);

        if (index < 0)
        {
            Debug.LogWarning(
                "[PetUIManager] Không tìm thấy Pet " +
                "trong ownedBeasts.",
                this
            );
            return;
        }

        SelectPetByIndex(index);
    }

    private void SelectPetByIndex(int index)
    {
        if (!IsValidPetIndex(index))
        {
            ClearDisplay();
            return;
        }

        selectedIndex = index;

        RuntimeBeastData selectedPet =
            playerData.ownedBeasts[index];

        UpdateMainDisplay(selectedPet);
        UpdateSlotSelection(selectedPet);

        /*
         * Đổi background PetDisplayArea dựa trên:
         * selectedPet.baseBeast.enhanceElement.
         */
        if (petDisplayAreaUI != null)
        {
            petDisplayAreaUI.Display(selectedPet);
        }

        if (petStatContentUI != null)
        {
            petStatContentUI.Display(selectedPet);
        }

        if (skillContentUI != null)
        {
            skillContentUI.Display(
                selectedPet,
                playerData,
                OnSkillUpgradedCallback
            );
        }

        /*
         * Chỉ gọi Enhance khi tab Enhance đang mở.
         * Tránh panel nguyên tố bị tắt sai thứ tự.
         */
        if (currentTab == PetTab.Enhance)
        {
            RefreshEnhanceTab();
        }
    }

    private void SelectPreviousPet()
    {
        int previousIndex =
            FindNextValidPetIndex(
                selectedIndex,
                -1
            );

        if (previousIndex >= 0)
        {
            SelectPetByIndex(previousIndex);
        }
    }

    private void SelectNextPet()
    {
        int nextIndex =
            FindNextValidPetIndex(
                selectedIndex,
                1
            );

        if (nextIndex >= 0)
        {
            SelectPetByIndex(nextIndex);
        }
    }

    private int FindNextValidPetIndex(
        int startIndex,
        int direction
    )
    {
        if (playerData == null ||
            playerData.ownedBeasts == null ||
            playerData.ownedBeasts.Count == 0)
        {
            return -1;
        }

        direction = direction >= 0 ? 1 : -1;

        int index = startIndex;

        for (int i = 0;
             i < playerData.ownedBeasts.Count;
             i++)
        {
            index += direction;

            if (index >= playerData.ownedBeasts.Count)
            {
                index = 0;
            }

            if (index < 0)
            {
                index =
                    playerData.ownedBeasts.Count - 1;
            }

            RuntimeBeastData pet =
                playerData.ownedBeasts[index];

            if (pet != null &&
                pet.baseBeast != null)
            {
                return index;
            }
        }

        return -1;
    }

    private bool IsValidPetIndex(int index)
    {
        if (playerData == null ||
            playerData.ownedBeasts == null)
        {
            return false;
        }

        if (index < 0 ||
            index >= playerData.ownedBeasts.Count)
        {
            return false;
        }

        RuntimeBeastData pet =
            playerData.ownedBeasts[index];

        return pet != null &&
               pet.baseBeast != null;
    }

    private RuntimeBeastData GetSelectedPet()
    {
        if (!IsValidPetIndex(selectedIndex))
            return null;

        return playerData.ownedBeasts[selectedIndex];
    }

    #endregion

    #region Hiển thị Pet chính

    private void UpdateMainDisplay(
        RuntimeBeastData pet
    )
    {
        if (pet == null || pet.baseBeast == null)
        {
            ClearDisplay();
            return;
        }

        BeastData beastData = pet.baseBeast;

        if (petNameText != null)
        {
            petNameText.text =
                beastData.beastName;
        }

        if (petLevelText != null)
        {
            petLevelText.text =
                $"Lv. {pet.currentLevel}";
        }

        SetImage(
            petDisplayImage,
            beastData.frontSprite
        );

        if (elementIcon == null && petUIRoot != null)
        {
            Transform topInfo = petUIRoot.transform.Find("MainContent/PetDisplayArea/PetTopInfo");
            if (topInfo != null) elementIcon = topInfo.GetComponentInChildren<Image>(true);
        }

        SetImage(elementIcon, null);

        if (elementIcon != null)
        {
            elementIcon.enabled = false;
            elementIcon.gameObject.SetActive(false);
        }

        CleanEmptyWhiteImages(petUIRoot != null ? petUIRoot.transform : transform);
    }

    private void CleanEmptyWhiteImages(Transform parent)
    {
        if (parent == null) return;
        Image[] images = parent.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img == petDisplayImage) continue;

            if (img.sprite == null)
            {
                string n = img.name.ToLower();
                if (n.Contains("icon") || n.Contains("element") || n.Contains("coin") || n.Contains("gold") || n.Contains("gem") || n.Contains("resource"))
                {
                    img.enabled = false;
                }
            }
        }
    }

    private void UpdateSlotSelection(
        RuntimeBeastData selectedPet
    )
    {
        foreach (PetSlotUI slot in createdSlots)
        {
            if (slot == null)
                continue;

            slot.SetSelected(
                slot.Data == selectedPet
            );
        }
    }

    #endregion

    #region Skill

    private void OnSkillUpgradedCallback()
    {
        RuntimeBeastData selectedPet =
            GetSelectedPet();

        if (selectedPet == null)
            return;

        if (petStatContentUI != null)
        {
            petStatContentUI.Display(selectedPet);
        }

        if (currentTab == PetTab.Enhance)
        {
            RefreshEnhanceTab();
        }
    }

    #endregion

    #region Chuyển Tab

    private void ShowSkillTab()
    {
        ShowTab(PetTab.Skill);
    }

    private void ShowStatsTab()
    {
        ShowTab(PetTab.Stats);
    }

    private void ShowEnhanceTab()
    {
        ShowTab(PetTab.Enhance);
    }

    private void ShowTab(PetTab tab)
    {
        currentTab = tab;

        /*
         * Bật Content trước.
         * Sau đó mới gọi Display để Awake của Content
         * không tắt panel nguyên tố vừa được mở.
         */
        SetActiveSafe(
            skillContent,
            tab == PetTab.Skill
        );

        SetActiveSafe(
            petStatContent,
            tab == PetTab.Stats
        );

        SetActiveSafe(
            enhancePetContent,
            tab == PetTab.Enhance
        );

        SetActiveSafe(
            skillSelectedIndicator,
            tab == PetTab.Skill
        );

        SetActiveSafe(
            statSelectedIndicator,
            tab == PetTab.Stats
        );

        SetActiveSafe(
            enhanceSelectedIndicator,
            tab == PetTab.Enhance
        );

        RuntimeBeastData selectedPet =
            GetSelectedPet();

        switch (tab)
        {
            case PetTab.Skill:
                RefreshSkillTab(selectedPet);
                break;

            case PetTab.Stats:
                RefreshStatsTab(selectedPet);
                break;

            case PetTab.Enhance:
                RefreshEnhanceTab();
                break;
        }
    }

    private void RefreshSkillTab(
        RuntimeBeastData selectedPet
    )
    {
        if (skillContentUI == null)
            return;

        if (selectedPet == null)
        {
            skillContentUI.Clear();
            return;
        }

        skillContentUI.Display(
            selectedPet,
            playerData,
            OnSkillUpgradedCallback
        );
    }

    private void RefreshStatsTab(
        RuntimeBeastData selectedPet
    )
    {
        if (petStatContentUI == null)
            return;

        if (selectedPet == null)
        {
            petStatContentUI.Clear();
            return;
        }

        petStatContentUI.Display(selectedPet);
    }

    #endregion

    #region Enhance và tiến hóa

    private void RefreshEnhanceTab()
    {
        RuntimeBeastData selectedPet =
            GetSelectedPet();

        if (selectedPet == null ||
            selectedPet.baseBeast == null)
        {
            if (enhancePetContentUI != null)
            {
                enhancePetContentUI.Clear();
            }

            ClearEvolutionDisplay(
                "Không có thông tin Pet."
            );
            return;
        }

        /*
         * EnhancePetContentUI phải đọc:
         * selectedPet.baseBeast.enhanceElement.
         */
        if (enhancePetContentUI != null)
        {
            enhancePetContentUI.Display(selectedPet);
        }
        else
        {
            Debug.LogError(
                "[PetUIManager] Chưa gán " +
                "Enhance Pet Content UI.",
                this
            );
        }

        RefreshEvolutionInformation(selectedPet);
    }

    private void RefreshEvolutionInformation(
        RuntimeBeastData selectedPet
    )
    {
        if (selectedPet == null ||
            selectedPet.baseBeast == null)
        {
            ClearEvolutionDisplay(
                "Không có thông tin Pet."
            );
            return;
        }

        BeastData currentBeast =
            selectedPet.baseBeast;

        /*
         * Luôn gán Image 2 trước khi kiểm tra Evolve Target.
         * Evolve Target đang None thì Image 2 vẫn được hiện.
         */
        SetImage(
            evolveTargetImage,
            currentBeast.image2
        );

        BeastData evolutionTarget =
            currentBeast.evolveTarget;

        int requiredLevel =
            currentBeast.evolveLevel;

        int requiredGold =
            currentBeast.evolveGoldCost;

        if (evolveCostText != null)
        {
            evolveCostText.text =
                requiredGold.ToString("N0");
        }

        if (evolveLevelReqText != null)
        {
            evolveLevelReqText.text =
                $"Yêu cầu: Lv. {requiredLevel}";
        }

        if (evolutionTarget == null)
        {
            if (evolveTargetNameText != null)
            {
                evolveTargetNameText.text =
                    "Chưa gán Evolve Target";
            }

            if (evolveWarningText != null)
            {
                evolveWarningText.text =
                    "Image 2 vẫn được hiển thị, nhưng chưa thể " +
                    "tiến hóa vì Evolve Target đang để trống.";
            }

            if (evolveButton != null)
            {
                evolveButton.interactable = false;
            }

            return;
        }

        if (evolveTargetNameText != null)
        {
            evolveTargetNameText.text =
                evolutionTarget.beastName;
        }

        bool enoughLevel =
            selectedPet.currentLevel >= requiredLevel;

        bool enoughGold =
            playerData != null &&
            playerData.gold >= requiredGold;

        bool canEvolve =
            enoughLevel &&
            enoughGold;

        if (evolveWarningText != null)
        {
            if (!enoughLevel)
            {
                evolveWarningText.text =
                    $"Cần đạt Lv. {requiredLevel}!";
            }
            else if (playerData == null)
            {
                evolveWarningText.text =
                    "Chưa gán PlayerData!";
            }
            else if (!enoughGold)
            {
                evolveWarningText.text =
                    $"Không đủ vàng! Cần {requiredGold:N0}.";
            }
            else if (currentBeast.image2 == null)
            {
                evolveWarningText.text =
                    "Có thể tiến hóa, nhưng chưa gán Image 2.";
            }
            else
            {
                evolveWarningText.text =
                    "Có thể tiến hóa!";
            }
        }

        if (evolveButton != null)
        {
            evolveButton.interactable =
                canEvolve;
        }
    }

    private void OnEvolveButtonClicked()
    {
        RuntimeBeastData selectedPet =
            GetSelectedPet();

        if (selectedPet == null ||
            selectedPet.baseBeast == null ||
            playerData == null)
        {
            return;
        }

        BeastData currentBeast =
            selectedPet.baseBeast;

        BeastData evolutionTarget =
            currentBeast.evolveTarget;

        if (evolutionTarget == null)
        {
            Debug.LogWarning(
                $"[PetUIManager] Pet " +
                $"'{currentBeast.beastName}' chưa có Evolve Target.",
                currentBeast
            );

            RefreshEnhanceTab();
            return;
        }

        int requiredLevel =
            currentBeast.evolveLevel;

        int requiredGold =
            currentBeast.evolveGoldCost;

        bool enoughLevel =
            selectedPet.currentLevel >= requiredLevel;

        bool enoughGold =
            playerData.gold >= requiredGold;

        if (!enoughLevel || !enoughGold)
        {
            RefreshEnhanceTab();
            return;
        }

        playerData.gold -= requiredGold;
        playerData.gold = Mathf.Max(
            0,
            playerData.gold
        );

        selectedPet.Evolve();

        playerData.Save();

        BuildPetList();

        if (IsValidPetIndex(selectedIndex))
        {
            SelectPetByIndex(selectedIndex);
        }
        else
        {
            int firstPetIndex =
                FindNextValidPetIndex(-1, 1);

            if (firstPetIndex >= 0)
            {
                SelectPetByIndex(firstPetIndex);
            }
            else
            {
                ClearDisplay();
            }
        }

        ShowTab(PetTab.Enhance);
    }

    private void ClearEvolutionDisplay(
        string warningMessage
    )
    {
        if (evolveTargetNameText != null)
        {
            evolveTargetNameText.text = "-";
        }

        SetImage(evolveTargetImage, null);

        if (evolveCostText != null)
        {
            evolveCostText.text = "-";
        }

        if (evolveLevelReqText != null)
        {
            evolveLevelReqText.text = "-";
        }

        if (evolveWarningText != null)
        {
            evolveWarningText.text =
                warningMessage;
        }

        if (evolveButton != null)
        {
            evolveButton.interactable = false;
        }
    }

    #endregion

    #region Mở và đóng UI

    public void OpenPetUI()
    {
        // Bật PetUI (object này) và PetPanel (petUIRoot)
        gameObject.SetActive(true);

        if (petUIRoot == null)
        {
            Debug.LogError("[PetUIManager] petUIRoot NULL! Hãy kéo PetPanel vào ô Pet UI Root.", this);
            return;
        }

        petUIRoot.SetActive(true);
        petUIRoot.transform.SetAsLastSibling();

        Canvas cv = petUIRoot.GetComponentInParent<Canvas>();
        if (cv != null)
        {
            // Lưu lại sortingOrder gốc rồi đẩy lên cao nhất
            originalSortingOrder = cv.sortingOrder;
            cv.sortingOrder = 9999;
        }

        InteractHintManager.Instance?.RegisterPanelOpen();

        // Luôn gọi Initialize (lần đầu: chạy đầy đủ; các lần sau: skip)
        Initialize();
        // Luôn rebuild + refresh để chắc chắn hiển thị đúng
        RebuildPetList();
        ShowTab(currentTab);

        // Đảm bảo ở lần bấm đầu tiên, tất cả các component con đã Awake/Start xong và hiển thị đủ ngay
        StartCoroutine(RefreshNextFrame());
    }

    private System.Collections.IEnumerator RefreshNextFrame()
    {
        yield return null;
        if (gameObject.activeInHierarchy)
        {
            RebuildPetList();
            ShowTab(currentTab);
        }
    }


    public void ClosePetUI()
    {
        // Tắt PetPanel
        if (petUIRoot != null)
            petUIRoot.SetActive(false);

        InteractHintManager.Instance?.RegisterPanelClose();

        Canvas cv = petUIRoot != null ? petUIRoot.GetComponentInParent<Canvas>() : null;
        if (cv != null && originalSortingOrder != -1)
        {
            cv.sortingOrder = originalSortingOrder;
        }
    }

    #endregion

    #region Xóa giao diện

    private void ClearDisplay()
    {
        selectedIndex = -1;

        if (petNameText != null)
        {
            petNameText.text = "NO PET";
        }

        if (petLevelText != null)
        {
            petLevelText.text =
                string.Empty;
        }

        SetImage(petDisplayImage, null);
        SetImage(elementIcon, null);

        if (elementIcon != null)
        {
            elementIcon.gameObject.SetActive(false);
        }

        // Trả nền PetDisplayArea về nền mặc định.
        if (petDisplayAreaUI != null)
        {
            petDisplayAreaUI.Clear();
        }

        if (petStatContentUI != null)
        {
            petStatContentUI.Clear();
        }

        if (skillContentUI != null)
        {
            skillContentUI.Clear();
        }

        if (enhancePetContentUI != null)
        {
            enhancePetContentUI.Clear();
        }

        ClearEvolutionDisplay(
            "Không có Pet."
        );

        UpdateSlotSelection(null);
    }

    #endregion

    #region Hàm hỗ trợ

    private static void SetImage(
        Image target,
        Sprite sprite
    )
    {
        if (target == null)
            return;

        target.sprite = sprite;
        target.enabled = sprite != null;
        target.preserveAspect = true;

        Color imageColor = target.color;
        imageColor.r = 1f;
        imageColor.g = 1f;
        imageColor.b = 1f;
        imageColor.a = 1f;
        target.color = imageColor;
    }

    private static void SetActiveSafe(
        GameObject target,
        bool active
    )
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    #endregion
}