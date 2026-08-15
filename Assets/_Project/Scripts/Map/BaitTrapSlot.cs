using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gắn script này vào Khay Đặt Mồi trong Safari Zone (Giai đoạn 5).
/// Cách chơi:
/// 1. Player lại gần bấm F -> Đặt mồi (Item Mồi nhử trong túi).
/// 2. Sau thời gian chờ -> Dấu chấm than (!) xuất hiện trên khay.
/// 3. Player bấm F -> Vào trận đấu với Beast hiếm và có thể ném bóng bắt!
/// </summary>
public class BaitTrapSlot : MonoBehaviour, Kinnly.IInteractable
{
    [Header("Dữ liệu & Cấu hình")]
    [SerializeField] private BattleTransferData battleTransferData;
    [SerializeField] private PlayerData playerData;

    [Header("Vật Phẩm Mồi Yêu Cầu")]
    [Tooltip("Tên hoặc Item mồi nhử cần có trong túi (Ví dụ: BaitItem, MonsterBait)")]
    public string requiredBaitItemName = "Mồi Nhử";
    public Kinnly.Item baitItemAsset;

    [Header("Thời Gian Chờ Thú Mắc Bẫy")]
    [Tooltip("Thời gian chờ (giây)")]
    public float waitTimeToCatch = 5f;

    [Header("Danh Sách Quái Hiếm Có Thể Dính Bẫy")]
    public List<BeastData> rareBeastPool = new List<BeastData>();
    [Tooltip("Level của Beast khi gặp trong bẫy")]
    public int beastLevel = 10;

    [Header("Giao Diện & Dấu Chấm Than")]
    [Tooltip("GameObject dấu ! hiển thị trên đầu khay khi quái đã cắn câu")]
    public GameObject exclamationMark;

    [Tooltip("Sprite khay khi có thức ăn (tùy chọn)")]
    public SpriteRenderer trapRenderer;
    public Sprite emptyTrapSprite;
    public Sprite baitedTrapSprite;

    private enum TrapState { Empty, Waiting, ReadyToFight }
    private TrapState currentState = TrapState.Empty;
    private BeastData trappedBeast;

    private void Start()
    {
        if (exclamationMark != null) exclamationMark.SetActive(false);
        UpdateVisuals();
    }

    public void Interact(Kinnly.PlayerInventory playerInventory)
    {
        switch (currentState)
        {
            case TrapState.Empty:
                TryPlaceBait(playerInventory);
                break;

            case TrapState.Waiting:
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue("Khay Bẫy", "Mồi đã được đặt! Hãy kiên nhẫn chờ quái vật xuất hiện...");
                }
                break;

            case TrapState.ReadyToFight:
                TriggerTrapBattle(playerInventory?.gameObject);
                break;
        }
    }

    private void TryPlaceBait(Kinnly.PlayerInventory playerInventory)
    {
        bool hasBait = false;

        if (playerInventory != null)
        {
            // Kiểm tra theo Item Asset
            if (baitItemAsset != null && playerInventory.HasItem(baitItemAsset, 1))
            {
                playerInventory.TryRemoveItem(baitItemAsset, 1);
                hasBait = true;
            }
            // Hoặc cho phép đặt trực tiếp nếu không yêu cầu chặt chẽ
            else if (baitItemAsset == null)
            {
                hasBait = true;
            }
        }

        if (hasBait)
        {
            currentState = TrapState.Waiting;
            UpdateVisuals();

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue("Khay Bẫy", "Đã đặt thức ăn vào khay! Hãy lùi lại và chờ một chút...");
            }

            StartCoroutine(WaitAndAttractRoutine());
        }
        else
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue("Khay Bẫy", $"Bạn cần có [{requiredBaitItemName}] trong túi để đặt vào khay!");
            }
        }
    }

    private IEnumerator WaitAndAttractRoutine()
    {
        yield return new WaitForSeconds(waitTimeToCatch);

        // Chọn ngẫu nhiên 1 Beast từ danh sách
        if (rareBeastPool != null && rareBeastPool.Count > 0)
        {
            int idx = Random.Range(0, rareBeastPool.Count);
            trappedBeast = rareBeastPool[idx];
        }

        currentState = TrapState.ReadyToFight;
        if (exclamationMark != null) exclamationMark.SetActive(true);
    }

    private void TriggerTrapBattle(GameObject playerObj)
    {
        if (trappedBeast == null) return;
        if (battleTransferData == null)
        {
            Debug.LogError("[BaitTrapSlot] Chưa gán BattleTransferData!");
            return;
        }

        if (exclamationMark != null) exclamationMark.SetActive(false);

        battleTransferData.ResetData();
        battleTransferData.originScene = BattleTransferData.OriginScene.Map;
        battleTransferData.isTrainerBattle = false; // Quái hoang dã -> cho phép ném bóng bắt
        battleTransferData.isGymLeaderBattle = false;
        battleTransferData.lastEncounteredBeastId = "Safari_Bait_" + trappedBeast.beastName;
        battleTransferData.returnToLastPosition = true;

        if (playerObj != null)
            battleTransferData.lastPlayerPosition = playerObj.transform.position;

        List<RuntimeBeastData> enemyTeam = new List<RuntimeBeastData>
        {
            new RuntimeBeastData(trappedBeast, beastLevel)
        };
        battleTransferData.SetEnemyTeam(enemyTeam);

        // Reset lại bẫy về rỗng sau khi đánh
        currentState = TrapState.Empty;
        UpdateVisuals();

        GameSceneManager.GoToBattle();
    }

    private void UpdateVisuals()
    {
        if (trapRenderer != null)
        {
            if (currentState == TrapState.Empty && emptyTrapSprite != null)
                trapRenderer.sprite = emptyTrapSprite;
            else if (currentState != TrapState.Empty && baitedTrapSprite != null)
                trapRenderer.sprite = baitedTrapSprite;
        }
    }
}
