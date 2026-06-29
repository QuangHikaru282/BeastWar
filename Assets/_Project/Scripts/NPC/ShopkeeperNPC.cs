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
        if (shopManager != null)
        {
            Debug.Log(greetingText);
            shopManager.OpenShop();
        }
        else
        {
            Debug.LogWarning("Chưa gán ShopManager cho NPC Thương Gia!");
        }
    }
}
