using UnityEngine;

/// <summary>
/// Điều khiển nhân vật di chuyển trên Map (2D Top-down).
/// Gắn lên GameObject Player cùng Rigidbody2D và Collider2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMapController : MonoBehaviour
{
    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation (tùy chọn)")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool canMove = true;

    private static readonly int AnimSpeed = Animator.StringToHash("speed");
    private static readonly int AnimDirX  = Animator.StringToHash("dirX");
    private static readonly int AnimDirY  = Animator.StringToHash("dirY");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!canMove) { moveInput = Vector2.zero; return; }

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (animator != null)
        {
            animator.SetFloat(AnimSpeed, moveInput.magnitude);
            if (moveInput != Vector2.zero)
            {
                animator.SetFloat(AnimDirX, moveInput.x);
                animator.SetFloat(AnimDirY, moveInput.y);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    /// <summary>Tạm dừng điều khiển (khi hiển thị popup encounter).</summary>
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!value) rb.linearVelocity = Vector2.zero;
    }
}
