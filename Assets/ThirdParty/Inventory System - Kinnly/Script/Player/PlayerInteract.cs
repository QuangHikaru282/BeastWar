using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Kinnly
{
    public class PlayerInteract : MonoBehaviour
    {
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
        PlayerInventory playerInventory;
        PlayerMovement playerMovement;

        GameObject insideTrigger;
        bool isMouse;
        bool isInteracting;
        float interactTime;

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
            Direction();
            MouseControl();
            OnDrawOutline();

            if (Input.GetKey(KeyCode.E) || Input.GetMouseButton(1))
            {
                if (isInteracting == false)
                {
                    Interact();
                    interactTime = 0.15f;
                }
            }

            if (interactTime > 0)
            {
                interactTime -= Time.deltaTime;
            }
            isInteracting = interactTime > 0 ? true : false;
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

        private void Interact()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // --- FARMING HARVEST BRIDGE ---
            var terrainManager = BeastBall.Farming.FarmingTerrainManager.Instance;
            if (terrainManager != null)
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;
                Vector3Int cellPos = terrainManager.Grid.WorldToCell(mouseWorldPos);

                // 1. KIỂM TRA SỬ DỤNG CÔNG CỤ (Cuốc, Bình Tưới, Hạt Giống...)
                if (playerInventory.CurrentlySelectedInventoryItem != null && 
                    playerInventory.CurrentlySelectedInventoryItem.Item != null &&
                    playerInventory.CurrentlySelectedInventoryItem.Item.farmingItemDelegate != null)
                {
                    var farmingItem = playerInventory.CurrentlySelectedInventoryItem.Item.farmingItemDelegate;
                    
                    // Kiểm tra xem công cụ này có thể dùng lên ô đất hiện tại không
                    if (farmingItem.CanUse(cellPos))
                    {
                        bool success = farmingItem.Use(cellPos);
                        if (success)
                        {
                            // Nếu là vật phẩm tiêu hao (Hạt giống), trừ số lượng đi 1
                            if (farmingItem.Consumable)
                            {
                                playerInventory.RemoveItem(playerInventory.CurrentlySelectedInventoryItem, 1);
                            }
                            
                            if (playerMovement != null) playerMovement.SetDirection(this.transform.localPosition);
                            return; // Dừng lại vì đã dùng tool thành công
                        }
                    }
                }

                // 2. NẾU KHÔNG DÙNG TOOL -> MẶC ĐỊNH LÀ THU HOẠCH TAY KHÔNG
                var cropData = terrainManager.GetCropDataAt(cellPos);
                
                // Nếu có cây trồng và đã chín (GrowthRatio = 1)
                if (cropData != null && Mathf.Approximately(cropData.GrowthRatio, 1.0f))
                {
                    var crop = terrainManager.HarvestAt(cellPos);
                    if (crop != null && crop.Produce != null)
                    {
                        // Tìm file Vỏ bọc Kinnly tương ứng trong thư mục Resources
                        Kinnly.Item droppedKinnlyItem = null;
                        var allKinnlyItems = Resources.LoadAll<Kinnly.Item>("");
                        foreach (var ki in allKinnlyItems)
                        {
                            if (ki.farmingItemDelegate == crop.Produce)
                            {
                                droppedKinnlyItem = ki;
                                break;
                            }
                        }

                        if (droppedKinnlyItem != null)
                        {
                            playerInventory.SpawnItemDrop(droppedKinnlyItem, crop.ProductPerHarvest);
                        }
                        else
                        {
                            Debug.LogWarning("Farming: Không tìm thấy file Kinnly_... nào bọc lấy " + crop.Produce.name + " trong thư mục Resources!");
                        }
                        
                        if (playerMovement != null) playerMovement.SetDirection(this.transform.localPosition);
                        return; // Đã thu hoạch xong, dừng hàm lại
                    }
                }
            }
            // ------------------------------

            if (insideTrigger == null)
            {
                return;
            }

            try
            {
                if (insideTrigger != null)
                {
                    if (insideTrigger.GetComponent<IInteractable>() != null)
                    {
                        insideTrigger.GetComponent<IInteractable>().Interact(playerInventory);
                        if (playerMovement != null) playerMovement.SetDirection(this.transform.localPosition);
                    }
                }
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