using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public Image itemImage;
    public TMP_Text priceText;
    public Button buyButton;

    private ShopItemData itemData;

    public void Setup(ShopItemData data)
    {
        itemData = data;

        itemImage.sprite = data.itemImage;
        priceText.text = data.price.ToString() + " Gold";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuyItem);
    }

    void BuyItem()
    {
        Debug.Log("Đã mua: " + itemData.itemName);

        // Sau này trừ tiền ở đây
        // PlayerGold -= itemData.price;
    }
}