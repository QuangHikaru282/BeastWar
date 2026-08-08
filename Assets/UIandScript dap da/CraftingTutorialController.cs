using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kinnly
{
    public class CraftingTutorialController : MonoBehaviour
    {
        private const string TutorialSaveKey = "CraftingTutorialCompleted";

        [Header("Crafting System")]
        [SerializeField] private CraftingUI craftingUI;

        [Header("Tutorial Root")]
        [SerializeField] private GameObject tutorialRoot;
        [Tooltip("Object CraftingTutorialOverlay")]
        [SerializeField] private RectTransform overlayRect;

        [Header("Dark Blockers")]
        [SerializeField] private Image topBlocker;
        [SerializeField] private Image bottomBlocker;
        [SerializeField] private Image leftBlocker;
        [SerializeField] private Image rightBlocker;

        [Header("Highlight")]
        [SerializeField] private Image highlightFrame;
        [SerializeField] private float highlightPadding = 8f;

        [Header("Hand Pointer")]
        [SerializeField] private RectTransform handPointer;
        [SerializeField] private Vector2 pointerOffset = new Vector2(30f, 0f);
        [SerializeField] private float pointerDistance = 8f;
        [SerializeField] private float pointerSpeed = 4f;
        [SerializeField] private float pointerScreenMargin = 5f;

        [Header("Tutorial Message")]
        [SerializeField] private RectTransform tutorialMessagePanel;
        [Tooltip("Text nội dung hướng dẫn, không phải Text của nút")]
        [SerializeField] private TMP_Text tutorialText;
        [SerializeField] private Button nextButton;
        [Tooltip("TextMeshPro nằm bên trong nút Tiếp tục")]
        [SerializeField] private TMP_Text nextButtonText;
        [SerializeField] private Button skipButton;

        [Header("Button Targets")]
        [Tooltip("Đúng GameObject có component Button mở danh sách công thức")]
        [SerializeField] private Button recipeListButtonTarget;
        [Tooltip("Đúng GameObject có component Button Chế Tạo")]
        [SerializeField] private Button craftButtonTarget;

        [Header("Area Targets")]
        [SerializeField] private RectTransform recipeListArea;
        [SerializeField] private RectTransform requirementsArea;
        [SerializeField] private RectTransform materialInteractionArea;

        private RectTransform currentTarget;
        private Vector2 pointerBasePosition;
        private Vector2 pointerClampMinimum;
        private Vector2 pointerClampMaximum;

        // Chỉ hai bước nguyên liệu cần con trỏ quay sang trái.
        // Những bước còn lại vẫn giữ hướng gốc sang phải.
        private bool ShouldPointerFaceLeft =>
            currentStep == 2 || currentStep == 3;

        private int currentStep = -1;
        private bool tutorialRunning;
        private Coroutine pendingCoroutine;

        private void Awake()
        {
            ConfigureRaycastSettings();
            RegisterButtons();

            if (tutorialRoot != null)
                tutorialRoot.SetActive(false);
        }

        private void OnEnable()
        {
            SubscribeToCraftingEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromCraftingEvents();
            StopPendingCoroutine();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCraftingEvents();
            RemoveButtonListeners();
        }

        private void LateUpdate()
        {
            if (!tutorialRunning || tutorialRoot == null ||
                !tutorialRoot.activeInHierarchy)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            if (currentTarget != null)
            {
                UpdateTutorialTargetPosition();
                AnimatePointer();
            }

            // Khi đã chọn đủ nguyên liệu, chuyển sang nút Chế Tạo.
            if (currentStep == 3 && craftingUI != null && craftingUI.CanCraft)
                ShowStep(4);
        }

        private void ConfigureRaycastSettings()
        {
            SetImageRaycast(overlayRect, false);

            if (tutorialRoot != null)
            {
                Image rootImage = tutorialRoot.GetComponent<Image>();
                if (rootImage != null)
                    rootImage.raycastTarget = false;
            }

            if (highlightFrame != null)
                highlightFrame.raycastTarget = false;

            SetImageRaycast(handPointer, false);
            ConfigureBlocker(topBlocker);
            ConfigureBlocker(bottomBlocker);
            ConfigureBlocker(leftBlocker);
            ConfigureBlocker(rightBlocker);
        }

        private static void SetImageRaycast(RectTransform target, bool active)
        {
            if (target == null)
                return;

            Image image = target.GetComponent<Image>();
            if (image != null)
                image.raycastTarget = active;
        }

        private static void ConfigureBlocker(Image blocker)
        {
            if (blocker != null)
                blocker.raycastTarget = true;
        }

        private void RegisterButtons()
        {
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(NextStep);
                nextButton.onClick.AddListener(NextStep);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipTutorial);
                skipButton.onClick.AddListener(SkipTutorial);
            }

            if (craftButtonTarget != null)
            {
                craftButtonTarget.onClick.RemoveListener(HandleCraftButtonClicked);
                craftButtonTarget.onClick.AddListener(HandleCraftButtonClicked);
            }
        }

        private void RemoveButtonListeners()
        {
            if (nextButton != null)
                nextButton.onClick.RemoveListener(NextStep);

            if (skipButton != null)
                skipButton.onClick.RemoveListener(SkipTutorial);

            if (craftButtonTarget != null)
                craftButtonTarget.onClick.RemoveListener(HandleCraftButtonClicked);
        }

        private void SubscribeToCraftingEvents()
        {
            if (craftingUI == null)
                return;

            craftingUI.CraftingOpened -= HandleCraftingOpened;
            craftingUI.CraftingOpened += HandleCraftingOpened;

            craftingUI.RecipeListOpened -= HandleRecipeListOpened;
            craftingUI.RecipeListOpened += HandleRecipeListOpened;

            craftingUI.RecipeSelected -= HandleRecipeSelected;
            craftingUI.RecipeSelected += HandleRecipeSelected;

            craftingUI.CraftSucceeded -= HandleCraftSucceeded;
            craftingUI.CraftSucceeded += HandleCraftSucceeded;
        }

        private void UnsubscribeFromCraftingEvents()
        {
            if (craftingUI == null)
                return;

            craftingUI.CraftingOpened -= HandleCraftingOpened;
            craftingUI.RecipeListOpened -= HandleRecipeListOpened;
            craftingUI.RecipeSelected -= HandleRecipeSelected;
            craftingUI.CraftSucceeded -= HandleCraftSucceeded;
        }

        private void HandleCraftingOpened()
        {
            if (PlayerPrefs.GetInt(TutorialSaveKey, 0) == 0)
                BeginTutorial();
        }

        private void HandleRecipeListOpened()
        {
            if (tutorialRunning && currentStep == 0)
                ShowStepAfterLayout(1);
        }

        private void HandleRecipeSelected(CraftingRecipeData selectedRecipe)
        {
            if (tutorialRunning && currentStep == 1)
                ShowStepAfterLayout(2);
        }

        private void HandleCraftSucceeded()
        {
            if (tutorialRunning && currentStep == 4)
                ShowStepAfterLayout(5);
        }

        private void HandleCraftButtonClicked()
        {
            if (!tutorialRunning || currentStep != 4)
                return;

            StopPendingCoroutine();
            pendingCoroutine = StartCoroutine(FinishAfterCraftClick());
        }

        private IEnumerator FinishAfterCraftClick()
        {
            // Cho CraftingUI một frame để chạy hàm chế tạo trước.
            yield return null;

            if (tutorialRunning && currentStep == 4)
                ShowStep(5);

            pendingCoroutine = null;
        }

        public void BeginTutorial()
        {
            if (!ValidateRequiredReferences())
                return;

            StopPendingCoroutine();
            tutorialRunning = true;
            currentStep = -1;
            currentTarget = null;

            tutorialRoot.SetActive(true);
            tutorialRoot.transform.SetAsLastSibling();

            ConfigureRaycastSettings();
            EnsureVisualOrder();
            Canvas.ForceUpdateCanvases();
            ShowStepAfterLayout(0);
        }

        private bool ValidateRequiredReferences()
        {
            bool valid = true;

            valid &= CheckReference(tutorialRoot, "Tutorial Root");
            valid &= CheckReference(overlayRect, "Overlay Rect");
            valid &= CheckReference(topBlocker, "Top Blocker");
            valid &= CheckReference(bottomBlocker, "Bottom Blocker");
            valid &= CheckReference(leftBlocker, "Left Blocker");
            valid &= CheckReference(rightBlocker, "Right Blocker");
            valid &= CheckReference(highlightFrame, "Highlight Frame");
            valid &= CheckReference(handPointer, "Hand Pointer");
            valid &= CheckReference(tutorialMessagePanel, "Tutorial Message Panel");
            valid &= CheckReference(tutorialText, "Tutorial Text");
            valid &= CheckReference(nextButton, "Next Button");
            valid &= CheckReference(nextButtonText, "Next Button Text");
            valid &= CheckReference(skipButton, "Skip Button");
            valid &= CheckReference(recipeListButtonTarget, "Recipe List Button Target");
            valid &= CheckReference(craftButtonTarget, "Craft Button Target");
            valid &= CheckReference(recipeListArea, "Recipe List Area");
            valid &= CheckReference(requirementsArea, "Requirements Area");
            valid &= CheckReference(materialInteractionArea, "Material Interaction Area");

            if (overlayRect != null)
            {
                valid &= CheckDirectChild(topBlocker, "Top Blocker");
                valid &= CheckDirectChild(bottomBlocker, "Bottom Blocker");
                valid &= CheckDirectChild(leftBlocker, "Left Blocker");
                valid &= CheckDirectChild(rightBlocker, "Right Blocker");
                valid &= CheckDirectChild(highlightFrame, "Highlight Frame");
                valid &= CheckDirectChild(handPointer, "Hand Pointer");
            }

            return valid;
        }

        private static bool CheckReference(Object reference, string fieldName)
        {
            if (reference != null)
                return true;

            Debug.LogError("Crafting Tutorial: chưa gán " + fieldName + ".");
            return false;
        }

        private bool CheckDirectChild(Component component, string fieldName)
        {
            if (component == null || component.transform.parent == overlayRect)
                return true;

            Debug.LogError(fieldName + " phải là con trực tiếp của " +
                           overlayRect.name + ".");
            return false;
        }

        private void EnsureVisualOrder()
        {
            if (highlightFrame != null)
                highlightFrame.transform.SetAsLastSibling();

            if (handPointer != null)
                handPointer.SetAsLastSibling();

            if (tutorialMessagePanel != null)
                tutorialMessagePanel.SetAsLastSibling();
        }

        private void ShowStepAfterLayout(int step)
        {
            StopPendingCoroutine();
            pendingCoroutine = StartCoroutine(WaitForLayoutAndShowStep(step));
        }

        private IEnumerator WaitForLayoutAndShowStep(int step)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();

            ShowStep(step);
            pendingCoroutine = null;
        }

        private void StopPendingCoroutine()
        {
            if (pendingCoroutine == null)
                return;

            StopCoroutine(pendingCoroutine);
            pendingCoroutine = null;
        }

        private void ShowStep(int step)
        {
            if (!tutorialRunning)
                return;

            currentStep = step;

            switch (step)
            {
                case 0:
                    SetTutorialStep(
                        recipeListButtonTarget.GetComponent<RectTransform>(),
                        "Bấm DANH SÁCH CÔNG THỨC để xem các vật phẩm có thể chế tạo.",
                        false);
                    break;

                case 1:
                    SetTutorialStep(
                        recipeListArea,
                        "Hãy chọn một công thức trong danh sách.",
                        false);
                    break;

                case 2:
                    SetTutorialStep(
                        requirementsArea,
                        "Đây là những nguyên liệu cần thiết. Bạn phải bấm TIẾP TỤC trước khi được chọn nguyên liệu.",
                        true);
                    break;

                case 3:
                    SetTutorialStep(
                        materialInteractionArea,
                        "Bây giờ hãy chọn đúng nguyên liệu và đủ số lượng.",
                        false);
                    break;

                case 4:
                    SetTutorialStep(
                        craftButtonTarget.GetComponent<RectTransform>(),
                        "Bạn đã chọn đủ nguyên liệu. Hãy bấm CHẾ TẠO.",
                        false);
                    break;

                case 5:
                    ShowCompletionStep();
                    break;

                default:
                    Debug.LogWarning("Không tồn tại Tutorial Step: " + step);
                    break;
            }
        }

        private void SetTutorialStep(
            RectTransform target,
            string message,
            bool mustPressNext)
        {
            currentTarget = target;

            if (currentTarget == null)
            {
                Debug.LogError("Target của bước " + currentStep + " đang NULL.");
                return;
            }

            SetNormalBlockersActive(true);

            if (highlightFrame != null)
            {
                highlightFrame.gameObject.SetActive(true);

                // true ở bước 2: Image này chặn click vào vùng bên trong.
                // Không dùng alphaHitTestMinimumThreshold vì texture không Readable
                // sẽ gây InvalidOperationException và làm Text không đổi.
                highlightFrame.raycastTarget = mustPressNext;
            }

            if (handPointer != null)
            {
                handPointer.gameObject.SetActive(true);
                SetImageRaycast(handPointer, false);
            }

            SetTutorialMessage(message);

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(mustPressNext);
                if (mustPressNext)
                    SetNextButtonText("TIẾP TỤC");
            }

            if (skipButton != null)
                skipButton.gameObject.SetActive(true);

            EnsureVisualOrder();
            Canvas.ForceUpdateCanvases();
            UpdateTutorialTargetPosition();
        }

        private void SetTutorialMessage(string message)
        {
            if (tutorialText == null)
            {
                Debug.LogError("Chưa gán Tutorial Text trong Inspector.");
                return;
            }

            tutorialText.SetText(message);
            tutorialText.ForceMeshUpdate();

            if (tutorialMessagePanel != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(tutorialMessagePanel);
        }

        private void SetNextButtonText(string message)
        {
            if (nextButtonText == null)
            {
                Debug.LogError("Chưa gán Next Button Text trong Inspector.");
                return;
            }

            nextButtonText.SetText(message);
            nextButtonText.ForceMeshUpdate();
        }

        private void ShowCompletionStep()
        {
            currentTarget = null;

            if (highlightFrame != null)
            {
                highlightFrame.raycastTarget = false;
                highlightFrame.gameObject.SetActive(false);
            }

            if (handPointer != null)
                handPointer.gameObject.SetActive(false);

            SetFullScreenBlocker();
            SetTutorialMessage(
                "Hoàn thành! Bạn đã biết cách chọn công thức, chọn nguyên liệu và chế tạo vật phẩm.");

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
                SetNextButtonText("HOÀN THÀNH");
            }

            if (skipButton != null)
                skipButton.gameObject.SetActive(false);

            EnsureVisualOrder();
        }

        private void NextStep()
        {
            if (!tutorialRunning)
                return;

            if (currentStep == 2)
                ShowStepAfterLayout(3);
            else if (currentStep == 5)
                CompleteTutorial();
        }

        private void SkipTutorial()
        {
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            PlayerPrefs.SetInt(TutorialSaveKey, 1);
            PlayerPrefs.Save();

            StopPendingCoroutine();
            tutorialRunning = false;
            currentStep = -1;
            currentTarget = null;

            if (highlightFrame != null)
                highlightFrame.raycastTarget = false;

            if (tutorialRoot != null)
                tutorialRoot.SetActive(false);
        }

        public void ReplayTutorial()
        {
            PlayerPrefs.DeleteKey(TutorialSaveKey);
            PlayerPrefs.Save();
            BeginTutorial();
        }

        [ContextMenu("Reset Crafting Tutorial")]
        public void ResetTutorialForTesting()
        {
            PlayerPrefs.DeleteKey(TutorialSaveKey);
            PlayerPrefs.Save();

            StopPendingCoroutine();
            tutorialRunning = false;
            currentStep = -1;
            currentTarget = null;

            if (highlightFrame != null)
                highlightFrame.raycastTarget = false;

            if (tutorialRoot != null)
                tutorialRoot.SetActive(false);

            Debug.Log("Đã reset Crafting Tutorial. Hãy mở lại bàn chế tạo.");
        }

        private void UpdateTutorialTargetPosition()
        {
            if (currentTarget == null || overlayRect == null ||
                !currentTarget.gameObject.activeInHierarchy)
            {
                return;
            }

            GetBoundsInsideOverlay(currentTarget, out Vector2 minimum,
                                   out Vector2 maximum);

            Rect overlayArea = overlayRect.rect;
            minimum -= Vector2.one * highlightPadding;
            maximum += Vector2.one * highlightPadding;

            minimum.x = Mathf.Clamp(minimum.x, overlayArea.xMin, overlayArea.xMax);
            minimum.y = Mathf.Clamp(minimum.y, overlayArea.yMin, overlayArea.yMax);
            maximum.x = Mathf.Clamp(maximum.x, overlayArea.xMin, overlayArea.xMax);
            maximum.y = Mathf.Clamp(maximum.y, overlayArea.yMin, overlayArea.yMax);

            if (maximum.x <= minimum.x || maximum.y <= minimum.y)
                return;

            SetOverlayRect(highlightFrame.rectTransform, minimum, maximum);

            SetBlockerRect(topBlocker,
                new Vector2(overlayArea.xMin, maximum.y),
                new Vector2(overlayArea.xMax, overlayArea.yMax));

            SetBlockerRect(bottomBlocker,
                new Vector2(overlayArea.xMin, overlayArea.yMin),
                new Vector2(overlayArea.xMax, minimum.y));

            SetBlockerRect(leftBlocker,
                new Vector2(overlayArea.xMin, minimum.y),
                new Vector2(minimum.x, maximum.y));

            SetBlockerRect(rightBlocker,
                new Vector2(maximum.x, minimum.y),
                new Vector2(overlayArea.xMax, maximum.y));

            UpdatePointerPosition(minimum, maximum, overlayArea);
        }

        private void GetBoundsInsideOverlay(
            RectTransform target,
            out Vector2 minimum,
            out Vector2 maximum)
        {
            Vector3[] worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);

            minimum = new Vector2(float.MaxValue, float.MaxValue);
            maximum = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < worldCorners.Length; i++)
            {
                Vector3 local = overlayRect.InverseTransformPoint(worldCorners[i]);
                Vector2 point = new Vector2(local.x, local.y);
                minimum = Vector2.Min(minimum, point);
                maximum = Vector2.Max(maximum, point);
            }
        }

        private void SetBlockerRect(Image blocker, Vector2 minimum, Vector2 maximum)
        {
            if (blocker == null)
                return;

            blocker.gameObject.SetActive(true);
            blocker.raycastTarget = true;
            SetOverlayRect(blocker.rectTransform, minimum, maximum);
        }

        private void SetOverlayRect(
            RectTransform target,
            Vector2 minimum,
            Vector2 maximum)
        {
            if (target == null || overlayRect == null)
                return;

            if (target.parent != overlayRect)
            {
                Debug.LogError(target.name + " phải là con trực tiếp của " +
                               overlayRect.name + ".");
                return;
            }

            Vector2 center = (minimum + maximum) * 0.5f;
            Vector2 size = new Vector2(
                Mathf.Max(0.01f, maximum.x - minimum.x),
                Mathf.Max(0.01f, maximum.y - minimum.y));

            target.anchorMin = new Vector2(0.5f, 0.5f);
            target.anchorMax = new Vector2(0.5f, 0.5f);
            target.pivot = new Vector2(0.5f, 0.5f);
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
            target.anchoredPosition = center;
            target.sizeDelta = size;
        }

        private void SetNormalBlockersActive(bool active)
        {
            if (topBlocker != null) topBlocker.gameObject.SetActive(active);
            if (bottomBlocker != null) bottomBlocker.gameObject.SetActive(active);
            if (leftBlocker != null) leftBlocker.gameObject.SetActive(active);
            if (rightBlocker != null) rightBlocker.gameObject.SetActive(active);
        }

        private void SetFullScreenBlocker()
        {
            if (overlayRect == null || topBlocker == null)
                return;

            Rect area = overlayRect.rect;
            topBlocker.gameObject.SetActive(true);
            topBlocker.raycastTarget = true;

            SetOverlayRect(topBlocker.rectTransform,
                new Vector2(area.xMin, area.yMin),
                new Vector2(area.xMax, area.yMax));

            if (bottomBlocker != null) bottomBlocker.gameObject.SetActive(false);
            if (leftBlocker != null) leftBlocker.gameObject.SetActive(false);
            if (rightBlocker != null) rightBlocker.gameObject.SetActive(false);
        }

        private void UpdatePointerPosition(
            Vector2 targetMinimum,
            Vector2 targetMaximum,
            Rect overlayArea)
        {
            if (handPointer == null || overlayRect == null)
                return;

            if (handPointer.parent != overlayRect)
            {
                Debug.LogError("Hand Pointer phải là con trực tiếp của " +
                               overlayRect.name + ".");
                return;
            }

            handPointer.anchorMin = new Vector2(0.5f, 0.5f);
            handPointer.anchorMax = new Vector2(0.5f, 0.5f);
            handPointer.pivot = new Vector2(0.5f, 0.5f);

            // Bước 2: giới thiệu nguyên liệu cần thiết.
            // Bước 3: hướng dẫn chọn nguyên liệu.
            // Lật ngang Image ở đúng hai bước này; từ bước 4 trở đi
            // localScale trở lại (1, 1, 1), nên hướng cũ được giữ nguyên.
            handPointer.localScale = ShouldPointerFaceLeft
                ? new Vector3(-1f, 1f, 1f)
                : Vector3.one;

            handPointer.localRotation = Quaternion.identity;

            float halfWidth = Mathf.Max(8f, Mathf.Abs(handPointer.rect.width) * 0.5f);
            float halfHeight = Mathf.Max(8f, Mathf.Abs(handPointer.rect.height) * 0.5f);
            float margin = Mathf.Max(0f, pointerScreenMargin);

            pointerClampMinimum = new Vector2(
                overlayArea.xMin + halfWidth + margin,
                overlayArea.yMin + halfHeight + margin);

            pointerClampMaximum = new Vector2(
                overlayArea.xMax - halfWidth - margin,
                overlayArea.yMax - halfHeight - margin);

            float gap = Mathf.Max(8f, Mathf.Abs(pointerOffset.x));
            float rightX = targetMaximum.x + gap + halfWidth;
            float leftX = targetMinimum.x - gap - halfWidth;
            bool canUseRight = rightX <= pointerClampMaximum.x;
            bool canUseLeft = leftX >= pointerClampMinimum.x;

            float pointerX;
            if (pointerOffset.x >= 0f && canUseRight)
                pointerX = rightX;
            else if (pointerOffset.x < 0f && canUseLeft)
                pointerX = leftX;
            else if (canUseLeft)
                pointerX = leftX;
            else
                pointerX = rightX;

            float centerY = (targetMinimum.y + targetMaximum.y) * 0.5f;
            pointerBasePosition = new Vector2(pointerX, centerY + pointerOffset.y);

            pointerBasePosition.x = Mathf.Clamp(
                pointerBasePosition.x, pointerClampMinimum.x, pointerClampMaximum.x);
            pointerBasePosition.y = Mathf.Clamp(
                pointerBasePosition.y, pointerClampMinimum.y, pointerClampMaximum.y);
        }

        private void AnimatePointer()
        {
            if (handPointer == null)
                return;

            float movement = Mathf.Sin(Time.unscaledTime * pointerSpeed) *
                             pointerDistance;
            Vector2 position = pointerBasePosition + new Vector2(0f, movement);

            position.x = Mathf.Clamp(position.x,
                pointerClampMinimum.x, pointerClampMaximum.x);
            position.y = Mathf.Clamp(position.y,
                pointerClampMinimum.y, pointerClampMaximum.y);

            handPointer.anchoredPosition = position;
        }
    }
}