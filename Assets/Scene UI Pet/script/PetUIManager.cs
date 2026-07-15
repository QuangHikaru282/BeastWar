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

    [Header("Bảng chỉ số")]
    [SerializeField] private PetStatContentUI petStatContentUI;

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

    public void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        BuildPetList();
        ShowTab(PetTab.Stats);

        if (pets.Count > 0)
            SelectPetByIndex(0);
        else
            ClearDisplay();
    }

    private void BuildPetList()
    {
        if (petListContent == null)
        {
            Debug.LogError("PetUIManager: Chưa gán PetListContent.");
            return;
        }

        if (petSlotPrefab == null)
        {
            Debug.LogError("PetUIManager: Chưa gán PetSlotPrefab.");
            return;
        }

        for (int i = petListContent.childCount - 1; i >= 0; i--)
            Destroy(petListContent.GetChild(i).gameObject);

        createdSlots.Clear();

        foreach (PetData pet in pets)
        {
            if (pet == null)
                continue;

            PetSlotUI slot = Instantiate(petSlotPrefab, petListContent);
            slot.name = $"PetSlot_{pet.PetName}";
            slot.Setup(pet, SelectPet);

            createdSlots.Add(slot);
        }
    }

    private void SelectPet(PetData pet)
    {
        if (pet == null)
            return;

        int index = pets.IndexOf(pet);

        if (index < 0)
        {
            Debug.LogWarning($"Không tìm thấy pet {pet.PetName} trong danh sách.");
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

        index = Mathf.Clamp(index, 0, pets.Count - 1);

        if (pets[index] == null)
        {
            Debug.LogWarning($"Pet tại vị trí {index} đang bị null.");
            return;
        }

        selectedIndex = index;
        PetData selectedPet = pets[selectedIndex];

        UpdateMainDisplay(selectedPet);
        UpdateSlotSelection(selectedPet);

        if (petStatContentUI != null)
            petStatContentUI.Display(selectedPet);
    }

    private void UpdateMainDisplay(PetData pet)
    {
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
            if (slot != null)
                slot.SetSelected(slot.Data == selectedPet);
        }
    }

    private void SelectPreviousPet()
    {
        if (pets == null || pets.Count == 0)
            return;

        int newIndex = selectedIndex - 1;

        if (newIndex < 0)
            newIndex = pets.Count - 1;

        SelectPetByIndex(newIndex);
    }

    private void SelectNextPet()
    {
        if (pets == null || pets.Count == 0)
            return;

        int newIndex = selectedIndex + 1;

        if (newIndex >= pets.Count)
            newIndex = 0;

        SelectPetByIndex(newIndex);
    }

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
        SetActiveSafe(skillContent, tab == PetTab.Skill);
        SetActiveSafe(petStatContent, tab == PetTab.Stats);
        SetActiveSafe(enhancePetContent, tab == PetTab.Enhance);

        SetActiveSafe(skillSelectedIndicator, tab == PetTab.Skill);
        SetActiveSafe(statSelectedIndicator, tab == PetTab.Stats);
        SetActiveSafe(enhanceSelectedIndicator, tab == PetTab.Enhance);
    }

    public void OpenPetUI()
    {
        if (petUIRoot != null)
            petUIRoot.SetActive(true);

        if (!initialized)
            Initialize();

        if (selectedIndex >= 0 && selectedIndex < pets.Count)
            SelectPetByIndex(selectedIndex);
    }

    public void ClosePetUI()
    {
        if (petUIRoot != null)
            petUIRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }

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
    }

    private static void SetImage(Image target, Sprite sprite)
    {
        if (target == null)
            return;

        target.sprite = sprite;
        target.enabled = sprite != null;
        target.preserveAspect = true;
    }

    private static void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }
}