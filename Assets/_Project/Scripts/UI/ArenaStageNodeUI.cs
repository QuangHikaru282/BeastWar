using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gắn vào Prefab nút ải trong Arena.
/// </summary>
public class ArenaStageNodeUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button nodeButton;
    [SerializeField] private Image nodeBackground;
    [SerializeField] private TextMeshProUGUI stageNumberText;
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Image[] starIcons;

    [Header("Màu sắc trạng thái")]
    [SerializeField] private Color colorUnlocked = Color.white;
    [SerializeField] private Color colorLocked   = new Color(0.4f, 0.4f, 0.4f, 0.9f);
    [SerializeField] private Color colorStarOn   = new Color(1f, 0.9f, 0.1f);
    [SerializeField] private Color colorStarOff  = new Color(0.4f, 0.4f, 0.4f, 0.5f);

    private StageData stageData;
    private ArenaUIManager manager;

    public void Initialize(StageData data, int stars, bool isUnlocked, ArenaUIManager mgr)
    {
        stageData = data;
        manager   = mgr;

        if (stageNumberText != null) stageNumberText.text = data.stageId.ToString();
        if (stageNameText != null) stageNameText.text = data.stageName;
        if (nodeBackground != null) nodeBackground.color = isUnlocked ? colorUnlocked : colorLocked;
        if (lockIcon != null) lockIcon.SetActive(!isUnlocked);

        if (starIcons != null)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                    starIcons[i].color = (i < stars) ? colorStarOn : colorStarOff;
            }
        }

        if (nodeButton != null)
        {
            nodeButton.interactable = isUnlocked;
            nodeButton.onClick.RemoveAllListeners();
            nodeButton.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        manager?.OnStageSelected(stageData);
    }
}
