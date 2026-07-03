using UnityEngine;
using Kinnly;
using System.Linq;

public class FarmWaterPoint : MonoBehaviour, IInteractable
{
    [Header("Feedback")]
    public string successMessage = "Đã gán Thú hệ Nước để tưới cây tự động!";
    public string failMessage = "Bạn cần có Thú hệ Nước (Water) trong đội hình để gán vào vị trí này!";
    public string alreadyDoneMessage = "Thú hệ Nước đang làm việc chăm chỉ ở đây rồi!";

    private bool hasAssignedBeast = false;

    public void Interact(PlayerInventory playerInventory)
    {
        if (hasAssignedBeast)
        {
            Debug.Log($"<color=cyan>[Farm]</color> {alreadyDoneMessage}");
            return;
        }

        // Kiểm tra xem đội hình có Thú hệ Nước không
        bool hasWaterBeast = false;
        
        if (global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null)
        {
            var formation = global::QuestManager.Instance.playerData.currentFormation;
            if (formation != null)
            {
                foreach (var beast in formation)
                {
                    if (beast != null && beast.element == BeastElement.Water)
                    {
                        hasWaterBeast = true;
                        break;
                    }
                }
            }
        }

        if (hasWaterBeast)
        {
            hasAssignedBeast = true;
            Debug.Log($"<color=green>[Farm]</color> {successMessage}");

            if (global::QuestManager.Instance != null)
            {
                global::QuestManager.Instance.OnWaterBeastAssigned();
            }
        }
        else
        {
            Debug.Log($"<color=orange>[Farm]</color> {failMessage}");
        }
    }
}
