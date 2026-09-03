using UnityEngine;
using Kinnly;

/// <summary>
/// NPC Chủ Cửa Hàng Xe Đạp (Bike Shop):
/// 1. Nếu người chơi chưa có phiếu: báo giá 1,000,000 Đồng (không mua nổi).
/// 2. Nếu có Phiếu Giảm Giá (Bike Voucher) từ Bill: bán xe đạp với giá 1 Đồng.
/// 3. Hướng dẫn người chơi bấm phím 'B' trên bàn phím để lên/xuống xe đạp bất kỳ lúc nào.
/// </summary>
public class BikeShopNPC : MonoBehaviour, IInteractable
{
    [Header("Dữ liệu người chơi")]
    [Tooltip("Kéo PlayerData asset vào đây")]
    [SerializeField] private PlayerData playerData;

    [Header("Câu thoại")]
    [TextArea(2, 4)]
    [SerializeField] private string[] normalDialogue = new string[]
    {
        "Chào mừng đến với Cửa Hàng Xe Đạp Siêu Tốc!",
        "Dòng xe đạp của chúng tôi là hàng thủ công cao cấp nhất vùng, có giá 1.000.000 Đồng!",
        "Trừ khi quý khách có Phiếu Giảm Giá Đặc Biệt từ ngài Bill, nếu không thì không ai mua nổi đâu..."
    };

    [TextArea(2, 4)]
    [SerializeField] private string[] voucherExchangeDialogue = new string[]
    {
        "Ồ! Quý khách đang cầm trên tay Phiếu Giảm Giá Đặc Biệt của ngài Bill sao?!",
        "Tuyệt vời quá! Theo thỏa thuận với ngài Bill, chúng tôi xin bán cho quý khách chiếc xe đạp này với giá tượng trưng chỉ 1 ĐỒNG!",
        "Giao dịch thành công! Hãy nhấn phím [B] trên bàn phím để lên/xuống xe đạp bất cứ lúc nào trên đường nhé!"
    };

    [TextArea(2, 4)]
    [SerializeField] private string[] notEnoughMoneyDialogue = new string[]
    {
        "Quý khách có phiếu giảm giá nhưng trong túi không có đủ 1 Đồng để thanh toán rồi..."
    };

    [TextArea(2, 4)]
    [SerializeField] private string[] ownedDialogue = new string[]
    {
        "Chiếc xe đạp lướt đi rất êm và nhanh đúng không?",
        "Hãy nhớ nhấn phím [B] để leo lên xe đạp mỗi khi cần di chuyển nhanh nhé!"
    };

    public void Interact(PlayerInventory playerInventory)
    {
        if (DialogueManager.Instance == null || playerData == null) return;

        if (playerData.hasBicycle)
        {
            DialogueManager.Instance.StartDialogue("Chủ Tiệm Xe Đạp", ownedDialogue, null);
        }
        else if (playerData.hasBikeVoucher)
        {
            if (playerData.gold >= 1)
            {
                DialogueManager.Instance.StartDialogue("Chủ Tiệm Xe Đạp", voucherExchangeDialogue, () =>
                {
                    playerData.gold -= 1;
                    playerData.hasBicycle = true;
                    playerData.hasBikeVoucher = false;
                    playerData.Save();
                    Debug.Log("<color=green>[BikeShopNPC]</color> Người chơi đã mua thành công Xe Đạp với giá 1 Đồng!");
                });
            }
            else
            {
                DialogueManager.Instance.StartDialogue("Chủ Tiệm Xe Đạp", notEnoughMoneyDialogue, null);
            }
        }
        else
        {
            DialogueManager.Instance.StartDialogue("Chủ Tiệm Xe Đạp", normalDialogue, null);
        }
    }
}
