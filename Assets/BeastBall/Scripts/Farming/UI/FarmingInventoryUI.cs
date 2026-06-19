using UnityEngine;
using UnityEngine.UI;

namespace BeastBall.Farming.UI
{
    /// <summary>
    /// Hiển thị Inventory Backend (FarmingInventoryManager) lên màn hình cho người chơi thấy.
    /// Script này sẽ được gắn vào Canvas chứa các ô vuông ở dưới đáy màn hình.
    /// </summary>
    public class FarmingInventoryUI : MonoBehaviour
    {
        [System.Serializable]
        public class UISlot
        {
            public Image Icon;          // Hình ảnh item (Cái cuốc, cái bình...)
            public Text AmountText;     // Chữ số lượng (x5, x10)
            public GameObject Highlight;// Khung viền sáng báo hiệu đang cầm (Equip) ô này
        }

        [Tooltip("Kéo thả 9 cái ô UI của bạn trên Canvas vào đây")]
        public UISlot[] Slots;

        private void Update()
        {
            // Trong thực tế, chỉ nên gọi RefreshUI() khi có Item thêm vào/bớt ra (dùng Event).
            // Đặt ở Update() chỉ để bạn test dễ dàng ngay lúc này: UI sẽ luôn tự động đồng bộ.
            RefreshUI();
        }

        public void RefreshUI()
        {
            var inventory = FarmingInventoryManager.Instance;
            if (inventory == null) return;

            // Lướt qua 9 ô của UI và đồng bộ với 9 ô của Logic Backend
            for (int i = 0; i < Slots.Length; i++)
            {
                if (i >= FarmingInventoryManager.InventorySize) break;

                var logicEntry = inventory.Entries[i]; // Lấy dữ liệu ngầm ở ô thứ i
                var uiSlot = Slots[i];                 // Lấy ô vuông UI thứ i

                if (logicEntry != null && logicEntry.Item != null)
                {
                    // Nếu ô đó có đồ -> Bật hình ảnh lên, lấy Sprite từ Item
                    uiSlot.Icon.sprite = logicEntry.Item.ItemSprite;
                    uiSlot.Icon.enabled = true;
                    
                    // Nếu số lượng > 1 thì hiện số (vd: 5 hạt giống), còn dụng cụ (số lượng 1) thì ẩn số đi
                    uiSlot.AmountText.text = logicEntry.StackSize > 1 ? logicEntry.StackSize.ToString() : "";
                }
                else
                {
                    // Nếu ô đó rỗng -> Tắt hình ảnh và chữ đi
                    uiSlot.Icon.enabled = false;
                    uiSlot.AmountText.text = "";
                }

                // Nếu ô thứ i đúng bằng ô đang được Equip -> Bật viền sáng lên
                if (uiSlot.Highlight != null)
                {
                    uiSlot.Highlight.SetActive(i == inventory.EquippedItemIdx);
                }
            }
        }
    }
}
