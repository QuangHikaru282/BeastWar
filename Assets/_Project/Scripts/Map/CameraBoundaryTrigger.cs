using UnityEngine;

/// <summary>
/// Gắn vào một vùng Trigger 2D nằm ở ranh giới giữa 2 Map.
/// Khi Player bước qua ranh giới đi vào Map mới, Camera sẽ lập tức cập nhật lại tọa độ giới hạn (Bounds).
/// </summary>
public class CameraBoundaryTrigger : MonoBehaviour
{
    [Header("Giới hạn Camera cho Map này")]
    [Tooltip("Tọa độ góc Dưới-Trái của khung giới hạn")]
    public Vector3 newMinValue;
    
    [Tooltip("Tọa độ góc Trên-Phải của khung giới hạn")]
    public Vector3 newMaxValue;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tìm CameraMovement (do script này hiện đang nằm trong CoreGame/Camera)
            CameraMovement camMovement = Object.FindFirstObjectByType<CameraMovement>();
            if (camMovement != null)
            {
                camMovement.minValue = newMinValue;
                camMovement.maxValue = newMaxValue;
                Debug.Log($"[Seamless] Đã cập nhật Camera Bounds cho map mới: Min({newMinValue}), Max({newMaxValue})");
            }
            else
            {
                Debug.LogWarning("[Seamless] Không tìm thấy script CameraMovement trong Scene!");
            }
        }
    }

    // Vẽ khung màu xanh lá cây trong Scene View để bạn dễ dàng căn chỉnh tọa độ bằng mắt
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = (newMinValue + newMaxValue) / 2f;
        Vector3 size = newMaxValue - newMinValue;
        Gizmos.DrawWireCube(center, size);
    }
}
