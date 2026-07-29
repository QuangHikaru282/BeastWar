using UnityEngine;

/// <summary>
/// Hiển thị dấu chấm than (!) màu vàng nhảy nhót trên đầu NPC hoặc Object khi đó là mục tiêu của Nhiệm vụ hiện tại.
/// 
/// Cách dùng:
/// Gắn script này vào NPC hoặc Object ngoài map (ví dụ: Trưởng Làng, Bàn Chế Tạo, Bãi Cỏ...).
/// Đặt targetQuestId là ID nhiệm vụ tương ứng.
/// </summary>
public class QuestMarker : MonoBehaviour
{
    [Header("Cấu hình Quest")]
    [Tooltip("ID nhiệm vụ mà NPC/Object này là mục tiêu (VD: 0 cho Trưởng làng, 16 cho Quest 1000 vàng)")]
    public int targetQuestId = 0;

    [Header("Tùy chỉnh Marker (!/?)")]
    [Tooltip("Khoảng cách Y nổi trên đầu đối tượng (Có thể chỉnh real-time trong Inspector)")]
    public float offsetY = 1.8f;

    [Tooltip("Màu sắc của dấu chấm than (Mặc định: Màu vàng kim)")]
    public Color markerColor = new Color(1f, 0.88f, 0.1f, 1f);

    [Tooltip("Kích thước icon (World scale)")]
    public float markerScale = 0.85f;

    [Header("Tùy chỉnh Sorting Layer")]
    [Tooltip("Sorting Layer Name (Mặc định: WalkBehind để nổi trên nhà cửa và NPC)")]
    public string sortingLayerName = "WalkBehind";

    [Tooltip("Order in Layer (Mặc định: 500 để nổi trên mọi sprite)")]
    public int orderInLayer = 500;

    [Header("Hiệu ứng nảy (Bouncing)")]
    public bool enableBounce = true;
    public float bounceHeight = 0.25f;
    public float bounceSpeed = 5f;

    private GameObject markerObject;
    private SpriteRenderer markerRenderer;
    private Vector3 baseLocalPos;
    private static Sprite cachedExclamationSprite;

    private void Awake()
    {
        CreateMarkerObject();
    }

    private void OnEnable()
    {
        QuestManager.OnQuestAdvanced += HandleQuestUpdated;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestAdvanced -= HandleQuestUpdated;
    }

    private void Start()
    {
        UpdateMarkerVisibility();
    }

    private void Update()
    {
        baseLocalPos = new Vector3(0, offsetY, 0);

        if (markerRenderer != null)
        {
            if (markerRenderer.color != markerColor)
            {
                markerRenderer.color = markerColor;
            }
            if (!string.IsNullOrEmpty(sortingLayerName) && markerRenderer.sortingLayerName != sortingLayerName)
            {
                markerRenderer.sortingLayerName = sortingLayerName;
            }
            if (markerRenderer.sortingOrder != orderInLayer)
            {
                markerRenderer.sortingOrder = orderInLayer;
            }
        }

        if (markerObject != null && markerObject.activeSelf && enableBounce)
        {
            // Hiệu ứng nhún nảy mượt mà + nhẹ nhàng co giãn (Pulsing)
            float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            float pulse = 1f + Mathf.Sin(Time.time * bounceSpeed * 1.5f) * 0.08f;

            markerObject.transform.localPosition = new Vector3(baseLocalPos.x, baseLocalPos.y + bounce, baseLocalPos.z);
            markerObject.transform.localScale = Vector3.one * (markerScale * pulse);
        }
    }

    public void UpdateMarkerVisibility()
    {
        if (markerObject == null) return;

        bool shouldShow = false;

        if (QuestManager.Instance != null && QuestManager.Instance.playerData != null)
        {
            int currentQuestId = QuestManager.Instance.playerData.currentMainQuestId;
            shouldShow = (currentQuestId == targetQuestId);
        }

        markerObject.SetActive(shouldShow);
    }

    private void HandleQuestUpdated()
    {
        UpdateMarkerVisibility();
    }

    private void CreateMarkerObject()
    {
        if (markerObject != null) return;

        markerObject = new GameObject("QuestExclamationMarker");
        markerObject.transform.SetParent(transform);

        baseLocalPos = new Vector3(0, offsetY, 0);
        markerObject.transform.localPosition = baseLocalPos;
        markerObject.transform.localScale = Vector3.one * markerScale;

        markerRenderer = markerObject.AddComponent<SpriteRenderer>();
        markerRenderer.sprite = GetOrCreateExclamationSprite();
        markerRenderer.color = markerColor;
        if (!string.IsNullOrEmpty(sortingLayerName))
            markerRenderer.sortingLayerName = sortingLayerName;
        markerRenderer.sortingOrder = orderInLayer;
    }

    private static Sprite GetOrCreateExclamationSprite()
    {
        if (cachedExclamationSprite != null) return cachedExclamationSprite;

        // Tạo Texture Pixel Art 32x32 sắc nét với ruột Trắng (để áp màu Marker Color) và Viền Đen Đậm
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color black = new Color(0.05f, 0.05f, 0.05f, 1f);
        Color white = Color.white;
        Color highlight = new Color(1f, 1f, 1f, 1f);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);

        // Hàm helper tô viền đen và ruột trắng
        void FillRect(int minX, int minY, int maxX, int maxY, Color c)
        {
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    tex.SetPixel(x, y, c);
        }

        // 1. Viền Đen ngoài cùng cho Thân Dấu Chấm Than (Y: 10->29, X: 11->20)
        FillRect(11, 10, 20, 29, black);

        // 2. Ruột Trắng cho Thân Dấu Chấm Than (Y: 11->28, X: 12->19)
        FillRect(12, 11, 19, 28, white);

        // 3. Highlight màu sáng cho Thân
        FillRect(14, 13, 17, 27, highlight);

        // 4. Viền Đen ngoài cùng cho Chấm Tròn Phía Dưới (Y: 2->8, X: 11->20)
        FillRect(11, 2, 20, 8, black);

        // 5. Ruột Trắng cho Chấm Tròn Phía Dưới (Y: 3->7, X: 12->19)
        FillRect(12, 3, 19, 7, white);

        // 6. Highlight màu sáng cho Chấm Tròn
        FillRect(14, 4, 17, 6, highlight);

        tex.Apply();
        cachedExclamationSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return cachedExclamationSprite;
    }
}
