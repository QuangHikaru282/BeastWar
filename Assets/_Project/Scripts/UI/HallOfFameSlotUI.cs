using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý từng ô hiển thị Pet trong Sảnh Danh Vọng (Hall of Fame).
/// </summary>
public class HallOfFameSlotUI : MonoBehaviour
{
    [Header("Thành phần UI")]
    [SerializeField] private Image petSpriteImage;
    [SerializeField] private TextMeshProUGUI nameTextTMP;
    [SerializeField] private Text nameTextLegacy;
    [SerializeField] private TextMeshProUGUI levelTextTMP;
    [SerializeField] private Text levelTextLegacy;
    [SerializeField] private GameObject contentContainer;

    public void Setup(RuntimeBeastData beast)
    {
        if (beast == null || beast.baseBeast == null)
        {
            HideSlot();
            return;
        }

        if (contentContainer != null) contentContainer.SetActive(true);
        gameObject.SetActive(true);

        if (petSpriteImage != null)
        {
            petSpriteImage.gameObject.SetActive(true);
            if (beast.baseBeast.frontSprite != null)
            {
                petSpriteImage.sprite = beast.baseBeast.frontSprite;
                petSpriteImage.preserveAspect = true;
            }
        }

        SetText(nameTextTMP, nameTextLegacy, beast.baseBeast.beastName);
        SetText(levelTextTMP, levelTextLegacy, $"Lv. {beast.currentLevel}");
    }

    public void HideSlot()
    {
        if (contentContainer != null) contentContainer.SetActive(false);
        else gameObject.SetActive(false);
    }

    private void SetText(TextMeshProUGUI tmp, Text legacy, string content)
    {
        if (tmp != null) tmp.text = content;
        if (legacy != null) legacy.text = content;
    }
}
