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

    [Header("Danh sách dữ liệu Pet")]
    [SerializeField] private List<PetData> pets = new();

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

        if (pets == null)
            return;

        foreach (PetData pet in pets)
        {
            if (pet == null)
                continue;

            PetSlotUI newSlot = Instantiate(
                petSlotPrefab,
                petListContent
            );

            newSlot.name = $"PetSlot_{pet.PetName}";
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
    }

    #endregion

    #region Chọn Pet

    private void SelectPet(PetData pet)
    {
        if (pet == null || pets == null)
            return;

        int index = pets.IndexOf(pet);

        if (index < 0)
        {
            Debug.LogWarning(
                $"Không tìm thấy pet {pet.PetName} trong danh sách.",
                this
            );
            return;
        }

        SelectPetByIndex(index);
    }

    private void SelectPetByIndex(int index)
    {
        if (pets == null || pets.Count == 0)
        {
            ClearDisplay();
            return;
        }

        if (index < 0 || index >= pets.Count)
        {
            Debug.LogWarning(
                $"PetUIManager: Chỉ số pet {index} không hợp lệ.",
                this
            );
            return;
        }

        PetData selectedPet = pets[index];

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
            skillContentUI.Display(selectedPet);
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
        if (pets == null || pets.Count == 0)
            return -1;

        int index = startIndex;

        for (int i = 0; i < pets.Count; i++)
        {
            index += direction;

            if (index >= pets.Count)
                index = 0;

            if (index < 0)
                index = pets.Count - 1;

            if (pets[index] != null)
                return index;
        }

        return -1;
    }

    #endregion

    #region Hiển thị Pet

    private void UpdateMainDisplay(PetData pet)
    {
        if (pet == null)
        {
            ClearDisplay();
            return;
        }

        if (petNameText != null)
            petNameText.text = pet.PetName;

        if (petLevelText != null)
            petLevelText.text = $"Lv. {pet.Level}";

        SetImage(petDisplayImage, pet.DisplayImage);
        SetImage(elementIcon, pet.ElementIcon);
    }

    private void UpdateSlotSelection(PetData selectedPet)
    {
        foreach (PetSlotUI slot in createdSlots)
        {
            if (slot == null)
                continue;

            bool isSelected = slot.Data == selectedPet;
            slot.SetSelected(isSelected);
        }
    }

    private PetData GetSelectedPet()
    {
        if (pets == null)
            return null;

        if (selectedIndex < 0 || selectedIndex >= pets.Count)
            return null;

        return pets[selectedIndex];
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
            PetData selectedPet = GetSelectedPet();

            if (selectedPet != null)
                skillContentUI.Display(selectedPet);
            else
                skillContentUI.Clear();
        }

        // Khi mở tab Stats, cập nhật lại chỉ số.
        if (tab == PetTab.Stats && petStatContentUI != null)
        {
            PetData selectedPet = GetSelectedPet();

            if (selectedPet != null)
                petStatContentUI.Display(selectedPet);
            else
                petStatContentUI.Clear();
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

        PetData selectedPet = GetSelectedPet();

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