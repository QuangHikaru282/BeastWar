using UnityEngine;
using Kinnly;

public class ElderNPC : MonoBehaviour, IInteractable
{
    [Header("UI Component")]
    [Tooltip("Kéo bảng UI StarterSelectionUI vào đây")]
    public GameObject starterSelectionUI;

    [Header("State")]
    public bool hasGivenStarter = false;

    public void Interact(PlayerInventory playerInventory)
    {
        if (!hasGivenStarter && starterSelectionUI != null)
        {
            Debug.Log("Trưởng làng: Làng của chúng ta đang bị quái vật quấy phá. Cháu hãy nhận lấy một Pet khởi đầu và giúp ta giải quyết chúng nhé!");
            starterSelectionUI.SetActive(true);
            
            // Khóa di chuyển của người chơi tạm thời nếu cần
            // PlayerMovement playerMovement = Player.Instance.gameObject.GetComponent<PlayerMovement>();
            // if (playerMovement != null) playerMovement.isUsingTools = true;
        }
        else if (hasGivenStarter)
        {
            Debug.Log("Trưởng làng: Cháu đã nhận bạn đồng hành rồi, chúc cháu lên đường bình an!");
        }
        else
        {
            Debug.LogWarning("Chưa gán giao diện StarterSelectionUI cho NPC Trưởng làng!");
        }
    }
}
