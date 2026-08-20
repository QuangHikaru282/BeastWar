using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Quản lý Trainer (Nhà huấn luyện) trên bản đồ:
/// - Trainer Chủ động (isAggressive = true): Khi Player bước vào vùng nhìn (Trigger), Trainer sẽ chạy đến trước mặt Player và khiêu chiến.
/// - Trainer Bị động (isAggressive = false): Đứng yên, chỉ khi Player chủ động lại gần bấm phím F mới khiêu chiến.
/// Tự động có thời gian an toàn (cooldown) sau khi load map để không bị vòng lặp thách đấu liên tục khi thua trận.
/// </summary>
public class TrainerController : MonoBehaviour, Kinnly.IInteractable
{
    [Header("1. Chế độ Khiêu chiến")]
    [Tooltip("TÍCH CHỌN nếu Trainer này CHỦ ĐỘNG chạy lại thách đấu khi Player đi vào vùng nhìn.\nBỎ TÍCH nếu Trainer ĐỨNG YÊN, chỉ đấu khi Player tự lại gần bấm F.")]
    public bool isAggressive = true;

    [Tooltip("Tốc độ chạy đến trước mặt Player (khi chủ động)")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Tooltip("Khoảng cách dừng lại trước mặt Player")]
    [SerializeField] private float stopDistance = 1.0f;

    [Header("2. Đội hình Trainer")]
    public List<BeastData> trainerTeam;
    
    [Header("3. Phần thưởng Chiến thắng")]
    [Tooltip("Số tiền Vàng thưởng khi người chơi thắng Trainer này (Ví dụ: 150, 300, 1000). Nếu để 0 sẽ tính theo quái.")]
    public int rewardGold = 200;

    [Header("4. Dữ liệu hệ thống")]
    public BattleTransferData battleTransferData;
    public string uniqueTrainerId;
    public bool alreadyDefeated = false;

    [Header("5. Giao diện & Hiệu ứng")]
    public GameObject exclamationMark; // Dấu chấm than hiện trên đầu (!)
    public GameObject dialogPanel;      // Khung thoại trước khi đánh (tùy chọn)
    public TextMeshProUGUI dialogText;
    
    [Header("6. Cấu hình Gym Leader")]
    [Tooltip("Tích chọn nếu NPC này là Gym Leader")]
    public bool isGymLeader = false;

    [Tooltip("ID Huy hiệu Gym (Ví dụ: BoulderBadge, CascadeBadge, ThunderBadge)")]
    public string gymBadgeId = "BoulderBadge";

    [TextArea]
    public string challengeText = "Ngươi đã lọt vào tầm mắt của ta! Hãy đấu một trận nào!";

    [Header("6. Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Tên Animation State đứng yên (Ví dụ: Idle hoặc idle)")]
    [SerializeField] private string idleStateName = "idle";

    [Tooltip("Tên Animation State đi bộ/chạy (Ví dụ: Npc1 hoặc Walk)")]
    [SerializeField] private string walkStateName = "Npc1";

    private bool isTriggered = false;
    private bool isCooldown = true;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueTrainerId))
        {
            uniqueTrainerId = "Trainer_" + transform.position.ToString();
        }
        
        if (exclamationMark != null) exclamationMark.SetActive(false);
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private IEnumerator Start()
    {
        // Kiểm tra xem Trainer này đã từng bị đánh bại chưa
        if (global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null)
        {
            if (global::QuestManager.Instance.playerData.defeatedTrainers.Contains(uniqueTrainerId))
            {
                alreadyDefeated = true;
            }
        }

        if (alreadyDefeated)
        {
            // Tắt các collider kích hoạt nếu đã bị đánh bại
            Collider2D[] cols = GetComponents<Collider2D>();
            foreach (var col in cols)
            {
                if (col.isTrigger) col.enabled = false;
            }
        }

        // Cho người chơi 3 giây an toàn sau khi load scene để di chuyển ra xa nếu vừa thua trận
        isCooldown = true;
        yield return new WaitForSeconds(3.0f);
        isCooldown = false;
    }

    /// <summary>
    /// Xử lý khi Player chạm vào vùng nhìn (Collider Trigger)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (alreadyDefeated || isTriggered || isCooldown) return;

        // Chỉ phản ứng nếu Trainer này là loại CHỦ ĐỘNG khiêu chiến
        if (isAggressive && collision.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(TrainerEncounterRoutine(collision.gameObject, true));
        }
    }

    /// <summary>
    /// Xử lý khi Player chủ động lại gần bấm phím F (IInteractable)
    /// </summary>
    public void Interact(Kinnly.PlayerInventory playerInventory)
    {
        if (alreadyDefeated)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(
                    "Nhà Huấn Luyện",
                    "Ngươi quả thực rất mạnh mẽ! Ta sẽ rèn luyện thêm và phục thù sau!"
                );
            }
            return;
        }

        if (!isTriggered)
        {
            isTriggered = true;
            GameObject playerObj = playerInventory != null ? playerInventory.gameObject : GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                // Nếu tự bấm F thì Trainer đứng yên nói chuyện (shouldMove = false)
                StartCoroutine(TrainerEncounterRoutine(playerObj, false));
            }
        }
    }

    private IEnumerator TrainerEncounterRoutine(GameObject playerObj, bool shouldMoveToPlayer)
    {
        // 1. Dừng và khóa chuyển động của người chơi
        LockPlayer(playerObj, true);

        // Quay mặt về phía người chơi
        FaceTowards(playerObj.transform.position);

        // 2. Hiện dấu chấm than (!)
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(true);
            yield return new WaitForSeconds(0.6f);
            exclamationMark.SetActive(false);
        }

        // 3. Nếu là Trainer chủ động -> Chạy lại trước mặt Player
        if (shouldMoveToPlayer)
        {
            yield return StartCoroutine(MoveTowardsPlayerRoutine(playerObj.transform.position));
        }

        // 4. Hiện câu thoại thách đấu
        bool dialogueFinished = false;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                "Nhà Huấn Luyện",
                string.IsNullOrEmpty(challengeText) ? "Ngươi đã lọt vào tầm mắt của ta! Hãy đấu một trận nào!" : challengeText,
                () => { dialogueFinished = true; }
            );
            yield return new WaitUntil(() => dialogueFinished);
        }
        else if (dialogPanel != null && dialogText != null)
        {
            dialogText.text = challengeText;
            dialogPanel.SetActive(true);
            
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
            dialogPanel.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(0.8f);
        }

        // 5. Chuyển vào trận chiến (Battle Scene)
        StartBattle(playerObj.transform.position);
    }

    /// <summary>
    /// Di chuyển Trainer tiến lại gần Player
    /// </summary>
    private IEnumerator MoveTowardsPlayerRoutine(Vector3 playerPos)
    {
        // Bật animation đi bộ (chuyển sang Npc1)
        SetWalkAnimation(true);

        // Tính vị trí đích (cách người chơi một khoảng stopDistance theo hướng đối diện)
        Vector2 directionToPlayer = ((Vector2)playerPos - (Vector2)transform.position).normalized;
        Vector2 targetPos = (Vector2)playerPos - directionToPlayer * stopDistance;

        while (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            FaceTowards(playerPos);
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Dừng lại và chuyển về animation đứng yên (Idle)
        transform.position = targetPos;
        SetWalkAnimation(false);
        FaceTowards(playerPos);
        yield return new WaitForSeconds(0.2f);
    }

    private void FaceTowards(Vector3 targetPos)
    {
        if (spriteRenderer != null)
        {
            if (targetPos.x < transform.position.x)
            {
                spriteRenderer.flipX = true; // Nhìn sang trái
            }
            else if (targetPos.x > transform.position.x)
            {
                spriteRenderer.flipX = false; // Nhìn sang phải
            }
        }
    }

    private void SetWalkAnimation(bool isMoving)
    {
        if (animator == null) return;

        bool hasParam = false;
        foreach (var param in animator.parameters)
        {
            if (param.name == "isWalking" || param.name == "IsWalking" || param.name == "isMove" || param.name == "IsMoving")
            {
                animator.SetBool(param.name, isMoving);
                hasParam = true;
            }
            else if (param.name == "Speed" || param.name == "speed")
            {
                animator.SetFloat(param.name, isMoving ? 1f : 0f);
                hasParam = true;
            }
        }

        // Nếu Animator chưa tạo Parameter trung gian, tự động Play trực tiếp State tương ứng
        if (!hasParam)
        {
            string targetState = isMoving ? walkStateName : idleStateName;
            if (!string.IsNullOrEmpty(targetState))
            {
                animator.Play(targetState);
            }
        }
    }

    private void LockPlayer(GameObject playerObj, bool isLocked)
    {
        if (playerObj == null) return;

        PlayerMapController playerCtrl = playerObj.GetComponent<PlayerMapController>();
        if (playerCtrl != null)
        {
            playerCtrl.SetCanMove(!isLocked);
        }

        var rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void StartBattle(Vector3 playerPos)
    {
        if (battleTransferData == null)
        {
            battleTransferData = Resources.Load<BattleTransferData>("BattleTransferData");
            if (battleTransferData == null)
            {
                Debug.LogError("Chưa gán BattleTransferData cho Trainer!");
                return;
            }
        }

        battleTransferData.ResetData();
        battleTransferData.originScene = BattleTransferData.OriginScene.Map;
        battleTransferData.lastPlayerPosition = playerPos;
        battleTransferData.returnToLastPosition = true;
        
        // Cờ đặc biệt: Trận đấu Trainer & Gym Leader
        battleTransferData.isTrainerBattle = true;
        battleTransferData.isGymLeaderBattle = isGymLeader;
        battleTransferData.rewardBadgeId = gymBadgeId;
        battleTransferData.customRewardGold = rewardGold;
        
        List<RuntimeBeastData> runtimeTeam = new List<RuntimeBeastData>();
        if (trainerTeam != null)
        {
            foreach (var beast in trainerTeam)
            {
                if (beast != null) runtimeTeam.Add(new RuntimeBeastData(beast, 1));
            }
        }
        battleTransferData.SetEnemyTeam(runtimeTeam);
        
        // Lưu ID để khi quay về biết Trainer nào bị đánh bại
        battleTransferData.lastEncounteredBeastId = uniqueTrainerId; 

        GameSceneManager.GoToBattle();
    }
}
