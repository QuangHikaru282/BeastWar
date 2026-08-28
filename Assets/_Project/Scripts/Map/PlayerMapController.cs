using TMPro;
using UnityEngine;
using Kinnly;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMapController : MonoBehaviour
{
    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Va chạm ảo với vật cản")]
    [Tooltip("Layer của những object có tag vatcan")]
    [SerializeField] private LayerMask vatCanLayer;

    [Tooltip("Kích thước vùng chặn ảo của Player")]
    [SerializeField]
    private Vector2 blockBoxSize =
        new Vector2(0.6f, 0.8f);

    [Tooltip("Điều chỉnh vị trí vùng chặn ảo")]
    [SerializeField]
    private Vector2 blockBoxOffset =
        Vector2.zero;

    [SerializeField] private float skinWidth = 0.05f;

    [Header("Đẩy vật cản")]
    [SerializeField] private float pushSpeed = 2f;

    [Tooltip("Khoảng cách hiện hướng dẫn")]
    [SerializeField] private float pushDetectionRadius = 1.2f;

    [SerializeField] private TMP_Text pushGuideText;

    [SerializeField]
    private string pushGuideMessage =
        "Giữ SPACE và di chuyển để đẩy";

    [Header("Tương tác")]
    public float interactionRange = 1.5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool canMove = true;
    private bool holdingSpace;

    private PlayerInventory inventory;
    private PushableObstacle2D currentPushedObstacle;

    private static readonly int AnimSpeed =
        Animator.StringToHash("speed");

    private static readonly int AnimDirX =
        Animator.StringToHash("dirX");

    private static readonly int AnimDirY =
        Animator.StringToHash("dirY");

    public bool CanMove
    {
        get => canMove;
        set => SetCanMove(value);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<PlayerInventory>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }
    private void Start()
    {
        HidePushText();
    }

    private void Update()
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            holdingSpace = false;

            HidePushText();
            UpdateAnimation();
            return;
        }

        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        holdingSpace = Input.GetKey(KeyCode.Space);

        HandleMouseInteraction();
        DetectNearbyObstacle();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            StopCurrentPush();
            return;
        }

        HandleMovementAndPush();
    }

    private void HandleMovementAndPush()
    {
        if (moveInput.sqrMagnitude <= 0.01f)
        {
            rb.linearVelocity = Vector2.zero;
            StopCurrentPush();
            return;
        }

        Vector2 desiredVelocity = moveInput * moveSpeed;

        Vector2 castOrigin =
            rb.position + blockBoxOffset;

        float castDistance =
            moveSpeed * Time.fixedDeltaTime + skinWidth;

        RaycastHit2D hit = Physics2D.BoxCast(
            castOrigin,
            blockBoxSize,
            0f,
            moveInput,
            castDistance,
            vatCanLayer
        );

        if (hit.collider == null || !IsVatCan(hit.collider))
        {
            StopCurrentPush();
            rb.linearVelocity = desiredVelocity;
            return;
        }

        PushableObstacle2D obstacle =
            hit.collider.GetComponent<PushableObstacle2D>();

        if (obstacle == null)
        {
            obstacle =
                hit.collider.GetComponentInParent<PushableObstacle2D>();
        }

        /*
         * Đang giữ Space:
         * Cho Player đi tới và đẩy vật cản.
         */
        if (holdingSpace && obstacle != null)
        {
            if (currentPushedObstacle != null &&
                currentPushedObstacle != obstacle)
            {
                currentPushedObstacle.StopPush();
            }

            currentPushedObstacle = obstacle;
            currentPushedObstacle.Push(moveInput, pushSpeed);

            // Player đi bằng tốc độ đẩy để không xuyên qua vật
            float playerPushSpeed =
                Mathf.Min(moveSpeed, pushSpeed);

            rb.linearVelocity =
                moveInput * playerPushSpeed;

            return;
        }

        /*
         * Không giữ Space:
         * Loại bỏ phần vận tốc đang hướng vào vật cản.
         * Player vẫn có thể đi ngang hoặc đi lùi ra ngoài.
         */
        StopCurrentPush();

        Vector2 blockedVelocity =
            RemoveVelocityIntoObstacle(
                desiredVelocity,
                hit
            );

        rb.linearVelocity = blockedVelocity;
    }

    private Vector2 RemoveVelocityIntoObstacle(
        Vector2 velocity,
        RaycastHit2D hit)
    {
        Vector2 directionIntoObstacle;

        if (hit.distance <= 0.001f)
        {
            // Player đang chạm hoặc hơi nằm trong vật cản
            directionIntoObstacle =
                ((Vector2)hit.collider.bounds.center -
                 rb.position).normalized;
        }
        else
        {
            // Normal hướng ra ngoài nên phải đảo ngược
            directionIntoObstacle = -hit.normal;
        }

        float velocityIntoObstacle =
            Vector2.Dot(
                velocity,
                directionIntoObstacle
            );

        if (velocityIntoObstacle > 0f)
        {
            velocity -=
                directionIntoObstacle *
                velocityIntoObstacle;
        }

        return velocity;
    }

    private bool IsVatCan(Collider2D detectedCollider)
    {
        if (detectedCollider.CompareTag("vatcan"))
            return true;

        Transform parent = detectedCollider.transform.parent;

        return parent != null &&
               parent.CompareTag("vatcan");
    }

    private void DetectNearbyObstacle()
    {
        Vector2 detectionPosition =
            (Vector2)transform.position + blockBoxOffset;

        Collider2D[] detectedColliders =
            Physics2D.OverlapCircleAll(
                detectionPosition,
                pushDetectionRadius,
                vatCanLayer
            );

        bool foundObstacle = false;

        foreach (Collider2D detectedCollider
                 in detectedColliders)
        {
            if (IsVatCan(detectedCollider))
            {
                foundObstacle = true;
                break;
            }
        }

        if (pushGuideText == null)
            return;

        pushGuideText.gameObject.SetActive(foundObstacle);

        if (foundObstacle)
        {
            pushGuideText.text = pushGuideMessage;
        }
    }

    private void StopCurrentPush()
    {
        if (currentPushedObstacle == null)
            return;

        currentPushedObstacle.StopPush();
        currentPushedObstacle = null;
    }

    private void HandleMouseInteraction()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current
                .IsPointerOverGameObject())
        {
            return;
        }

        if (Camera.main == null)
            return;

        Vector2 mouseWorldPosition =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );

        RaycastHit2D hit =
            Physics2D.Raycast(
                mouseWorldPosition,
                Vector2.zero
            );

        if (hit.collider == null)
            return;

        IInteractable interactable =
            hit.collider
                .GetComponentInParent<IInteractable>();

        if (interactable == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            hit.collider.bounds.center
        );

        if (distance <= interactionRange)
        {
            interactable.Interact(inventory);
        }
        else
        {
            Debug.Log(
                "Bạn cần tiến lại gần hơn để tương tác!"
            );
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        animator.SetFloat(
            AnimSpeed,
            moveInput.magnitude
        );

        if (moveInput == Vector2.zero)
            return;

        animator.SetFloat(AnimDirX, moveInput.x);
        animator.SetFloat(AnimDirY, moveInput.y);

        SpriteRenderer spriteRenderer =
            animator.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        if (moveInput.x < 0f)
        {
            spriteRenderer.flipX = true;
        }
        else if (moveInput.x > 0f)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void HidePushText()
    {
        if (pushGuideText != null)
        {
            pushGuideText.gameObject.SetActive(false);
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!value)
        {
            moveInput = Vector2.zero;
            holdingSpace = false;
            rb.linearVelocity = Vector2.zero;

            StopCurrentPush();
            HidePushText();

            if (animator != null)
            {
                animator.SetFloat(AnimSpeed, 0f);
            }
        }
    }

    public void SetAnimator(Animator anim)
    {
        animator = anim;
    }

    private void OnDisable()
    {
        StopCurrentPush();
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 center =
            (Vector2)transform.position +
            blockBoxOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            center,
            blockBoxSize
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            center,
            pushDetectionRadius
        );
    }
}