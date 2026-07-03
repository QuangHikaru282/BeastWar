using UnityEngine;
using UnityEngine.UI;

public class UIToggleManager : MonoBehaviour
{
    [Header("Cài đặt UI")]
    public GameObject uiPanel; // Giao diện cần bật/tắt (VD: Sảnh Đội, PetInfo)
    [Tooltip("Kéo Nút Mở Sảnh Đội vào đây để nó tự động biến mất khi bảng mở ra")]
    public GameObject openButton;
    public KeyCode toggleKey = KeyCode.P; // Phím tắt mặc định là P

    [Header("Các bảng phụ (Sẽ tự động tắt theo khi bấm phím P)")]
    public GameObject[] subPanels;

    [Header("Tùy chọn Text dưới Nút")]
    public TMPro.TextMeshProUGUI hotkeyText;

    private void Start()
    {
        if (hotkeyText != null)
        {
            hotkeyText.text = $"[{toggleKey.ToString()}]";
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleUI();
        }
    }

    // Gắn hàm này vào sự kiện OnClick của Button (nút Mở Sảnh Đội ngoài màn hình)
    public void ToggleUI()
    {
        if (uiPanel != null)
        {
            bool isActive = !uiPanel.activeSelf;
            uiPanel.SetActive(isActive);

            // Nếu bảng chính bị tắt đi (bởi phím P), tắt luôn tất cả bảng phụ
            if (!isActive && subPanels != null)
            {
                foreach (var panel in subPanels)
                {
                    if (panel != null) panel.SetActive(false);
                }
            }

            // Nếu bảng hiện ra -> Tắt nút Mở. Nếu bảng đóng lại -> Bật nút Mở lên.
            if (openButton != null)
            {
                openButton.SetActive(!isActive);
            }
        }
    }
}
