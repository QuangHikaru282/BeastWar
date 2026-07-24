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

    [Header("Animation (tùy chọn)")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool canMove = true;

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
    }

    private void Update()
    {
        if (!canMove) 
        { 
            moveInput = Vector2.zero; 
            return; 
        }

        // 1. Nhận input WASD/Joystick
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // 2. Nhận input Click chuột (Click để Tương tác)
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                // Bắn Raycast kiểm tra có click trúng Object tương tác không
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
                if (hit.collider != null)
                {
                    IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                    if (interactable != null)
                    {
                        float distance = Vector2.Distance(transform.position, hit.collider.bounds.center);
                        if (distance <= interactionRange)
                        {
                            // Đã đứng gần -> Tương tác
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

        // 4. Xử lý Animation
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
        rb.linearVelocity = moveInput * moveSpeed;
    }

    /// <summary>Tạm dừng điều khiển (khi hiển thị popup encounter).</summary>
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!value) 
        {
            rb.linearVelocity = Vector2.zero;
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
