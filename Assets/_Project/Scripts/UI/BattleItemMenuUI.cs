using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Menu vong tron hien ra cac icon binh thuoc khi hover chuot vao nut Balo.
/// Gan script nay vao nut Backpack (BackpackBtn).
/// </summary>
public class BattleItemMenuUI : MonoBehaviour, IPointerEnterHandler
{
    [Header("Prefab cua tung nut item trong menu vong tron")]
    [SerializeField] private GameObject itemButtonPrefab;

    [Header("Container chua cac nut item")]
    [SerializeField] private Transform menuContainer;

    [Header("Ban kinh vong tron (pixels)")]
    [SerializeField] private float radius = 100f;

    [Header("Goc bat dau phat tia (0 = ben phai)")]
    [SerializeField] private float startAngle = 90f;

    private List<GameObject> spawnedButtons = new List<GameObject>();
    private bool isOpen = false;

    private void Start()
    {
        if (menuContainer != null)
            menuContainer.gameObject.SetActive(false);
        BuildMenu();
    }

    private void BuildMenu()
    {
        if (BattleItemHandler.Instance == null) return;
        if (itemButtonPrefab == null) return;
        if (menuContainer == null) return;

        // Xoa cac nut cu
        foreach (var b in spawnedButtons) Destroy(b);
        spawnedButtons.Clear();

        var items = BattleItemHandler.Instance.BattleItems;
        int count = items.Count;

        for (int i = 0; i < count; i++)
        {
            var item = items[i];
            float angle = startAngle + (i * (360f / count));
            float rad = angle * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            GameObject btn = Instantiate(itemButtonPrefab, menuContainer);
            btn.GetComponent<RectTransform>().anchoredPosition = pos;

            // Dat icon (Tim Image o obj con, bo qua Image hinh nen cua Button)
            Image img = null;
            foreach (Transform child in btn.transform)
            {
                img = child.GetComponent<Image>();
                if (img != null) break;
            }
            if (img != null && item.icon != null) img.sprite = item.icon;

            // Dat tooltip / text
            var txt = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt != null) txt.text = item.displayName;
            var txtLeg = btn.GetComponentInChildren<Text>();
            if (txtLeg != null) txtLeg.text = item.displayName;

            // Gan su kien click
            var capturedItem = item;
            var button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnItemClicked(capturedItem));
            }

            // Mo nhat neu het hang
            int qty = BattleItemHandler.Instance.GetItemCount(item.itemName);
            if (button != null) button.interactable = qty > 0;

            spawnedButtons.Add(btn);
        }
    }

    private void OnItemClicked(BattleItemHandler.BattleItem item)
    {
        BattleItemHandler.Instance?.UseItem(item);
        CloseMenu();
        // Rebuild de cap nhat so luong
        BuildMenu();
    }

    public void OpenMenu()
    {
        if (isOpen) return;
        isOpen = true;
        menuContainer.gameObject.SetActive(true);
        BuildMenu(); // Cap nhat so luong moi nhat

        // Animation mo ra
        foreach (var btn in spawnedButtons)
        {
            var rt = btn.GetComponent<RectTransform>();
            Vector2 target = rt.anchoredPosition;
            rt.anchoredPosition = Vector2.zero;
            rt.DOAnchorPos(target, 0.25f).SetEase(Ease.OutBack);

            var cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.DOFade(1f, 0.2f);
        }
    }

    public void CloseMenu()
    {
        if (!isOpen) return;
        isOpen = false;
        menuContainer.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OpenMenu();
    }

    private void Update()
    {
        if (!isOpen) return;

        // Chuyen doi vi tri chuot tu Screen Space sang Local Space cua nut nay
        // De khong bi anh huong boi do phan giai hay Canvas Scaler
        Canvas canvas = GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = canvas.worldCamera;
        }

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            Input.mousePosition,
            cam,
            out localMousePos);
        
        float distance = localMousePos.magnitude;
        float closeRadius = radius * 2.5f; // Vung an toan (gap 2.5 lan ban kinh) de tha ho di chuot

        if (distance > closeRadius)
        {
            CloseMenu();
        }
    }
}
