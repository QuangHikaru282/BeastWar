using UnityEngine;
using Kinnly;

public class ElderNPC : MonoBehaviour, IInteractable
{
    [Header("UI Component")]
    [Tooltip("Kéo GameObject StarterSelectionPanel vào đây")]
    public GameObject starterSelectionUI;

    [Header("State")]
    public bool hasGivenStarter = false;

    [Header("Tương tác bằng phím F")]
    [Tooltip("Phím dùng để tương tác với NPC")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Tooltip("Đối tượng thông báo 'Nhấn F' khi đứng gần NPC")]
    [SerializeField] private GameObject interactPrompt;

    private bool playerIsNearby;
    private PlayerInventory nearbyPlayerInventory;

    private void Start()
    {
        // Ẩn bảng chọn Pet khi vừa bắt đầu game
        if (starterSelectionUI != null)
        {
            starterSelectionUI.SetActive(false);
        }

        // Ẩn thông báo nhấn F
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerIsNearby)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            Interact(nearbyPlayerInventory);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerIsNearby = true;

        nearbyPlayerInventory =
            other.GetComponent<PlayerInventory>();

        if (nearbyPlayerInventory == null)
        {
            nearbyPlayerInventory =
                other.GetComponentInParent<PlayerInventory>();
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(true);
        }

        Debug.Log("Đã đến gần Trưởng làng. Nhấn F để tương tác.");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerIsNearby = false;
        nearbyPlayerInventory = null;

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    public void Interact(PlayerInventory playerInventory)
    {
        // 1. Giao Pet khởi đầu
        if (!hasGivenStarter)
        {
            if (starterSelectionUI == null)
            {
                Debug.LogWarning(
                    "Chưa gán giao diện StarterSelectionUI " +
                    "cho NPC Trưởng làng!"
                );

                return;
            }

            Debug.Log(
                "Trưởng làng: Làng của chúng ta đang bị quái vật " +
                "quấy phá. Cháu hãy nhận lấy một Pet khởi đầu và " +
                "giúp ta giải quyết chúng nhé!"
            );

            starterSelectionUI.SetActive(true);
            starterSelectionUI.transform.SetAsLastSibling();

            // Khi bảng mở thì ẩn thông báo nhấn F
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            Debug.Log("Đã mở bảng chọn Pet khởi đầu.");
            return;
        }

        // 2. Kiểm tra Quest 16: thu thập 1000 vàng
        if (
            hasGivenStarter
            && global::QuestManager.Instance != null
            && global::QuestManager.Instance.playerData != null
            && global::QuestManager.Instance
                .playerData.currentMainQuestId == 16
        )
        {
            var pData =
                global::QuestManager.Instance.playerData;

            if (pData.gold >= 1000)
            {
                pData.gold -= 1000;

                Debug.Log(
                    "Trưởng làng: Tuyệt vời! Cháu đã mang về đủ " +
                    "1000 vàng. Ta sẽ dùng số tiền này để mở rộng " +
                    "Nông Trại cho cháu!"
                );

                global::QuestManager.Instance.AdvanceQuest();
            }
            else
            {
                Debug.Log(
                    $"Trưởng làng: Cháu vẫn chưa đủ 1000 vàng. " +
                    $"Hiện tại cháu mới có {pData.gold} vàng thôi. " +
                    "Hãy cố gắng lên nhé!"
                );
            }

            return;
        }

        // 3. Thoại bình thường
        if (hasGivenStarter)
        {
            Debug.Log(
                "Trưởng làng: Cháu đã nhận bạn đồng hành rồi, " +
                "chúc cháu lên đường bình an!"
            );
        }
    }
}