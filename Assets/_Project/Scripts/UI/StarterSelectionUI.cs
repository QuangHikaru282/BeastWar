using UnityEngine;

public class StarterSelectionUI : MonoBehaviour
{
    [Header("Data References")]
    [Tooltip("Kéo file PlayerData (trong thư mục Data) vào đây")]
    public PlayerData playerData;
    
    [Tooltip("Kéo NPC Trưởng làng vào đây để đánh dấu trạng thái")]
    public ElderNPC elderNPC;

    [Header("3 Pets Khởi Đầu")]
    public BeastData starter1;
    public BeastData starter2;
    public BeastData starter3;

    public void ChooseStarter1()
    {
        GivePet(starter1);
    }

    public void ChooseStarter2()
    {
        GivePet(starter2);
    }

    public void ChooseStarter3()
    {
        GivePet(starter3);
    }

    private void GivePet(BeastData chosenBeast)
    {
        if (playerData == null)
        {
            Debug.LogError("Chưa gán PlayerData cho StarterSelectionUI!");
            return;
        }

        if (chosenBeast == null)
        {
            Debug.LogError("Chưa gán BeastData cho nút chọn!");
            return;
        }

        // 1. Thêm vào kho thú cưng (danh sách sở hữu)
        playerData.AddBeast(chosenBeast);
        Debug.Log("Bạn đã nhận được Pet: " + chosenBeast.beastName);

        // 2. Tự động đưa vào đội hình chiến đấu luôn nếu đội hình đang trống
        if (playerData.currentFormation.Count < PlayerData.MaxFormationSize)
        {
            playerData.currentFormation.Add(chosenBeast);
            Debug.Log("Đã tự động thêm " + chosenBeast.beastName + " vào đội hình chiến đấu!");
        }

        // 3. Đánh dấu trưởng làng đã cho quà
        if (elderNPC != null)
        {
            elderNPC.hasGivenStarter = true;
        }

        // Bắt đầu nhiệm vụ thu phục Pet (nếu chưa làm)
        if (playerData.tutorialQuestStage == 0)
        {
            playerData.tutorialQuestStage = 1;
            Debug.Log("Bắt đầu nhiệm vụ: Thu phục thú cưng!");
        }

        // Mở lại di chuyển của người chơi (nếu lúc nãy đã khóa)
        // PlayerMovement playerMovement = Player.Instance.gameObject.GetComponent<PlayerMovement>();
        // if (playerMovement != null) playerMovement.isUsingTools = false;

        // 4. Đóng bảng UI
        gameObject.SetActive(false);
    }
}
