using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kinnly
{
    public class PlayerTools : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] Animator playerAnimator;

        [Header("Config")]
        [SerializeField] int damage;

        [Header("Direction")]
        [SerializeField] Vector2 offsetUp;
        [SerializeField] Vector2 offsetDown;
        [SerializeField] Vector2 offsetLeft;
        [SerializeField] Vector2 offsetRight;

        //If isMouseControl Active, collider will be based on MousePosition if it's near the player.
        [Header("Mouse Control")]
        [SerializeField] bool isMouseControl;
        [SerializeField] Vector2 mouseControlSize;
        [SerializeField] Vector2 mouseControlOffset;

        [Header("Outline")]
        [SerializeField] GameObject outline;

        GameObject player;
        PlayerMovement playerMovement;
        PlayerInventory playerInventory;

        GameObject insideTrigger;
        Item currentlySelectedItem;

        bool isMouse;
        bool isUsingTools;

        // Start is called before the first frame update
        private void Start()
        {
            player = Player.Instance.gameObject;
            playerInventory = Player.Instance.gameObject.GetComponent<PlayerInventory>();
            playerMovement = Player.Instance.gameObject.GetComponent<PlayerMovement>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Farming Debug: BẠN VỪA BẤM CHUỘT TRÁI! Script PlayerTools ĐANG CHẠY.");
            }

            Direction();
            MouseControl();
            GetCurrentlySelectedItem();
            OnDrawOutline();

            if (Input.GetMouseButton(0))
            {
                if (playerMovement == null || playerMovement.isUsingTools == false)
                {
                    UseTools();
                }
                else
                {
                    if (Input.GetMouseButtonDown(0)) Debug.Log("Lỗi: isUsingTools đang bị kẹt ở trạng thái TRUE nên không thể vung cuốc!");
                }
            }

            if (playerMovement != null && playerMovement.isUsingTools == false)
            {
                isUsingTools = false;
            }
        }

        void Direction()
        {
            if (isMouse || playerMovement == null)
            {
                return;
            }

            Vector2 _direction;
            _direction = playerMovement.direction;

            if (_direction == Vector2.up)
            {
                transform.localPosition = new Vector2(offsetUp.x, _direction.y * offsetUp.y);
            }
            else if (_direction == Vector2.down)
            {
                transform.localPosition = new Vector2(offsetDown.x, _direction.y * offsetDown.y);
            }
            else if (_direction == Vector2.right)
            {
                transform.localPosition = new Vector2(_direction.x * offsetRight.x, offsetRight.y);
            }
            else if (_direction == Vector2.left)
            {
                transform.localPosition = new Vector2(_direction.x * offsetLeft.x, offsetLeft.y);
            }
        }

        void MouseControl()
        {
            if (isMouseControl == false)
            {
                return;
            }

            Vector3 _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 _playerPosition = player.transform.position + new Vector3(mouseControlOffset.x, mouseControlOffset.y, 0f);
            _mousePosition.z = 0;

            float _distanceX = _mousePosition.x - _playerPosition.x;
            float _distanceY = _mousePosition.y - _playerPosition.y;
            Vector2 _distance = new Vector2(_distanceX, _distanceY);

            if (Mathf.Abs(_distance.x) < mouseControlSize.x + mouseControlOffset.x && Mathf.Abs(_distance.y) < mouseControlSize.y + mouseControlOffset.y)
            {
                isMouse = true;
                transform.position = new Vector3(Mathf.Round(_mousePosition.x), Mathf.Round(_mousePosition.y));
            }
            else
            {
                isMouse = false;
            }
        }

        private void GetCurrentlySelectedItem()
        {
            try
            {
                currentlySelectedItem = playerInventory.CurrentlySelectedInventoryItem.Item;
            }
            catch
            {
                currentlySelectedItem = null;
            }
        }

        private void UseTools()
        {
            if (currentlySelectedItem == null)
            {
                Debug.Log("Lỗi 1: Không có Item nào đang được chọn trên tay.");
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Lỗi 2: Chuột đang đè lên giao diện UI (Túi đồ, Nút bấm...), nên bị chặn click.");
                return;
            }

            // --- FARMING BRIDGE ---
            if (currentlySelectedItem.farmingItemDelegate != null)
            {
                var terrainManager = BeastBall.Farming.FarmingTerrainManager.Instance;
                if (terrainManager != null)
                {
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPos.z = 0;
                    Vector3Int cellPos = terrainManager.Grid.WorldToCell(mouseWorldPos);
                    Debug.Log($"Farming Debug: Đang click chuột tại {mouseWorldPos}, đổi thành ô Lưới (Grid): {cellPos}");
                    
                    if (currentlySelectedItem.farmingItemDelegate.CanUse(cellPos))
                    {
                        Debug.Log("Farming Debug: Hàm CanUse trả về TRUE. Đang tiến hành thực hiện hành động Farming!");
                        // Play swing animation if it's considered a tool
                        if (currentlySelectedItem.isTools)
                        {
                            if (playerAnimator != null) playerAnimator.SetTrigger("Attack");
                            if (playerMovement != null) playerMovement.SetDirection(transform.localPosition);
                        }
                        
                        // Execute Farming action (Till, Water, Plant)
                        currentlySelectedItem.farmingItemDelegate.Use(cellPos);
                        
                        // Consume logic for Seeds/Consumables
                        if (currentlySelectedItem.farmingItemDelegate.Consumable)
                        {
                            playerInventory.RemoveItem(playerInventory.CurrentlySelectedInventoryItem, 1);
                        }
                        
                        if (playerMovement != null) playerMovement.isUsingTools = true;
                        return; // Stop default Kinnly tool logic to prevent conflicts
                    }
                    else 
                    {
                        var tile = terrainManager.GroundTilemap.GetTile(cellPos);
                        string tileName = tile != null ? tile.name : "NULL (Đất trống)";
                        string requiredTileName = terrainManager.TilleableTile != null ? terrainManager.TilleableTile.name : "CHƯA GÁN";
                        Debug.Log($"Farming Debug: Hàm CanUse = FALSE tại {cellPos}.\\nTile hiện tại ở ô này là: [{tileName}].\\nTile bắt buộc phải có là: [{requiredTileName}].");
                    }
                }
                else 
                {
                    Debug.Log("Lỗi 3: Không tìm thấy FarmingTerrainManager.Instance trong Scene. Bạn đã tạo nó chưa?");
                }
            }
            else 
            {
                Debug.Log($"Lỗi 4: Item tên '{currentlySelectedItem.name}' không có Farming Item Delegate (bị bỏ trống).");
            }
            // ----------------------

            if (currentlySelectedItem.isTools)
            {
                if (playerAnimator != null) playerAnimator.SetTrigger("Attack");
                if (playerMovement != null) playerMovement.SetDirection(transform.localPosition);
            }
        }

        public void DamageInsideTrigger()
        {
            if (isUsingTools)
            {
                return;
            }

            try
            {
                insideTrigger.GetComponent<IDamageable>().Damage(playerInventory, damage);
                isUsingTools = true;
            }
            catch
            {
                insideTrigger = null;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            insideTrigger = null;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            insideTrigger = collision.gameObject;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            insideTrigger = null;
        }

        private void OnDrawOutline()
        {
            if (outline == null) return;

            if (insideTrigger != null)
            {
                outline.SetActive(true);
                outline.transform.position = insideTrigger.transform.position;
            }
            else
            {
                outline.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (isMouseControl == false)
            {
                return;
            }

            player = GetComponentInParent<Player>().gameObject;

            Vector3 _playerPosition = player.transform.position + new Vector3(mouseControlOffset.x, mouseControlOffset.y, 0f);
            Vector3 _size = new Vector3(mouseControlSize.x * 2, mouseControlSize.y * 2, 1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_playerPosition, _size);
        }
    }
}