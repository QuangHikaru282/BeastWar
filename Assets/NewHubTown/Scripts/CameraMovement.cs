
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public Transform target;
    
    public Vector3 offset;
    [Range(1,10)]
    public float smoothfactor;
    public Vector3 minValue, maxValue;
    // public Vector2 maxPosition;
    // public Vector2 minPosition;
    // Start is called before the first frame update
    private void FixedUpdate(){
        Follow();
    }

    void Follow(){
        if (target == null) return;
        Vector3 targetPosition = target.position + offset;
        
        // Nếu min/max chưa được thiết lập hoặc bằng nhau, bám theo target không clamp
        float clampedX = (minValue.x < maxValue.x) ? Mathf.Clamp(targetPosition.x, minValue.x, maxValue.x) : targetPosition.x;
        float clampedY = (minValue.y < maxValue.y) ? Mathf.Clamp(targetPosition.y, minValue.y, maxValue.y) : targetPosition.y;
        float clampedZ = targetPosition.z;

        Vector3 boundPosition = new Vector3(clampedX, clampedY, clampedZ);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, boundPosition, smoothfactor * Time.fixedDeltaTime);
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
    /// Đưa camera lập tức đến vị trí nhân vật (tránh bị kẹt ở map cũ).
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }

        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            float clampedX = (minValue.x < maxValue.x) ? Mathf.Clamp(targetPosition.x, minValue.x, maxValue.x) : targetPosition.x;
            float clampedY = (minValue.y < maxValue.y) ? Mathf.Clamp(targetPosition.y, minValue.y, maxValue.y) : targetPosition.y;
            transform.position = new Vector3(clampedX, clampedY, targetPosition.z != 0 ? targetPosition.z : -10f);
        }
    }
}
