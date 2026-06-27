using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterController : MonoBehaviour
    {
        public float speed;

        public Animator animator;

        private void Start()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }


        private void Update()
        {
            Vector2 dir = Vector2.zero;
            if (Input.GetKey(KeyCode.A))
            {
                dir.x = -1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                dir.x = 1;
            }

            if (Input.GetKey(KeyCode.W))
            {
                dir.y = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                dir.y = -1;
            }

            dir.Normalize();
            if (animator != null) 
            {
                // Cập nhật tham số cho Blend Tree mới
                animator.SetFloat("speed", dir.magnitude);
                if (dir != Vector2.zero)
                {
                    animator.SetFloat("dirX", dir.x);
                    animator.SetFloat("dirY", dir.y);
                }

                // Lật ảnh khi sang trái
                SpriteRenderer sr = animator.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (dir.x < 0) sr.flipX = true;
                    else if (dir.x > 0) sr.flipX = false;
                }
            }

            GetComponent<Rigidbody2D>().linearVelocity = speed * dir;
        }
    }
}
