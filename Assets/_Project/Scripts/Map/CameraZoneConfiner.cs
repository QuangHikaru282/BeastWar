using UnityEngine;

/// <summary>
/// Đặt script này vào từng Khu vực (Zone 1, Zone 2, Zone 3, Zone 4).
/// Yêu cầu: GameObject có BoxCollider2D (Is Trigger = True) bao quanh khu vực đó.
/// Khi Player bước vào khu vực nào:
/// 1. Tự động khoá Camera không cho trôi ra ngoài biên của khu vực đó.
/// 2. Camera sẽ di chuyển mượt mà bám theo Player bên trong khung giới hạn.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CameraZoneConfiner : MonoBehaviour
{
    private BoxCollider2D zoneCollider;
    private CameraMovement camMovement;

    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyZoneCameraBounds();
        }
    }

    /// <summary>
    /// Tự động tính toán toạ độ Min / Max từ kích thước BoxCollider2D và gán cho CameraMovement.
    /// </summary>
    public void ApplyZoneCameraBounds()
    {
        if (camMovement == null)
        {
            camMovement = Object.FindFirstObjectByType<CameraMovement>();
        }

        if (camMovement != null && zoneCollider != null)
        {
            Bounds b = zoneCollider.bounds;

            // Tính Min và Max dựa vào toạ độ thực tế của Collider khu vực
            camMovement.minValue = new Vector3(b.min.x, b.min.y, camMovement.minValue.z);
            camMovement.maxValue = new Vector3(b.max.x, b.max.y, camMovement.maxValue.z);

            Debug.Log($"<color=cyan>[CameraZone]</color> Đã chuyển Camera sang {gameObject.name} | Min({b.min.x:F1}, {b.min.y:F1}) -> Max({b.max.x:F1}, {b.max.y:F1})");
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
