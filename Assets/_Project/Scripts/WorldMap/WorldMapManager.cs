using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý toàn bộ màn chọn ải (World Map).
/// Tự động sinh ra các nút ải và vẽ đường nối theo hình rắn (S-curve).
///
/// SETUP trong Unity Editor:
///  1. Tạo Scene "WorldMapScene".
///  2. Tạo Canvas → trong Canvas tạo một ScrollRect.
///  3. Trong ScrollRect.Content tạo một RawImage nền (background bản đồ).
///  4. Tạo một GameObject rỗng tên "WorldMapManager", gắn script này vào.
///  5. Kéo các tham chiếu vào Inspector theo hướng dẫn bên dưới.
/// </summary>
public class WorldMapManager : MonoBehaviour
{
    // ─── Inspector ───────────────────────────────────────────────────

    [Header("Dữ liệu")]
    [SerializeField] private WorldMapData worldMapData;
    [SerializeField] private PlayerData   playerData;
    [SerializeField] private BattleTransferData battleTransferData;

    [Header("UI References")]
    [Tooltip("Content RectTransform của ScrollRect (nơi các node sẽ được tạo)")]
    [SerializeField] private RectTransform mapContainer;

    [Tooltip("Prefab nút ải (có script StageNodeUI)")]
    [SerializeField] private GameObject stageNodePrefab;

    [Header("Cấu hình bố cục rắn (Ngang)")]
    [Tooltip("Số node trên mỗi cột dọc")]
    [SerializeField] private int nodesPerColumn = 3;

    [Tooltip("Khoảng cách ngang giữa 2 cột (pixel)")]
    [SerializeField] private float columnWidth = 250f;

    [Tooltip("Tọa độ Y của từng hàng (số phần tử = nodesPerColumn)")]
    [SerializeField] private float[] rowY = { 150f, 0f, -150f };

    [Tooltip("Offset X nhẹ cho các hàng để tạo cảm giác uốn lượn")]
    [SerializeField] private float[] rowXOffset = { 0f, 35f, 0f };

    [Tooltip("X bắt đầu từ trái sang phải")]
    [SerializeField] private float startX = 150f;

    [Header("Đường nối giữa các ải")]
    [SerializeField] private Color pathColorUnlocked = new Color(0.95f, 0.75f, 0.15f, 1f);
    [SerializeField] private Color pathColorLocked   = new Color(0.45f, 0.45f, 0.45f, 0.7f);
    [SerializeField] private float pathThickness = 18f;
    [SerializeField] private Sprite pathSprite; // (tuỳ chọn) sprite đường, để null dùng màu đặc

    // ─── Runtime ─────────────────────────────────────────────────────

    private readonly List<RectTransform> nodeRects = new List<RectTransform>();

    // ─── Unity Lifecycle ─────────────────────────────────────────────

    private void Start()
    {
        BuildMap();
    }

    // ─── Build Map ───────────────────────────────────────────────────

    private void BuildMap()
    {
        if (worldMapData == null || worldMapData.stages.Count == 0)
        {
            Debug.LogWarning("[WorldMapManager] Chưa gán WorldMapData hoặc danh sách ải trống!");
            return;
        }
        if (stageNodePrefab == null)
        {
            Debug.LogError("[WorldMapManager] Chưa gán StageNodePrefab!");
            return;
        }
        if (mapContainer == null)
        {
            Debug.LogError("[WorldMapManager] Chưa gán MapContainer!");
            return;
        }

        int stageCount = worldMapData.stages.Count;
        List<Vector2> positions = GeneratePositions(stageCount);

        // Tự động điều chỉnh chiều rộng Content để scroll vừa đủ theo chiều ngang
        int columnCount = Mathf.CeilToInt((float)stageCount / nodesPerColumn);
        float contentWidth = startX + (columnCount + 0.5f) * columnWidth + 200f;
        mapContainer.sizeDelta = new Vector2(contentWidth, mapContainer.sizeDelta.y);

        // Tạo từng node ải
        for (int i = 0; i < stageCount; i++)
        {
            StageData stage = worldMapData.stages[i];
            int stars      = playerData.GetStageStars(stage.stageId);
            bool unlocked  = stage.stageId <= playerData.highestUnlockedStage;

            GameObject nodeGo  = Instantiate(stageNodePrefab, mapContainer);
            RectTransform nodeRT = nodeGo.GetComponent<RectTransform>();
            
            // Đặt Anchor về Middle-Left (0, 0.5) và Pivot về Center (0.5, 0.5) để không bị lệch khi Content giãn ra
            nodeRT.anchorMin = new Vector2(0f, 0.5f);
            nodeRT.anchorMax = new Vector2(0f, 0.5f);
            nodeRT.pivot     = new Vector2(0.5f, 0.5f);
            nodeRT.anchoredPosition = positions[i];

            StageNodeUI nodeUI = nodeGo.GetComponent<StageNodeUI>();
            if (nodeUI != null)
                nodeUI.Initialize(stage, stars, unlocked, this);

            nodeRects.Add(nodeRT);
        }

        // Vẽ đường nối (đặt trước khi tạo node → nằm phía sau)
        for (int i = 0; i < stageCount - 1; i++)
        {
            // Đường sáng nếu cả 2 node đều đã mở
            bool pathUnlocked = (i + 2) <= playerData.highestUnlockedStage;
            DrawPath(nodeRects[i], nodeRects[i + 1], pathUnlocked);
        }
    }

    /// <summary>Tính vị trí các node theo bố cục rắn uốn lượn (ngang).</summary>
    private List<Vector2> GeneratePositions(int count)
    {
        // Đảm bảo mảng có đủ phần tử
        if (rowY == null || rowY.Length < nodesPerColumn)
        {
            rowY = new float[] { 150f, 0f, -150f };
        }
        if (rowXOffset == null || rowXOffset.Length < nodesPerColumn)
        {
            rowXOffset = new float[] { 0f, 35f, 0f };
        }

        var positions = new List<Vector2>(count);

        for (int i = 0; i < count; i++)
        {
            int col = i / nodesPerColumn;
            int row = i % nodesPerColumn;

            // Đổi chiều đi mỗi cột lẻ → hình rắn
            if (col % 2 == 1)
                row = (nodesPerColumn - 1) - row;

            float x = startX + col * columnWidth + rowXOffset[row];
            float y = rowY[row];

            positions.Add(new Vector2(x, y));
        }

        return positions;
    }

    // ─── Vẽ Đường ────────────────────────────────────────────────────

    /// <summary>Vẽ một đoạn đường thẳng nối 2 node bằng UI Image xoay.</summary>
    private void DrawPath(RectTransform from, RectTransform to, bool isUnlocked)
    {
        // Chia đường thành nhiều đoạn để dễ bo cong sau này, hiện tại 1 đoạn thẳng
        CreateSegment(from.anchoredPosition, to.anchoredPosition, isUnlocked);
    }

    private void CreateSegment(Vector2 fromPos, Vector2 toPos, bool isUnlocked)
    {
        GameObject lineGo = new GameObject("PathSegment");
        lineGo.transform.SetParent(mapContainer, false);
        lineGo.transform.SetAsFirstSibling(); // Nằm sau cùng → phía sau các node

        RectTransform rt  = lineGo.AddComponent<RectTransform>();
        Image lineImage   = lineGo.AddComponent<Image>();

        if (pathSprite != null)
            lineImage.sprite = pathSprite;

        lineImage.color = isUnlocked ? pathColorUnlocked : pathColorLocked;

        // Tính toán vị trí, chiều dài, góc xoay
        Vector2 dir   = toPos - fromPos;
        float   dist  = dir.magnitude;
        float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rt.anchorMin        = new Vector2(0f, 0.5f);
        rt.anchorMax        = new Vector2(0f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = (fromPos + toPos) * 0.5f;
        rt.sizeDelta        = new Vector2(dist, pathThickness);
        rt.localRotation    = Quaternion.Euler(0f, 0f, angle);
    }

    // ─── Stage Selected ──────────────────────────────────────────────

    /// <summary>Được gọi bởi StageNodeUI khi người chơi bấm vào một ải.</summary>
    public void OnStageSelected(StageData stage)
    {
        if (battleTransferData == null)
        {
            Debug.LogError("[WorldMapManager] Chưa gán BattleTransferData!");
            return;
        }

        Debug.Log($"[WorldMapManager] Chọn ải {stage.stageId}: {stage.stageName}");

        // Ghi dữ liệu địch vào kênh truyền
        battleTransferData.SetEnemyTeam(stage.enemyTeam);
        battleTransferData.originScene    = BattleTransferData.OriginScene.WorldMap;
        battleTransferData.isSingleBattle = false;
        battleTransferData.currentStageId = stage.stageId;
        battleTransferData.lastEncounteredBeastId = "";

        GameSceneManager.GoToBattle();
    }
}
