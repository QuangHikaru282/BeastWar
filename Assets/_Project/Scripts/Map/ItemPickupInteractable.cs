using UnityEngine;

/// <summary>
/// Gắn script này vào Item Hóa Thạch hoặc Rương/Vật phẩm rơi trong Hang Động.
/// Khi Player lại gần bấm F -> Nhận Hóa Thạch vào Inventory -> Vật phẩm biến mất.
/// </summary>
public class ItemPickupInteractable : MonoBehaviour, Kinnly.IInteractable
{
    [Header("Cấu hình Vật phẩm")]
    [Tooltip("Kéo file Item Asset vào đây (Ví dụ: HoaThach)")]
    public Kinnly.Item itemToGive;
    public int amount = 1;

    [Header("Thoại nhận đồ")]
    [TextArea(2, 3)]
    public string pickupMessage = "Bạn đã nhặt được Hóa Thạch Cổ Đại!";

    public void Interact(Kinnly.PlayerInventory playerInventory)
    {
        if (playerInventory != null && itemToGive != null)
        {
            playerInventory.AddItem(itemToGive, amount);

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue("Nhặt Vật Phẩm", pickupMessage);
            }

            // Xóa vật phẩm khỏi scene sau khi nhặt
            gameObject.SetActive(false);
        }
    }
}
