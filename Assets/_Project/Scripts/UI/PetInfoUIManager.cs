using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PetInfoUIManager : MonoBehaviour
{
    [Header("Data")]
    public PlayerData playerData;

    [Header("Danh sách Thú (Cột trái)")]
    public Transform beastListContainer;
    public GameObject beastButtonPrefab; // Một nút bấm chứa Image và Text

    [Header("Thông tin cơ bản (Giữa)")]
    public Image displayImage; // Ảnh của thú
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI elementText;
    
    [Header("Kỹ năng (Cột phải)")]
    public Transform skillListContainer;
    public GameObject skillSlotPrefab; // Prefab chứa PetSkillSlotUI

    [Header("Tài nguyên")]
    public TextMeshProUGUI goldText; // Hiển thị số vàng hiện tại

    private RuntimeBeastData selectedBeast;
    private List<GameObject> activeBeastButtons = new List<GameObject>();
    private List<GameObject> activeSkillSlots = new List<GameObject>();

    private void OnEnable()
    {
        RefreshGoldUI();
        LoadBeastList();
    }

    public void RefreshGoldUI()
    {
        if (goldText != null && playerData != null)
        {
            goldText.text = playerData.gold.ToString();
        }
        
        // Nếu có thú đang chọn, cập nhật lại cả list kỹ năng để nút nâng cấp (sáng/tối) thay đổi theo tiền
        if (selectedBeast != null)
        {
            LoadSkills(selectedBeast);
        }
    }

    private void LoadBeastList()
    {
        if (playerData == null) return;

        // Xóa danh sách cũ
        foreach (var btn in activeBeastButtons)
        {
            Destroy(btn);
        }
        activeBeastButtons.Clear();

        // Tạo nút cho từng con thú
        for (int i = 0; i < playerData.ownedBeasts.Count; i++)
        {
            RuntimeBeastData beast = playerData.ownedBeasts[i];
            if (beast == null) continue;

            GameObject btnObj = Instantiate(beastButtonPrefab, beastListContainer);
            btnObj.SetActive(true); // <--- BẬT HIỂN THỊ NÚT LÊN (vì bản gốc đang bị ẩn)
            activeBeastButtons.Add(btnObj);

            // Cập nhật tên hoặc avatar lên nút
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = beast.baseBeast.beastName;

            // Tìm Image để gán avatar (bỏ qua Image nền của chính nút bấm)
            Image[] images = btnObj.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                // Nếu tên của GameObject chứa chữ "Anh" hoặc "Avatar" hoặc "Icon" thì gán hình vào đó
                if (img.gameObject.name.ToLower().Contains("anh") || img.gameObject.name.ToLower().Contains("avatar") || img.gameObject.name.ToLower().Contains("icon"))
                {
                    img.sprite = beast.baseBeast.frontSprite;
                    break;
                }
            }

            // Gán sự kiện click
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectBeast(beast));
            }

            // Mặc định chọn con đầu tiên
            if (i == 0)
            {
                SelectBeast(beast);
            }
        }
    }

    public void SelectBeast(RuntimeBeastData beast)
    {
        selectedBeast = beast;

        // Cập nhật thông tin giữa màn hình
        if (displayImage != null)
        {
            displayImage.sprite = beast.baseBeast.frontSprite;
            // Đã xóa SetNativeSize() để bạn có thể tự kéo to ảnh bằng Rect Transform trong Unity
        }

        if (nameText != null) nameText.text = beast.baseBeast.beastName;
        if (levelText != null) levelText.text = $"Lv.{beast.currentLevel}";
        if (powerText != null) powerText.text = $"Lực chiến: {beast.CombatPower}";
        if (elementText != null) elementText.text = $"Hệ: {beast.baseBeast.element}";

        // Tải danh sách kỹ năng bên phải
        LoadSkills(beast);
    }

    private void LoadSkills(RuntimeBeastData beast)
    {
        // Xóa kỹ năng cũ
        foreach (var slot in activeSkillSlots)
        {
            Destroy(slot);
        }
        activeSkillSlots.Clear();

        if (beast.moves == null) return;

        foreach (var move in beast.moves)
        {
            if (move == null) continue;

            GameObject slotObj = Instantiate(skillSlotPrefab, skillListContainer);
            slotObj.SetActive(true); // <--- BẬT HIỂN THỊ KỸ NĂNG LÊN
            activeSkillSlots.Add(slotObj);

            PetSkillSlotUI slotUI = slotObj.GetComponent<PetSkillSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(move, playerData, this);
            }
        }
    }
}
