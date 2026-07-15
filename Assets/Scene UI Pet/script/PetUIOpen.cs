using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class PetUIOpen : MonoBehaviour
{
    [Header("Giao diện Pet")]
    [SerializeField] private GameObject petUIRoot;

    [Header("Manager của giao diện Pet")]
    [SerializeField] private PetUIManager petUIManager;

    private Button openButton;

    private void Awake()
    {
        openButton = GetComponent<Button>();
        openButton.onClick.AddListener(OpenPetUI);

        // Khi bắt đầu game, giao diện Pet sẽ được ẩn.
        if (petUIRoot != null)
            petUIRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(OpenPetUI);
    }

    public void OpenPetUI()
    {
        if (petUIRoot == null)
        {
            Debug.LogError(
                "PetUIOpenButton: Bạn chưa gán PetUI Root trong Inspector.",
                this
            );
            return;
        }

        // Bật toàn bộ giao diện Pet.
        petUIRoot.SetActive(true);

        // Đưa PetUI lên trên các UI khác trong cùng Canvas.
        petUIRoot.transform.SetAsLastSibling();

        // Tạo danh sách và hiển thị pet đầu tiên nếu chưa khởi tạo.
        if (petUIManager != null)
            petUIManager.Initialize();
        else
            Debug.LogWarning(
                "PetUIOpenButton: Chưa gán PetUIManager.",
                this
            );
    }
}