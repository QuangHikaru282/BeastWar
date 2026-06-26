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
    [Header("Text hướng dẫn")]
    [SerializeField] private TextMeshProUGUI guideText;

    [Header("UI Kĩ năng (Skill Panel)")]
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Button[] skillButtons = new Button[3];
    [SerializeField] private TextMeshProUGUI[] skillTextsTMP = new TextMeshProUGUI[3];
    [SerializeField] private Text[] skillTextsLegacy = new Text[3];

    // Callback trả về lựa chọn cho BattleManager
    private Action<BeastUnit, BeastUnit, MoveData, bool> onActionConfirmed;

    private List<BeastUnit> playerTeam;
    private List<BeastUnit> enemyTeam;
    private BeastUnit selectedAttacker;
    private MoveData selectedMove;

    public void Initialize(List<BeastUnit> pTeam, List<BeastUnit> eTeam,
                           Action<BeastUnit, BeastUnit, MoveData, bool> callback)
    {
        playerTeam = pTeam;
        enemyTeam  = eTeam;
        onActionConfirmed = callback;

        // Tự động tìm UI tĩnh của BattleSceneF nếu chưa được gán trong Inspector
        if (guideText == null)
        {
            GameObject txtObj = GameObject.Find("DialogueText");
            if (txtObj != null) guideText = txtObj.GetComponent<TextMeshProUGUI>();
        }

        if (skillPanel == null)
        {
            GameObject panelObj = GameObject.Find("CombatButtons");
            if (panelObj != null) skillPanel = panelObj;
        }

        if (skillPanel != null && (skillButtons == null || skillButtons.Length == 0 || skillButtons[0] == null))
        {
            skillButtons = new Button[3];
            skillTextsTMP = new TextMeshProUGUI[3];
            skillTextsLegacy = new Text[3];
            
            // Tìm các nút bên trong CombatButtons
            Transform[] children = skillPanel.GetComponentsInChildren<Transform>(true);
            int btnIndex = 0;
            foreach (Transform t in children)
            {
                if (t.name.Contains("AttackButton") && btnIndex < 3)
                {
                    skillButtons[btnIndex] = t.GetComponent<Button>();
                    Transform txt = t.Find("Text");
                    if (txt != null)
                    {
                        skillTextsTMP[btnIndex] = txt.GetComponent<TextMeshProUGUI>();
                        skillTextsLegacy[btnIndex] = txt.GetComponent<Text>();
                    }
                    btnIndex++;
                }
            }
            Debug.Log($"[ActionPanel] Đã tự động link {btnIndex} nút kĩ năng từ CombatButtons.");
        }

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

    public void Show(BeastUnit defaultAttacker)
    {
        selectedAttacker = defaultAttacker;
        selectedMove = null;
        
        if (selectedAttacker != null)
        {
            ShowSkillPanelForBeast(selectedAttacker);
            SetGuide($"Lượt của {selectedAttacker.Data.beastName}! Hãy chọn kĩ năng.");
        }
    }

    public void Hide()
    {
        SetGuide("");
        if (skillPanel != null) skillPanel.SetActive(false);
    }

    /// <summary>
    /// Hàm này giờ không cần thiết nữa do tự động đánh, nhưng giữ lại phòng hờ
    /// </summary>
    public void OnBeastClicked(BeastUnit beast)
    {
        // Bỏ qua click chuột
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
                if (skillTextsTMP[i] != null) skillTextsTMP[i].text = beast.Data.moves[i].moveName;
                if (skillTextsLegacy[i] != null) skillTextsLegacy[i].text = beast.Data.moves[i].moveName;
            }
            else
            {
                // Tắt các nút dư thừa hoặc làm mờ đi nếu quái chưa học đủ 3 chiêu
                skillButtons[i].interactable = false;
                if (skillTextsTMP[i] != null) skillTextsTMP[i].text = "- Trống -";
                if (skillTextsLegacy[i] != null) skillTextsLegacy[i].text = "- Trống -";
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
                // Tự động tìm mục tiêu là quái địch đầu tiên còn sống
                BeastUnit target = enemyTeam.Find(e => e != null && e.IsAlive);
                if (target != null)
                {
                    onActionConfirmed?.Invoke(selectedAttacker, target, selectedMove, false);
                    Hide();
                }
            }
        }
    }

    private void SetGuide(string msg)
    {
        if (guideText != null) guideText.text = msg;
    }
}
