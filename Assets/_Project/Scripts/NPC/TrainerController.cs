using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrainerController : MonoBehaviour
{
    [Header("Đội hình Trainer")]
    public List<BeastData> trainerTeam;
    
    [Header("Dữ liệu hệ thống")]
    public BattleTransferData battleTransferData;
    public string uniqueTrainerId;
    public bool alreadyDefeated = false;

    [Header("Giao diện")]
    public GameObject exclamationMark; // Dấu chấm than hiện lên đầu
    public GameObject dialogPanel; // Khung thoại trước khi đánh
    public TextMeshProUGUI dialogText;
    
    [TextArea]
    public string challengeText = "Ngươi đã lọt vào tầm mắt của ta! Hãy đấu một trận nào!";

    private bool isTriggered = false;
    private MonoBehaviour playerController;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueTrainerId))
        {
            uniqueTrainerId = "Trainer_" + transform.position.ToString();
        }
        
        if (exclamationMark != null) exclamationMark.SetActive(false);
        if (dialogPanel != null) dialogPanel.SetActive(false);
    }

    private void Start()
    {
        // Đọc từ PlayerData xem Trainer này đã bị đánh bại chưa
        if (global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null)
        {
            if (global::QuestManager.Instance.playerData.defeatedTrainers.Contains(uniqueTrainerId))
            {
                alreadyDefeated = true;
            }
        }

        if (alreadyDefeated)
        {
            // Vô hiệu hóa vùng nhìn nếu đã thua
            GetComponent<Collider2D>().enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (alreadyDefeated || isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(TrainerEncounterRoutine(collision.gameObject));
        }
    }

    private IEnumerator TrainerEncounterRoutine(GameObject playerObj)
    {
        // 1. Dừng người chơi
        playerController = playerObj.GetComponent("PlayerMapController") as MonoBehaviour;
        if (playerController == null)
            playerController = playerObj.GetComponent("TopDownCharacterController") as MonoBehaviour;
            
        if (playerController != null)
            playerController.enabled = false;
            
        var rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Hiện dấu chấm than !
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(true);
            yield return new WaitForSeconds(1f);
            exclamationMark.SetActive(false);
        }

        // 3. Hiện câu thoại thách đấu
        if (dialogPanel != null && dialogText != null)
        {
            dialogText.text = challengeText;
            dialogPanel.SetActive(true);
            
            // Chờ người chơi bấm phím bất kỳ hoặc chuột để tiếp tục
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => Input.anyKeyDown);
            dialogPanel.SetActive(false);
        }

        // 4. Chuyển vào Battle
        StartBattle(playerObj.transform.position);
    }

    private void StartBattle(Vector3 playerPos)
    {
        if (battleTransferData == null)
        {
            Debug.LogError("Chưa gán BattleTransferData cho Trainer!");
            return;
        }

        battleTransferData.ResetData();
        battleTransferData.originScene = BattleTransferData.OriginScene.Map;
        battleTransferData.lastPlayerPosition = playerPos;
        battleTransferData.returnToLastPosition = true;
        
        // Cờ đặc biệt: Trận đấu Trainer!
        battleTransferData.isTrainerBattle = true;
        battleTransferData.SetEnemyTeam(trainerTeam);
        
        // Lưu ID để khi quay về biết Trainer nào bị đánh bại
        battleTransferData.lastEncounteredBeastId = uniqueTrainerId; 

        GameSceneManager.GoToBattle();
    }
}
