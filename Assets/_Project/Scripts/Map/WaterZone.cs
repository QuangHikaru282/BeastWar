using UnityEngine;

/// <summary>
/// Gắn script này lên GameObject hoặc Tilemap vùng Nước (cần có Collider2D với Is Trigger = TRUE).
/// Tự động phát hiện người chơi bước xuống nước hoặc bước lên bờ.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WaterZone : MonoBehaviour
{
    [Header("Cài đặt")]
    [Tooltip("Tag của nhân vật người chơi")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(playerTag)) return;

        PlayerSwimmingController swimmer = collision.GetComponent<PlayerSwimmingController>();
        if (swimmer != null)
        {
            swimmer.OnEnterWater(transform.position);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(playerTag)) return;

        PlayerSwimmingController swimmer = collision.GetComponent<PlayerSwimmingController>();
        if (swimmer != null)
        {
            swimmer.OnExitWater();
        }
    }
}
