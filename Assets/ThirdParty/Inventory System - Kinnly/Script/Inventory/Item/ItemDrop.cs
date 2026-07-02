using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Kinnly
{
    public class ItemDrop : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Item item;
        [SerializeField] int amount;

        bool isDelay;
        bool isNear;
        bool isSlotAvailable;

        private Player player;
        float speed;
        float delay;

        Vector3 startPos;
        Vector3 endPos;
        float bounceTimer;
        float bounceDuration;

        // Start is called before the first frame update
        void Start()
        {
            player = Player.Instance;

            if (item == null)
            {
                return;
            }

            speed = 10f;
            delay = 0.5f; // Thời gian chờ trên mặt đất trước khi có thể bị hút
            isDelay = true;
            spriteRenderer.sprite = item.image;

            startPos = transform.position;
            // Vị trí rớt ngẫu nhiên xung quanh
            endPos = startPos + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 0f), 0f);
            bounceTimer = 0f;
            bounceDuration = 0.4f; // Nảy trong 0.4s
        }

        // Update is called once per frame
        void Update()
        {
            if (isDelay)
            {
                TimeCountDown();
            }
            else
            {
                CheckDistance();
                if (isNear)
                {
                    CheckSlotAvailability();
                    if (isSlotAvailable)
                    {
                        MovingtoTarget();
                        AddingItem();
                    }
                }
            }
        }

        public void SetItem(Item item, int amount)
        {
            this.item = item;
            this.amount = amount;
            this.gameObject.name = item.name;
            Start();
        }

        private void TimeCountDown()
        {
            if (bounceTimer < bounceDuration)
            {
                bounceTimer += Time.deltaTime;
                float t = Mathf.Clamp01(bounceTimer / bounceDuration);
                // Hiệu ứng parabol: cao nhất ở giữa (t=0.5)
                float height = Mathf.Sin(t * Mathf.PI) * 1.0f; // Cao tối đa 1 unit
                transform.position = Vector3.Lerp(startPos, endPos, t) + new Vector3(0, height, 0);
            }

            delay -= Time.deltaTime;
            if (delay <= 0f)
            {
                isDelay = false;
            }
        }

        private void CheckDistance()
        {
            if (player == null) return;
            // Rút ngắn khoảng cách hút đồ (chỉ hút khi đứng cách 1.5 unit, thay vì 5 unit)
            if (Vector2.Distance(this.transform.position, player.transform.position) <= 1.5f)
            {
                isNear = true;
            }
        }

        private void CheckSlotAvailability()
        {
            if (player == null) return;
            PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
            if (playerInventory.IsSlotAvailable(item, amount))
            {
                isSlotAvailable = true;
            }
            else
            {
                isDelay = false;
                isNear = false;
                delay = 1f;
            }
        }

        private void MovingtoTarget()
        {
            if (player == null) return;
            Vector3 direction = player.transform.position - transform.position;
            direction.Normalize();
            transform.Translate(direction * speed * Time.deltaTime);
            speed += 20f * Time.deltaTime;
        }

        private void AddingItem()
        {
            if (player == null) return;
            if (Vector2.Distance(this.transform.position, player.transform.position) <= 0.5f)
            {
                PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
                if (playerInventory != null)
                {
                    playerInventory.AddItem(item, amount);
                    Destroy(gameObject);
                }
            }
        }
    }
}