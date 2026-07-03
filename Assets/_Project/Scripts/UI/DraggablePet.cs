using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePet : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public BeastData myBeast;
    [HideInInspector] public PartySlotUI myCurrentSlot;

    private Image image;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isDragging = false;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    [Header("Tùy chỉnh Kích thước")]
    [Tooltip("Kích thước khi nằm trên Đội Hình chính")]
    public Vector2 partySize = new Vector2(250, 250);
    [Tooltip("Kích thước khi nằm dưới Kho chứa")]
    public Vector2 storageSize = new Vector2(80, 80);

    public void Setup(BeastData beast, PartySlotUI slot)
    {
        myBeast = beast;
        myCurrentSlot = slot;
        if (image != null && beast != null)
        {
            image.sprite = beast.frontSprite;
        }

        UpdateSizeBasedOnSlot();
    }

    public void UpdateSizeBasedOnSlot()
    {
        if (myCurrentSlot != null)
        {
            if (myCurrentSlot.slotType == PartySlotUI.SlotType.Party)
            {
                rectTransform.sizeDelta = partySize;
            }
            else
            {
                rectTransform.sizeDelta = storageSize;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Nếu không phải đang kéo thả thì mới tính là Click
        if (!isDragging && myBeast != null)
        {
            PartyUIManager mgr = FindFirstObjectByType<PartyUIManager>();
            if (mgr != null)
            {
                mgr.OpenPetInfo(myBeast);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        parentAfterDrag = transform.parent;
        
        // Nhấc con thú lên trên cùng để không bị đè bởi các UI khác khi kéo
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // Tắt chặn tia raycast để chuột có thể xuyên qua và chạm vào ô thả (Slot) ở dưới
        image.raycastTarget = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // Làm mờ đi một chút khi đang kéo
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Di chuyển hình ảnh theo con trỏ chuột
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector3 globalMousePos))
        {
            rectTransform.position = globalMousePos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        // Trả lại vị trí cũ nếu không thả trúng vào một Slot hợp lệ
        transform.SetParent(parentAfterDrag);
        
        // Bật lại raycast
        image.raycastTarget = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}
