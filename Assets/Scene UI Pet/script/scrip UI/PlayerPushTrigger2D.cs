using TMPro;
using UnityEngine;

public class PlayerPushTrigger2D : MonoBehaviour
{
    [Header("Đẩy vật cản")]
    [SerializeField] private float pushSpeed = 2f;

    [Header("UI hướng dẫn")]
    [SerializeField] private TMP_Text interactionText;
    [SerializeField]
    private string guideText =
        "Giữ SPACE và di chuyển để đẩy";

    private PushableObstacle2D touchingObstacle;
    private Vector2 moveInput;

    private void Start()
    {
        HideText();
    }

    private void Update()
    {
        ReadMovementInput();
        HandlePush();
    }

    private void ReadMovementInput()
    {
        // Chỉ đọc phím, không điều khiển Player
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void HandlePush()
    {
        if (touchingObstacle == null)
            return;

        bool holdingSpace = Input.GetKey(KeyCode.Space);
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool movingTowardObstacle = IsMovingTowardObstacle();

        if (holdingSpace && isMoving && movingTowardObstacle)
        {
            touchingObstacle.Push(moveInput, pushSpeed);
        }
        else
        {
            // Không giữ Space thì vật cản không có hiệu ứng
            touchingObstacle.StopPush();
        }
    }

    private bool IsMovingTowardObstacle()
    {
        if (touchingObstacle == null)
            return false;

        Vector2 directionToObstacle =
            ((Vector2)touchingObstacle.transform.position -
             (Vector2)transform.position).normalized;

        float dotValue = Vector2.Dot(
            moveInput,
            directionToObstacle
        );

        // Chỉ đẩy khi nhân vật đi về phía vật cản
        return dotValue > 0.1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        FindObstacle(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        FindObstacle(other);
    }

    private void FindObstacle(Collider2D other)
    {
        if (!other.CompareTag("vatcan"))
            return;

        PushableObstacle2D obstacle =
            other.GetComponent<PushableObstacle2D>();

        if (obstacle == null)
        {
            obstacle = other.GetComponentInParent<PushableObstacle2D>();
        }

        if (obstacle == null)
            return;

        touchingObstacle = obstacle;
        ShowText();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("vatcan"))
            return;

        PushableObstacle2D exitedObstacle =
            other.GetComponent<PushableObstacle2D>();

        if (exitedObstacle == null)
        {
            exitedObstacle =
                other.GetComponentInParent<PushableObstacle2D>();
        }

        if (exitedObstacle != touchingObstacle)
            return;

        touchingObstacle.StopPush();
        touchingObstacle = null;

        HideText();
    }

    private void ShowText()
    {
        if (interactionText == null)
            return;

        interactionText.text = guideText;
        interactionText.gameObject.SetActive(true);
    }

    private void HideText()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (touchingObstacle != null)
        {
            touchingObstacle.StopPush();
        }
    }
}