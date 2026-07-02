using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý UI của Arena (Danh sách 10 ải).
/// Gắn vào một GameObject trong MapScene.
/// </summary>
public class ArenaUIManager : MonoBehaviour
{
    [Header("Dữ liệu")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private ArenaData arenaData;
    [SerializeField] private BattleTransferData battleTransferData;

    [Header("UI Mở Arena (Nút HUD)")]
    [SerializeField] private Button openArenaButton;
    [SerializeField] private GameObject lockOverlay; // Icon/Image báo hiệu nút bị khóa

    [Header("UI Panel Arena (Overlay)")]
    [SerializeField] private GameObject arenaPanel;
    [SerializeField] private Button closePanelButton;
    [SerializeField] private Transform stageListContainer;
    [SerializeField] private GameObject stageNodePrefab;
    [SerializeField] private TextMeshProUGUI progressText;

    private void Start()
    {
        if (openArenaButton == null) Debug.LogWarning("[Arena] CHÚ Ý: Chưa kéo nút mở Arena vào ô 'Open Arena Button'!");
        if (playerData == null) Debug.LogWarning("[Arena] CHÚ Ý: Chưa kéo file PlayerData vào ô 'Player Data'!");
        if (arenaPanel == null) Debug.LogWarning("[Arena] CHÚ Ý: Chưa kéo ArenaPanel vào ô 'Arena Panel'!");

        // Ẩn panel khi mới vào
        if (arenaPanel != null) arenaPanel.SetActive(false);

        // Gán sự kiện nút
        if (openArenaButton != null) openArenaButton.onClick.AddListener(OpenArenaPanel);
        if (closePanelButton != null) closePanelButton.onClick.AddListener(CloseArenaPanel);

        CheckUnlockNewMap();
    }

    private void Update()
    {
        UpdateArenaButtonState();
    }

    private void UpdateArenaButtonState()
    {
        if (openArenaButton == null || playerData == null) return;

        // Chỉ mở khóa khi hoàn thành Tutorial (đã thu phục pet, tutorialQuestStage >= 2)
        bool isUnlocked = playerData.tutorialQuestStage >= 2;
        
        openArenaButton.interactable = isUnlocked;
        if (lockOverlay != null) lockOverlay.SetActive(!isUnlocked);
    }

    public void OpenArenaPanel()
    {
        if (arenaPanel == null) return;
        arenaPanel.SetActive(true);

        // Đẩy Panel này lên trên cùng để không bị các UI khác (như UI Nhiệm vụ) che mất
        Canvas canvas = arenaPanel.GetComponent<Canvas>();
        if (canvas == null) canvas = arenaPanel.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100; // Đặt một số rất lớn để luôn nổi lên trên

        // Bắt buộc phải có GraphicRaycaster để click chuột xuyên qua Canvas mới tạo
        if (arenaPanel.GetComponent<GraphicRaycaster>() == null)
        {
            arenaPanel.AddComponent<GraphicRaycaster>();
        }

        RefreshStageList();
    }

    public void CloseArenaPanel()
    {
        if (arenaPanel == null) return;
        arenaPanel.SetActive(false);
    }

    private void RefreshStageList()
    {
        if (arenaData == null || playerData == null || stageListContainer == null) return;

        // Xóa list cũ (nếu có)
        foreach (Transform child in stageListContainer)
        {
            Destroy(child.gameObject);
        }

        int completedStages = 0;

        // Tạo lại list ải
        for (int i = 0; i < arenaData.stages.Count; i++)
        {
            StageData data = arenaData.stages[i];
            int stageId = data.stageId; // Nên là 1 đến 10

            // Kiểm tra trạng thái mở khóa và sao
            bool isUnlocked = stageId <= playerData.arenaHighestUnlockedStage;
            int stars = playerData.GetArenaStageStars(stageId);

            if (stars > 0) completedStages++;

            // Tạo prefab
            GameObject nodeGO = Instantiate(stageNodePrefab, stageListContainer);
            ArenaStageNodeUI nodeUI = nodeGO.GetComponent<ArenaStageNodeUI>();

            if (nodeUI != null)
            {
                nodeUI.Initialize(data, stars, isUnlocked, this);
            }
        }

        // Cập nhật text tiến trình mở khóa map mới
        if (progressText != null)
        {
            if (completedStages >= arenaData.stagesToUnlockNewMap)
            {
                progressText.text = $"Tiến trình: {completedStages}/{arenaData.stagesToUnlockNewMap} (Đã mở khóa Thành Thị!)";
            }
            else
            {
                progressText.text = $"Hoàn thành {completedStages}/{arenaData.stagesToUnlockNewMap} ải để mở khóa Thành Thị.";
            }
        }
    }

    /// <summary>
    /// Được gọi bởi ArenaStageNodeUI khi người chơi bấm chọn 1 ải.
    /// </summary>
    public void OnStageSelected(StageData stage)
    {
        if (battleTransferData == null || stage == null) return;

        Debug.Log($"[Arena] Đang chuẩn bị vào ải Arena: {stage.stageName}");

        // Cài đặt thông số cho Battle
        battleTransferData.ResetData();
        battleTransferData.originScene = BattleTransferData.OriginScene.Arena;
        battleTransferData.currentArenaStageId = stage.stageId;
        battleTransferData.SetEnemyTeam(stage.enemyTeam);
        battleTransferData.isSingleBattle = false; // Đấu đội hình

        // Chuyển sang scene battle
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.GoToBattle();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BattleSceneF");
        }
    }

    private void CheckUnlockNewMap()
    {
        if (playerData == null || arenaData == null) return;
        
        // Đếm số ải đã qua
        int completedStages = 0;
        for (int i = 0; i < arenaData.stages.Count; i++)
        {
            if (playerData.GetArenaStageStars(arenaData.stages[i].stageId) > 0)
                completedStages++;
        }

        if (completedStages >= arenaData.stagesToUnlockNewMap)
        {
            if (!playerData.IsMapUnlocked("City"))
            {
                playerData.UnlockMap("City");
                Debug.Log("🎉 [Arena] Chúc mừng! Bạn đã đủ điều kiện mở khóa map Thành Thị.");
                // TODO: Có thể gọi một UI Popup thông báo ở đây nếu có hệ thống UI Popup.
            }
        }
    }
}
