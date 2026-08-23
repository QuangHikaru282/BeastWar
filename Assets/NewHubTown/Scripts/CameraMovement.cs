using UnityEngine;

/// <summary>
/// Điều khiển Camera di chuyển mượt mà bám theo Player và giới hạn trong khung toạ độ Min / Max của Map.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Range(1, 20)]
    public float smoothfactor = 5f;
    public Vector3 minValue, maxValue;

    private void Awake()
    {
        // Nếu Camera đang bị lồng làm con của Player trong Prefab, tách ra để Camera có thể dừng lại ở rìa map độc lập với nhân vật
        if (transform.parent != null)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        FindPlayerTarget();
        SnapToTarget();
    }

    private void LateUpdate()
    {
        Follow();
    }

    private void FindPlayerTarget()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p == null)
            {
                var pmc = Object.FindFirstObjectByType<PlayerMapController>();
                if (pmc != null) p = pmc.gameObject;
            }
            if (p != null) target = p.transform;
        }
    }

    /// <summary>
    /// Tìm vùng CameraZoneConfiner thực sự chứa vị trí hiện tại của nhân vật.
    /// Nếu không nằm trong bất kỳ Zone nào (như Shop, Nhà, phòng nhỏ), sẽ tự động xóa giới hạn cũ để camera tự do theo Player.
    /// </summary>
    public void RefreshCurrentZone()
    {
        if (target == null) FindPlayerTarget();
        if (target == null) return;

        CameraZoneConfiner[] allZones = Object.FindObjectsByType<CameraZoneConfiner>(FindObjectsSortMode.None);
        CameraZoneConfiner matchedZone = null;

        foreach (var z in allZones)
        {
            if (z != null && z.gameObject.activeInHierarchy)
            {
                BoxCollider2D col = z.GetComponent<BoxCollider2D>();
                if (col != null && col.bounds.Contains(target.position))
                {
                    matchedZone = z;
                    break;
                }
            }
        }

        if (matchedZone != null)
        {
            matchedZone.ApplyZoneCameraBounds();
        }
        else
        {
            // Không nằm trong Zone nào -> Xóa toàn bộ giới hạn của Map cũ
            ResetBounds();
        }
    }

    void Follow()
    {
        if (target == null)
        {
            FindPlayerTarget();
            if (target == null) return;
        }

        Vector3 targetPosition = target.position + offset;
        
        bool hasLimitX = minValue.x <= maxValue.x && (minValue.x != 0 || maxValue.x != 0);
        bool hasLimitY = minValue.y <= maxValue.y && (minValue.y != 0 || maxValue.y != 0);

        // Nếu Player đi ra ngoài vùng giới hạn hiện tại, tự động tìm và đổi sang Zone mới chứa Player
        if (hasLimitX && (targetPosition.x < minValue.x - 1f || targetPosition.x > maxValue.x + 1f) ||
            hasLimitY && (targetPosition.y < minValue.y - 1f || targetPosition.y > maxValue.y + 1f))
        {
            RefreshCurrentZone();
            hasLimitX = minValue.x <= maxValue.x && (minValue.x != 0 || maxValue.x != 0);
            hasLimitY = minValue.y <= maxValue.y && (minValue.y != 0 || maxValue.y != 0);
        }

        float clampedX = hasLimitX ? Mathf.Clamp(targetPosition.x, minValue.x, maxValue.x) : targetPosition.x;
        float clampedY = hasLimitY ? Mathf.Clamp(targetPosition.y, minValue.y, maxValue.y) : targetPosition.y;
        float clampedZ = targetPosition.z != 0 ? targetPosition.z : -10f;

        Vector3 boundPosition = new Vector3(clampedX, clampedY, clampedZ);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, boundPosition, smoothfactor * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Xoá giới hạn cũ để camera tự do theo Player trong scene mới nếu scene mới chưa đặt bounds.
    /// </summary>
    public void ResetBounds()
    {
        minValue = Vector3.zero;
        maxValue = Vector3.zero;
    }

    /// <summary>
    /// Đưa camera lập tức đến vị trí nhân vật (tránh bị kẹt ở map cũ hoặc zone cũ).
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null)
        {
            FindPlayerTarget();
        }

        if (target != null)
        {
            RefreshCurrentZone();

            Vector3 targetPosition = target.position + offset;
            bool hasLimitX = minValue.x <= maxValue.x && (minValue.x != 0 || maxValue.x != 0);
            float clampedX = hasLimitX ? Mathf.Clamp(targetPosition.x, minValue.x, maxValue.x) : targetPosition.x;

            bool hasLimitY = minValue.y <= maxValue.y && (minValue.y != 0 || maxValue.y != 0);
            float clampedY = hasLimitY ? Mathf.Clamp(targetPosition.y, minValue.y, maxValue.y) : targetPosition.y;

            transform.position = new Vector3(clampedX, clampedY, targetPosition.z != 0 ? targetPosition.z : -10f);
        }
    }
}
