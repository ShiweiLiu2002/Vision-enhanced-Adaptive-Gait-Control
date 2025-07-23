using System;
using UnityEngine;

namespace ModularAgents.TrainingEvents
{
    public class AnimatorResetHandler : TrainingEventHandler
    {
        [SerializeField]
        private Animator animator;

        public override EventHandler Handler => ResetAnimator;

        private void Awake()
        {
            if (!animator)
                animator = GetComponent<Animator>();

            if (!animator)
                Debug.LogWarning($"[AnimatorResetHandler] Animator not found on {gameObject.name}");
        }

        private void ResetAnimator(object sender, EventArgs e)
        {
            if (!animator) return;
            animator.Play(0, 0, 0f);

            animator.Update(0f);

            animator.applyRootMotion = true;
        }
    }
}
