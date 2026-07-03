using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Gắn lên GameObject Wild Beast trên Map.
/// Khi Player va chạm → hiện panel xác nhận → chuyển sang Formation Scene.
/// </summary>
public class WildBeastEncounter : MonoBehaviour
{
    [Header("Dữ liệu")]
    [SerializeField] private BattleTransferData battleTransferData;
    [SerializeField] private PlayerData playerData;

    [Header("ID Duy Nhất (Cực kỳ quan trọng để lưu trạng thái!)")]
    public string uniqueId;

    [Header("Đội Beast của quái (1–3 con)")]
    [SerializeField] private List<BeastData> enemyTeam = new List<BeastData>();

    [Header("UI Panel xác nhận")]
    [SerializeField] private GameObject encounterPanel;   // Panel hỏi "Chiến đấu không?" hoặc "Thu phục?"
    [SerializeField] private TextMeshProUGUI encounterText;
    [SerializeField] private Button btnFight;
    [SerializeField] private Button btnRun;

    private PlayerMapController playerController;
    private MonoBehaviour cainosController;
    private Kinnly.PlayerInventory playerInventory;   // ← thêm
    private bool hasTriggered = false;
    private bool isStunned = false;

    private void Awake()
    {
        // Tự động tạo ID nếu người dùng quên nhập trong Inspector
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = gameObject.name + "_" + transform.position.x + "_" + transform.position.y;
            Debug.Log($"[WildBeast] Tự động tạo Unique ID cho quái: {uniqueId}");
        }
    }

    private void Start()
    {
        // 1. Nếu đã bị bắt trước đó -> Xóa khỏi bản đồ
        if (battleTransferData != null && battleTransferData.caughtBeastIds.Contains(uniqueId))
        {
            Destroy(gameObject);
            return;
        }

        // 2. Kiểm tra xem quái có đang bị choáng (đã bị đánh bại) không
        isStunned = battleTransferData != null && battleTransferData.stunnedBeastIds.Contains(uniqueId);
        if (isStunned)
        {
            // Dừng tuần tra
            var patrol = GetComponent<WildBeastPatrol>();
            if (patrol != null) patrol.SetMoving(false);

            // Tạo hiệu ứng bị choáng: Xoay ngang quái vật (90 độ) nằm gục xuống đất và đổi màu hơi xám
            var spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, 90f);
                spriteRenderer.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            }
        }

        if (encounterPanel != null) encounterPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleEncounter(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleEncounter(collision.gameObject);
    }

    private void HandleEncounter(GameObject playerObj)
    {
        if (hasTriggered) return;
        if (!playerObj.CompareTag("Player")) return;

        hasTriggered = true;

        // Khóa di chuyển của Player
        playerController = playerObj.GetComponent<PlayerMapController>();
        playerInventory   = playerObj.GetComponent<Kinnly.PlayerInventory>();  // ← thêm

        // Đóng inventory nếu đang mở
        if (playerInventory != null) playerInventory.ForceCloseInventory();

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }
        else
        {
            cainosController = playerObj.GetComponent("TopDownCharacterController") as MonoBehaviour;
            if (cainosController != null)
            {
                cainosController.enabled = false;
                var rb = playerObj.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
                var anim = playerObj.GetComponentInChildren<Animator>();
                if (anim != null) anim.SetFloat("speed", 0f);
            }
        }

        // Dừng tuần tra
        var patrol = GetComponent<WildBeastPatrol>();
        if (patrol != null) patrol.SetMoving(false);

        // Hiển thị Panel
        if (encounterPanel != null)
        {
            btnFight.onClick.RemoveAllListeners();
            btnRun.onClick.RemoveAllListeners();

            if (isStunned)
            {
                // Trạng thái choáng: Hỏi thu phục
                string beastName = (enemyTeam != null && enemyTeam.Count > 0 && enemyTeam[0] != null) 
                    ? enemyTeam[0].beastName 
                    : "Quái vật";

                if (encounterText != null)
                    encounterText.text = $"{beastName} đang bị CHOÁNG!\nBạn có muốn ném BeastBall để THU PHỤC không?";

                // Nút "Fight" biến thành nút "Thu phục"
                var fightText = btnFight.GetComponentInChildren<TextMeshProUGUI>();
                if (fightText != null) fightText.text = "Thu phục";

                btnFight.onClick.AddListener(StartMapCatch);
            }
            else
            {
                // Trạng thái bình thường: Hỏi chiến đấu
                var validEnemies = enemyTeam.FindAll(b => b != null);
                string names = validEnemies.Count > 0 
                    ? string.Join(", ", validEnemies.ConvertAll(b => b.beastName)) 
                    : "Quái hoang dã";

                if (encounterText != null)
                    encounterText.text = $"Một {names} xuất hiện!\nChiến đấu không?";

                var fightText = btnFight.GetComponentInChildren<TextMeshProUGUI>();
                if (fightText != null) fightText.text = "Chiến đấu";

                btnFight.onClick.AddListener(OnFightPressed);
            }

            btnRun.onClick.AddListener(OnRunPressed);
            encounterPanel.SetActive(true);
        }
        else
        {
            if (isStunned)
            {
                StartMapCatch();
            }
            else
            {
                StartBattle();
            }
        }
    }

    private void StartMapCatch()
    {
        StartCoroutine(MapCatchRoutine());
    }

    [Header("Hiệu ứng thu phục")]
    [SerializeField] private GameObject captureBallPrefab; // Kéo prefab quả bóng (ví dụ: xr2a847...) vào ô này

    private System.Collections.IEnumerator MapCatchRoutine()
    {
        // Vô hiệu hóa các nút bấm trong lúc đang thu phục
        if (btnFight != null) btnFight.interactable = false;
        if (btnRun != null) btnRun.interactable = false;

        string beastName = (enemyTeam != null && enemyTeam.Count > 0 && enemyTeam[0] != null) 
            ? enemyTeam[0].beastName 
            : "Quái vật";

        if (encounterText != null) encounterText.text = $"Ném BeastBall vào {beastName}...";

        // Lấy vị trí của Player làm điểm xuất phát (nếu không có thì lấy vị trí hiện tại lùi lại 2 đơn vị)
        Vector3 startPos = transform.position + new Vector3(-2f, 0, 0);
        if (playerController != null) startPos = playerController.transform.position;
        else if (cainosController != null) startPos = cainosController.transform.position;

        GameObject ballObj = null;

        // BƯỚC 1: QUĂNG BÓNG
        if (captureBallPrefab != null)
        {
            ballObj = Instantiate(captureBallPrefab, startPos, Quaternion.identity);
            
            // Bay theo đường cong (Jump) đến vị trí của quái
            ballObj.transform.DOJump(transform.position, jumpPower: 2f, numJumps: 1, duration: 0.8f).SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(0.8f);

        // BƯỚC 2: HÚT QUÁI VẬT VÀO BÓNG
        // Thu nhỏ quái vật lại thành 0
        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.3f);

        // BƯỚC 3: LẮC BÓNG 3 LẦN
        for (int i = 1; i <= 3; i++)
        {
            if (encounterText != null) encounterText.text = $"Lắc bóng{new string('.', i)} ⚪";
            
            if (ballObj != null)
            {
                // Lắc bóng sang trái phải
                ballObj.transform.DOPunchRotation(new Vector3(0, 0, 30f), 0.5f, vibrato: 5);
            }
            yield return new WaitForSeconds(0.6f);
        }

        // Đã bị đánh bại -> Tỷ lệ thu phục là 100% thành công!
        if (encounterText != null) 
            encounterText.text = $"🎉 Thu phục THÀNH CÔNG!\n{beastName} đã gia nhập đội hình của bạn!";

        // BƯỚC 4: QUẢ BÓNG THU NHỎ DẦN RỒI BIẾN MẤT
        if (ballObj != null)
        {
            ballObj.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack);
        }
        yield return new WaitForSeconds(0.5f);

        // Thêm vào danh sách quái sở hữu của người chơi
        if (enemyTeam != null && enemyTeam.Count > 0 && enemyTeam[0] != null)
        {
            playerData.AddBeast(enemyTeam[0]);
            Debug.Log("Đã thu phục thành công!");
        }

        // Lưu trạng thái đã bị bắt
        if (battleTransferData != null)
        {
            battleTransferData.stunnedBeastIds.Remove(uniqueId);
            if (!battleTransferData.caughtBeastIds.Contains(uniqueId))
                battleTransferData.caughtBeastIds.Add(uniqueId);
        }

        yield return new WaitForSeconds(2.0f);

        // Đóng panel và dọn dẹp
        if (encounterPanel != null) encounterPanel.SetActive(false);
        UnlockPlayer();

        if (ballObj != null) Destroy(ballObj);
        
        // Xóa quái khỏi map
        Destroy(gameObject);
    }

    private void OnFightPressed()
    {
        if (encounterPanel != null) encounterPanel.SetActive(false);
        StartBattle();
    }

    private void OnRunPressed()
    {
        if (encounterPanel != null) encounterPanel.SetActive(false);
        UnlockPlayer();

        // Tiếp tục cho quái tuần tra lại nếu không bị choáng
        if (!isStunned)
        {
            var patrol = GetComponent<WildBeastPatrol>();
            if (patrol != null) patrol.SetMoving(true);
        }
        
        hasTriggered = false;
    }

    private void UnlockPlayer()
    {
        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }
        else if (cainosController != null)
        {
            cainosController.enabled = true;
        }
    }

    private void StartBattle()
    {
        // Truyền đội địch và ID quái sang BattleScene
        battleTransferData.lastEncounteredBeastId = uniqueId;
        battleTransferData.SetEnemyTeam(enemyTeam);
        battleTransferData.originScene = BattleTransferData.OriginScene.Map; // Lưu lại nguồn gốc để quay về
        battleTransferData.isTrainerBattle = false; // Đảm bảo đây không phải trận đánh Trainer
        
        // LƯU LẠI VỊ TRÍ NGƯỜI CHƠI TRƯỚC TRẬN ĐẤU
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            battleTransferData.lastPlayerPosition = player.transform.position;
            battleTransferData.returnToLastPosition = true;
        }

        GameSceneManager.GoToBattle();
    }
}
