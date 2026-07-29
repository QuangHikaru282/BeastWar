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

    private void AutoFindReferences()
    {
        if (petUIManager == null)
        {
            petUIManager = FindFirstObjectByType<PetUIManager>(FindObjectsInactive.Include);
        }

        if (petUIRoot == null)
        {
            if (petUIManager != null)
            {
                petUIRoot = petUIManager.gameObject;
            }
            else
            {
                GameObject rootObj = GameObject.Find("PetPanel");
                if (rootObj == null) rootObj = GameObject.Find("PetUI");
                if (rootObj != null) petUIRoot = rootObj;
            }
        }
    }

    public void OpenPetUI()
    {
        AutoFindReferences();

        if (petUIManager != null)
        {
            petUIManager.OpenPetUI();
            return;
        }

        if (petUIRoot != null)
        {
            petUIRoot.SetActive(true);
            petUIRoot.transform.SetAsLastSibling();
            InteractHintManager.Instance?.RegisterPanelOpen();
        }
        else
        {
            Debug.LogError("PetUIOpenButton: Không tìm thấy PetUIManager hoặc PetUIRoot trong Scene.", this);
        }
    }
}