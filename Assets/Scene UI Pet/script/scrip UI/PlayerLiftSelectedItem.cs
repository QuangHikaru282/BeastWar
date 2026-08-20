using System.Collections;
using UnityEngine;

namespace Kinnly
{
    [DefaultExecutionOrder(100)]
    public class PlayerLiftSelectedItem : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField]
        private KeyCode liftKey = KeyCode.G;

        [SerializeField]
        private string horizontalAxis = "Horizontal";

        [SerializeField]
        private string verticalAxis = "Vertical";

        [Header("Inventory")]
        [SerializeField]
        private PlayerInventory playerInventory;

        [Header("Animator")]
        [SerializeField]
        private Animator playerAnimator;

        [SerializeField]
        private string liftBoolName =
            "IsLiftingItem";

        [SerializeField]
        private string speedParameter =
            "speed";

        [SerializeField]
        private string directionXParameter =
            "dirX";

        [SerializeField]
        private string directionYParameter =
            "dirY";

        [Tooltip(
            "Script tự cập nhật speed, dirX và dirY"
        )]
        [SerializeField]
        private bool updateMovementParameters = true;

        [Header("Huong mac dinh")]
        [SerializeField]
        private Vector2 defaultDirection =
            Vector2.down;

        [Min(0.001f)]
        [SerializeField]
        private float movementThreshold = 0.01f;

        [Header("Xoay huong Player")]
        [Tooltip(
            "SpriteRenderer của nhân vật hiện tại"
        )]
        [SerializeField]
        private SpriteRenderer playerSpriteRenderer;

        [Tooltip(
            "PickUp3 gốc đang nhìn sang phải"
        )]
        [SerializeField]
        private bool pickUp3FacesRight = true;

        [Header("Vi tri Item")]
        [SerializeField]
        private Transform handItemPoint;

        [SerializeField]
        private Transform liftItemPoint;

        [Header("Hinh anh Item")]
        [Tooltip(
            "SpriteRenderer của LiftedItemVisual"
        )]
        [SerializeField]
        private SpriteRenderer liftedItemRenderer;

        [Tooltip(
            "Object hiển thị item bình thường trên tay. " +
            "Có thể để trống."
        )]
        [SerializeField]
        private GameObject normalHeldItemVisual;

        [Header("Kich thuoc Item")]
        [Tooltip(
            "Kích thước cạnh dài nhất của mọi item"
        )]
        [Min(0.05f)]
        [SerializeField]
        private float fixedItemSize = 0.7f;

        [Header("Thoi diem hien Item")]
        [Tooltip(
            "Đặt bằng thời lượng animation nâng item. " +
            "Ví dụ animation dài 0.5 giây thì đặt 0.5."
        )]
        [Min(0f)]
        [SerializeField]
        private float itemRevealDelay = 0.5f;

        [Tooltip(
            "Giữ item không bị xoay hoặc lật theo Player"
        )]
        [SerializeField]
        private bool keepItemWorldRotation = true;

        private Coroutine revealItemCoroutine;
        private Transform itemVisualOriginalParent;

        private Vector2 movementInput;
        private Vector2 lastDirection;

        private Item currentlyDisplayedItem;

        private bool isHoldingItem;
        private bool waitingToRevealItem;
        private bool normalHeldItemWasActive;
        private bool animatorErrorWasLogged;

        public bool IsHoldingItem
        {
            get
            {
                return isHoldingItem;
            }
        }

        public Vector2 LastDirection
        {
            get
            {
                return lastDirection;
            }
        }

        private void Awake()
        {
            FindMissingReferences();

            lastDirection =
                GetCardinalDirection(
                    defaultDirection
                );

            ReadDirectionFromAnimator();

            if (liftedItemRenderer != null)
            {
                itemVisualOriginalParent =
                    liftedItemRenderer.transform.parent;

                HideLiftedItemVisual();
            }
        }

        private void Update()
        {
            ReadMovementInput();
            UpdateAnimatorMovementParameters();

            // Khi đang nâng và item đã hiện,
            // kiểm tra xem người chơi có đổi Toolbar không.
            if (isHoldingItem &&
                !waitingToRevealItem)
            {
                RefreshLiftedItemFromToolbar(false);
            }

            if (!Input.GetKeyDown(liftKey))
            {
                return;
            }

            if (playerInventory != null &&
                playerInventory.IsOpen)
            {
                return;
            }

            ToggleHoldingItem();
        }

        private void LateUpdate()
        {
            UpdatePickupDirectionFlip();

            if (!isHoldingItem ||
                waitingToRevealItem ||
                liftedItemRenderer == null ||
                liftItemPoint == null ||
                !liftedItemRenderer.gameObject.activeSelf)
            {
                return;
            }

            // Item luôn đi theo điểm trên đầu.
            liftedItemRenderer.transform.position =
                liftItemPoint.position;

            if (keepItemWorldRotation)
            {
                liftedItemRenderer.transform.rotation =
                    Quaternion.identity;
            }
        }

        private void FindMissingReferences()
        {
            if (playerInventory == null)
            {
                playerInventory =
                    GetComponent<PlayerInventory>();
            }

            FindCorrectAnimator();

            if (playerSpriteRenderer == null &&
                playerAnimator != null)
            {
                playerSpriteRenderer =
                    playerAnimator
                        .GetComponent<SpriteRenderer>();
            }
        }

        private void FindCorrectAnimator()
        {
            if (playerAnimator != null &&
                playerAnimator.gameObject.activeInHierarchy &&
                HasAnimatorParameter(
                    playerAnimator,
                    liftBoolName,
                    AnimatorControllerParameterType.Bool
                ))
            {
                if (playerSpriteRenderer == null ||
                    !playerSpriteRenderer.gameObject
                        .activeInHierarchy)
                {
                    playerSpriteRenderer =
                        playerAnimator
                            .GetComponent<SpriteRenderer>();
                }

                return;
            }

            Animator[] childAnimators =
                GetComponentsInChildren<Animator>(true);

            foreach (Animator candidate in childAnimators)
            {
                if (candidate == null ||
                    !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!HasAnimatorParameter(
                    candidate,
                    liftBoolName,
                    AnimatorControllerParameterType.Bool
                ))
                {
                    continue;
                }

                playerAnimator = candidate;

                SpriteRenderer candidateRenderer =
                    candidate.GetComponent<SpriteRenderer>();

                if (candidateRenderer != null)
                {
                    playerSpriteRenderer =
                        candidateRenderer;
                }

                animatorErrorWasLogged = false;

                if (isHoldingItem)
                {
                    playerAnimator.SetBool(
                        liftBoolName,
                        true
                    );
                }

                return;
            }
        }

        private void ReadDirectionFromAnimator()
        {
            if (playerAnimator == null)
            {
                return;
            }

            if (!HasAnimatorParameter(
                    playerAnimator,
                    directionXParameter,
                    AnimatorControllerParameterType.Float
                ) ||
                !HasAnimatorParameter(
                    playerAnimator,
                    directionYParameter,
                    AnimatorControllerParameterType.Float
                ))
            {
                return;
            }

            Vector2 animatorDirection =
                new Vector2(
                    playerAnimator.GetFloat(
                        directionXParameter
                    ),
                    playerAnimator.GetFloat(
                        directionYParameter
                    )
                );

            if (animatorDirection.sqrMagnitude >
                movementThreshold * movementThreshold)
            {
                lastDirection =
                    GetCardinalDirection(
                        animatorDirection
                    );
            }
        }

        private void ReadMovementInput()
        {
            movementInput =
                new Vector2(
                    Input.GetAxisRaw(horizontalAxis),
                    Input.GetAxisRaw(verticalAxis)
                );

            float thresholdSqr =
                movementThreshold *
                movementThreshold;

            // Khi đứng yên vẫn giữ lại hướng cuối cùng.
            if (movementInput.sqrMagnitude <=
                thresholdSqr)
            {
                movementInput = Vector2.zero;
                return;
            }

            lastDirection =
                GetCardinalDirection(
                    movementInput
                );
        }

        private Vector2 GetCardinalDirection(
            Vector2 direction
        )
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.down;
            }

            if (Mathf.Abs(direction.x) >=
                Mathf.Abs(direction.y))
            {
                if (direction.x >= 0f)
                {
                    return Vector2.right;
                }

                return Vector2.left;
            }

            if (direction.y >= 0f)
            {
                return Vector2.up;
            }

            return Vector2.down;
        }

        private void UpdateAnimatorMovementParameters()
        {
            FindCorrectAnimator();

            if (playerAnimator == null ||
                !updateMovementParameters)
            {
                return;
            }

            if (HasAnimatorParameter(
                playerAnimator,
                directionXParameter,
                AnimatorControllerParameterType.Float
            ))
            {
                playerAnimator.SetFloat(
                    directionXParameter,
                    lastDirection.x
                );
            }

            if (HasAnimatorParameter(
                playerAnimator,
                directionYParameter,
                AnimatorControllerParameterType.Float
            ))
            {
                playerAnimator.SetFloat(
                    directionYParameter,
                    lastDirection.y
                );
            }

            if (HasAnimatorParameter(
                playerAnimator,
                speedParameter,
                AnimatorControllerParameterType.Float
            ))
            {
                float currentSpeed =
                    movementInput == Vector2.zero
                        ? 0f
                        : 1f;

                playerAnimator.SetFloat(
                    speedParameter,
                    currentSpeed
                );
            }
        }

        private void UpdatePickupDirectionFlip()
        {
            if (!isHoldingItem ||
                playerSpriteRenderer == null)
            {
                return;
            }

            // Hướng trái sử dụng PickUp3
            // nhưng lật Sprite theo chiều ngang.
            if (lastDirection.x < -0.5f)
            {
                playerSpriteRenderer.flipX =
                    pickUp3FacesRight;

                return;
            }

            // Hướng phải dùng PickUp3 nguyên bản.
            if (lastDirection.x > 0.5f)
            {
                playerSpriteRenderer.flipX =
                    !pickUp3FacesRight;

                return;
            }

            // PickUp1 và PickUp2 không lật.
            playerSpriteRenderer.flipX = false;
        }

        public void ToggleHoldingItem()
        {
            if (isHoldingItem)
            {
                StopHoldingItem();
            }
            else
            {
                StartHoldingItem();
            }
        }

        public void StartHoldingItem()
        {
            if (isHoldingItem)
            {
                return;
            }

            FindCorrectAnimator();

            if (!ValidateReferences())
            {
                return;
            }

            Item selectedItem =
                GetCurrentlySelectedItem();

            if (selectedItem == null)
            {
                Debug.Log(
                    "Ô Toolbar đang chọn không có Item.",
                    this
                );

                return;
            }

            if (selectedItem.image == null)
            {
                Debug.LogWarning(
                    selectedItem.name +
                    " chưa có Sprite trong trường Image.",
                    selectedItem
                );

                return;
            }

            isHoldingItem = true;
            waitingToRevealItem = true;
            currentlyDisplayedItem = null;

            UpdatePickupDirectionFlip();
            SetHoldingAnimation(true);
            HideNormalHeldItem();

            // Trong lúc animation nâng đang chạy,
            // không cho hình ảnh item xuất hiện.
            HideLiftedItemVisual();

            if (revealItemCoroutine != null)
            {
                StopCoroutine(revealItemCoroutine);
            }

            revealItemCoroutine = StartCoroutine(
                RevealItemAfterAnimation()
            );
        }

        private IEnumerator RevealItemAfterAnimation()
        {
            if (itemRevealDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(
                    itemRevealDelay
                );
            }
            else
            {
                yield return null;
            }

            if (!isHoldingItem)
            {
                yield break;
            }

            waitingToRevealItem = false;
            revealItemCoroutine = null;

            // Lấy item đang chọn tại thời điểm
            // animation kết thúc.
            RefreshLiftedItemFromToolbar(true);
        }

        private Item GetCurrentlySelectedItem()
        {
            if (playerInventory == null)
            {
                return null;
            }

            InventoryItem selectedInventoryItem =
                playerInventory
                    .CurrentlySelectedInventoryItem;

            if (selectedInventoryItem == null ||
                selectedInventoryItem.Item == null ||
                selectedInventoryItem.Amount <= 0)
            {
                return null;
            }

            return selectedInventoryItem.Item;
        }

        private void RefreshLiftedItemFromToolbar(
            bool forceRefresh
        )
        {
            if (!isHoldingItem ||
                waitingToRevealItem ||
                liftedItemRenderer == null)
            {
                return;
            }

            Item selectedItem =
                GetCurrentlySelectedItem();

            // Nếu chuyển sang ô trống,
            // tạm thời ẩn item trên đầu.
            if (selectedItem == null ||
                selectedItem.image == null)
            {
                currentlyDisplayedItem = null;
                HideLiftedItemVisual();
                return;
            }

            // Không cần cập nhật lại nếu vẫn là item cũ.
            if (!forceRefresh &&
                currentlyDisplayedItem ==
                selectedItem &&
                liftedItemRenderer.enabled &&
                liftedItemRenderer.gameObject.activeSelf)
            {
                return;
            }

            currentlyDisplayedItem =
                selectedItem;

            liftedItemRenderer.gameObject.SetActive(
                true
            );

            liftedItemRenderer.enabled = true;
            liftedItemRenderer.sprite =
                selectedItem.image;

            Transform itemTransform =
                liftedItemRenderer.transform;

            // Luôn giữ LiftedItemVisual trong Player,
            // không đặt trong Visual bị lật.
            itemTransform.SetParent(
                itemVisualOriginalParent,
                true
            );

            itemTransform.position =
                liftItemPoint.position;

            if (keepItemWorldRotation)
            {
                itemTransform.rotation =
                    Quaternion.identity;
            }

            ApplyFixedItemScale();
        }

        private bool ValidateReferences()
        {
            if (playerInventory == null)
            {
                Debug.LogError(
                    "PlayerLiftSelectedItem: " +
                    "Chưa gán PlayerInventory.",
                    this
                );

                return false;
            }

            if (handItemPoint == null)
            {
                Debug.LogError(
                    "PlayerLiftSelectedItem: " +
                    "Chưa gán HandItemPoint.",
                    this
                );

                return false;
            }

            if (liftItemPoint == null)
            {
                Debug.LogError(
                    "PlayerLiftSelectedItem: " +
                    "Chưa gán LiftItemPoint.",
                    this
                );

                return false;
            }

            if (liftedItemRenderer == null)
            {
                Debug.LogError(
                    "PlayerLiftSelectedItem: " +
                    "Chưa gán LiftedItemRenderer.",
                    this
                );

                return false;
            }

            return true;
        }

        private void HideNormalHeldItem()
        {
            if (normalHeldItemVisual == null)
            {
                return;
            }

            if (liftedItemRenderer != null &&
                normalHeldItemVisual ==
                liftedItemRenderer.gameObject)
            {
                Debug.LogWarning(
                    "Normal Held Item Visual không được " +
                    "là LiftedItemVisual.",
                    this
                );

                return;
            }

            normalHeldItemWasActive =
                normalHeldItemVisual.activeSelf;

            normalHeldItemVisual.SetActive(false);
        }

        private void HideLiftedItemVisual()
        {
            if (liftedItemRenderer == null)
            {
                return;
            }

            liftedItemRenderer.sprite = null;
            liftedItemRenderer.enabled = false;

            if (itemVisualOriginalParent != null)
            {
                liftedItemRenderer.transform.SetParent(
                    itemVisualOriginalParent,
                    false
                );
            }

            liftedItemRenderer.gameObject.SetActive(
                false
            );
        }

        private void ApplyFixedItemScale()
        {
            if (liftedItemRenderer == null ||
                liftedItemRenderer.sprite == null)
            {
                return;
            }

            Transform itemTransform =
                liftedItemRenderer.transform;

            Vector3 spriteSize =
                liftedItemRenderer.sprite.bounds.size;

            float largestSpriteSide =
                Mathf.Max(
                    spriteSize.x,
                    spriteSize.y
                );

            if (largestSpriteSide <= 0.0001f)
            {
                return;
            }

            float parentWorldScale = 1f;

            if (itemTransform.parent != null)
            {
                Vector3 parentScale =
                    itemTransform.parent.lossyScale;

                parentWorldScale =
                    Mathf.Max(
                        Mathf.Abs(parentScale.x),
                        Mathf.Abs(parentScale.y)
                    );

                if (parentWorldScale <= 0.0001f)
                {
                    parentWorldScale = 1f;
                }
            }

            float calculatedScale =
                fixedItemSize /
                (largestSpriteSide *
                 parentWorldScale);

            itemTransform.localScale =
                new Vector3(
                    calculatedScale,
                    calculatedScale,
                    1f
                );
        }

        public void StopHoldingItem()
        {
            if (!isHoldingItem)
            {
                return;
            }

            if (revealItemCoroutine != null)
            {
                StopCoroutine(revealItemCoroutine);
                revealItemCoroutine = null;
            }

            isHoldingItem = false;
            waitingToRevealItem = false;
            currentlyDisplayedItem = null;

            SetHoldingAnimation(false);
            HideLiftedItemVisual();
            RestoreNormalHeldItem();
        }

        private void RestoreNormalHeldItem()
        {
            if (normalHeldItemVisual == null)
            {
                return;
            }

            if (liftedItemRenderer != null &&
                normalHeldItemVisual ==
                liftedItemRenderer.gameObject)
            {
                return;
            }

            normalHeldItemVisual.SetActive(
                normalHeldItemWasActive
            );
        }

        private void SetHoldingAnimation(bool value)
        {
            FindCorrectAnimator();

            if (playerAnimator == null)
            {
                LogAnimatorErrorOnce(
                    "Không tìm thấy Animator đang hoạt động."
                );

                return;
            }

            if (!HasAnimatorParameter(
                playerAnimator,
                liftBoolName,
                AnimatorControllerParameterType.Bool
            ))
            {
                LogAnimatorErrorOnce(
                    "Animator chưa có Bool Parameter: " +
                    liftBoolName
                );

                return;
            }

            animatorErrorWasLogged = false;

            playerAnimator.SetBool(
                liftBoolName,
                value
            );
        }

        private bool HasAnimatorParameter(
            Animator animator,
            string parameterName,
            AnimatorControllerParameterType type
        )
        {
            if (animator == null ||
                string.IsNullOrEmpty(parameterName))
            {
                return false;
            }

            foreach (
                AnimatorControllerParameter parameter
                in animator.parameters
            )
            {
                if (parameter.name == parameterName &&
                    parameter.type == type)
                {
                    return true;
                }
            }

            return false;
        }

        private void LogAnimatorErrorOnce(
            string message
        )
        {
            if (animatorErrorWasLogged)
            {
                return;
            }

            animatorErrorWasLogged = true;

            Debug.LogError(
                "PlayerLiftSelectedItem: " +
                message,
                this
            );
        }

        private void OnDisable()
        {
            if (revealItemCoroutine != null)
            {
                StopCoroutine(revealItemCoroutine);
                revealItemCoroutine = null;
            }

            if (isHoldingItem)
            {
                StopHoldingItem();
            }
        }
    }
}