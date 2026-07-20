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

    private readonly List<PetSlotUI> createdSlots =
        new List<PetSlotUI>();

    private int selectedIndex = -1;
    private bool initialized;
    private PetTab currentTab = PetTab.Stats;

    private void Awake()
    {
        RegisterButtons();
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

        /*
         * PetUIManager không được nằm trong object bị tắt.
         * Trong cấu trúc của bạn, Pet UI Root nên là PetPanel.
         */
        bool managerInsideRoot =
            petUIRoot == gameObject ||
            transform.IsChildOf(petUIRoot.transform);

        if (managerInsideRoot)
        {
            Debug.LogError(
                "[PetUIManager] PetUIManager đang nằm bên trong " +
                "Pet UI Root. Hãy gắn PetUIManager lên object " +
                "PetUI bên ngoài PetPanel.",
                this
            );
            return;
        }

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

        /*
         * BeastData chưa có Sprite riêng cho icon nguyên tố.
         * Tạm thời ẩn elementIcon.
         */
        SetImage(elementIcon, null);

        if (elementIcon != null)
        {
            elementIcon.gameObject.SetActive(false);
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
        Debug.Log(
            "[PetUIManager] Đã bấm nút mở Pet UI."
        );

        if (petUIRoot == null)
        {
            Debug.LogError(
                "[PetUIManager] Chưa gán Pet UI Root.",
                this
            );
            return;
        }

        petUIRoot.SetActive(true);
        petUIRoot.transform.SetAsLastSibling();

        if (!initialized)
        {
            Initialize();
            return;
        }

        RuntimeBeastData selectedPet =
            GetSelectedPet();

        if (selectedPet != null)
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

        ShowTab(currentTab);
    }

    public void ClosePetUI()
    {
        if (petUIRoot == null)
        {
            Debug.LogWarning(
                "[PetUIManager] Chưa gán Pet UI Root.",
                this
            );
            return;
        }

        // Chỉ tắt PetPanel.
        petUIRoot.SetActive(false);
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