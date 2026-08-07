using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace BeastWar.UI
{
    /// <summary>
    /// Điều khiển bảng giới thiệu thế giới / hướng dẫn đầu game phong cách Pokémon FireRed.
    /// Kích hoạt khi bắt đầu game, hiển thị văn bản từng ký tự (Typewriter effect),
    /// hỗ trợ nút UI Tiếp (A) / Lùi (D) cũng như phím bấm bàn phím (A/D/Space/Enter).
    /// 
    /// LƯU Ý DỰ ÁN: Mọi reference UI bắt buộc gán thủ công qua Inspector.
    /// </summary>
    public class WorldIntroUI : MonoBehaviour
    {
        [Header("--- UI References (Gán thủ công trong Inspector) ---")]
        [Tooltip("Khung chứa toàn bộ giao diện Intro (Panel)")]
        [SerializeField] private GameObject introPanel;

        [Tooltip("Text hiển thị nội dung văn bản (TextMeshProUGUI)")]
        [SerializeField] private TextMeshProUGUI introText;

        [Tooltip("Nút Tiếp / Trang kế tiếp (Ví dụ: Nút có chữ ⒶTiếp)")]
        [SerializeField] private Button nextButton;

        [Tooltip("Nút Lùi / Trang trước (Ví dụ: Nút có chữ ⒷLùi hoặc D: Lùi) - Sẽ tự ẩn khi ở trang đầu")]
        [SerializeField] private Button backButton;

        [Tooltip("Biểu tượng mũi tên đỏ/chớp tắt báo hiệu hoàn tất chữ trang hiện tại")]
        [SerializeField] private GameObject nextArrowIndicator;

        [Header("--- Cấu hình nội dung (Pages) ---")]
        [Tooltip("Danh sách nội dung các trang văn bản giới thiệu thế giới")]
        [SerializeField, TextArea(4, 10)] 
        private string[] introPages = new string[]
        {
            "Trong thế giới này, bạn sẽ bắt tay vào 1 cuộc đại phiêu lưu mà bạn sẽ như 1 vị anh hùng.\n\nTrò chuyện với mọi người, kiểm tra mọi thứ ở bất cứ nơi nào bạn đi, thị trấn, trên đường hay hang động. Thu thập thông tin, gợi ý từ mọi nguồn.",
            "Những con đường mới sẽ mở ra khi bạn giúp người khác, vượt qua thử thách, giải quyết bí ẩn.\n\nĐôi khi bạn sẽ bị thách thức bởi những HLV khác & bị PKM hoang dã tấn công. Hãy dũng cảm & tiến lên phía trước.",
            "Thông qua đó, chúng tôi hy vọng rằng bạn sẽ tương tác với mọi loại người và đạt được phát triển cá nhân. Đó là mục tiêu lớn nhất của chúng tôi.\n\nNhấn nút A để cuộc phiêu lưu của bạn bắt đầu!"
        };

        [Header("--- Tùy chỉnh hiệu ứng & Phím bấm ---")]
        [Tooltip("Tốc độ gõ từng chữ (giây / ký tự)")]
        [SerializeField] private float typingSpeed = 0.03f;

        [Tooltip("Tự động mở và bắt đầu Intro ngay khi Script được kích hoạt (Mặc định = false để chờ đặt tên xong)")]
        [SerializeField] private bool autoStartOnEnable = false;

        [Tooltip("Phím phím bấm cho nút Tiếp (Mặc định: Phím A)")]
        [SerializeField] private KeyCode nextKey = KeyCode.A;

        [Tooltip("Phím phím bấm phụ cho nút Tiếp (Mặc định: Space)")]
        [SerializeField] private KeyCode nextKeySecondary = KeyCode.Space;

        [Tooltip("Phím phím bấm cho nút Lùi (Mặc định: Phím D)")]
        [SerializeField] private KeyCode backKey = KeyCode.D;

        [Header("--- Sự kiện khi hoàn tất Intro ---")]
        [Tooltip("Sự kiện được gọi khi người dùng đọc hết trang cuối cùng và bấm Tiếp")]
        [SerializeField] private UnityEvent onIntroCompleted;

        /// <summary>
        /// Sự kiện để các script bên ngoài (ví dụ CharacterSelectController) đăng ký nhận khi kết thúc Intro
        /// </summary>
        public UnityEvent OnIntroCompletedEvent => onIntroCompleted;

        // Private variables
        private int currentPageIndex = 0;
        private Coroutine typingCoroutine;
        private bool isTyping = false;

        private void Awake()
        {
            // Mặc định ẩn panel nếu không tự động bật
            if (!autoStartOnEnable && introPanel != null)
            {
                introPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (autoStartOnEnable)
            {
                StartIntro();
            }
        }

        private void Start()
        {
            // Đăng ký sự kiện click nút UI
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(OnNextButtonClicked);
                nextButton.onClick.AddListener(OnNextButtonClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackButtonClicked);
                backButton.onClick.AddListener(OnBackButtonClicked);
            }
        }

        private void Update()
        {
            // Kiểm tra phím bấm điều hướng (A / D / Space / Enter)
            if (introPanel != null && introPanel.activeSelf)
            {
                if (Input.GetKeyDown(nextKey) || Input.GetKeyDown(nextKeySecondary) || Input.GetKeyDown(KeyCode.Return))
                {
                    OnNextButtonClicked();
                }
                else if (Input.GetKeyDown(backKey))
                {
                    OnBackButtonClicked();
                }
            }
        }

        /// <summary>
        /// Bắt đầu hiển thị Intro từ trang đầu tiên (Trang 0)
        /// </summary>
        public void StartIntro()
        {
            if (introPages == null || introPages.Length == 0)
            {
                Debug.LogWarning("[WorldIntroUI] Danh sách introPages đang trống!");
                EndIntro();
                return;
            }

            currentPageIndex = 0;
            if (introPanel != null) introPanel.SetActive(true);

            ShowPage(currentPageIndex);
        }

        /// <summary>
        /// Xử lý khi nhấn nút/phím TIẾP (A)
        /// </summary>
        public void OnNextButtonClicked()
        {
            if (isTyping)
            {
                // Nếu chữ đang gõ: dừng Coroutine và hiện toàn bộ văn bản của trang ngay lập tức
                CompleteCurrentPageTyping();
            }
            else
            {
                // Nếu đã gõ xong: chuyển sang trang tiếp theo
                currentPageIndex++;

                if (currentPageIndex < introPages.Length)
                {
                    ShowPage(currentPageIndex);
                }
                else
                {
                    // Đã đọc hết tất cả các trang
                    EndIntro();
                }
            }
        }

        /// <summary>
        /// Xử lý khi nhấn nút/phím LÙI (D)
        /// </summary>
        public void OnBackButtonClicked()
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                ShowPage(currentPageIndex);
            }
        }

        /// <summary>
        /// Hiển thị trang văn bản cụ thể theo index
        /// </summary>
        private void ShowPage(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= introPages.Length) return;

            // Cập nhật trạng thái hiển thị Nút Lùi
            if (backButton != null)
            {
                backButton.gameObject.SetActive(pageIndex > 0);
            }

            // Ẩn mũi tên indicator khi đang gõ chữ
            if (nextArrowIndicator != null)
            {
                nextArrowIndicator.SetActive(false);
            }

            // Dừng coroutine gõ chữ cũ nếu đang chạy
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            // Bắt đầu hiệu ứng typewriter cho trang mới
            typingCoroutine = StartCoroutine(TypeTextRoutine(introPages[pageIndex]));
        }

        /// <summary>
        /// Coroutine gõ chữ từ từ từng ký tự
        /// </summary>
        private IEnumerator TypeTextRoutine(string fullText)
        {
            isTyping = true;
            if (introText != null) introText.text = "";

            for (int i = 0; i <= fullText.Length; i++)
            {
                if (introText != null)
                {
                    introText.text = fullText.Substring(0, i);
                }
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;

            // Hiện mũi tên chỉ báo đã gõ xong
            if (nextArrowIndicator != null)
            {
                nextArrowIndicator.SetActive(true);
            }
        }

        /// <summary>
        /// Hiện full văn bản trang hiện tại ngay lập tức
        /// </summary>
        private void CompleteCurrentPageTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            isTyping = false;

            if (introText != null && currentPageIndex >= 0 && currentPageIndex < introPages.Length)
            {
                introText.text = introPages[currentPageIndex];
            }

            if (nextArrowIndicator != null)
            {
                nextArrowIndicator.SetActive(true);
            }
        }

        /// <summary>
        /// Kết thúc Intro, ẩn panel và kích hoạt sự kiện OnIntroCompleted
        /// </summary>
        private void EndIntro()
        {
            if (introPanel != null)
            {
                introPanel.SetActive(false);
            }

            onIntroCompleted?.Invoke();
        }
    }
}
