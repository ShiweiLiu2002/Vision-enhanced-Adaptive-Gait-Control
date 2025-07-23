using System;
using System.Collections;
using UnityEngine;

namespace ModularAgents.TrainingEvents
{
    public class DelayedTriggerEventHandler : TrainingEventHandler
    {
        [Tooltip("延迟秒数")]
        public float delay = 0.2f;

        [Tooltip("延迟后要触发的 handler")]
        public TrainingEventHandler nextHandler;

        public override EventHandler Handler => OnTrigger;

        private void OnTrigger(object sender, EventArgs e)
        {
            StartCoroutine(DelayedTriggerCoroutine());
        }

        private IEnumerator DelayedTriggerCoroutine()
        {
            yield return new WaitForSeconds(delay);
            if (nextHandler != null && nextHandler.Handler != null)
            {
                nextHandler.Handler.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
