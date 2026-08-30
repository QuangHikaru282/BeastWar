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

        AutoFindReferences();

        // Chỉ ẩn PetPanel con khi bắt đầu game, không ẩn PetUI cha
        if (petUIRoot != null && petUIRoot.name != "PetUI")
        {
            petUIRoot.SetActive(false);
        }
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

        if (petUIRoot == null || petUIRoot.name == "PetUI")
        {
            GameObject panelObj = GameObject.Find("PetPanel");
            if (panelObj != null)
            {
                petUIRoot = panelObj;
            }
            else if (petUIManager != null && petUIManager.transform.childCount > 0)
            {
                petUIRoot = petUIManager.transform.GetChild(0).gameObject;
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