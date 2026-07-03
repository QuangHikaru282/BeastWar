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
        // 1. Giao Pet Khởi đầu
        if (!hasGivenStarter && starterSelectionUI != null)
        {
            Debug.Log("Trưởng làng: Làng của chúng ta đang bị quái vật quấy phá. Cháu hãy nhận lấy một Pet khởi đầu và giúp ta giải quyết chúng nhé!");
            starterSelectionUI.SetActive(true);
        }
        // 2. Kiểm tra Quest 16 (Thu thập 1000 vàng)
        else if (hasGivenStarter && global::QuestManager.Instance != null && global::QuestManager.Instance.playerData.currentMainQuestId == 16)
        {
            var pData = global::QuestManager.Instance.playerData;
            if (pData.gold >= 1000)
            {
                pData.gold -= 1000;
                Debug.Log("Trưởng làng: Tuyệt vời! Cháu đã mang về đủ 1000 vàng. Ta sẽ dùng số tiền này để mở rộng Nông Trại cho cháu!");
                global::QuestManager.Instance.AdvanceQuest(); // Hoàn thành game!
            }
            else
            {
                Debug.Log($"Trưởng làng: Cháu vẫn chưa đủ 1000 vàng. Hiện tại cháu mới có {pData.gold} vàng thôi. Hãy cố gắng lên nhé!");
            }
        }
        // 3. Thoại bình thường
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
