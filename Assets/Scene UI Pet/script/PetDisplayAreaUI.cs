using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thay đổi background của PetDisplayArea
/// dựa theo BeastData.enhanceElement.
/// </summary>
[DisallowMultipleComponent]
public class PetDisplayAreaUI : MonoBehaviour
{
    [Header("Image nền của PetDisplayArea")]
    [Tooltip("Kéo component Image của PetDisplayArea vào đây.")]
    [SerializeField] private Image backgroundImage;

    [Header("Background 7 nguyên tố")]
    [SerializeField] private Sprite backgroundMetal;
    [SerializeField] private Sprite backgroundWater;
    [SerializeField] private Sprite backgroundWood;
    [SerializeField] private Sprite backgroundFire;
    [SerializeField] private Sprite backgroundEarth;
    [SerializeField] private Sprite backgroundLight;
    [SerializeField] private Sprite backgroundDark;

    [Header("Background mặc định")]
    [SerializeField] private Sprite defaultBackground;

    private RuntimeBeastData currentPet;

    private void Awake()
    {
        // Tự lấy Image trên PetDisplayArea nếu chưa kéo.
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (backgroundImage == null)
        {
            Debug.LogError(
                "[PetDisplayAreaUI] PetDisplayArea chưa có component Image.",
                this
            );
        }
    }

    /// <summary>
    /// Hiển thị background phù hợp với pet.
    /// </summary>
    public void Display(RuntimeBeastData pet)
    {
        currentPet = pet;

        if (pet == null || pet.baseBeast == null)
        {
            Clear();
            return;
        }

        EnhanceElement element =
            pet.baseBeast.enhanceElement;

        Sprite targetBackground =
            GetBackgroundByElement(element);

        if (targetBackground == null)
        {
            targetBackground = defaultBackground;

            Debug.LogWarning(
                $"[PetDisplayAreaUI] Chưa gán background cho hệ {element}.",
                this
            );
        }

        SetBackground(targetBackground);

        Debug.Log(
            $"[PetDisplayAreaUI] Pet: {pet.baseBeast.beastName} | " +
            $"Enhance Element: {element} | " +
            $"Background: " +
            $"{(targetBackground != null ? targetBackground.name : "NULL")}",
            this
        );
    }

    public void Refresh()
    {
        Display(currentPet);
    }

    public void Clear()
    {
        currentPet = null;
        SetBackground(defaultBackground);
    }

    private Sprite GetBackgroundByElement(
        EnhanceElement element
    )
    {
        switch (element)
        {
            case EnhanceElement.Metal:
                return backgroundMetal;

            case EnhanceElement.Water:
                return backgroundWater;

            case EnhanceElement.Wood:
                return backgroundWood;

            case EnhanceElement.Fire:
                return backgroundFire;

            case EnhanceElement.Earth:
                return backgroundEarth;

            case EnhanceElement.Light:
                return backgroundLight;

            case EnhanceElement.Dark:
                return backgroundDark;

            default:
                return defaultBackground;
        }
    }

    private void SetBackground(Sprite sprite)
    {
        if (backgroundImage == null)
            return;

        backgroundImage.sprite = sprite;
        backgroundImage.enabled = sprite != null;

        // Background cần kéo giãn theo PetDisplayArea.
        backgroundImage.preserveAspect = false;
        backgroundImage.type = Image.Type.Simple;

        Color imageColor = backgroundImage.color;
        imageColor.r = 1f;
        imageColor.g = 1f;
        imageColor.b = 1f;
        imageColor.a = 1f;
        backgroundImage.color = imageColor;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }
#endif
}