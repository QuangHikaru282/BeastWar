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

    private void Start()
    {
        // Tự động áp dụng bounds khi scene vừa load lên nếu người chơi đang ở trong zone này
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            var pmc = Object.FindFirstObjectByType<PlayerMapController>();
            if (pmc != null) player = pmc.gameObject;
        }

        if (player != null && zoneCollider != null)
        {
            if (zoneCollider.bounds.Contains(player.transform.position))
            {
                ApplyZoneCameraBounds();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMapController>() != null)
        {
            ApplyZoneCameraBounds();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Đảm bảo nếu camera bounds bị reset khi chuyển scene, zone hiện tại sẽ gán lại
        if (other.CompareTag("Player") || other.GetComponent<PlayerMapController>() != null)
        {
            if (camMovement == null) camMovement = Object.FindFirstObjectByType<CameraMovement>();
            if (camMovement != null && camMovement.minValue == Vector3.zero && camMovement.maxValue == Vector3.zero)
            {
                ApplyZoneCameraBounds();
            }
        }
    }

    /// <summary>
    /// Tự động tính toán toạ độ Min / Max từ kích thước BoxCollider2D (đã trừ kích thước khung nhìn Camera) và gán cho CameraMovement.
    /// </summary>
    public void ApplyZoneCameraBounds()
    {
        if (camMovement == null)
        {
            camMovement = Object.FindFirstObjectByType<CameraMovement>();
        }

        if (zoneCollider == null)
        {
            zoneCollider = GetComponent<BoxCollider2D>();
        }

        if (camMovement != null && zoneCollider != null)
        {
            Bounds b = zoneCollider.bounds;

            Camera cam = Camera.main;
            if (cam == null && camMovement != null)
            {
                cam = camMovement.GetComponent<Camera>();
            }

            float minX = b.min.x;
            float maxX = b.max.x;
            float minY = b.min.y;
            float maxY = b.max.y;

            // Trừ đi nửa chiều rộng và nửa chiều cao của Camera để mép màn hình không vượt ra ngoài biên Collider
            if (cam != null && cam.orthographic)
            {
                float vertExtent = cam.orthographicSize;
                float horzExtent = vertExtent * cam.aspect;

                minX = b.min.x + horzExtent;
                maxX = b.max.x - horzExtent;
                minY = b.min.y + vertExtent;
                maxY = b.max.y - vertExtent;

                // Nếu khu vực nhỏ hơn khung nhìn Camera, giữ Camera cố định ở tâm
                if (minX > maxX) minX = maxX = (b.min.x + b.max.x) / 2f;
                if (minY > maxY) minY = maxY = (b.min.y + b.max.y) / 2f;
            }

            camMovement.minValue = new Vector3(minX, minY, camMovement.minValue.z);
            camMovement.maxValue = new Vector3(maxX, maxY, camMovement.maxValue.z);

            Debug.Log($"<color=cyan>[CameraZone]</color> Đã chuyển Camera sang {gameObject.name} | Bounds: Min({minX:F1}, {minY:F1}) -> Max({maxX:F1}, {maxY:F1})");
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
