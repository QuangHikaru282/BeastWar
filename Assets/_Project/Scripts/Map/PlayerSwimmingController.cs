using UnityEngine;

/// <summary>
/// Quản lý trạng thái Bơi (Swim/Surf) của nhân vật khi di chuyển xuống nước.
/// Gắn script này lên Player (cùng cấp với PlayerMapController).
/// Mọi thiết lập được kéo thả thủ công qua Inspector.
/// </summary>
[RequireComponent(typeof(PlayerMapController))]
public class PlayerSwimmingController : MonoBehaviour
{
    [Header("── Điều Kiện Mở Khóa")]
    [Tooltip("Kéo ScriptableObject PlayerData vào đây")]
    [SerializeField] private PlayerData playerData;

    [Tooltip("ID Huy hiệu Gym Nước cần có để bơi")]
    [SerializeField] private string requiredBadgeId = "WaterBadge";

    [Tooltip("Tick chọn nếu muốn bơi thử nghiệm ngay mà không cần Huy hiệu")]
    [SerializeField] private bool bypassBadgeCheck = false;

    [Header("── Cài Đặt Tốc Độ")]
    [Tooltip("Tốc độ bơi dưới nước")]
    [SerializeField] private float swimSpeed = 3.8f;

    [Header("── Mô Hình Ngoại Hình")]
    [Tooltip("Kéo GameObject MaleModel (trong ảnh 1 của bạn) vào đây")]
    [SerializeField] private GameObject maleModel;

    [Tooltip("Kéo GameObject FemaleModel (trong ảnh 1 của bạn) vào đây")]
    [SerializeField] private GameObject femaleModel;

    [Tooltip("Kéo GameObject SwimVisual (bạn đã tạo ở ảnh 3, 4) vào đây")]
    [SerializeField] private GameObject swimVisual;

    [Tooltip("Animator của SwimVisual (Tùy chọn, nếu không dùng Animator thì bỏ trống)")]
    [SerializeField] private Animator swimAnimator;

    [Header("── Hoạt Họa Bơi 4 Hướng (Tự Động)")]
    [Tooltip("4 frame bơi nhìn xuống dưới (Swim_0 -> Swim_3)")]
    [SerializeField] private Sprite[] swimDownFrames;

    [Tooltip("4 frame bơi nhìn lên trên (Swim_4 -> Swim_7)")]
    [SerializeField] private Sprite[] swimUpFrames;

    [Tooltip("4 frame bơi nhìn sang ngang (Swim_8 -> Swim_11)")]
    [SerializeField] private Sprite[] swimSideFrames;

    [Tooltip("Tốc độ chuyển frame bơi (khuyên dùng 6 - 10 fps)")]
    [SerializeField] private float animationFps = 8f;

    [Header("── Thông Báo Khi Chưa Mở Khóa")]
    [TextArea(2, 3)]
    [SerializeField] private string lockedMessage = "Nước sâu quá! Bạn cần đánh bại Hội Quán Nước (nhận WaterBadge) mới có thể bơi.";

    [Header("── Trạng Thái (Runtime)")]
    [SerializeField] private bool isSwimming = false;

    public bool IsSwimming => isSwimming;

    private PlayerMapController playerMapCtrl;
    private Rigidbody2D rb;
    private SpriteRenderer swimRenderer;

    private enum SwimDirection { Down, Up, Left, Right }
    private SwimDirection currentDir = SwimDirection.Down;
    private float animTimer = 0f;
    private int currentFrameIndex = 0;

    private static readonly int AnimSpeed = Animator.StringToHash("speed");
    private static readonly int AnimDirX  = Animator.StringToHash("dirX");
    private static readonly int AnimDirY  = Animator.StringToHash("dirY");

    private void Awake()
    {
        playerMapCtrl = GetComponent<PlayerMapController>();
        rb = GetComponent<Rigidbody2D>();

        if (swimVisual == null)
        {
            Transform t = transform.Find("SwimVisual");
            if (t != null) swimVisual = t.gameObject;
        }

        if (swimVisual != null)
        {
            swimRenderer = swimVisual.GetComponent<SpriteRenderer>();
            if (swimAnimator == null) swimAnimator = swimVisual.GetComponent<Animator>();

            // Mặc định tắt mô hình bơi lúc bắt đầu game
            swimVisual.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isSwimming) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(inputX, inputY).normalized;

        // 1. Đồng bộ qua Animator nếu có gắn Controller
        if (swimAnimator != null && swimAnimator.runtimeAnimatorController != null)
        {
            swimAnimator.SetFloat(AnimSpeed, inputDir.magnitude);
            if (inputDir != Vector2.zero)
            {
                swimAnimator.SetFloat(AnimDirX, inputDir.x);
                swimAnimator.SetFloat(AnimDirY, inputDir.y);

                if (swimRenderer != null)
                {
                    if (inputDir.x < -0.01f) swimRenderer.flipX = true;
                    else if (inputDir.x > 0.01f) swimRenderer.flipX = false;
                }
            }
        }
        else
        {
            // 2. Tự động chuyển Sprite 4 hướng và tạo hiệu ứng quẫy nước
            UpdateSpriteAnimation(inputDir);
        }
    }

    private void UpdateSpriteAnimation(Vector2 inputDir)
    {
        if (swimRenderer == null && swimVisual != null)
        {
            swimRenderer = swimVisual.GetComponent<SpriteRenderer>();
        }
        if (swimRenderer == null) return;

        bool isMoving = inputDir.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            // Xác định hướng bơi chính
            if (Mathf.Abs(inputDir.x) > Mathf.Abs(inputDir.y))
            {
                currentDir = inputDir.x < 0 ? SwimDirection.Left : SwimDirection.Right;
            }
            else
            {
                currentDir = inputDir.y < 0 ? SwimDirection.Down : SwimDirection.Up;
            }

            // Chạy chu kỳ frame animation
            animTimer += Time.deltaTime * animationFps;
            if (animTimer >= 1f)
            {
                animTimer -= 1f;
                currentFrameIndex++;
            }
        }
        else
        {
            currentFrameIndex = 0;
            animTimer = 0f;
        }

        // Chọn tập frame theo hướng
        Sprite[] activeFrames = null;
        bool flipX = false;

        switch (currentDir)
        {
            case SwimDirection.Down:
                activeFrames = swimDownFrames;
                break;
            case SwimDirection.Up:
                activeFrames = swimUpFrames;
                break;
            case SwimDirection.Left:
                activeFrames = swimSideFrames;
                flipX = true;
                break;
            case SwimDirection.Right:
                activeFrames = swimSideFrames;
                flipX = false;
                break;
        }

        if (activeFrames != null && activeFrames.Length > 0)
        {
            int index = currentFrameIndex % activeFrames.Length;
            swimRenderer.sprite = activeFrames[index];
            swimRenderer.flipX = flipX;
        }
    }

    /// <summary>
    /// Kiểm tra người chơi đã đủ điều kiện bơi hay chưa
    /// </summary>
    public bool CanSwim()
    {
        if (bypassBadgeCheck) return true;

        PlayerData data = playerData != null ? playerData : (QuestManager.Instance != null ? QuestManager.Instance.playerData : Resources.Load<PlayerData>("PlayerData"));
        if (data == null) return false;

        return data.gymBadges != null && data.gymBadges.Contains(requiredBadgeId);
    }

    /// <summary>
    /// Được gọi bởi WaterZone khi người chơi bước vào vùng nước
    /// </summary>
    public void OnEnterWater(Vector3 waterCenter)
    {
        if (isSwimming) return;

        if (CanSwim())
        {
            // Đã có Huy hiệu -> Cho phép bơi
            isSwimming = true;

            if (playerMapCtrl != null)
            {
                playerMapCtrl.MoveSpeed = swimSpeed;
            }

            if (maleModel != null) maleModel.SetActive(false);
            if (femaleModel != null) femaleModel.SetActive(false);
            if (swimVisual != null) swimVisual.SetActive(true);

            Debug.Log("<color=cyan>[Swimming]</color> Nhân vật đã nhảy xuống nước và bắt đầu bơi!");
        }
        else
        {
            // Chưa có Huy hiệu -> Đẩy lùi người chơi lại bờ
            Vector2 playerPos = transform.position;
            Vector2 repelDir = ((Vector2)transform.position - (Vector2)waterCenter).normalized;
            if (repelDir == Vector2.zero) repelDir = Vector2.down;

            // Đẩy lùi nhẹ lại
            transform.position = playerPos + repelDir * 0.6f;
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Hiện câu thoại thông báo
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue("Nước Sâu", lockedMessage);
            }
            else
            {
                Debug.LogWarning($"[Swimming] {lockedMessage}");
            }
        }
    }

    /// <summary>
    /// Được gọi bởi WaterZone khi người chơi bước lên bờ
    /// </summary>
    public void OnExitWater()
    {
        if (!isSwimming) return;

        isSwimming = false;

        if (playerMapCtrl != null)
        {
            playerMapCtrl.MoveSpeed = 5f;
        }

        if (swimVisual != null) swimVisual.SetActive(false);

        // Khôi phục lại đúng giới tính Nam/Nữ trên cạn
        PlayerAvatarSelector avatarSelector = GetComponent<PlayerAvatarSelector>();
        if (avatarSelector != null)
        {
            avatarSelector.UpdateAvatar();
        }
        else
        {
            if (maleModel != null) maleModel.SetActive(true);
        }

        Debug.Log("<color=yellow>[Swimming]</color> Nhân vật đã bước lên cạn!");
    }
}
