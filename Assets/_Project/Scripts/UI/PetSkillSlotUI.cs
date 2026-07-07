using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetSkillSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image skillIcon;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI costText;
    public Button upgradeButton;

    private RuntimeMoveData currentMove;
    private PlayerData currentPlayerData;
    private PetInfoUIManager manager;

    public void Setup(RuntimeMoveData move, PlayerData player, PetInfoUIManager uIManager)
    {
        currentMove = move;
        currentPlayerData = player;
        manager = uIManager;

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (currentMove == null) return;

        if (skillIcon != null && currentMove.baseMove.icon != null)
        {
            skillIcon.sprite = currentMove.baseMove.icon;
            skillIcon.gameObject.SetActive(true);
        }
        else if (skillIcon != null)
        {
            skillIcon.gameObject.SetActive(false);
        }

        if (skillNameText != null) skillNameText.text = currentMove.baseMove.moveName;
        if (levelText != null) levelText.text = $"Lv.{currentMove.currentLevel}";
        if (powerText != null) powerText.text = $"Sức mạnh: {currentMove.power}";
        
        if (costText != null)
        {
            if (currentMove.currentLevel >= currentMove.baseMove.maxLevel)
            {
                costText.text = "MAX";
                upgradeButton.interactable = false;
            }
            else
            {
                int cost = currentMove.GetUpgradeCost();
                costText.text = cost.ToString();
                
                // Vô hiệu hóa nút nếu không đủ tiền
                upgradeButton.interactable = (currentPlayerData.gold >= cost);
            }
        }
    }

    private void OnUpgradeClicked()
    {
        if (currentMove != null && currentPlayerData != null)
        {
            bool success = currentMove.TryUpgrade(currentPlayerData);
            if (success)
            {
                RefreshUI();
                // Cập nhật lại UI tiền ở manager nếu cần
                if (manager != null) manager.RefreshGoldUI();
            }
        }
    }
}
