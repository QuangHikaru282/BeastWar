using UnityEngine;
using Kinnly;

public class ShopkeeperNPC : MonoBehaviour, IInteractable
{
    [Header("Shop Component")]
    [Tooltip("Kéo chữ Shopmanager từ trong Prefab vào đây để mở cửa hàng")]
    public ShopManager shopManager;

    [Header("Thoại")]
    public string greetingText = "Thương gia: Chào mừng quý khách! Ngài cần mua gì?";

    public void Interact(PlayerInventory playerInventory)
    {
        if (shopManager == null)
        {
            shopManager = GetComponentInChildren<ShopManager>(true);
            if (shopManager == null)
                shopManager = FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                "Thương Gia",
                string.IsNullOrEmpty(greetingText) ? "Chào mừng quý khách! Ngài cần tìm mua hay bán vật phẩm gì hôm nay?" : greetingText,
                () => {
                    if (shopManager != null)
                    {
                        shopManager.OpenShop();
                    }
                }
            );
        }
        else
        {
            if (shopManager != null) shopManager.OpenShop();
        }
    }
}
