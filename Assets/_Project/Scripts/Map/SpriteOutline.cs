using UnityEngine;

/// <summary>
/// Tạo viền trắng xung quanh SpriteRenderer bằng cách vẽ 4 bản sao offset.
/// Gắn script này vào NPC / vật phẩm tương tác.
/// Không cần shader đặc biệt — tương thích mọi phiên bản Unity.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
    [Header("Cấu hình viền")]
    [Tooltip("Màu viền highlight (mặc định: trắng)")]
    [SerializeField] private Color outlineColor = Color.white;

    [Tooltip("Độ dày viền (pixel world-space)")]
    [SerializeField] private float outlineThickness = 0.06f;

    [Tooltip("Sorting Order của viền (thường = SpriteRenderer - 1)")]
    [SerializeField] private int outlineSortingOrder = -1;

    [Header("Cấu hình vị trí nút Hint (Nút F)")]
    [Tooltip("Độ cao điều chỉnh riêng cho nút F của đối tượng này (có thể âm hoặc dương)")]
    public float customHintOffsetY = 0f;

    private SpriteRenderer sourceRenderer;
    private GameObject[] outlineObjects;
    private SpriteRenderer[] outlineRenderers;
    private bool isHighlighted = false;

    // 4 hướng offset: trên, dưới, trái, phải
    private static readonly Vector3[] Offsets = new Vector3[]
    {
        new Vector3( 0,  1, 0),
        new Vector3( 0, -1, 0),
        new Vector3(-1,  0, 0),
        new Vector3( 1,  0, 0),
    };

    private void Awake()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();
        CreateOutlineObjects();
        SetHighlight(false);
    }

    private void LateUpdate()
    {
        // Đồng bộ sprite nếu animation thay đổi frame
        if (isHighlighted && outlineRenderers != null)
        {
            foreach (var r in outlineRenderers)
            {
                if (r != null) r.sprite = sourceRenderer.sprite;
            }
        }
    }

    private void CreateOutlineObjects()
    {
        outlineObjects = new GameObject[4];
        outlineRenderers = new SpriteRenderer[4];

        // Tạo Material tô màu nguyên khối (Solid Color) để viền trắng sáng thuần khiết
        Shader solidShader = Shader.Find("Sprites/SolidColor");
        if (solidShader == null) solidShader = Shader.Find("GUI/Text Shader");
        
        Material outlineMaterial = null;
        if (solidShader != null)
        {
            outlineMaterial = new Material(solidShader);
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject obj = new GameObject($"Outline_{i}");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Offsets[i] * outlineThickness;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sourceRenderer.sprite;
            sr.color = outlineColor;
            sr.flipX = sourceRenderer.flipX;
            sr.flipY = sourceRenderer.flipY;
            sr.sortingLayerID = sourceRenderer.sortingLayerID;
            sr.sortingOrder = sourceRenderer.sortingOrder + outlineSortingOrder;

            if (outlineMaterial != null)
            {
                sr.material = outlineMaterial;
            }

            outlineObjects[i] = obj;
            outlineRenderers[i] = sr;
        }
    }

    /// <summary>Bật hoặc tắt viền trắng.</summary>
    public void SetHighlight(bool active)
    {
        isHighlighted = active;

        if (outlineObjects == null) return;

        foreach (var obj in outlineObjects)
        {
            if (obj != null) obj.SetActive(active);
        }
    }

    // Đồng bộ flipX khi sprite flip (ví dụ NPC quay trái/phải)
    private void OnWillRenderObject()
    {
        if (!isHighlighted || outlineRenderers == null) return;

        foreach (var r in outlineRenderers)
        {
            if (r != null)
            {
                r.flipX = sourceRenderer.flipX;
                r.flipY = sourceRenderer.flipY;
            }
        }
    }
}
