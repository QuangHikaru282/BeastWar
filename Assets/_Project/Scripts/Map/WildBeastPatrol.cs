using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Điều khiển Quái hoang dã đi tuần tra theo hình tam giác (hoặc đa giác)
/// và dừng lại đứng yên tại chỗ trong một khoảng thời gian trước khi đi tiếp.
/// </summary>
public class WildBeastPatrol : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float waitTimeAtPoint = 2f; // Thời gian đứng chờ tại mỗi điểm

    [Header("Tọa độ các điểm tuần tra (Tương đối so với điểm xuất phát)")]
    [Tooltip("Nhập các tọa độ (X, Y) để tạo thành lộ trình. Ví dụ 3 điểm để tạo hình tam giác.")]
    [SerializeField] private List<Vector2> patrolOffsets = new List<Vector2>()
    {
        new Vector2(0f, 0f),       // Điểm 1 (Điểm xuất phát)
        new Vector2(3f, 3f),       // Điểm 2
        new Vector2(-3f, 3f)       // Điểm 3
    };

    [Header("Hoạt ảnh (tùy chọn)")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private List<Vector3> patrolPoints = new List<Vector3>();
    private int currentPointIndex = 0;
    private bool isMoving = true;
    private bool isWaiting = false;
    private Vector3 startPosition;

    // Các cờ kiểm tra tham số Animator tồn tại để tránh cảnh báo Console
    private bool hasIsMovingParam = false;
    private bool hasDirectionParam = false;

    private void Start()
    {
        startPosition = transform.position;

        // Khởi tạo các điểm tuần tra tuyệt đối từ các chỉ số lệch (offsets)
        if (patrolOffsets == null || patrolOffsets.Count == 0)
        {
            // Dự phòng nếu danh sách trống
            patrolPoints.Add(startPosition);
        }
        else
        {
            foreach (Vector2 offset in patrolOffsets)
            {
                patrolPoints.Add(startPosition + new Vector3(offset.x, offset.y, 0f));
            }
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();

        // Kiểm tra xem các tham số có tồn tại trong Animator không
        if (animator != null)
        {
            hasIsMovingParam = HasAnimatorParameter(animator, "IsMoving");
            hasDirectionParam = HasAnimatorParameter(animator, "Direction");
        }

        // Bắt đầu di chuyển tới điểm đầu tiên
        currentPointIndex = 0;
    }

    private bool HasAnimatorParameter(Animator anim, string paramName)
    {
        if (anim == null) return false;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    private void Update()
    {
        // Nếu bị script Encounter khóa di chuyển (khi chạm người chơi)
        if (!isMoving)
        {
            UpdateAnimation(Vector3.zero);
            return;
        }

        // Nếu đang trong trạng thái chờ tại điểm
        if (isWaiting)
        {
            UpdateAnimation(Vector3.zero);
            return;
        }

        Vector3 targetPos = patrolPoints[currentPointIndex];
        
        // Di chuyển đến điểm đích tiếp theo
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Hướng di chuyển hiện tại
        Vector3 moveDir = (targetPos - transform.position).normalized;
        UpdateAnimation(moveDir);

        // Kiểm tra xem đã đến đích chưa
        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            StartCoroutine(WaitAtPointRoutine());
        }
    }

    private IEnumerator WaitAtPointRoutine()
    {
        isWaiting = true;

        // Đợi trong thời gian chỉ định
        yield return new WaitForSeconds(waitTimeAtPoint);

        // Chuyển sang điểm đích tiếp theo trong danh sách (lặp lại khi hết)
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
        
        isWaiting = false;
    }

    private void UpdateAnimation(Vector3 dir)
    {
        bool moving = dir.magnitude > 0.05f;

        if (moving)
        {
            // 1. Tự động lật hình ảnh dựa trên trục X (Đi sang trái thì lật)
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = dir.x < 0;
            }

            // 2. Tính toán hướng đi thông minh (Up, Down, Left, Right) dựa trên góc xiên
            if (animator != null)
            {
                if (hasIsMovingParam) animator.SetBool("IsMoving", true);

                if (hasDirectionParam)
                {
                    // So sánh xem quái đang di chuyển thiên về chiều ngang hay chiều dọc nhiều hơn
                    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                    {
                        // Di chuyển ngang
                        animator.SetInteger("Direction", dir.x < 0 ? 3 : 2); // 3 = Trái, 2 = Phải
                    }
                    else
                    {
                        // Di chuyển dọc
                        animator.SetInteger("Direction", dir.y < 0 ? 0 : 1); // 0 = Dưới, 1 = Trên
                    }
                }
            }
        }
        else
        {
            // Khi đứng yên
            if (animator != null && hasIsMovingParam)
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }

    /// <summary>Khóa hoặc mở khóa di chuyển (Dùng cho script va chạm Encounter)</summary>
    public void SetMoving(bool moving)
    {
        isMoving = moving;
        if (!moving)
        {
            StopAllCoroutines();
            isWaiting = false;
        }
    }

    // Hiển thị đường đi tuần tra trong cửa sổ Scene (Chỉ hiển thị khi thiết kế)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 basePos = Application.isPlaying ? startPosition : transform.position;

        if (patrolOffsets != null && patrolOffsets.Count > 0)
        {
            List<Vector3> points = new List<Vector3>();
            foreach (Vector2 offset in patrolOffsets)
            {
                points.Add(basePos + new Vector3(offset.x, offset.y, 0f));
            }

            for (int i = 0; i < points.Count; i++)
            {
                Gizmos.DrawSphere(points[i], 0.2f);
                int nextIndex = (i + 1) % points.Count;
                Gizmos.DrawLine(points[i], points[nextIndex]);
            }
        }
    }
}
