using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Gắn lên quái vật hoang dã xuất hiện trong HuntingScene.
/// Báo cáo lại cho HuntingManager khi bị bắt hoặc đánh bại.
/// </summary>
public class HuntingBeastEncounter : MonoBehaviour
{
    [Header("Dữ liệu")]
    [SerializeField] private BattleTransferData battleTransferData;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject beastBallPrefab;

    [Header("ID được gán động")]
    public string uniqueId;

    [Header("Đội Beast của quái (thường là 1 con)")]
    public List<BeastData> enemyTeam = new List<BeastData>();

    [Header("UI Panel xác nhận")]
    [SerializeField] private GameObject encounterPanel;
    [SerializeField] private TextMeshProUGUI encounterText;
    [SerializeField] private Button btnFight;
    [SerializeField] private Button btnRun;

    private MonoBehaviour playerController;
    private bool hasTriggered = false;
    private bool isStunned = false;

    private void Start()
    {
        // Kiểm tra xem quái có đang bị choáng (đã bị đánh bại sau trận đấu) không
        isStunned = battleTransferData != null && battleTransferData.stunnedBeastIds.Contains(uniqueId);
        
        if (isStunned)
        {
            var patrol = GetComponent<WildBeastPatrol>();
            if (patrol != null) patrol.SetMoving(false);

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
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        LockPlayer(other);

        var patrol = GetComponent<WildBeastPatrol>();
        if (patrol != null) patrol.SetMoving(false);

        if (encounterPanel != null)
        {
            btnFight.onClick.RemoveAllListeners();
            btnRun.onClick.RemoveAllListeners();

            if (isStunned)
            {
                string beastName = GetBeastName();
                if (encounterText != null)
                    encounterText.text = $"{beastName} đang bị CHOÁNG!\nBạn có muốn ném BeastBall để THU PHỤC không?";

                var fightText = btnFight.GetComponentInChildren<TextMeshProUGUI>();
                if (fightText != null) fightText.text = "Thu phục";

                btnFight.onClick.AddListener(StartMapCatch);
            }
            else
            {
                string beastName = GetBeastName();
                if (encounterText != null)
                    encounterText.text = $"Một {beastName} xuất hiện!\nChiến đấu không?";

                var fightText = btnFight.GetComponentInChildren<TextMeshProUGUI>();
                if (fightText != null) fightText.text = "Chiến đấu";

                btnFight.onClick.AddListener(OnFightPressed);
            }

            btnRun.onClick.AddListener(OnRunPressed);
            encounterPanel.SetActive(true);
        }
        else
        {
            if (isStunned) StartMapCatch();
            else StartBattle();
        }
    }

    private string GetBeastName()
    {
        if (enemyTeam != null && enemyTeam.Count > 0 && enemyTeam[0] != null)
            return enemyTeam[0].beastName;
        return "Quái hoang dã";
    }

    private void LockPlayer(Collider2D playerCollider)
    {
        playerController = playerCollider.GetComponent("PlayerMapController") as MonoBehaviour;
        if (playerController != null)
        {
            playerController.SendMessage("SetCanMove", false, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            playerController = playerCollider.GetComponent("TopDownCharacterController") as MonoBehaviour;
            if (playerController != null)
            {
                playerController.enabled = false;
                var rb = playerCollider.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
                var anim = playerCollider.GetComponent<Animator>();
                if (anim != null) anim.SetBool("IsMoving", false);
            }
        }
    }

    private void UnlockPlayer()
    {
        if (playerController != null)
        {
            if (playerController.GetType().Name == "PlayerMapController")
                playerController.SendMessage("SetCanMove", true, SendMessageOptions.DontRequireReceiver);
            else
                playerController.enabled = true;
        }
    }

    private void StartMapCatch()
    {
        StartCoroutine(MapCatchRoutine());
    }

    private IEnumerator MapCatchRoutine()
    {
        if (btnFight != null) btnFight.interactable = false;
        if (btnRun != null) btnRun.interactable = false;

        string beastName = GetBeastName();

        if (encounterText != null) encounterText.text = $"Ném BeastBall vào {beastName}...";
        
        GameObject ball = null;

        // 1. Sinh ra quả bóng tại vị trí người chơi
        if (beastBallPrefab != null && playerController != null)
        {
            ball = Instantiate(beastBallPrefab, playerController.transform.position, Quaternion.identity);
            
            // 2. Bóng nảy hình vòng cung tới vị trí của quái thú trong 0.6s
            yield return ball.transform.DOJump(transform.position, 1.5f, 1, 0.6f).WaitForCompletion();

            // 3. Ẩn quái thú đi (Cảm giác đã bị hút vào bóng)
            var spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.enabled = false;
        }
        else
        {
            yield return new WaitForSeconds(0.6f);
        }

        // 4. Quả bóng lắc lư 3 lần
        for (int i = 1; i <= 3; i++)
        {
            if (encounterText != null) encounterText.text = $"Lắc bóng{new string('.', i)} ⚪";
            
            if (ball != null)
            {
                // Lắc sang trái phải bằng DOPunchRotation
                yield return ball.transform.DOPunchRotation(new Vector3(0, 0, 35f), 0.5f, 5, 0.5f).WaitForCompletion();
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                yield return new WaitForSeconds(0.6f);
            }
        }

        if (encounterText != null) 
            encounterText.text = $"🎉 Thu phục THÀNH CÔNG!\n{beastName} đã gia nhập đội hình của bạn!";

        if (ball != null)
        {
            // Hiệu ứng bóng xẹp đi biến mất
            ball.transform.DOScale(Vector3.zero, 0.3f);
            Destroy(ball, 0.35f);
        }

        if (enemyTeam != null && enemyTeam.Count > 0 && enemyTeam[0] != null)
        {
            playerData.AddBeast(enemyTeam[0]);
            Debug.Log("đã thu phục");
        }

        if (battleTransferData != null)
        {
            battleTransferData.stunnedBeastIds.Remove(uniqueId);
        }

        yield return new WaitForSeconds(2.0f);

        if (encounterPanel != null) encounterPanel.SetActive(false);
        UnlockPlayer();

        // Báo cho HuntingManager biết con thú này đã bị gỡ bỏ
        if (HuntingManager.Instance != null)
        {
            HuntingManager.Instance.OnBeastRemoved(uniqueId);
        }

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

        if (!isStunned)
        {
            var patrol = GetComponent<WildBeastPatrol>();
            if (patrol != null) patrol.SetMoving(true);
        }
        
        hasTriggered = false;
    }

    private void StartBattle()
    {
        battleTransferData.lastEncounteredBeastId = uniqueId;
        battleTransferData.SetEnemyTeam(enemyTeam);
        // Đánh dấu trận này đến từ HuntingScene để BattleManager quay về đúng scene
        battleTransferData.originScene = BattleTransferData.OriginScene.Hunting;
        battleTransferData.isSingleBattle = true; // Chỉ dùng 1 Beast của player (1v1)
        GameSceneManager.GoToBattle();
    }
}
