using UnityEngine;
using Kinnly;

/// <summary>
/// Điều khiển nhân vật di chuyển trên Map (2D Top-down).
/// Gắn lên GameObject Player cùng Rigidbody2D và Collider2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMapController : MonoBehaviour
{
    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 5f;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    [Header("Animation (tùy chọn)")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Trạng thái di chuyển")]
    [SerializeField] private bool canMove = true;

    public bool CanMove
    {
        get => canMove;
        set => canMove = value;
    }

    [Header("Tương tác")]
    public float interactionRange = 1.5f;
    private Kinnly.PlayerInventory inventory;

    private static readonly int AnimSpeed = Animator.StringToHash("speed");
    private static readonly int AnimDirX  = Animator.StringToHash("dirX");
    private static readonly int AnimDirY  = Animator.StringToHash("dirY");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<Kinnly.PlayerInventory>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Đảm bảo tốc độ không bao giờ bị nhận số 0
        if (moveSpeed <= 0.1f) moveSpeed = 5f;
        canMove = true;
    }

    private void Update()
    {
        if (!canMove) 
        { 
            moveInput = Vector2.zero; 
            return; 
        }

        // 1. Nhận input WASD/Joystick/Phím mũi tên
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        // 2. Nhận input Click chuột (Click để Tương tác)
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
                if (hit.collider != null)
                {
                    IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                    if (interactable != null)
                    {
                        float distance = Vector2.Distance(transform.position, hit.collider.bounds.center);
                        if (distance <= interactionRange)
                        {
                            interactable.Interact(inventory);
                        }
                        else
                        {
                            Debug.Log("Bạn cần tiến lại gần hơn để tương tác!");
                        }
                    }
                }
            }
        }

        // 3. Xử lý Animation
        if (animator != null)
        {
            animator.SetFloat(AnimSpeed, moveInput.magnitude);
            if (moveInput != Vector2.zero)
            {
                animator.SetFloat(AnimDirX, moveInput.x);
                animator.SetFloat(AnimDirY, moveInput.y);

                // Tự động lật mặt nhân vật (trái/phải)
                SpriteRenderer sr = animator.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (moveInput.x < 0) sr.flipX = true;
                    else if (moveInput.x > 0) sr.flipX = false;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        float speed = moveSpeed > 0.1f ? moveSpeed : 5f;
        rb.linearVelocity = moveInput * speed;
    }

    /// <summary>Tạm dừng điều khiển (khi hiển thị popup encounter, hội thoại...).</summary>
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!value) 
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            // Ép tốc độ về 0 để Animator chuyển về trạng thái Idle (đứng im)
            if (animator != null)
            {
                animator.SetFloat(AnimSpeed, 0f);
            }
        }
    }

    /// <summary>Cập nhật Animator khi đổi nhân vật.</summary>
    public void SetAnimator(Animator anim)
    {
        animator = anim;
    }
}
