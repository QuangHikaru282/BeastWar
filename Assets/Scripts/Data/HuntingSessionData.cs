using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HuntingSessionData", menuName = "BeastBall/HuntingSessionData")]
public class HuntingSessionData : ScriptableObject
{
    [Header("Kho Thú Hoang Dã (Sẽ chọn ngẫu nhiên từ đây)")]
    public List<BeastData> wildBeastPool = new List<BeastData>();

    [Header("Cấu hình đi săn")]
    [Tooltip("Tổng số thú xuất hiện trong một phiên đi săn (VD: 10 con)")]
    public int totalBeastsInSession = 10;

    [Tooltip("Số lượng thú tối đa xuất hiện ĐỒNG THỜI trên bản đồ (VD: 5 con)")]
    public int maxActiveBeasts = 5;
}
