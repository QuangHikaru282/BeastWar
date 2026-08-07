using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Điều khiển toàn bộ màn hình chọn nhân vật trong MainMenu:
/// - Camera zoom vào 2 nhân vật khi bấm Play
/// - Phím A/D để chuyển chọn giữa Nam và Nữ
/// - Nhân vật được chọn sẽ quay mặt về phía camera (trigger Animator)
/// - Nhân vật không được chọn sẽ quay lưng (idle bình thường)
/// - Ô nhập tên hiển thị bên dưới nhân vật được chọn
/// - Bấm Xác Nhận → lưu tên + giới tính → vào game
/// </summary>
public class CharacterSelectController : MonoBehaviour
{
    // ─── Camera ───────────────────────────────────────────────────────
    [Header("Camera Zoom")]
    [Tooltip("Main Camera của Scene")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Orthographic Size ban đầu của camera")]
    [SerializeField] private float cameraStartSize = 5f;

    [Tooltip("Orthographic Size sau khi zoom vào nhân vật")]
    [SerializeField] private float cameraZoomSize = 3f;

    [Tooltip("Vị trí camera ban đầu (tự động lấy lúc Start nếu không đặt)")]
    [SerializeField] private Vector3 cameraStartPos;

    [Tooltip("Vị trí camera sau khi zoom vào nhân vật")]
    [SerializeField] private Vector3 cameraZoomPos;

    [Tooltip("Thời gian zoom (giây)")]
    [SerializeField] private float zoomDuration = 1.2f;

    // ─── Nhân vật ─────────────────────────────────────────────────────
    [Header("Nhân vật")]
    [Tooltip("GameObject nhân vật Nam (Idle_4)")]
    [SerializeField] private GameObject maleCharacter;

    [Tooltip("GameObject nhân vật Nữ (Idle_4 (1))")]
    [SerializeField] private GameObject femaleCharacter;

    [Tooltip("Tên parameter bool trong Animator để quay mặt (true = nhìn về phía camera)")]
    [SerializeField] private string faceFrontParam = "FaceFront";

    [Tooltip("Tên trigger Animator khi được chọn (để chạy animation quay người)")]
    [SerializeField] private string selectTrigger = "Select";

    // ─── UI Main Menu ──────────────────────────────────────────────────
    [Header("UI Main Menu")]
    [Tooltip("ButtonContainer chứa các nút Play/Continue/Setting/Quit")]
    [SerializeField] private GameObject buttonContainer;

    [Tooltip("MainMenuPanel (cha của ButtonContainer) — sẽ bị ẩn khi vào chế độ chọn nhân vật")]
    [SerializeField] private GameObject mainMenuPanel;

    // ─── UI Chọn nhân vật ─────────────────────────────────────────────
    [Header("UI Chọn nhân vật (Canvas World Space hoặc Screen Space)")]
    [Tooltip("Panel chứa toàn bộ UI chọn nhân vật (mũi tên, ô tên, nút xác nhận)")]
    [SerializeField] private GameObject charSelectPanel;

    [Tooltip("Text hiển thị tên nhân vật đang chọn (Nam / Nữ)")]
    [SerializeField] private TMP_Text charNameLabel;

    [Tooltip("Ô nhập tên do người chơi đặt")]
    [SerializeField] private TMP_InputField nameInputField;

    [Tooltip("Nút Xác Nhận")]
    [SerializeField] private Button confirmButton;

    [Tooltip("Nút mũi tên trái (A) — tùy chọn")]
    [SerializeField] private Button arrowLeftButton;

    [Tooltip("Nút mũi tên phải (D) — tùy chọn")]
    [SerializeField] private Button arrowRightButton;

    // ─── Data & Intro ──────────────────────────────────────────────────
    [Header("Data & Intro")]
    [Tooltip("Kéo file PlayerData asset vào đây")]
    [SerializeField] private PlayerData playerData;

    [Tooltip("Tên Scene GameCore / HubTownNew để chuyển vào")]
    [SerializeField] private string gameSceneName = "GameCore, HubTownNew";

    [Tooltip("Kéo WorldIntroUI vào đây nếu muốn hiển thị bảng giới thiệu thế giới sau khi bấm Xác Nhận đặt tên")]
    [SerializeField] private BeastWar.UI.WorldIntroUI worldIntroUI;

    // ─── Private ───────────────────────────────────────────────────────
    private int selectedIndex = 0;  // 0 = Nam, 1 = Nữ
    private bool isSelecting = false;

    private Animator maleAnimator;
    private Animator femaleAnimator;

    private readonly string[] charLabels = new string[] { "Nam", "Nữ" };

    // ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (maleCharacter != null)
            maleAnimator = maleCharacter.GetComponent<Animator>();

        if (femaleCharacter != null)
            femaleAnimator = femaleCharacter.GetComponent<Animator>();

        // Ẩn UI chọn nhân vật lúc đầu
        if (charSelectPanel != null)
            charSelectPanel.SetActive(false);
    }

    private void Start()
    {
        // Ghi nhớ vị trí và size ban đầu của camera
        if (mainCamera != null)
        {
            cameraStartSize = mainCamera.orthographicSize;
            cameraStartPos = mainCamera.transform.position;
        }

        // Đăng ký sự kiện nút
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);

        if (arrowLeftButton != null)
            arrowLeftButton.onClick.AddListener(SelectPrevious);

        if (arrowRightButton != null)
            arrowRightButton.onClick.AddListener(SelectNext);
    }

    private void Update()
    {
        if (!isSelecting) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            SelectPrevious();

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            SelectNext();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            OnConfirm();
    }

    // ─────────────────────────────────────────────────────────────────
    // Gọi từ PlayIntroVideo khi bấm nút Play
    // ─────────────────────────────────────────────────────────────────
    public void StartCharacterSelect()
    {
        if (isSelecting) return;

        // Ẩn toàn bộ menu chính (kể cả nền trắng của MainMenuPanel)
        if (buttonContainer != null)
            buttonContainer.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        // Chạy coroutine zoom camera rồi hiện UI chọn nhân vật
        StartCoroutine(ZoomCameraAndShowSelect());
    }

    // ─────────────────────────────────────────────────────────────────
    // Camera Zoom
    // ─────────────────────────────────────────────────────────────────
    private IEnumerator ZoomCameraAndShowSelect()
    {
        float elapsed = 0f;
        float startSize = mainCamera.orthographicSize;
        Vector3 startPos = mainCamera.transform.position;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration);

            if (mainCamera.orthographic)
                mainCamera.orthographicSize = Mathf.Lerp(startSize, cameraZoomSize, t);

            mainCamera.transform.position = Vector3.Lerp(startPos, cameraZoomPos, t);

            yield return null;
        }

        // Đảm bảo giá trị cuối cùng
        if (mainCamera.orthographic)
            mainCamera.orthographicSize = cameraZoomSize;
        mainCamera.transform.position = cameraZoomPos;

        // Bắt đầu chế độ chọn nhân vật
        BeginSelection();
    }

    // ─────────────────────────────────────────────────────────────────
    // Bắt đầu chọn nhân vật
    // ─────────────────────────────────────────────────────────────────
    private void BeginSelection()
    {
        isSelecting = true;
        selectedIndex = 0;

        if (charSelectPanel != null)
            charSelectPanel.SetActive(true);

        if (nameInputField != null)
            nameInputField.text = "";

        RefreshCharacterDisplay();
    }

    // ─────────────────────────────────────────────────────────────────
    // Chuyển sang nhân vật trước (A / ←)
    // ─────────────────────────────────────────────────────────────────
    public void SelectPrevious()
    {
        if (!isSelecting) return;
        selectedIndex = (selectedIndex - 1 + 2) % 2;
        RefreshCharacterDisplay();
    }

    // ─────────────────────────────────────────────────────────────────
    // Chuyển sang nhân vật tiếp theo (D / →)
    // ─────────────────────────────────────────────────────────────────
    public void SelectNext()
    {
        if (!isSelecting) return;
        selectedIndex = (selectedIndex + 1) % 2;
        RefreshCharacterDisplay();
    }

    // ─────────────────────────────────────────────────────────────────
    // Cập nhật hiển thị theo nhân vật đang chọn
    // ─────────────────────────────────────────────────────────────────
    private void RefreshCharacterDisplay()
    {
        bool maleSelected = selectedIndex == 0;

        // Cập nhật Animator: nhân vật được chọn nhìn về phía camera
        SetFaceFront(maleAnimator, maleSelected);
        SetFaceFront(femaleAnimator, !maleSelected);

        // Cập nhật label tên nhân vật
        if (charNameLabel != null)
            charNameLabel.text = charLabels[selectedIndex];
    }

    private void SetFaceFront(Animator anim, bool faceFront)
    {
        if (anim == null) return;

        // Thử set bool parameter
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == faceFrontParam && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(faceFrontParam, faceFront);
                return;
            }

            // Nếu không có bool, thử set trigger khi vừa được chọn
            if (faceFront && param.name == selectTrigger && param.type == AnimatorControllerParameterType.Trigger)
            {
                anim.SetTrigger(selectTrigger);
                return;
            }
        }

        // Fallback: flip scale X nếu Animator không có parameter phù hợp
        if (anim.transform != null)
        {
            Vector3 s = anim.transform.localScale;
            s.x = faceFront ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            anim.transform.localScale = s;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Xác nhận tên và vào game
    // ─────────────────────────────────────────────────────────────────
    private void OnConfirm()
    {
        if (!isSelecting) return;

        string inputName = nameInputField != null ? nameInputField.text.Trim() : "";

        if (string.IsNullOrEmpty(inputName))
        {
            // Nếu bỏ trống tên, dùng tên mặc định
            inputName = selectedIndex == 0 ? "Trainer" : "Trainer";
        }

        string gender = selectedIndex == 0 ? "Male" : "Female";

        // Lưu vào PlayerData ScriptableObject
        if (playerData != null)
        {
            playerData.playerName = inputName;
            playerData.characterGender = gender;
        }

        // Lưu dự phòng qua PlayerPrefs (để các scene khác đọc được)
        PlayerPrefs.SetString("PlayerName", inputName);
        PlayerPrefs.SetString("SelectedCharacter", gender);
        PlayerPrefs.Save();

        Debug.Log($"[CharSelect] Đã chọn: {gender} | Tên: {inputName}");

        isSelecting = false;

        // Ẩn UI chọn nhân vật
        if (charSelectPanel != null)
            charSelectPanel.SetActive(false);

        // Nếu có gán WorldIntroUI: hiển thị bảng giới thiệu thế giới trước khi chuyển scene
        if (worldIntroUI != null)
        {
            worldIntroUI.OnIntroCompletedEvent.RemoveListener(LoadGame);
            worldIntroUI.OnIntroCompletedEvent.AddListener(LoadGame);
            worldIntroUI.StartIntro();
        }
        else
        {
            // Nếu không gán Intro, vào thẳng game
            LoadGame();
        }
    }

    private void LoadGame()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(gameSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                gameSceneName.Split(',')[0].Trim()
            );
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Tiện ích: cho phép Inspector xem preview vị trí zoom camera
    // ─────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(cameraZoomPos, 0.3f);
    }
}
