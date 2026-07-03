using UnityEngine;
using UnityEngine.EventSystems;

public class PartySlotUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum SlotType { Party, Storage }
    public SlotType slotType;
    public int partyIndex = -1; // Chỉ định vị trí (0, 1, 2) nếu đây là ô trên Đội Hình

    private PartyUIManager manager;
    private UnityEngine.UI.Image highlightImage;

    private void Awake()
    {
        highlightImage = GetComponent<UnityEngine.UI.Image>();
    }

    public void SetupManager(PartyUIManager mgr)
    {
        manager = mgr;
    }

    // Khi có một vật thể (chuột) kéo lướt qua ô này -> Highlight sáng lên
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && highlightImage != null)
        {
            highlightImage.color = new Color(0.8f, 1f, 0.8f, 1f); // Đổi màu xanh nhạt
        }
    }

    // Khi kéo ra khỏi ô -> Tắt highlight
    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImage != null)
        {
            highlightImage.color = Color.white; // Trở lại bình thường
        }
    }

    // Khi thả chuột rơi vào ô này
    public void OnDrop(PointerEventData eventData)
    {
        // Trả lại màu gốc
        if (highlightImage != null)
        {
            highlightImage.color = Color.white;
        }

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggablePet draggable = dropped.GetComponent<DraggablePet>();
        if (draggable != null && manager != null)
        {
            // Báo cho Manager xử lý logic Swap/Chèn
            manager.OnPetDropped(draggable, this);
        }
    }
}
