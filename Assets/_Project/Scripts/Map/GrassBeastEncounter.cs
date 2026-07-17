using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Gan vao vung co (Tall Grass). Tu dong tung xuc xac khi Player di chuyen ben trong.
/// Kich hoat tran chien ngau nhien giong Pokemon.
/// </summary>
public class GrassBeastEncounter : MonoBehaviour
{
    [Header("Du lieu & Cau hinh")]
    [SerializeField] private BattleTransferData battleTransferData;
    
    [Tooltip("Danh sach quai co the xuat hien trong vung nay")]
    [SerializeField] private List<BeastData> wildPool = new List<BeastData>();
    
    [Tooltip("Ty le cham tran moi buoc nhay (Vi du: 0.1 = 10%)")]
    [Range(0f, 1f)]
    [SerializeField] private float encounterRate = 0.15f;
    
    [Tooltip("Thoi gian cho giua cac lan tung xuc xac (giay)")]
    [SerializeField] private float stepCooldown = 0.5f;

    [Header("Level quai ngau nhien")]
    [SerializeField] private int minLevel = 1;
    [SerializeField] private int maxLevel = 3;

    private bool isPlayerInside = false;
    private float lastCheckTime = 0f;
    private Rigidbody2D playerRb;
    private PlayerMapController playerController;
    private MonoBehaviour cainosController;
    
    // Ngăn chặn việc kích hoạt nhiều trận cùng lúc
    private static bool isTransitioning = false;

    private void Start()
    {
        isTransitioning = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTransitioning)
        {
            isPlayerInside = true;
            playerRb = collision.GetComponent<Rigidbody2D>();
            playerController = collision.GetComponent<PlayerMapController>();
            
            if (playerController == null)
            {
                cainosController = collision.GetComponent("TopDownCharacterController") as MonoBehaviour;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerRb = null;
        }
    }

    private void Update()
    {
        if (!isPlayerInside || isTransitioning || playerRb == null) return;

        // Kiem tra xem player co dang di chuyen khong (velocity > 0.1)
        if (playerRb.linearVelocity.sqrMagnitude > 0.01f)
        {
            if (Time.time - lastCheckTime > stepCooldown)
            {
                lastCheckTime = Time.time;
                CheckEncounter();
            }
        }
    }

    private void CheckEncounter()
    {
        if (wildPool == null || wildPool.Count == 0) return;

        float roll = Random.value; // 0.0 -> 1.0
        if (roll < encounterRate)
        {
            TriggerEncounter();
        }
    }

    private void TriggerEncounter()
    {
        isTransitioning = true;
        
        // Dung Player lai
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
        if (playerController != null) playerController.SetCanMove(false);
        else if (cainosController != null) cainosController.enabled = false;
        
        var anim = playerRb != null ? playerRb.GetComponentInChildren<Animator>() : null;
        if (anim != null) anim.SetFloat("speed", 0f);

        // Dong tui neu dang mo
        var inventory = playerRb != null ? playerRb.GetComponent<Kinnly.PlayerInventory>() : null;
        if (inventory != null) inventory.ForceCloseInventory();

        StartCoroutine(EncounterRoutine());
    }

    private IEnumerator EncounterRoutine()
    {
        Debug.Log("WILD BEAST ENCOUNTERED IN TALL GRASS!");

        // Hieu ung chop nhay man hinh (Giong Pokemon)
        // O day ta lam rung camera va doi mau nhe
        Camera.main.transform.DOShakePosition(1.0f, 0.2f, 20);
        
        yield return new WaitForSeconds(1.0f);

        // Chon random quai tu pool
        BeastData chosenBeast = wildPool[Random.Range(0, wildPool.Count)];
        int level = Random.Range(minLevel, maxLevel + 1);

        if (battleTransferData != null)
        {
            List<RuntimeBeastData> runtimeTeam = new List<RuntimeBeastData>();
            runtimeTeam.Add(new RuntimeBeastData(chosenBeast, level));

            battleTransferData.SetEnemyTeam(runtimeTeam);
            battleTransferData.originScene = BattleTransferData.OriginScene.Map;
            battleTransferData.isTrainerBattle = false;
            
            // Generate a random unique ID for this instance so if we catch it, it doesn't break
            battleTransferData.lastEncounteredBeastId = "Grass_" + System.Guid.NewGuid().ToString();
            
            // Luu vi tri
            if (playerRb != null)
            {
                battleTransferData.lastPlayerPosition = playerRb.transform.position;
                battleTransferData.returnToLastPosition = true;
            }

            // Chuyen scene
            GameSceneManager.GoToBattle();
        }
        else
        {
            Debug.LogError("[GrassBeast] Thieu battleTransferData!");
            isTransitioning = false;
            
            if (playerController != null) playerController.SetCanMove(true);
            else if (cainosController != null) cainosController.enabled = true;
        }
    }
}
