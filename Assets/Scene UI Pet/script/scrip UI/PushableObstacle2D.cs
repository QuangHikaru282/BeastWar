using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PushableObstacle2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isBeingPushed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Không giữ Space thì khóa vật cản
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void Push(Vector2 direction, float pushSpeed)
    {
        if (direction.sqrMagnitude <= 0.01f)
        {
            StopPush();
            return;
        }

        isBeingPushed = true;

        // Cho phép di chuyển nhưng không cho xoay
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb.linearVelocity = direction.normalized * pushSpeed;
    }

    public void StopPush()
    {
        if (!isBeingPushed)
            return;

        isBeingPushed = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}