using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Xử lý lượt Player trong BattleScene.
/// Flow đơn giản: Click thú phe ta → Click thú phe địch → Tấn công.
/// Không cần nút UI — chọn trực tiếp trên sân đấu.
/// </summary>
public class ActionPanel : MonoBehaviour
{
    private enum ActionState { SelectAttacker, SelectTarget }

    [Header("Text hướng dẫn (tùy chọn)")]
    [SerializeField] private TextMeshProUGUI guideText;

    // Callback trả về lựa chọn cho BattleManager
    private Action<BeastUnit, BeastUnit, MoveData, bool> onActionConfirmed;

    private List<BeastUnit> playerTeam;
    private List<BeastUnit> enemyTeam;
    private BeastUnit selectedAttacker;
    private ActionState currentState;

    public void Initialize(List<BeastUnit> pTeam, List<BeastUnit> eTeam,
                           Action<BeastUnit, BeastUnit, MoveData, bool> callback)
    {
        playerTeam = pTeam;
        enemyTeam  = eTeam;
        onActionConfirmed = callback;
    }

    public void Show()
    {
        currentState = ActionState.SelectAttacker;
        selectedAttacker = null;
        SetGuide("Chọn thú phe ta để tấn công!");
    }

    public void Hide()
    {
        SetGuide("");
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
            currentState = ActionState.SelectTarget;
            SetGuide($"Đã chọn {beast.Data.beastName}! Giờ chọn thú địch để tấn công!");
        }
        else if (currentState == ActionState.SelectTarget)
        {
            // Chỉ chấp nhận thú phe địch
            if (beast.IsPlayerTeam) return;

            // Xác nhận: tấn công thường (move = null), không phải catch
            onActionConfirmed?.Invoke(selectedAttacker, beast, null, false);
            Hide();
        }
    }

    private void SetGuide(string msg)
    {
        if (guideText != null) guideText.text = msg;
    }
}
