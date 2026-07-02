using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("Các thành phần của item")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text ownedText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button itemButton;

    private ShopItemData itemData;
    private ShopManager shopManager;

    private void Awake()
    {
        // Nếu chưa kéo Button vào Inspector,
        // code sẽ thử lấy Button trên object gốc.
        if (itemButton == null)
        {
            itemButton = GetComponent<Button>();
        }
    }

    public void Setup(ShopItemData data, ShopManager manager)
    {
        itemData = data;
        shopManager = manager;

        if (icon != null)
        {
            icon.sprite = data.icon;
            icon.enabled = data.icon != null;
            icon.preserveAspect = true;
        }

        if (itemNameText != null)
        {
            itemNameText.text = data.itemName;
        }

        if (priceText != null)
        {
            priceText.text = data.price.ToString("N0") + " G";
        }

        RefreshOwned();

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(SelectThisItem);
        }
    }

    public void RefreshOwned()
    {
        if (ownedText != null && itemData != null)
        {
            ownedText.text = "[Owned: " + itemData.owned + "]";
        }
    }

    private void SelectThisItem()
    {
        if (shopManager != null && itemData != null)
        {
            shopManager.SelectItem(itemData);
        }
    }
}