using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gắn vào Prefab nút ải trên WorldMap.
/// Prefab cần có: Button, Image (nền node), TextMeshProUGUI (số ải),
/// và 3 Image con đặt tên Star1/Star2/Star3 (icon ngôi sao).
/// </summary>
public class StageNodeUI : MonoBehaviour
{
    [Header("UI Components (gán trong Inspector của Prefab)")]
    [SerializeField] private Button nodeButton;
    [SerializeField] private Image nodeBackground;       // Hình nền của nút
    [SerializeField] private TextMeshProUGUI stageNumberText;
    [SerializeField] private TextMeshProUGUI stageNameText;  // (tuỳ chọn)
    [SerializeField] private GameObject lockIcon;         // Icon ổ khoá (ẩn khi mở)
    [SerializeField] private Image[] starIcons;           // Mảng 3 icon sao

    [Header("Màu sắc trạng thái")]
    [SerializeField] private Color colorUnlocked = Color.white;
    [SerializeField] private Color colorLocked   = new Color(0.4f, 0.4f, 0.4f, 0.9f);
    [SerializeField] private Color colorStarOn   = new Color(1f, 0.9f, 0.1f);
    [SerializeField] private Color colorStarOff  = new Color(0.4f, 0.4f, 0.4f, 0.5f);

    private StageData stageData;
    private WorldMapManager manager;

    /// <summary>Gọi bởi WorldMapManager khi tạo node.</summary>
    public void Initialize(StageData data, int stars, bool isUnlocked, WorldMapManager mgr)
    {
        stageData = data;
        manager   = mgr;

        // Số ải
        if (stageNumberText != null)
            stageNumberText.text = data.stageId.ToString();

        // Tên ải (tuỳ chọn)
        if (stageNameText != null)
            stageNameText.text = data.stageName;

        // Màu nền
        if (nodeBackground != null)
            nodeBackground.color = isUnlocked ? colorUnlocked : colorLocked;

        // Ổ khoá
        if (lockIcon != null)
            lockIcon.SetActive(!isUnlocked);

        // Sao
        if (starIcons != null)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                    starIcons[i].color = (i < stars) ? colorStarOn : colorStarOff;
            }
        }

        // Nút bấm
        if (nodeButton != null)
        {
            nodeButton.interactable = isUnlocked;
            nodeButton.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        manager?.OnStageSelected(stageData);
    }
}
