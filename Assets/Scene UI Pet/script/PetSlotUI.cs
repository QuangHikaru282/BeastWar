using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetSlotUI : MonoBehaviour
{
    [Header("Component chính")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Image petIcon;

    [Header("Text")]
    [SerializeField] private TMP_Text petNameText;
    [SerializeField] private TMP_Text petLevelText;

    [Header("Trạng thái")]
    [SerializeField] private GameObject selectedBorder;
    [SerializeField] private Image elementIcon;

    private RuntimeBeastData petData;
    private Action<RuntimeBeastData> clickCallback;

    public RuntimeBeastData Data => petData;

    public void Setup(RuntimeBeastData data, Action<RuntimeBeastData> onClicked)
    {
        petData = data;
        clickCallback = onClicked;

        if (petData == null || petData.baseBeast == null)
        {
            Debug.LogWarning($"{name}: RuntimeBeastData đang bị null.");
            gameObject.SetActive(false);
            return;
        }

        if (petNameText != null)
            petNameText.text = petData.baseBeast.beastName;

        if (petLevelText != null)
            petLevelText.text = $"Lv. {petData.currentLevel}";

        SetImage(petIcon, petData.baseBeast.frontSprite);
        
        // Element icon có thể thiết lập sau nếu có ElementIconLibrary. Tạm thời vô hiệu hóa nếu không có.
        if (elementIcon != null) elementIcon.gameObject.SetActive(false);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(HandleClick);
        }

        SetSelected(false);
    }

    private void HandleClick()
    {
        if (petData != null)
            clickCallback?.Invoke(petData);
    }

    public void SetSelected(bool selected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(selected);
    }

    private static void SetImage(Image target, Sprite sprite)
    {
        if (target == null)
            return;

        target.sprite = sprite;
        target.enabled = sprite != null;
        target.preserveAspect = true;
    }

    private void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(HandleClick);
    }
}