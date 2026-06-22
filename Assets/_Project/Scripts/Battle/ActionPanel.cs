using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Xử lý lượt Player trong BattleScene.
/// Flow: Click thú phe ta → Bảng kĩ năng hiện lên → Chọn kĩ năng → Click thú phe địch → Tấn công.
/// </summary>
public class ActionPanel : MonoBehaviour
{
    private enum ActionState { SelectAttacker, SelectSkill, SelectTarget }

    [Header("Text hướng dẫn")]
    [SerializeField] private TextMeshProUGUI guideText;

    [Header("UI Kĩ năng (Skill Panel)")]
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Button[] skillButtons = new Button[3];
    [SerializeField] private TextMeshProUGUI[] skillTexts = new TextMeshProUGUI[3];

    // Callback trả về lựa chọn cho BattleManager
    private Action<BeastUnit, BeastUnit, MoveData, bool> onActionConfirmed;

    private List<BeastUnit> playerTeam;
    private List<BeastUnit> enemyTeam;
    private BeastUnit selectedAttacker;
    private MoveData selectedMove;
    private ActionState currentState;

    public void Initialize(List<BeastUnit> pTeam, List<BeastUnit> eTeam,
                           Action<BeastUnit, BeastUnit, MoveData, bool> callback)
    {
        playerTeam = pTeam;
        enemyTeam  = eTeam;
        onActionConfirmed = callback;

        // Cài đặt sự kiện cho các nút bấm
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int index = i; // Bắt buộc phải có dòng này để tránh lỗi closure trong vòng lặp
            if (skillButtons[i] != null)
            {
                skillButtons[i].onClick.RemoveAllListeners();
                skillButtons[i].onClick.AddListener(() => OnSkillButtonClicked(index));
            }
        }
    }

    public void Show()
    {
        currentState = ActionState.SelectAttacker;
        selectedAttacker = null;
        selectedMove = null;
        if (skillPanel != null) skillPanel.SetActive(false);
        SetGuide("Chọn thú phe ta để ra đòn!");
    }

    public void Hide()
    {
        SetGuide("");
        if (skillPanel != null) skillPanel.SetActive(false);
    }

    /// <summary>
    /// Được gọi từ BattleManager.HandleBeastClick khi người chơi click vào thú trên sân.
    /// </summary>
    public void OnBeastClicked(BeastUnit beast)
    {
        if (beast == null || !beast.IsAlive) return;

        if (currentState == ActionState.SelectAttacker)
        {
            // Chỉ chấp nhận thú phe mình
            if (!beast.IsPlayerTeam) return;

            selectedAttacker = beast;
            currentState = ActionState.SelectSkill;
            
            // Hiển thị bảng kĩ năng
            ShowSkillPanelForBeast(beast);
            SetGuide($"Đã chọn {beast.Data.beastName}! Hãy chọn một kĩ năng.");
        }
        else if (currentState == ActionState.SelectTarget)
        {
            // Chỉ chấp nhận thú phe địch
            if (beast.IsPlayerTeam) return;

            // Xác nhận: tấn công với chiêu thức đã chọn
            onActionConfirmed?.Invoke(selectedAttacker, beast, selectedMove, false);
            Hide();
        }
    }

    private void ShowSkillPanelForBeast(BeastUnit beast)
    {
        if (skillPanel == null) return;
        
        skillPanel.SetActive(true);

        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < beast.Data.moves.Length && beast.Data.moves[i] != null)
            {
                skillButtons[i].gameObject.SetActive(true);
                skillButtons[i].interactable = true;
                if (skillTexts[i] != null)
                {
                    skillTexts[i].text = beast.Data.moves[i].moveName;
                }
            }
            else
            {
                // Tắt các nút dư thừa hoặc làm mờ đi nếu quái chưa học đủ 3 chiêu
                skillButtons[i].interactable = false;
                if (skillTexts[i] != null)
                {
                    skillTexts[i].text = "- Trống -";
                }
            }
        }
    }

    private void OnSkillButtonClicked(int skillIndex)
    {
        if (selectedAttacker == null) return;
        
        if (skillIndex >= 0 && skillIndex < selectedAttacker.Data.moves.Length)
        {
            selectedMove = selectedAttacker.Data.moves[skillIndex];
            
            if (selectedMove != null)
            {
                currentState = ActionState.SelectTarget;
                if (skillPanel != null) skillPanel.SetActive(false); // Ẩn bảng kĩ năng đi để dễ chọn mục tiêu
                SetGuide($"Dùng [{selectedMove.moveName}]! Giờ hãy chọn thú địch để tấn công!");
            }
        }
    }

    private void SetGuide(string msg)
    {
        if (guideText != null) guideText.text = msg;
    }
}
