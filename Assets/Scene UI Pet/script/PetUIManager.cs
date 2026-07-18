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

    [Header("Dữ liệu Người chơi (Thay cho list tĩnh)")]
    public PlayerData playerData;

    [Header("Danh sách Pet bên trái")]
    [SerializeField] private Transform petListContent;
    [SerializeField] private PetSlotUI petSlotPrefab;

    [Header("Pet hiển thị ở giữa")]
    [SerializeField] private Image petDisplayImage;
    [SerializeField] private Image elementIcon;
    [SerializeField] private TMP_Text petNameText;
    [SerializeField] private TMP_Text petLevelText;

    [Header("Nội dung chỉ số Pet")]
    [SerializeField] private PetStatContentUI petStatContentUI;

    [Header("Nội dung Skill")]
    [Tooltip("Kéo object SkillContent có component SkillContentUI vào đây.")]
    [SerializeField] private SkillContentUI skillContentUI;

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

    [Header("Tiến Hóa (Enhance Tab)")]
    [SerializeField] private Button evolveButton;
    [SerializeField] private TMP_Text evolveCostText;
    [SerializeField] private TMP_Text evolveLevelReqText;
    [SerializeField] private Image evolveTargetImage;
    [SerializeField] private TMP_Text evolveTargetNameText;
    [SerializeField] private TMP_Text evolveWarningText;

    [Header("Đóng giao diện")]
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject petUIRoot;

    private readonly List<PetSlotUI> createdSlots = new();

    private int selectedIndex = -1;
    private bool initialized;

    private void Awake()
    {
        RegisterButtons();
    }

    private void Start()
    {
        Initialize();
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

        // Mặc định mở bảng Pet Stats.
        ShowTab(PetTab.Stats);

        int firstPetIndex = FindNextValidPetIndex(-1, 1);

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
                "PetUIManager: Chưa gán Pet List Content.",
                this
            );
            return;
        }

        if (petSlotPrefab == null)
        {
            Debug.LogError(
                "PetUIManager: Chưa gán Pet Slot Prefab.",
                this
            );
            return;
        }

        // Xóa các PetSlot cũ trong danh sách.
        for (int i = petListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(petListContent.GetChild(i).gameObject);
        }

        createdSlots.Clear();

        if (playerData == null || playerData.ownedBeasts == null)
            return;

        foreach (RuntimeBeastData pet in playerData.ownedBeasts)
        {
            if (pet == null || pet.baseBeast == null)
                continue;

            PetSlotUI newSlot = Instantiate(
                petSlotPrefab,
                petListContent
            );

            newSlot.name = $"PetSlot_{pet.baseBeast.beastName}";
            newSlot.Setup(pet, SelectPet);

            createdSlots.Add(newSlot);
        }
    }

    #endregion

    #region Đăng ký Button

    private void RegisterButtons()
    {
        if (previousPetButton != null)
            previousPetButton.onClick.AddListener(SelectPreviousPet);

        if (nextPetButton != null)
            nextPetButton.onClick.AddListener(SelectNextPet);

        if (skillButton != null)
            skillButton.onClick.AddListener(ShowSkillTab);

        if (petStatButton != null)
            petStatButton.onClick.AddListener(ShowStatsTab);

        if (enhancePetButton != null)
            enhancePetButton.onClick.AddListener(ShowEnhanceTab);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePetUI);
            
        if (evolveButton != null)
            evolveButton.onClick.AddListener(OnEvolveButtonClicked);
    }

    private void UnregisterButtons()
    {
        if (previousPetButton != null)
            previousPetButton.onClick.RemoveListener(SelectPreviousPet);

        if (nextPetButton != null)
            nextPetButton.onClick.RemoveListener(SelectNextPet);

        if (skillButton != null)
            skillButton.onClick.RemoveListener(ShowSkillTab);

        if (petStatButton != null)
            petStatButton.onClick.RemoveListener(ShowStatsTab);

        if (enhancePetButton != null)
            enhancePetButton.onClick.RemoveListener(ShowEnhanceTab);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePetUI);
            
        if (evolveButton != null)
            evolveButton.onClick.RemoveListener(OnEvolveButtonClicked);
    }

    #endregion

    #region Chọn Pet

    private void SelectPet(RuntimeBeastData pet)
    {
        if (pet == null || playerData == null || playerData.ownedBeasts == null)
            return;

        int index = playerData.ownedBeasts.IndexOf(pet);

        if (index < 0)
        {
            Debug.LogWarning(
                $"Không tìm thấy pet {pet.baseBeast.beastName} trong danh sách.",
                this
            );
            return;
        }

        SelectPetByIndex(index);
    }

    private void SelectPetByIndex(int index)
    {
        if (playerData == null || playerData.ownedBeasts == null || playerData.ownedBeasts.Count == 0)
        {
            ClearDisplay();
            return;
        }

        if (index < 0 || index >= playerData.ownedBeasts.Count)
        {
            Debug.LogWarning(
                $"PetUIManager: Chỉ số pet {index} không hợp lệ.",
                this
            );
            return;
        }

        RuntimeBeastData selectedPet = playerData.ownedBeasts[index];

        if (selectedPet == null)
        {
            Debug.LogWarning(
                $"Pet tại vị trí {index} đang bị null.",
                this
            );
            return;
        }

        selectedIndex = index;

        // Cập nhật ảnh, tên và level ở giữa.
        UpdateMainDisplay(selectedPet);

        // Cập nhật viền PetSlot đang chọn.
        UpdateSlotSelection(selectedPet);

        // Cập nhật bảng Stats.
        if (petStatContentUI != null)
            petStatContentUI.Display(selectedPet);

        // Cập nhật 4 ô Skill cố định.
        // SkillContentUI sẽ tự ẩn những ô không có skill.
        if (skillContentUI != null)
            skillContentUI.Display(selectedPet, playerData, OnSkillUpgradedCallback);
    }

    private void OnSkillUpgradedCallback()
    {
        // Khi skill được nâng cấp, có thể cần update lại 1 số thứ, tạm thời để trống
    }

    private void SelectPreviousPet()
    {
        int newIndex = FindNextValidPetIndex(
            selectedIndex,
            -1
        );

        if (newIndex >= 0)
            SelectPetByIndex(newIndex);
    }

    private void SelectNextPet()
    {
        int newIndex = FindNextValidPetIndex(
            selectedIndex,
            1
        );

        if (newIndex >= 0)
            SelectPetByIndex(newIndex);
    }

    /// <summary>
    /// Tìm PetData tiếp theo không bị null.
    /// direction = 1: đi tới.
    /// direction = -1: đi lùi.
    /// </summary>
    private int FindNextValidPetIndex(
        int startIndex,
        int direction
    )
    {
        if (playerData == null || playerData.ownedBeasts == null || playerData.ownedBeasts.Count == 0)
            return -1;

        int index = startIndex;

        for (int i = 0; i < playerData.ownedBeasts.Count; i++)
        {
            index += direction;

            if (index >= playerData.ownedBeasts.Count)
                index = 0;

            if (index < 0)
                index = playerData.ownedBeasts.Count - 1;

            if (playerData.ownedBeasts[index] != null)
                return index;
        }

        return -1;
    }

    #endregion

    #region Hiển thị Pet

    private void UpdateMainDisplay(RuntimeBeastData pet)
    {
        if (pet == null || pet.baseBeast == null)
        {
            ClearDisplay();
            return;
        }

        if (petNameText != null)
            petNameText.text = pet.baseBeast.beastName;

        if (petLevelText != null)
            petLevelText.text = $"Lv. {pet.currentLevel}";

        SetImage(petDisplayImage, pet.baseBeast.frontSprite);
        
        // Element icon: disable temporarily since we don't use sprites for elements
        if (elementIcon != null) elementIcon.gameObject.SetActive(false);
    }

    private void UpdateSlotSelection(RuntimeBeastData selectedPet)
    {
        foreach (PetSlotUI slot in createdSlots)
        {
            if (slot == null)
                continue;

            bool isSelected = slot.Data == selectedPet;
            slot.SetSelected(isSelected);
        }
    }

    private RuntimeBeastData GetSelectedPet()
    {
        if (playerData == null || playerData.ownedBeasts == null)
            return null;

        if (selectedIndex < 0 || selectedIndex >= playerData.ownedBeasts.Count)
            return null;

        return playerData.ownedBeasts[selectedIndex];
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
        // Chỉ bật một bảng nội dung.
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

        // Viền hoặc hiệu ứng nút đang chọn.
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

        // Khi mở tab Skill, cập nhật lại đúng skill của pet hiện tại.
        if (tab == PetTab.Skill && skillContentUI != null)
        {
            RuntimeBeastData selectedPet = GetSelectedPet();

            if (selectedPet != null)
                skillContentUI.Display(selectedPet, playerData, OnSkillUpgradedCallback);
            else
                skillContentUI.Clear();
        }

        // Khi mở tab Stats, cập nhật lại chỉ số.
        if (tab == PetTab.Stats && petStatContentUI != null)
        {
            RuntimeBeastData selectedPet = GetSelectedPet();

            if (selectedPet != null)
                petStatContentUI.Display(selectedPet);
            else
                petStatContentUI.Clear();
        }

        // Khi mở tab Enhance, cập nhật thông tin tiến hóa
        if (tab == PetTab.Enhance)
        {
            RefreshEnhanceTab();
        }
    }

    private void RefreshEnhanceTab()
    {
        RuntimeBeastData selectedPet = GetSelectedPet();

        if (selectedPet == null || selectedPet.baseBeast == null)
        {
            if (evolveWarningText != null) evolveWarningText.text = "Không có thông tin thú.";
            if (evolveButton != null) evolveButton.interactable = false;
            return;
        }

        BeastData evolveTarget = selectedPet.baseBeast.evolveTarget;

        if (evolveTarget == null)
        {
            if (evolveTargetNameText != null) evolveTargetNameText.text = "Đã tối đa";
            if (evolveTargetImage != null)
            {
                evolveTargetImage.sprite = selectedPet.baseBeast.frontSprite; // Hiển thị lại ảnh cũ
                evolveTargetImage.enabled = evolveTargetImage.sprite != null;
            }
            if (evolveCostText != null) evolveCostText.text = "-";
            if (evolveLevelReqText != null) evolveLevelReqText.text = "-";
            if (evolveWarningText != null) evolveWarningText.text = "Thú này không thể tiến hóa thêm.";
            if (evolveButton != null) evolveButton.interactable = false;
            return;
        }

        if (evolveTargetNameText != null) evolveTargetNameText.text = evolveTarget.beastName;
        if (evolveTargetImage != null)
        {
            evolveTargetImage.sprite = evolveTarget.frontSprite;
            evolveTargetImage.enabled = evolveTargetImage.sprite != null;
        }

        int reqLevel = selectedPet.baseBeast.evolveLevel;
        int reqGold = selectedPet.baseBeast.evolveGoldCost;

        if (evolveCostText != null) evolveCostText.text = reqGold.ToString("N0");
        if (evolveLevelReqText != null) evolveLevelReqText.text = $"Yêu cầu: Lv. {reqLevel}";

        bool canEvolve = selectedPet.currentLevel >= reqLevel && playerData != null && playerData.gold >= reqGold;

        if (evolveWarningText != null)
        {
            if (selectedPet.currentLevel < reqLevel)
                evolveWarningText.text = "Chưa đủ cấp độ!";
            else if (playerData == null || playerData.gold < reqGold)
                evolveWarningText.text = "Không đủ Vàng!";
            else
                evolveWarningText.text = "Có thể tiến hóa!";
        }

        if (evolveButton != null)
            evolveButton.interactable = canEvolve;
    }

    private void OnEvolveButtonClicked()
    {
        RuntimeBeastData selectedPet = GetSelectedPet();

        if (selectedPet == null || selectedPet.baseBeast == null || playerData == null)
            return;

        BeastData evolveTarget = selectedPet.baseBeast.evolveTarget;
        if (evolveTarget == null) return;

        int reqLevel = selectedPet.baseBeast.evolveLevel;
        int reqGold = selectedPet.baseBeast.evolveGoldCost;

        if (selectedPet.currentLevel >= reqLevel && playerData.gold >= reqGold)
        {
            // Trừ vàng
            playerData.gold -= reqGold;
            
            // Tiến hóa
            selectedPet.Evolve();
            
            // Lưu dữ liệu
            playerData.Save();

            // Cập nhật lại list và UI
            BuildPetList();
            SelectPetByIndex(selectedIndex); // Chọn lại con thú vừa tiến hóa
            RefreshEnhanceTab();
            
            // Có thể chơi hiệu ứng ăn mừng ở đây
        }
    }

    #endregion

    #region Mở và đóng UI

    public void OpenPetUI()
    {
        if (petUIRoot != null)
        {
            petUIRoot.SetActive(true);
            petUIRoot.transform.SetAsLastSibling();
        }

        if (!initialized)
            Initialize();

        RuntimeBeastData selectedPet = GetSelectedPet();

        if (selectedPet != null)
        {
            SelectPetByIndex(selectedIndex);
        }
        else
        {
            int firstPetIndex = FindNextValidPetIndex(-1, 1);

            if (firstPetIndex >= 0)
                SelectPetByIndex(firstPetIndex);
            else
                ClearDisplay();
        }
    }

    public void ClosePetUI()
    {
        if (petUIRoot != null)
        {
            petUIRoot.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    #endregion

    #region Xóa giao diện

    private void ClearDisplay()
    {
        selectedIndex = -1;

        if (petNameText != null)
            petNameText.text = "NO PET";

        if (petLevelText != null)
            petLevelText.text = string.Empty;

        SetImage(petDisplayImage, null);
        SetImage(elementIcon, null);

        if (petStatContentUI != null)
            petStatContentUI.Clear();

        if (skillContentUI != null)
            skillContentUI.Clear();

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
    }

    private static void SetActiveSafe(
        GameObject target,
        bool active
    )
    {
        if (target != null)
            target.SetActive(active);
    }

    #endregion
}