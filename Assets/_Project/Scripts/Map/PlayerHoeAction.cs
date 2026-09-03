using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Kinnly;
using BeastBall.Farming;

/// <summary>
/// Quản lý hành động Vung Cuốc và Xới Đất (Tilling Soil) cho người chơi.
/// Gắn lên GameObject 'PF Player'.
/// </summary>
public class PlayerHoeAction : MonoBehaviour
{
    [Header("── Mô Hình Nhân Vật")]
    [Tooltip("Kéo đối tượng MaleModel vào đây")]
    [SerializeField] private GameObject maleModel;
    [Tooltip("Kéo đối tượng FemaleModel vào đây")]
    [SerializeField] private GameObject femaleModel;

    [Header("── Sprite Hoạt Ảnh Cuốc (18 frames)")]
    [Tooltip("6 frames cuốc nhìn xuống (Hoe_0 -> Hoe_5)")]
    [SerializeField] private Sprite[] hoeDownFrames;

    [Tooltip("6 frames cuốc nhìn lên (Hoe_6 -> Hoe_11)")]
    [SerializeField] private Sprite[] hoeUpFrames;

    [Tooltip("6 frames cuốc nhìn sang ngang (Hoe_12 -> Hoe_17)")]
    [SerializeField] private Sprite[] hoeSideFrames;

    [Header("── Cấu Hình")]
    [Tooltip("Thời gian thực hiện một lần vung cuốc (giây)")]
    [SerializeField] private float swingDuration = 0.45f;

    [Tooltip("Khoảng cách tối đa người chơi có thể cuốc tới")]
    [SerializeField] private float reachDistance = 2.0f;

    [Tooltip("Tự động cuốc ô đất trước mặt nếu click xa")]
    [SerializeField] private bool autoClampToReach = true;

    private PlayerInventory inventory;
    private PlayerMapController playerCtrl;
    private bool isSwinging = false;
    private bool isWalkingToTile = false;
    private Coroutine activeActionCoroutine = null;

    public bool IsSwinging => isSwinging;
    public bool IsWalkingToTile => isWalkingToTile;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        playerCtrl = GetComponent<PlayerMapController>();

        if (maleModel == null)
        {
            Transform t = transform.Find("MaleModel");
            if (t != null) maleModel = t.gameObject;
        }

        if (femaleModel == null)
        {
            Transform t = transform.Find("FemaleModel");
            if (t != null) femaleModel = t.gameObject;
        }
    }

    private void Update()
    {
        // Nếu người chơi đang bấm phím WASD để tự di chuyển mà đang có lệnh tự động đi -> HỦY
        if (isWalkingToTile && (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f))
        {
            if (activeActionCoroutine != null)
            {
                StopCoroutine(activeActionCoroutine);
                activeActionCoroutine = null;
            }
            CancelAutoWalkState();
            return;
        }

        if (isSwinging) return;

        // Bấm chuột trái hoặc chuột phải để vung cuốc
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Bỏ qua nếu chuột đang nằm trên giao diện UI (Túi đồ, menu...)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (IsHoldingHoe())
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0f;

                var terrainManager = GetOrCreateTerrainManager();
                if (terrainManager == null || terrainManager.Grid == null) return;

                Vector3Int targetCell = terrainManager.Grid.WorldToCell(mouseWorldPos);

                // Chỉ đi lại gần nếu ô đó thực sự là đất ruộng có thể cuốc (không cuốc trên cỏ/đã xới)
                if (!terrainManager.IsTillable(targetCell))
                {
                    Debug.Log($"[PlayerHoeAction] Ô {targetCell} không phải đất nông trại hợp lệ hoặc đã xới rồi.");
                    return;
                }

                // Hủy lệnh đi trước đó nếu đang đi dở
                if (activeActionCoroutine != null)
                {
                    StopCoroutine(activeActionCoroutine);
                    CancelAutoWalkState();
                }

                activeActionCoroutine = StartCoroutine(ApproachAndHoeRoutine(targetCell, terrainManager));
            }
        }
    }

    /// <summary>
    /// Kiểm tra người chơi có đang cầm Cuốc (Hoe) trên tay hay không
    /// </summary>
    public bool IsHoldingHoe()
    {
        if (inventory == null || inventory.CurrentlySelectedInventoryItem == null)
            return false;

        Kinnly.Item item = inventory.CurrentlySelectedInventoryItem.Item;
        if (item == null) return false;

        // Kiểm tra qua delegate Farming hoặc qua tên
        if (item.farmingItemDelegate is BeastBall.Farming.Hoe) return true;
        if (item.name != null && item.name.ToLower().Contains("hoe")) return true;
        if (item.name != null && item.name.ToLower().Contains("cuoc")) return true;
        if (item.name != null && item.name.ToLower().Contains("cuốc")) return true;

        return false;
    }

    /// <summary>
    /// Tiếp cận ô đất theo góc vuông 90 độ (Manhattan Path) rồi mới thực hiện vung cuốc
    /// </summary>
    private IEnumerator ApproachAndHoeRoutine(Vector3Int targetCell, FarmingTerrainManager terrainManager)
    {
        Vector3 cellCenter = terrainManager.Grid.GetCellCenterWorld(targetCell);
        Vector3 playerPos = transform.position;

        // 4 vị trí đứng vuông góc 90° quanh ô đất:
        // [0] Dưới nhìn lên (Up)
        // [1] Trên nhìn xuống (Down)
        // [2] Trái nhìn sang phải (Right)
        // [3] Phải nhìn sang trái (Left)
        Vector3Int[] neighborCells = new Vector3Int[]
        {
            targetCell + Vector3Int.down,
            targetCell + Vector3Int.up,
            targetCell + Vector3Int.left,
            targetCell + Vector3Int.right
        };

        Vector3 bestStandPos = Vector3.zero;
        float minDistance = float.MaxValue;

        for (int i = 0; i < 4; i++)
        {
            Vector3 standPos = terrainManager.Grid.GetCellCenterWorld(neighborCells[i]);
            standPos.z = playerPos.z;

            // Bỏ qua nếu vị trí đứng này bị chặn bởi vật cản tĩnh (tường, hàng rào...)
            Collider2D col = Physics2D.OverlapCircle(standPos, 0.2f);
            if (col != null && !col.isTrigger && !col.transform.IsChildOf(transform))
            {
                continue;
            }

            float d = Vector2.Distance(playerPos, standPos);
            if (d < minDistance)
            {
                minDistance = d;
                bestStandPos = standPos;
            }
        }

        if (minDistance == float.MaxValue)
        {
            bestStandPos = terrainManager.Grid.GetCellCenterWorld(neighborCells[0]);
            bestStandPos.z = playerPos.z;
        }

        // 1. NẾU ĐÃ ĐỨNG ĐỦ GẦN (Cách ô đất <= 1.3f) -> VUNG CUỐC NGAY LẬP TỨC
        if (Vector2.Distance(playerPos, cellCenter) <= 1.3f)
        {
            yield return StartCoroutine(PerformHoeSwing(cellCenter));
            activeActionCoroutine = null;
            yield break;
        }

        // 2. NẾU Ở XA -> DI CHUYỂN LẠI GẦN VUÔNG GÓC 90 ĐỘ (MANHATTAN PATHING)
        isWalkingToTile = true;
        if (playerCtrl != null) playerCtrl.SetCanMove(false);

        GameObject activeModel = maleModel != null && maleModel.activeSelf ? maleModel : femaleModel;
        if (activeModel == null && maleModel != null) activeModel = maleModel;
        SpriteRenderer sr = activeModel != null ? activeModel.GetComponent<SpriteRenderer>() : null;
        Animator anim = activeModel != null ? activeModel.GetComponent<Animator>() : null;

        float speed = playerCtrl != null && playerCtrl.MoveSpeed > 0.1f ? playerCtrl.MoveSpeed : 5.0f;

        // Xác định đường đi vuông góc 90 độ (Đi theo trục có khoảng cách lớn hơn trước, rồi bẻ góc 90°)
        float dx = Mathf.Abs(bestStandPos.x - playerPos.x);
        float dy = Mathf.Abs(bestStandPos.y - playerPos.y);

        System.Collections.Generic.List<Vector3> waypoints = new System.Collections.Generic.List<Vector3>();
        if (dx >= dy)
        {
            if (dx > 0.08f) waypoints.Add(new Vector3(bestStandPos.x, playerPos.y, playerPos.z));
            if (dy > 0.08f) waypoints.Add(new Vector3(bestStandPos.x, bestStandPos.y, playerPos.z));
        }
        else
        {
            if (dy > 0.08f) waypoints.Add(new Vector3(playerPos.x, bestStandPos.y, playerPos.z));
            if (dx > 0.08f) waypoints.Add(new Vector3(bestStandPos.x, bestStandPos.y, playerPos.z));
        }

        if (waypoints.Count == 0) waypoints.Add(bestStandPos);

        // Chạy qua từng điểm bẻ góc 90 độ
        for (int w = 0; w < waypoints.Count; w++)
        {
            Vector3 targetPt = waypoints[w];

            while (Vector2.Distance(transform.position, targetPt) > 0.06f)
            {
                // Cho phép người chơi bấm WASD để tự hủy lệnh bất cứ lúc nào
                if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
                {
                    CancelAutoWalkState();
                    activeActionCoroutine = null;
                    yield break;
                }

                Vector2 moveDir = ((Vector2)targetPt - (Vector2)transform.position).normalized;

                if (anim != null)
                {
                    anim.SetBool("Run", true);
                    anim.SetFloat("speed", 1f);
                    anim.SetFloat("Horizontal", moveDir.x);
                    anim.SetFloat("Vertical", moveDir.y);
                    anim.SetFloat("dirX", moveDir.x);
                    anim.SetFloat("dirY", moveDir.y);
                }

                if (sr != null)
                {
                    if (moveDir.x < -0.1f) sr.flipX = true;
                    else if (moveDir.x > 0.1f) sr.flipX = false;
                }

                transform.position = Vector3.MoveTowards(transform.position, targetPt, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPt;
        }

        // Đã đến vị trí đứng vuông góc trước ô đất!
        isWalkingToTile = false;
        if (anim != null)
        {
            anim.SetBool("Run", false);
            anim.SetFloat("speed", 0f);
        }

        // Hướng mặt thẳng vuông góc 90° vào ô đất mục tiêu
        Vector2 lookDir = ((Vector2)cellCenter - (Vector2)transform.position).normalized;
        if (anim != null)
        {
            anim.SetFloat("Horizontal", lookDir.x);
            anim.SetFloat("Vertical", lookDir.y);
            anim.SetFloat("dirX", lookDir.x);
            anim.SetFloat("dirY", lookDir.y);
        }
        if (sr != null)
        {
            if (lookDir.x < -0.1f) sr.flipX = true;
            else if (lookDir.x > 0.1f) sr.flipX = false;
        }

        yield return new WaitForSeconds(0.06f);

        // 3. Vung cuốc xới ô đất!
        yield return StartCoroutine(PerformHoeSwing(cellCenter));
        activeActionCoroutine = null;
    }

    private void CancelAutoWalkState()
    {
        isWalkingToTile = false;
        if (playerCtrl != null) playerCtrl.SetCanMove(true);

        GameObject activeModel = maleModel != null && maleModel.activeSelf ? maleModel : femaleModel;
        if (activeModel == null && maleModel != null) activeModel = maleModel;
        Animator anim = activeModel != null ? activeModel.GetComponent<Animator>() : null;
        if (anim != null)
        {
            anim.SetBool("Run", false);
            anim.SetFloat("speed", 0f);
        }
    }

    /// <summary>
    /// Coroutine thực hiện hoạt ảnh vung cuốc và xới đất
    /// </summary>
    private IEnumerator PerformHoeSwing(Vector3 targetWorldPos)
    {
        isSwinging = true;

        // 1. Tạm dừng di chuyển của người chơi khi đang vung cuốc
        if (playerCtrl != null)
        {
            playerCtrl.SetCanMove(false);
        }

        // 2. Xác định mô hình nhân vật đang hiển thị (Nam hoặc Nữ)
        GameObject activeModel = maleModel != null && maleModel.activeSelf ? maleModel : femaleModel;
        if (activeModel == null && maleModel != null) activeModel = maleModel;

        SpriteRenderer sr = activeModel != null ? activeModel.GetComponent<SpriteRenderer>() : null;
        Animator anim = activeModel != null ? activeModel.GetComponent<Animator>() : null;

        // Tạm dừng Animator trong lúc chạy hoạt ảnh vung cuốc frame-by-frame
        if (anim != null) anim.enabled = false;

        // 3. Tính toán hướng giữa người chơi và vị trí click chuột
        Vector2 diff = (Vector2)targetWorldPos - (Vector2)transform.position;
        Sprite[] activeFrames = hoeDownFrames;
        bool flipX = false;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            // Hướng ngang (Trái / Phải)
            activeFrames = hoeSideFrames;
            flipX = diff.x < 0;
        }
        else
        {
            // Hướng dọc (Lên / Xuống)
            if (diff.y > 0)
            {
                activeFrames = hoeUpFrames;
            }
            else
            {
                activeFrames = hoeDownFrames;
            }
        }

        if (activeFrames == null || activeFrames.Length == 0)
        {
            activeFrames = hoeDownFrames;
        }

        // 4. Chạy từng frame hoạt ảnh vung cuốc
        int frameCount = activeFrames != null ? activeFrames.Length : 0;
        float frameTime = frameCount > 0 ? swingDuration / frameCount : 0.08f;
        int impactFrame = Mathf.Min(3, frameCount - 1); // Thời điểm lưỡi cuốc đập xuống đất

        for (int i = 0; i < frameCount; i++)
        {
            if (sr != null)
            {
                sr.sprite = activeFrames[i];
                sr.flipX = flipX;
            }

            // Khi lưỡi cuốc chạm đất -> Thực hiện biến đất thành ô đất xới (Tilled Soil)
            if (i == impactFrame)
            {
                ExecuteTilling(targetWorldPos);
            }

            yield return new WaitForSeconds(frameTime);
        }

        // 5. Kết thúc hành động cuốc: Khôi phục lại Animator và khả năng di chuyển
        if (anim != null) anim.enabled = true;
        if (sr != null) sr.flipX = false;

        if (playerCtrl != null)
        {
            playerCtrl.SetCanMove(true);
        }

        isSwinging = false;
    }

    /// <summary>
    /// Thực hiện cuốc đất sinh ra ô đất xới (Tilled Tile) phong cách Stardew Valley
    /// </summary>
    private void ExecuteTilling(Vector3 targetWorldPos)
    {
        var terrainManager = GetOrCreateTerrainManager();
        if (terrainManager == null || terrainManager.Grid == null) return;

        Vector3Int cellPos = terrainManager.Grid.WorldToCell(targetWorldPos);

        // Nếu ô đất đã xới rồi thì không xới lại
        if (terrainManager.IsTilled(cellPos))
            return;

        // Chỉ xới đất nếu ô này hợp lệ (là đất nông trại, không phải thảm cỏ)
        if (!terrainManager.IsTillable(cellPos))
        {
            Debug.Log($"[PlayerHoeAction] Không thể cuốc tại {cellPos} (ngoài phạm vi đất nông trại hoặc trên thảm cỏ).");
            return;
        }

        // Cuốc xới ô đất
        terrainManager.TillAt(cellPos);
        terrainManager.SaveCurrentTerrainData();
        Debug.Log($"<color=green>[Farming]</color> Đã cuốc ô đất thành công tại: {cellPos}");
    }

    private FarmingTerrainManager GetOrCreateTerrainManager()
    {
        if (FarmingTerrainManager.Instance != null)
            return FarmingTerrainManager.Instance;

        var existing = Object.FindFirstObjectByType<FarmingTerrainManager>();
        if (existing != null) return existing;

        // Tự động tải từ Resources nếu Scene hiện tại chưa có
        GameObject prefab = Resources.Load<GameObject>("Farming System");
        if (prefab != null)
        {
            var go = Instantiate(prefab);
            go.name = "Farming System";
            return go.GetComponent<FarmingTerrainManager>();
        }

        return null;
    }
}
