using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Kinnly
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] float speed;

        [HideInInspector] public Vector2 direction;
        [HideInInspector] public bool isUsingTools;

        Rigidbody2D rb;
        public Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            Movement();
        }

        void Movement()
        {
            if (isUsingTools)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float xMovement = Input.GetAxisRaw("Horizontal");
            float yMovement = Input.GetAxisRaw("Vertical");

            Vector2 movement = new Vector2(xMovement, yMovement).normalized * speed;
            rb.linearVelocity = movement;

            if (xMovement != 0f || yMovement != 0f)
            {
                direction = new Vector2(xMovement, yMovement);
                if (animator != null)
                {
                    animator.SetFloat("Horizontal", xMovement);
                    animator.SetFloat("Vertical", yMovement);
                    animator.SetBool("Run", true);

                    // Hỗ trợ thêm tham số chuẩn cho Blend Tree mới
                    animator.SetFloat("dirX", xMovement);
                    animator.SetFloat("dirY", yMovement);
                    animator.SetFloat("speed", movement.magnitude);

                    // Lật hình ảnh nếu đi sang trái (chỉ dùng ảnh đi sang phải)
                    SpriteRenderer sr = animator.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (xMovement < 0) sr.flipX = true;
                        else if (xMovement > 0) sr.flipX = false;
                    }
                }
            }
            else
            {
                if (animator != null) 
                {
                    animator.SetBool("Run", false);
                    animator.SetFloat("speed", 0f);
                }
            }
        }

        public void SetDirection(Vector2 _direction)
        {
            direction.x = Mathf.Round(_direction.x);
            direction.y = Mathf.Round(_direction.y);
            if (animator != null)
            {
                animator.SetFloat("Horizontal", direction.x);
                animator.SetFloat("Vertical", direction.y);
            }
        }
    }
}