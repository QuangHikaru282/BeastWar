using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kinnly
{
    public class WoodCuttingUI : MonoBehaviour
    {
        [Header("Bang chinh")]
        [SerializeField]
        private GameObject woodCuttingPanel;

        [Header("Thanh canh thoi diem")]
        [SerializeField]
        private RectTransform indicator;

        [SerializeField]
        private float baseIndicatorSpeed = 0.8f;

        [Header("Animation Object")]
        [SerializeField]
        private ReusableActionAnimator playerChopAnimation;

        [SerializeField]
        private ReusableActionAnimator treeHitAnimation;

        [Header("Text")]
        [SerializeField]
        private TMP_Text resourceText;

        [SerializeField]
        private TMP_Text resultText;

        [Header("Button")]
        [SerializeField]
        private Button chopButton;

        [Header("Tai nguyen co the nhan")]
        [SerializeField]
        private Transform possibleResourceContainer;

        [SerializeField]
        private ResourceSlotUI resourceSlotPrefab;

        [Header("Tai nguyen da thu hoach")]
        [SerializeField]
        private Transform harvestedResourceContainer;

        [Header("Cong cu")]
        [SerializeField]
        private Transform toolContainer;

        [SerializeField]
        private ToolSlotUI toolSlotPrefab;

        [Tooltip("Keo truc tiep Item riu vao danh sach")]
        [SerializeField]
        private List<Item> allTools = new List<Item>();

        [Header("Inventory cua Player")]
        [SerializeField]
        private PlayerInventory playerInventory;

        // List cho phép các phần thưởng trùng nhau
        // vẫn xuất hiện thành nhiều ô riêng.
        private readonly List<LootResult> sessionRewards =
            new List<LootResult>();

        private readonly List<ToolSlotUI> createdToolSlots =
            new List<ToolSlotUI>();

        private TreeResource currentTree;
        private Item selectedTool;

        private float indicatorPosition;
        private float moveDirection = 1f;

        private bool indicatorIsMoving;
        private bool waitingForAnimation;

        private void Start()
        {
            if (woodCuttingPanel != null)
            {
                woodCuttingPanel.SetActive(false);
            }

            indicatorPosition = 0f;
            SetIndicatorPosition();
        }

        private void Update()
        {
            if (woodCuttingPanel == null ||
                !woodCuttingPanel.activeSelf)
            {
                return;
            }

            if (indicatorIsMoving)
            {
                MoveIndicator();

                if (Input.GetKeyDown(KeyCode.F))
                {
                    PressChopButton();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePanel();
            }
        }

        // =========================
        // INDICATOR
        // =========================

        private void MoveIndicator()
        {
            float speedMultiplier = 1f;

            if (selectedTool != null)
            {
                speedMultiplier =
                    selectedTool.axeIndicatorSpeedMultiplier;
            }

            indicatorPosition +=
                moveDirection *
                baseIndicatorSpeed *
                speedMultiplier *
                Time.unscaledDeltaTime;

            if (indicatorPosition >= 1f)
            {
                indicatorPosition = 1f;
                moveDirection = -1f;
            }
            else if (indicatorPosition <= 0f)
            {
                indicatorPosition = 0f;
                moveDirection = 1f;
            }

            SetIndicatorPosition();
        }

        private void SetIndicatorPosition()
        {
            if (indicator == null)
            {
                return;
            }

            Vector2 newAnchor = new Vector2(
                indicatorPosition,
                0.5f
            );

            indicator.anchorMin = newAnchor;
            indicator.anchorMax = newAnchor;
            indicator.anchoredPosition = Vector2.zero;
        }

        private bool IsGreenZone()
        {
            return indicatorPosition >= 0.4f &&
                   indicatorPosition <= 0.6f;
        }

        private bool IsYellowZone()
        {
            bool yellowLeft =
                indicatorPosition >= 0.2f &&
                indicatorPosition < 0.4f;

            bool yellowRight =
                indicatorPosition > 0.6f &&
                indicatorPosition <= 0.8f;

            return yellowLeft || yellowRight;
        }

        // =========================
        // MO BANG CHAT GO
        // =========================

        public void OpenPanel(TreeResource tree)
        {
            if (tree == null)
            {
                Debug.LogError(
                    "WoodCuttingUI: TreeResource đang null."
                );

                return;
            }

            if (woodCuttingPanel == null)
            {
                Debug.LogError(
                    "WoodCuttingUI: Chưa gán WoodCuttingPanel."
                );

                return;
            }

            StopAllCoroutines();

            currentTree = tree;
            sessionRewards.Clear();

            woodCuttingPanel.SetActive(true);

            indicatorPosition = 0f;
            moveDirection = 1f;

            indicatorIsMoving = false;
            waitingForAnimation = false;

            SetIndicatorPosition();

            BuildPossibleResourceUI();
            BuildToolUI();
            RefreshHarvestedUI();
            UpdateResourceText();

            if (!currentTree.HasWood())
            {
                SetResultText(
                    "Cây đã hết tài nguyên!"
                );

                SetChopButtonInteractable(false);
                return;
            }

            if (selectedTool == null)
            {
                SetResultText(
                    "Bạn không có rìu để chặt cây."
                );

                SetChopButtonInteractable(false);
                return;
            }

            indicatorIsMoving = true;
            SetChopButtonInteractable(true);

            SetResultText(
                "Đang dùng " +
                selectedTool.name +
                ". Nhấn F hoặc nút Chặt."
            );
        }

        // =========================
        // TAI NGUYEN CO THE NHAN
        // =========================

        private void BuildPossibleResourceUI()
        {
            if (possibleResourceContainer == null)
            {
                Debug.LogError(
                    "Chưa gán PossibleResourceContainer."
                );

                return;
            }

            if (resourceSlotPrefab == null)
            {
                Debug.LogError(
                    "Chưa gán ResourceSlotPrefab."
                );

                return;
            }

            if (currentTree == null)
            {
                return;
            }

            ClearContainer(possibleResourceContainer);

            foreach (TreeDrop drop in currentTree.PossibleDrops)
            {
                if (drop == null ||
                    drop.item == null)
                {
                    Debug.LogWarning(
                        "Một TreeDrop chưa được gán Item."
                    );

                    continue;
                }

                ResourceSlotUI newSlot = Instantiate(
                    resourceSlotPrefab,
                    possibleResourceContainer
                );

                newSlot.Setup(
                    drop.item,
                    0,
                    false,
                    drop.rarity
                );
            }
        }

        // =========================
        // CONG CU
        // =========================

        private void BuildToolUI()
        {
            selectedTool = null;
            createdToolSlots.Clear();

            if (toolContainer == null)
            {
                Debug.LogError(
                    "Chưa gán ToolContainer."
                );

                return;
            }

            if (toolSlotPrefab == null)
            {
                Debug.LogError(
                    "Chưa gán ToolSlotPrefab."
                );

                return;
            }

            if (playerInventory == null)
            {
                Debug.LogError(
                    "Chưa gán PlayerInventory."
                );

                return;
            }

            ClearContainer(toolContainer);

            foreach (Item toolItem in allTools)
            {
                if (toolItem == null)
                {
                    Debug.LogWarning(
                        "All Tools có phần tử None."
                    );

                    continue;
                }

                if (!toolItem.isTools ||
                    !toolItem.isAxe)
                {
                    Debug.LogWarning(
                        toolItem.name +
                        " chưa bật Is Tools hoặc Is Axe."
                    );

                    continue;
                }

                if (!PlayerOwnsTool(toolItem))
                {
                    Debug.LogWarning(
                        "Player chưa có rìu: " +
                        toolItem.name
                    );

                    continue;
                }

                ToolSlotUI newToolSlot = Instantiate(
                    toolSlotPrefab,
                    toolContainer
                );

                newToolSlot.Setup(
                    toolItem,
                    SelectTool
                );

                createdToolSlots.Add(newToolSlot);

                if (selectedTool == null)
                {
                    selectedTool = toolItem;
                }
            }

            RefreshToolSelection();
        }

        private bool PlayerOwnsTool(Item toolItem)
        {
            if (playerInventory == null ||
                toolItem == null)
            {
                return false;
            }

            foreach (
                GameObject inventorySlot
                in playerInventory.InventorySlots
            )
            {
                if (inventorySlot == null)
                {
                    continue;
                }

                InventoryItem inventoryItem =
                    inventorySlot.GetComponentInChildren
                    <InventoryItem>(true);

                if (inventoryItem == null ||
                    inventoryItem.Item == null ||
                    inventoryItem.Amount <= 0)
                {
                    continue;
                }

                bool sameReference =
                    inventoryItem.Item == toolItem;

                bool sameName =
                    inventoryItem.Item.name ==
                    toolItem.name;

                if (sameReference || sameName)
                {
                    return true;
                }
            }

            return false;
        }

        private void SelectTool(Item toolItem)
        {
            if (toolItem == null)
            {
                return;
            }

            if (waitingForAnimation)
            {
                return;
            }

            selectedTool = toolItem;

            RefreshToolSelection();

            if (currentTree != null &&
                currentTree.HasWood())
            {
                indicatorIsMoving = true;
                SetChopButtonInteractable(true);
            }

            SetResultText(
                "Đã chọn: " +
                selectedTool.name
            );
        }

        private void RefreshToolSelection()
        {
            foreach (
                ToolSlotUI toolSlot
                in createdToolSlots
            )
            {
                if (toolSlot == null)
                {
                    continue;
                }

                bool selected =
                    toolSlot.GetTool() == selectedTool;

                toolSlot.SetSelected(selected);
            }
        }

        // =========================
        // BAM F HOAC NUT CHAT
        // =========================

        public void PressChopButton()
        {
            if (currentTree == null)
            {
                SetResultText(
                    "Không tìm thấy cây."
                );

                return;
            }

            if (selectedTool == null)
            {
                SetResultText(
                    "Bạn chưa chọn rìu."
                );

                return;
            }

            if (!currentTree.HasWood())
            {
                SetResultText(
                    "Cây đã hết tài nguyên."
                );

                return;
            }

            if (!indicatorIsMoving ||
                waitingForAnimation)
            {
                return;
            }

            // Dừng Indicator ngay khi bấm.
            indicatorIsMoving = false;
            waitingForAnimation = true;

            SetChopButtonInteractable(false);

            // Hai object chạy Animation cùng lúc.
            PlayChopAnimations();

            // Xác định vị trí vừa bấm.
            if (IsGreenZone())
            {
                HarvestTree(
                    true,
                    "HOÀN HẢO!"
                );
            }
            else if (IsYellowZone())
            {
                HarvestTree(
                    false,
                    "THÀNH CÔNG!"
                );
            }
            else
            {
                // Trúng đỏ không mất tài nguyên.
                SetResultText(
                    "CHẶT HỤT!"
                );
            }

            UpdateResourceText();

            // Chờ Animation kết thúc.
            StartCoroutine(
                WaitForChopAnimation()
            );
        }

        // =========================
        // ANIMATION
        // =========================

        private void PlayChopAnimations()
        {
            if (playerChopAnimation != null)
            {
                playerChopAnimation.Play();
            }

            if (treeHitAnimation != null)
            {
                treeHitAnimation.Play();
            }
        }

        private float GetLongestAnimationDuration()
        {
            float playerDuration = 0f;
            float treeDuration = 0f;

            if (playerChopAnimation != null)
            {
                playerDuration =
                    playerChopAnimation.AnimationDuration;
            }

            if (treeHitAnimation != null)
            {
                treeDuration =
                    treeHitAnimation.AnimationDuration;
            }

            return Mathf.Max(
                playerDuration,
                treeDuration
            );
        }

        private IEnumerator WaitForChopAnimation()
        {
            float waitTime =
                GetLongestAnimationDuration();

            if (waitTime <= 0f)
            {
                waitTime = 0.1f;
            }

            yield return new WaitForSecondsRealtime(
                waitTime
            );

            if (currentTree == null)
            {
                yield break;
            }

            waitingForAnimation = false;

            if (currentTree.HasWood())
            {
                indicatorIsMoving = true;
                SetChopButtonInteractable(true);

                SetResultText(
                    "Nhấn F hoặc nút Chặt"
                );
            }
            else
            {
                indicatorIsMoving = false;
                SetChopButtonInteractable(false);

                SetResultText(
                    "Cây đã hết tài nguyên!"
                );
            }
        }

        // =========================
        // NHAN PHAN THUONG
        // =========================

        private void HarvestTree(
            bool hitGreen,
            string resultTitle
        )
        {
            if (currentTree == null ||
                playerInventory == null)
            {
                return;
            }

            float rareBonus = 0f;

            if (selectedTool != null)
            {
                rareBonus =
                    selectedTool.axeRareDropBonus;
            }

            List<LootResult> results =
                currentTree.Harvest(
                    hitGreen,
                    rareBonus
                );

            if (results == null ||
                results.Count == 0)
            {
                SetResultText(
                    resultTitle +
                    " Không nhận được vật phẩm."
                );

                return;
            }

            StringBuilder message =
                new StringBuilder();

            message.Append(resultTitle);
            message.Append(" Nhận được: ");

            for (int i = 0;
                 i < results.Count;
                 i++)
            {
                LootResult loot = results[i];

                if (loot == null ||
                    loot.item == null ||
                    loot.amount <= 0)
                {
                    continue;
                }

                // Thêm vật phẩm vào Inventory cũ.
                playerInventory.AddItem(
                    loot.item,
                    loot.amount
                );

                // Mỗi lần nhận tạo một ô riêng.
                // Không gộp vật phẩm trùng nhau.
                sessionRewards.Add(
                    new LootResult(
                        loot.item,
                        loot.amount,
                        loot.rarity
                    )
                );

                message.Append(loot.item.name);
                message.Append(" x");
                message.Append(loot.amount);

                if (i < results.Count - 1)
                {
                    message.Append(", ");
                }
            }

            SetResultText(
                message.ToString()
            );

            RefreshHarvestedUI();
        }

        private void RefreshHarvestedUI()
        {
            if (harvestedResourceContainer == null)
            {
                Debug.LogError(
                    "Chưa gán HarvestedResourceContainer."
                );

                return;
            }

            if (resourceSlotPrefab == null)
            {
                Debug.LogError(
                    "Chưa gán ResourceSlotPrefab."
                );

                return;
            }

            ClearContainer(
                harvestedResourceContainer
            );

            foreach (
                LootResult reward
                in sessionRewards
            )
            {
                if (reward == null ||
                    reward.item == null)
                {
                    continue;
                }

                ResourceSlotUI newSlot = Instantiate(
                    resourceSlotPrefab,
                    harvestedResourceContainer
                );

                newSlot.Setup(
                    reward.item,
                    reward.amount,
                    true,
                    reward.rarity
                );
            }
        }

        // =========================
        // TEXT VA BUTTON
        // =========================

        private void UpdateResourceText()
        {
            if (resourceText == null ||
                currentTree == null)
            {
                return;
            }

            resourceText.text =
                "Tài nguyên còn lại: " +
                currentTree.CurrentWood +
                "/" +
                currentTree.MaxWood;
        }

        private void SetResultText(string message)
        {
            if (resultText != null)
            {
                resultText.text = message;
            }
        }

        private void SetChopButtonInteractable(
            bool interactable
        )
        {
            if (chopButton != null)
            {
                chopButton.interactable =
                    interactable;
            }
        }

        // =========================
        // XOA CAC SLOT CU
        // =========================

        private void ClearContainer(
            Transform container
        )
        {
            if (container == null)
            {
                return;
            }

            for (
                int i = container.childCount - 1;
                i >= 0;
                i--
            )
            {
                GameObject child =
                    container.GetChild(i).gameObject;

                child.SetActive(false);
                Destroy(child);
            }
        }

        // =========================
        // DONG BANG
        // =========================

        public void ClosePanel()
        {
            StopAllCoroutines();

            indicatorIsMoving = false;
            waitingForAnimation = false;

            currentTree = null;
            selectedTool = null;

            if (woodCuttingPanel != null)
            {
                woodCuttingPanel.SetActive(false);
            }
        }
    }
}