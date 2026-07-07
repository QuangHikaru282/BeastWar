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

    private void OnEnable()
    {
        // Tự động đưa Panel lên trên cùng của Canvas để không bị các UI khác che khuất
        transform.SetAsLastSibling();
    }

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
        RuntimeBeastData runtimeBeast = new RuntimeBeastData(chosenBeast, 1);
        playerData.AddBeast(runtimeBeast);
        Debug.Log("Bạn đã nhận được Pet: " + chosenBeast.beastName);

        // 2. Tự động đưa vào đội hình chiến đấu luôn nếu đội hình đang trống
        if (playerData.currentFormation.Count < PlayerData.MaxFormationSize)
        {
            playerData.currentFormation.Add(runtimeBeast);
            Debug.Log("Đã tự động thêm " + chosenBeast.beastName + " vào đội hình chiến đấu!");
        }

        // 3. Đánh dấu trưởng làng đã cho quà
        if (elderNPC != null)
        {
            elderNPC.hasGivenStarter = true;
        }

        // Hoàn thành Nhiệm vụ 0 và phát phần thưởng (Cuốc + Hạt giống)
        if (global::QuestManager.Instance != null && playerData.currentMainQuestId == 0)
        {
            global::QuestManager.Instance.AdvanceQuest();
        }
        else if (playerData.currentMainQuestId == 0)
        {
            playerData.currentMainQuestId = 1;
        }

        // Mở lại di chuyển của người chơi (nếu lúc nãy đã khóa)
        // PlayerMovement playerMovement = Player.Instance.gameObject.GetComponent<PlayerMovement>();
        // if (playerMovement != null) playerMovement.isUsingTools = false;

        // 4. Đóng bảng UI
        gameObject.SetActive(false);
    }
}
