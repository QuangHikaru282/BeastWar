using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kinnly
{
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        public Item Item;
        public int Amount;

        private Transform playerTransform;
        private PlayerInventory playerInv;
        private bool canMagnetize = false;
        private bool isMagnetizing = false;
        public float magnetRadius = 2.5f;
        public float magnetSpeed = 6f;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerInv = player.GetComponent<PlayerInventory>();
            }
            // Đợi 0.8 giây cho hiệu ứng rớt đồ hoàn tất rồi mới cho phép hút
            Invoke(nameof(EnableMagnet), 0.8f);
        }

        private void EnableMagnet()
        {
            canMagnetize = true;
        }

        private void Update()
        {
            if (!canMagnetize || playerTransform == null || playerInv == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            
            // Nếu đứng gần thì bắt đầu hút
            if (distance < magnetRadius)
            {
                isMagnetizing = true;
            }

            // Hút vật phẩm về phía người chơi
            if (isMagnetizing)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
                
                // Nếu đã chạm vào người chơi thì nhặt
                if (distance < 0.3f)
                {
                    Interact(playerInv);
                    canMagnetize = false; // Ngăn chặn nhặt nhiều lần
                }
            }
        }

        public void Interact(PlayerInventory playerInventory)
        {
            playerInventory.AddItem(Item, Amount);
            Destroy(this.gameObject);
        }
    }
}