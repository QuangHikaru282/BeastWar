using UnityEngine;
using Kinnly;

/// <summary>
/// Cỗ máy đảo ngược biến hình trong nhà Bill.
/// Khi người chơi nhấn F / click vào máy, máy sẽ kích hoạt chuỗi giải cứu cho Bill.
/// </summary>
public class BillMachineInteractable : MonoBehaviour, IInteractable
{
    [Header("Liên kết NPC Bill")]
    [Tooltip("Kéo Component BillEventNPC trong cùng phòng vào đây")]
    [SerializeField] private BillEventNPC billNPC;

    public void Interact(PlayerInventory playerInventory)
    {
        if (billNPC != null)
        {
            billNPC.TriggerRescueMachine();
        }
        else
        {
            Debug.LogWarning("[BillMachineInteractable] Chưa gán reference BillEventNPC trong Inspector!");
        }
    }
}
