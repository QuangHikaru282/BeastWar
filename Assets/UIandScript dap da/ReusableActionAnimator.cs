using UnityEngine;

namespace Kinnly
{
    public class ReusableActionAnimator : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField]
        private Animator animator;

        [Header("Trigger")]
        [SerializeField]
        private string actionTrigger = "Action";

        [Header("Thoi luong Animation")]
        [Min(0.01f)]
        [SerializeField]
        private float animationDuration = 0.8f;

        public float AnimationDuration =>
            animationDuration;

        private void Awake()
        {
            if (animator == null)
            {
                animator =
                    GetComponent<Animator>();
            }
        }

        public void Play()
        {
            if (animator == null)
            {
                Debug.LogWarning(
                    gameObject.name +
                    " chưa được gán Animator."
                );

                return;
            }

            animator.ResetTrigger(actionTrigger);
            animator.SetTrigger(actionTrigger);
        }
    }
}