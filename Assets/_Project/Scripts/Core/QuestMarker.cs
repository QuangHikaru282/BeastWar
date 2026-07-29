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
    [Tooltip("Khoảng cách Y nổi trên đầu đối tượng")]
    public float offsetY = 1.6f;

    [Tooltip("Màu sắc của dấu chấm than (Mặc định: Màu vàng)")]
    public Color markerColor = new Color(1f, 0.85f, 0.1f, 1f);

    [Tooltip("Kích thước icon (World scale)")]
    public float markerScale = 0.35f;

    [Header("Hiệu ứng nảy (Bouncing)")]
    public bool enableBounce = true;
    public float bounceHeight = 0.15f;
    public float bounceSpeed = 4f;

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
        if (markerObject != null && markerObject.activeSelf && enableBounce)
        {
            // Hiệu ứng nhún nảy mượt mà
            float newY = baseLocalPos.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            markerObject.transform.localPosition = new Vector3(baseLocalPos.x, newY, baseLocalPos.z);
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
        markerRenderer.sortingOrder = 100; // Hiển thị phía trên sprite NPC
    }

    private static Sprite GetOrCreateExclamationSprite()
    {
        if (cachedExclamationSprite != null) return cachedExclamationSprite;

        // Tạo Texture Pixel Art hình dấu chấm than (!) màu trắng thuần khiết 16x16
        int width = 16;
        int height = 16;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color white = Color.white;

        // Xóa nền
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tex.SetPixel(x, y, transparent);

        // Vẽ thân dấu chấm than (!) (Cột dọc từ Y=6 đến Y=15)
        for (int y = 6; y <= 15; y++)
        {
            for (int x = 6; x <= 9; x++)
            {
                tex.SetPixel(x, y, white);
            }
        }

        // Vẽ chấm tròn phía dưới (!) (Từ Y=1 đến Y=3)
        for (int y = 1; y <= 3; y++)
        {
            for (int x = 6; x <= 9; x++)
            {
                tex.SetPixel(x, y, white);
            }
        }

        tex.Apply();
        cachedExclamationSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16f);
        return cachedExclamationSprite;
    }
}
