using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Shop UI")]
    public GameObject shopPanel;

    [Header("Scroll View Content")]
    public RectTransform content;

    [Header("Item Prefab")]
    public GameObject shopItemPrefab;


    public List<ShopItemData> items = new List<ShopItemData>();

    private bool isOpen = false;


    void Start()
    {
        shopPanel.SetActive(false);

        CreateShopItems();
    }



    void CreateShopItems()
    {
        // Xóa item cũ
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }


        // Tạo item
        foreach (ShopItemData data in items)
        {
            GameObject obj = Instantiate(shopItemPrefab);

            // Quan trọng với UI
            obj.transform.SetParent(content, false);


            obj.transform.localScale = Vector3.one;


            ShopItemUI shopUI = obj.GetComponent<ShopItemUI>();

            if (shopUI != null)
            {
                shopUI.Setup(data);
            }
        }


        // Update Layout
        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }



    public void ToggleShop()
    {
        isOpen = !isOpen;

        shopPanel.SetActive(isOpen);
    }



    public void CloseShop()
    {
        isOpen = false;

        shopPanel.SetActive(false);
    }
}