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

    private PetData petData;
    private Action<PetData> clickCallback;

    public PetData Data => petData;

    public void Setup(PetData data, Action<PetData> onClicked)
    {
        petData = data;
        clickCallback = onClicked;

        if (petData == null)
        {
            Debug.LogWarning($"{name}: PetData đang bị null.");
            gameObject.SetActive(false);
            return;
        }

        if (petNameText != null)
            petNameText.text = petData.PetName;

        if (petLevelText != null)
            petLevelText.text = $"Lv. {petData.Level}";

        SetImage(petIcon, petData.ListIcon);
        SetImage(elementIcon, petData.ElementIcon);

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