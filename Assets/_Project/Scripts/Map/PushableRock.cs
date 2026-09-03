using System.Collections;
using UnityEngine;
using DG.Tweening;
using Kinnly;

/// <summary>
/// Kĩ năng Đẩy Đá (tương tự Strength trong Pokémon).
/// Gắn script này lên GameObject Hòn đá (Stone).
/// Hòn đá cần có Collider2D (không tick isTrigger) và Rigidbody2D (Body Type: Kinematic).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PushableRock : MonoBehaviour, IInteractable
{
    [Header("Cài đặt Đẩy Đá")]
    [Tooltip("Khoảng cách mỗi lần đẩy (thường là 1 ô tile = 1 đơn vị)")]
    [SerializeField] private float pushDistance = 1.0f;

    [Tooltip("Thời gian trượt của hòn đá sang vị trí mới (giây)")]
    [SerializeField] private float slideDuration = 0.25f;

    [Tooltip("Thời gian người chơi cần tì người vào đá trước khi đá bắt đầu lăn (giây)")]
    [SerializeField] private float pushHoldTime = 0.35f;

    [Header("Kiểm tra Vật cản phía trước")]
    [Tooltip("Chọn các Layer coi là vật cản ngăn đá di chuyển (ví dụ: Collision, Decor, Water, Default...)")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("Bán kính vòng tròn kiểm tra vật cản ở ô đích")]
    [SerializeField] private float checkRadius = 0.35f;

    [Header("Nhận diện Người chơi")]
    [Tooltip("Tag của nhân vật người chơi")]
    [SerializeField] private string playerTag = "Player";

    [Header("Điều kiện Kĩ năng (Tùy chọn)")]
    [Tooltip("Tick chọn nếu cần có Huy Hiệu hoặc Nhiệm vụ mới được đẩy")]
    [SerializeField] private bool requireCondition = false;

    [Tooltip("Kéo file ScriptableObject PlayerData vào đây nếu muốn kiểm tra điều kiện")]
    [SerializeField] private PlayerData playerData;

    [Tooltip("ID Huy hiệu Gym cần có để đẩy (Ví dụ: EarthBadge, GrassBadge...). Bỏ trống nếu không yêu cầu")]
    [SerializeField] private string requiredBadgeId = "";

    [Tooltip("ID Nhiệm vụ cần hoàn thành (0 nếu không yêu cầu)")]
    [SerializeField] private int requiredQuestId = 0;

    [Tooltip("Câu thông báo hiện lên nếu người chơi chưa đủ điều kiện đẩy đá")]
    [SerializeField] private string lockedMessage = "Hòn đá này quá nặng! Cần có kĩ năng thích hợp để đẩy.";

    [Header("Âm thanh & Hiệu ứng (Tùy chọn)")]
    [Tooltip("Âm thanh khi đá trượt (nếu có)")]
    [SerializeField] private AudioClip pushSfx;

    // Trạng thái nội bộ
    private bool isMoving = false;
    private float currentHoldTimer = 0f;
    private Collider2D rockCollider;

    private void Awake()
    {
        rockCollider = GetComponent<Collider2D>();

        // Đảm bảo có Rigidbody2D Kinematic để tương tác vật lý 2D chuẩn xác
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // CÁCH 1: Người chơi đi bộ tì người vào đá để đẩy
    // ─────────────────────────────────────────────────────────────────────────────
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isMoving) return;

        if (!collision.gameObject.CompareTag(playerTag)) return;

        // Tính hướng từ người chơi đến hòn đá
        Vector2 contactPoint = collision.GetContact(0).point;
        Vector2 playerPos = collision.transform.position;
        Vector2 dir = ((Vector2)transform.position - playerPos);

        // Chuẩn hóa về 4 hướng chính (Lên, Xuống, Trái, Phải)
        Vector2 pushDir = GetCardinalDirection(dir);

        currentHoldTimer += Time.deltaTime;
        if (currentHoldTimer >= pushHoldTime)
        {
            currentHoldTimer = 0f;
            TryPush(pushDir);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            currentHoldTimer = 0f;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // CÁCH 2: Người chơi nhấn phím F (IInteractable) để đẩy
    // ─────────────────────────────────────────────────────────────────────────────
    public void Interact(PlayerInventory playerInventory)
    {
        if (isMoving) return;

        // Tìm hướng từ Player đến hòn đá
        GameObject player = playerInventory != null ? playerInventory.gameObject : GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        Vector2 dir = ((Vector2)transform.position - (Vector2)player.transform.position);
        Vector2 pushDir = GetCardinalDirection(dir);

        TryPush(pushDir);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // XỬ LÝ ĐẨY ĐÁ
    // ─────────────────────────────────────────────────────────────────────────────
    public void TryPush(Vector2 direction)
    {
        if (isMoving) return;

        // 1. Kiểm tra điều kiện mở khóa kĩ năng (nếu có)
        if (requireCondition && !CheckCanPush())
        {
            ShowLockedNotice();
            // Rung nhẹ đá để phản hồi cho người chơi
            transform.DOShakePosition(0.2f, 0.08f, 15, 90f);
            return;
        }

        // 2. Tính toán vị trí đích
        Vector2 currentPos = transform.position;
        Vector2 targetPos = currentPos + (direction * pushDistance);

        // 3. Kiểm tra xem ô đích có vật cản không
        if (IsBlocked(targetPos))
        {
            // Bị kẹt vật cản -> Rung đá báo hiệu không đẩy được
            transform.DOShakePosition(0.15f, 0.06f, 12, 90f);
            return;
        }

        // 4. Bắt đầu trượt đá sang vị trí mới
        StartSlide(targetPos);
    }

    private bool IsBlocked(Vector2 targetPos)
    {
        // Tắt tạm collider của đá để Overlap không tự va vào chính mình
        rockCollider.enabled = false;
        Collider2D hit = Physics2D.OverlapCircle(targetPos, checkRadius, obstacleLayer);
        rockCollider.enabled = true;

        return hit != null;
    }

    private void StartSlide(Vector2 targetPos)
    {
        isMoving = true;

        // Phát âm thanh đẩy đá nếu có
        if (pushSfx != null)
        {
            AudioSource.PlayClipAtPoint(pushSfx, transform.position);
        }

        // Trượt mượt mà đến vị trí mới bằng DOTween
        transform.DOMove(new Vector3(targetPos.x, targetPos.y, transform.position.z), slideDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isMoving = false;
            });
    }

    private bool CheckCanPush()
    {
        PlayerData data = playerData != null ? playerData : (QuestManager.Instance != null ? QuestManager.Instance.playerData : Resources.Load<PlayerData>("PlayerData"));
        if (data == null) return true;

        // Kiểm tra Huy hiệu Gym
        if (!string.IsNullOrEmpty(requiredBadgeId))
        {
            if (data.gymBadges == null || !data.gymBadges.Contains(requiredBadgeId))
            {
                return false;
            }
        }

        // Kiểm tra Nhiệm vụ chính
        if (requiredQuestId > 0)
        {
            if (data.currentMainQuestId < requiredQuestId)
            {
                return false;
            }
        }

        return true;
    }

    private void ShowLockedNotice()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Đá Cản Đường", lockedMessage);
        }
        else
        {
            Debug.Log($"[PushableRock] {lockedMessage}");
        }
    }

    /// <summary>
    /// Chuyển đổi vector hướng bất kỳ thành 1 trong 4 hướng chính: Up, Down, Left, Right
    /// </summary>
    private Vector2 GetCardinalDirection(Vector2 rawDir)
    {
        if (Mathf.Abs(rawDir.x) > Mathf.Abs(rawDir.y))
        {
            return rawDir.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            return rawDir.y > 0 ? Vector2.up : Vector2.down;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * pushDistance, checkRadius);
        Gizmos.DrawWireSphere(transform.position + Vector3.down * pushDistance, checkRadius);
        Gizmos.DrawWireSphere(transform.position + Vector3.left * pushDistance, checkRadius);
        Gizmos.DrawWireSphere(transform.position + Vector3.right * pushDistance, checkRadius);
    }
#endif
}
