using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class NPCShop : MonoBehaviour, Kinnly.IInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject pressFObject;

    [Header("SHOP")]
    [SerializeField] private ShopManager shopManager;

    private readonly HashSet<Collider2D> playerColliders =
        new HashSet<Collider2D>();

    private bool PlayerIsNear
    {
        get
        {
            return playerColliders.Count > 0;
        }
    }

    private void Start()
    {
        UpdatePrompt();
    }

    [Header("Thoại Chào Hỏi")]
    [Tooltip("Tên hiển thị của Thương Gia")]
    [SerializeField] private string npcName = "Thương Gia";

    [Tooltip("Ảnh đại diện (Avatar) của Thương Gia")]
    [SerializeField] private Sprite npcAvatar;

    [Tooltip("Câu chào hỏi trước khi mở Cửa Hàng")]
    [TextArea(2, 4)]
    [SerializeField] private string greetingText = "Chào mừng quý khách đến với Cửa Hàng! Ngài cần tìm mua hay bán vật phẩm gì hôm nay?";

    public void Interact(Kinnly.PlayerInventory playerInventory)
    {
        if (shopManager == null)
        {
            shopManager = GetComponentInChildren<ShopManager>(true);
            if (shopManager == null)
            {
                shopManager = FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
            }
        }

        if (shopManager == null)
        {
            Debug.LogError("NPCShop chưa được gán ShopManager.");
            return;
        }

        // Nếu shop đang mở -> Bấm F sẽ đóng shop
        if (shopManager.IsOpen)
        {
            shopManager.CloseShop();
            return;
        }

        // Nếu shop đang đóng -> Phát câu thoại chào mừng trước, sau đó mới mở Shop
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                npcName,
                greetingText,
                () => {
                    if (shopManager != null)
                    {
                        shopManager.OpenShop();
                    }
                },
                npcAvatar
            );
        }
        else
        {
            shopManager.OpenShop();
        }
    }

    private void Update()
    {
        UpdatePrompt();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerColliders.Add(other);
        UpdatePrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerColliders.Remove(other);

        if (!PlayerIsNear && shopManager != null)
        {
            if (shopManager.IsOpen)
            {
                shopManager.CloseShop();
                InteractHintManager.Instance?.RegisterPanelClose();
            }
        }

        UpdatePrompt();
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            return true;
        }

        Transform root = other.transform.root;

        return root != null && root.CompareTag("Player");
    }

    private void UpdatePrompt()
    {
        if (pressFObject == null)
        {
            return;
        }

        bool shouldShow =
            PlayerIsNear &&
            shopManager != null &&
            !shopManager.IsOpen;

        if (pressFObject.activeSelf != shouldShow)
        {
            pressFObject.SetActive(shouldShow);
        }
    }

    private bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               Keyboard.current.fKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F);
#endif
    }

    private void OnDisable()
    {
        playerColliders.Clear();

        if (pressFObject != null)
        {
            pressFObject.SetActive(false);
        }
    }
}