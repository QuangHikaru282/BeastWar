using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý hệ thống chỉ đường. Tạo hiệu ứng một dải các đốm sáng xanh di chuyển từ người chơi đến mục tiêu (giống game nhập vai).
/// Gắn vào một GameObject trống trong Scene (ví dụ: QuestNavigationSystem).
/// </summary>
public class QuestNavigation : MonoBehaviour
{
    public static QuestNavigation Instance { get; private set; }

    [Header("Cài đặt Hiệu ứng")]
    [Tooltip("Nhân vật chính của chúng ta")]
    public Transform player;

    [Tooltip("Số lượng đốm sáng trên đường đi")]
    public int dotCount = 8;
    
    [Tooltip("Tốc độ chạy của đốm sáng")]
    public float moveSpeed = 3f;

    [Tooltip("Khoảng cách giữa các đốm sáng")]
    public float spacing = 1f;

    [Tooltip("Kích thước của đốm sáng (chỉnh nhỏ lại nếu thấy quá to)")]
    public float dotSize = 0.25f;

    [Tooltip("Khoảng cách hạ thấp xuống chân nhân vật (số âm là kéo xuống)")]
    public float yOffset = -0.5f;

    [Tooltip("Màu sắc của dải sáng (VD: Màu vàng)")]
    public Color pathColor = new Color(1f, 0.9f, 0.1f, 1f); // Màu vàng

    private bool isNavigating = false;
    private Transform currentTarget;

    // Danh sách các đốm sáng
    private List<GameObject> pathDots = new List<GameObject>();
    private Sprite dotSprite;

    // Danh sách lưu trữ các QuestTarget có trong Scene
    private static Dictionary<int, List<QuestTarget>> activeTargets = new Dictionary<int, List<QuestTarget>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CreateDotSprite();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    public static void RegisterTarget(QuestTarget target)
    {
        if (!activeTargets.ContainsKey(target.questId))
        {
            activeTargets[target.questId] = new List<QuestTarget>();
        }
        if (!activeTargets[target.questId].Contains(target))
        {
            activeTargets[target.questId].Add(target);
        }
    }

    public static void UnregisterTarget(QuestTarget target)
    {
        if (activeTargets.ContainsKey(target.questId))
        {
            activeTargets[target.questId].Remove(target);
        }
    }

    public void ToggleNavigationForQuest(int questId)
    {
        if (isNavigating)
        {
            StopNavigation();
            return;
        }

        // Tự động tìm lại Player nếu nó bị hủy (chẳng hạn khi chuyển cảnh)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else 
            {
                Debug.LogWarning("[QuestNavigation] Không tìm thấy Player trong Scene!");
                return;
            }
        }

        if (activeTargets.ContainsKey(questId) && activeTargets[questId].Count > 0)
        {
            currentTarget = activeTargets[questId][0].transform;
            isNavigating = true;

            // Lấy Sorting Layer của Player để gán cho đốm sáng
            string playerLayer = "Default";
            if (player != null)
            {
                SpriteRenderer playerSr = player.GetComponentInChildren<SpriteRenderer>();
                if (playerSr != null) playerLayer = playerSr.sortingLayerName;
            }

            // Tạo các đốm sáng nếu chưa có đủ
            while (pathDots.Count < dotCount)
            {
                GameObject dot = new GameObject($"PathDot_{pathDots.Count}");
                dot.transform.SetParent(transform);
                SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
                sr.sprite = dotSprite;
                sr.color = pathColor;
                sr.sortingLayerName = playerLayer; // Đặt cùng Layer với người chơi
                sr.sortingOrder = 999; // Đảm bảo luôn nằm trên cùng trong layer đó
                pathDots.Add(dot);
            }
            
            // Bật tất cả đốm sáng và phân bố chúng
            for (int i = 0; i < pathDots.Count; i++)
            {
                pathDots[i].SetActive(true);
            }

            Debug.Log($"[QuestNavigation] Đã bật dải sáng chỉ đường đến Quest {questId}");
        }
        else
        {
            Debug.LogWarning($"[QuestNavigation] Không tìm thấy mục tiêu nào trên Scene cho Quest {questId}!");
            StopNavigation();
        }
    }

    public void StopNavigation()
    {
        isNavigating = false;
        currentTarget = null;
        foreach (var dot in pathDots)
        {
            dot.SetActive(false);
        }
    }

    private void Update()
    {
        if (isNavigating && currentTarget != null && player != null)
        {
            // Cộng thêm yOffset để dời điểm bắt đầu và kết thúc xuống dưới chân
            Vector3 startPos = player.position + new Vector3(0, yOffset, 0);
            Vector3 endPos = currentTarget.position + new Vector3(0, yOffset, 0);
            
            // Bỏ qua trục Z
            startPos.z = 0;
            endPos.z = 0;
            
            Vector3 direction = endPos - startPos;
            float totalDistance = direction.magnitude;

            // Tăng khoảng cách tắt an toàn, tránh tắt ngay lập tức nếu mục tiêu ở quá gần
            if (totalDistance < 2.0f)
            {
                StopNavigation();
                Debug.Log("[QuestNavigation] Đã đến rất gần mục tiêu, tự động tắt chỉ đường.");
                return;
            }

            direction.Normalize();

            // Tính toán vị trí và di chuyển các đốm sáng
            float timeOffset = Time.time * moveSpeed;
            
            for (int i = 0; i < pathDots.Count; i++)
            {
                // Tính khoảng cách của điểm này dựa trên thời gian để tạo hiệu ứng chảy (flowing)
                float rawDistance = (i * spacing) - (timeOffset % spacing);
                
                // Lặp lại quãng đường
                float maxTrailLength = Mathf.Min(dotCount * spacing, totalDistance);
                
                float actualDistance = rawDistance;
                while (actualDistance < 0) actualDistance += maxTrailLength;
                while (actualDistance > maxTrailLength) actualDistance -= maxTrailLength;

                if (actualDistance > totalDistance)
                {
                    pathDots[i].SetActive(false);
                }
                else
                {
                    pathDots[i].SetActive(true);
                    
                    SpriteRenderer sr = pathDots[i].GetComponent<SpriteRenderer>();
                    Color c = pathColor;
                    
                    // Làm mờ nhanh ở 2 đầu để không bị ảo, phần lớn đốm sáng sẽ hiển thị rõ 100%
                    float fadeDist = 0.5f;
                    if (actualDistance < fadeDist) c.a = pathColor.a * (actualDistance / fadeDist); 
                    else if (totalDistance - actualDistance < fadeDist) c.a = pathColor.a * ((totalDistance - actualDistance) / fadeDist); 
                    
                    sr.color = c;

                    // Cập nhật vị trí, hơi nổi lên 1 xíu ở trục Z để tránh bị che bởi Tilemap Z=0
                    Vector3 pos = startPos + direction * actualDistance;
                    pos.z = -1f; 
                    pathDots[i].transform.position = pos;
                    
                    // Kích thước dựa trên biến dotSize
                    float scale = dotSize + Mathf.Sin(Time.time * 10f + i) * (dotSize * 0.15f);
                    pathDots[i].transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
        }
        else if (isNavigating && (currentTarget == null || player == null))
        {
            StopNavigation();
        }
    }

    // Tạo đốm sáng theo phong cách Pixel Art (sắc nét, không mờ viền) để hợp với game
    private void CreateDotSprite()
    {
        int size = 16; 
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point; // QUAN TRỌNG: Làm cho pixel vuông vức, không bị mờ (Anti-aliasing)
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = (size / 2f) - 1f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                
                // Tròn sắc nét
                if (dist <= radius)
                {
                    tex.SetPixel(x, y, Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }
        tex.Apply();
        
        // PPU = 16 để hình vuông 16x16 đúng bằng 1 đơn vị trong game
        dotSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f); 
    }
}
