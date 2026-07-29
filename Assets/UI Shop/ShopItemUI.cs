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

    [Header("Cấu hình tỉ lệ Icon")]
    [Tooltip("Kích thước tối đa của Icon trong ô")]
    [SerializeField] private Vector2 maxIconSize = new Vector2(36f, 36f);
    [Tooltip("Bật V nếu muốn ép vị trí bằng code. Tắt V (false) nếu muốn tự chỉnh vị trí thủ công trên Unity Inspector.")]
    [SerializeField] private bool autoFitIcon = false;

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
            icon.raycastTarget = false;

            if (autoFitIcon && icon.rectTransform != null)
            {
                icon.rectTransform.anchorMin = new Vector2(0.5f, 0.70f);
                icon.rectTransform.anchorMax = new Vector2(0.5f, 0.70f);
                icon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                icon.rectTransform.anchoredPosition = Vector2.zero;
                icon.rectTransform.sizeDelta = maxIconSize;
            }
        }

        if (itemNameText != null)
        {
            itemNameText.text = data.itemName;
            itemNameText.alignment = TextAlignmentOptions.Center;
            itemNameText.fontSize = 13.5f;
            itemNameText.raycastTarget = false;

            if (autoFitIcon && itemNameText.rectTransform != null)
            {
                itemNameText.rectTransform.anchorMin = new Vector2(0.5f, 0.44f);
                itemNameText.rectTransform.anchorMax = new Vector2(0.5f, 0.44f);
                itemNameText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                itemNameText.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        if (priceText != null)
        {
            priceText.text = data.price.ToString("N0") + " G";
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.fontSize = 12f;
            priceText.raycastTarget = false;

            if (autoFitIcon && priceText.rectTransform != null)
            {
                priceRectAnchor(priceText.rectTransform, 0.16f);
            }
        }

        RefreshOwned();

        if (itemButton == null)
        {
            itemButton = GetComponent<Button>();
            if (itemButton == null) itemButton = GetComponentInChildren<Button>();
        }

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(SelectThisItem);
        }
    }

    private void priceRectAnchor(RectTransform rect, float posY)
    {
        rect.anchorMin = new Vector2(0.5f, posY);
        rect.anchorMax = new Vector2(0.5f, posY);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    public void RefreshOwned()
    {
        if (ownedText != null && itemData != null)
        {
            ownedText.text = "[Owned: " + itemData.owned + "]";
            ownedText.alignment = TextAlignmentOptions.Center;
            ownedText.fontSize = 11f;
            ownedText.raycastTarget = false;

            if (autoFitIcon && ownedText.rectTransform != null)
            {
                priceRectAnchor(ownedText.rectTransform, 0.28f);
            }
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