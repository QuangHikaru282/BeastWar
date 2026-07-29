using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StarterSelectionUI : MonoBehaviour
{
    [Header("Data References")]
    [Tooltip("Kéo file PlayerData trong thư mục Data vào đây")]
    public PlayerData playerData;

    [Tooltip("Kéo NPC Trưởng làng vào đây để đánh dấu trạng thái")]
    public ElderNPC elderNPC;

    [Header("3 Pets Khởi Đầu")]
    public BeastData starter1;
    public BeastData starter2;
    public BeastData starter3;

    [Header("Hiệu ứng ô chọn Pet")]
    [Tooltip("Khoảng cách ô Pet nhích lên khi rê chuột")]
    [SerializeField] private float moveUpDistance = 15f;

    [Tooltip("Tốc độ di chuyển của hiệu ứng")]
    [SerializeField] private float moveSpeed = 10f;

    [Tooltip("Bấm lần đầu để hiện Pet, bấm lần hai để chọn Pet")]
    [SerializeField] private bool confirmOnSecondClick = true;

    private readonly List<StarterSlotData> starterSlots = new();

    private bool hasInitialized;
    private bool hasChosenStarter;

    private readonly List<GameObject> hiddenUIElements = new();

    private void OnEnable()
    {
        // Đưa bảng chọn Pet lên trên các UI khác
        transform.SetAsLastSibling();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 9999;
        }

        // Ẩn tất cả các UI khác khi đang chọn Pet khởi đầu
        SetOtherUIActive(false);

        // Panel có thể bị tắt lúc bắt đầu,
        // vì vậy khởi tạo khi Panel được mở
        if (!hasInitialized)
        {
            InitializeStarterSlots();
        }

        if (!hasChosenStarter)
        {
            ResetStarterSlots();
        }

        Debug.Log("Đã mở bảng chọn Pet khởi đầu.");
    }

    private void OnDisable()
    {
        // Hiện lại các UI khác khi bảng đóng
        SetOtherUIActive(true);
    }

    private void SetOtherUIActive(bool active)
    {
        if (!active)
        {
            hiddenUIElements.Clear();
            Canvas myCanvas = GetComponentInParent<Canvas>();

            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in allCanvases)
            {
                if (canvas == null) continue;
                if (myCanvas != null && canvas == myCanvas) continue;
                if (canvas.gameObject == gameObject || canvas.transform.IsChildOf(transform)) continue;

                if (canvas.gameObject.activeSelf)
                {
                    canvas.gameObject.SetActive(false);
                    hiddenUIElements.Add(canvas.gameObject);
                }
            }

            string[] hudNames = new string[] { "OpenPetButton", "OpenPanelQuest", "QuestArrow", "TimePanel", "Mở Sảnh Đội", "Toolbar", "PlayerHUD", "PETUI", "QuestPanel" };
            foreach (var name in hudNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null && obj.activeSelf)
                {
                    obj.SetActive(false);
                    if (!hiddenUIElements.Contains(obj)) hiddenUIElements.Add(obj);
                }
            }
        }
        else
        {
            foreach (var obj in hiddenUIElements)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
            hiddenUIElements.Clear();
        }
    }

    private void Update()
    {
        UpdateSlotMovement();
    }

    private void InitializeStarterSlots()
    {
        if (hasInitialized)
            return;

        /*
         * Tìm toàn bộ Button nằm bên dưới StarterSelectionPanel.
         *
         * Thứ tự trong Hierarchy cần là:
         *
         * StarterSelectionPanel
         * ├── Button
         * │   └── Image
         * ├── Button (1)
         * │   └── Image
         * └── Button (2)
         *     └── Image
         */

        Button[] allButtons = GetComponentsInChildren<Button>(true);

        if (allButtons.Length < 3)
        {
            Debug.LogError(
                "StarterSelectionUI không tìm thấy đủ 3 Button chọn Pet!"
            );

            return;
        }

        // Chỉ lấy ba Button đầu tiên
        for (int i = 0; i < 3; i++)
        {
            Button button = allButtons[i];

            Image petImage = FindPetImage(button.transform);

            if (petImage == null)
            {
                Debug.LogError(
                    "Không tìm thấy Image Pet bên trong Button: "
                    + button.name
                );

                continue;
            }

            RectTransform buttonRect =
                button.GetComponent<RectTransform>();

            StarterSlotData slotData = new StarterSlotData
            {
                button = button,
                petImage = petImage,
                rectTransform = buttonRect,
                originalPosition = buttonRect.anchoredPosition,
                targetPosition = buttonRect.anchoredPosition,
                petIsVisible = false,
                pointerInside = false
            };

            starterSlots.Add(slotData);

            // Tự động thêm thành phần nhận sự kiện chuột
            StarterSlotHoverHandler hoverHandler =
                button.GetComponent<StarterSlotHoverHandler>();

            if (hoverHandler == null)
            {
                hoverHandler =
                    button.gameObject.AddComponent<StarterSlotHoverHandler>();
            }

            hoverHandler.Setup(this, slotData);
        }

        hasInitialized = starterSlots.Count == 3;

        if (!hasInitialized)
        {
            Debug.LogError(
                "Không thể khởi tạo đủ 3 ô chọn Pet. " +
                "Hãy kiểm tra Image con bên trong mỗi Button."
            );

            return;
        }

        ResetStarterSlots();
    }

    private Image FindPetImage(Transform buttonTransform)
    {
        Image[] images =
            buttonTransform.GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            // Bỏ qua Image của chính Button,
            // vì Image đó là khung vuông
            if (image.transform == buttonTransform)
                continue;

            // Image con đầu tiên được xem là hình Pet
            return image;
        }

        return null;
    }

    private void ResetStarterSlots()
    {
        BeastData[] starters = new BeastData[] { starter1, starter2, starter3 };

        for (int i = 0; i < starterSlots.Count; i++)
        {
            StarterSlotData slot = starterSlots[i];
            BeastData data = i < starters.Length ? starters[i] : null;

            if (slot.petImage != null)
            {
                if (data != null && data.frontSprite != null)
                {
                    slot.petImage.sprite = data.frontSprite;
                }
                slot.petImage.gameObject.SetActive(false);
            }

            slot.petIsVisible = false;
            slot.pointerInside = false;
            slot.targetPosition = slot.originalPosition;

            if (slot.rectTransform != null)
            {
                slot.rectTransform.anchoredPosition =
                    slot.originalPosition;
            }
        }
    }

    private void UpdateSlotMovement()
    {
        foreach (StarterSlotData slot in starterSlots)
        {
            if (slot.rectTransform == null)
                continue;

            slot.rectTransform.anchoredPosition = Vector2.Lerp(
                slot.rectTransform.anchoredPosition,
                slot.targetPosition,
                Time.unscaledDeltaTime * moveSpeed
            );
        }
    }

    public void HandlePointerEnter(StarterSlotData slot)
    {
        if (slot == null)
            return;

        slot.pointerInside = true;

        // Chỉ nhích lên khi hình Pet đã được mở
        if (slot.petIsVisible)
        {
            slot.targetPosition =
                slot.originalPosition
                + Vector2.up * moveUpDistance;
        }
    }

    public void HandlePointerExit(StarterSlotData slot)
    {
        if (slot == null)
            return;

        slot.pointerInside = false;
        slot.targetPosition = slot.originalPosition;
    }

    private void HandleStarterClick(
        int slotIndex,
        BeastData chosenBeast
    )
    {
        if (hasChosenStarter)
            return;

        if (!hasInitialized)
        {
            InitializeStarterSlots();
        }

        if (
            slotIndex < 0
            || slotIndex >= starterSlots.Count
        )
        {
            Debug.LogError(
                "Không tìm thấy ô chọn Pet số "
                + (slotIndex + 1)
            );

            return;
        }

        StarterSlotData slot = starterSlots[slotIndex];

        // Lần bấm đầu tiên: hiện hình Pet
        if (!slot.petIsVisible)
        {
            if (chosenBeast != null && chosenBeast.frontSprite != null)
            {
                slot.petImage.sprite = chosenBeast.frontSprite;
            }

            slot.petImage.gameObject.SetActive(true);
            slot.petIsVisible = true;

            if (slot.pointerInside)
            {
                slot.targetPosition =
                    slot.originalPosition
                    + Vector2.up * moveUpDistance;
            }

            Debug.Log(
                "Đã hiển thị Pet số "
                + (slotIndex + 1)
                + ". Bấm thêm lần nữa để chọn."
            );

            if (confirmOnSecondClick)
            {
                return;
            }
        }

        // Lần bấm thứ hai: nhận Pet
        GivePet(chosenBeast);
    }

    public void ChooseStarter1()
    {
        HandleStarterClick(0, starter1);
    }

    public void ChooseStarter2()
    {
        HandleStarterClick(1, starter2);
    }

    public void ChooseStarter3()
    {
        HandleStarterClick(2, starter3);
    }

    private void GivePet(BeastData chosenBeast)
    {
        if (hasChosenStarter)
            return;

        if (playerData == null)
        {
            Debug.LogError(
                "Chưa gán PlayerData cho StarterSelectionUI!"
            );

            return;
        }

        if (chosenBeast == null)
        {
            Debug.LogError(
                "Chưa gán BeastData cho nút chọn!"
            );

            return;
        }

        hasChosenStarter = true;

        // 1. Thêm vào kho thú cưng
        RuntimeBeastData runtimeBeast =
            new RuntimeBeastData(chosenBeast, 1);

        playerData.AddBeast(runtimeBeast);

        Debug.Log(
            "Bạn đã nhận được Pet: "
            + chosenBeast.beastName
        );

        // 2. Tự động đưa vào đội hình chiến đấu
        // nếu đội hình đang còn chỗ
        if (
            playerData.currentFormation.Count
            < PlayerData.MaxFormationSize
        )
        {
            playerData.currentFormation.Add(runtimeBeast);

            Debug.Log(
                "Đã tự động thêm "
                + chosenBeast.beastName
                + " vào đội hình chiến đấu!"
            );
        }

        // 3. Đánh dấu trưởng làng đã cho quà
        if (elderNPC == null)
        {
            elderNPC = FindFirstObjectByType<ElderNPC>();
        }

        if (elderNPC != null)
        {
            elderNPC.hasGivenStarter = true;
        }

        // Đánh dấu nhiệm vụ 0 đã hoàn thành mục tiêu (sẵn sàng bấm Nhận Thưởng trên Bảng Nhiệm Vụ)
        if (global::QuestManager.Instance != null && playerData.currentMainQuestId == 0)
        {
            global::QuestManager.Instance.MarkCurrentQuestCompleted();
        }

        // Mở lại di chuyển của người chơi nếu cần
        // PlayerMovement playerMovement =
        //     Player.Instance.gameObject
        //     .GetComponent<PlayerMovement>();
        //
        // if (playerMovement != null)
        // {
        //     playerMovement.isUsingTools = false;
        // }

        // 4. Đóng bảng UI
        gameObject.SetActive(false);
    }
}

/*
 * Lớp lưu dữ liệu cho từng ô chọn Pet.
 * Không cần gắn lớp này thủ công trong Inspector.
 */
[System.Serializable]
public class StarterSlotData
{
    public Button button;
    public Image petImage;
    public RectTransform rectTransform;

    public Vector2 originalPosition;
    public Vector2 targetPosition;

    public bool petIsVisible;
    public bool pointerInside;
}

/*
 * Lớp xử lý rê chuột.
 * Script StarterSelectionUI sẽ tự động thêm lớp này vào Button.
 */
public class StarterSlotHoverHandler :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private StarterSelectionUI selectionUI;
    private StarterSlotData slotData;

    public void Setup(
        StarterSelectionUI ui,
        StarterSlotData data
    )
    {
        selectionUI = ui;
        slotData = data;
    }

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        if (selectionUI != null)
        {
            selectionUI.HandlePointerEnter(slotData);
        }
    }

    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        if (selectionUI != null)
        {
            selectionUI.HandlePointerExit(slotData);
        }
    }
}