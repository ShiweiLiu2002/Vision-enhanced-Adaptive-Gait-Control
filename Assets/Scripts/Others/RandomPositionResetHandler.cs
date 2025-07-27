using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularAgents.TrainingEvents
{
    public class RandomPositionResetHandler : TrainingEventHandler
    {
        [SerializeField]
        private GameObject targetGameObject;

        [Tooltip("List of local positions to randomly select from.")]
        [SerializeField]
        private List<Vector3> localPositionList;

        public override EventHandler Handler => ResetPosition;

        private void Awake()
        {
            if (!targetGameObject)
                targetGameObject = gameObject;

            if (localPositionList == null || localPositionList.Count == 0)
                Debug.LogWarning($"[RandomPositionResetHandler] No positions assigned for {gameObject.name}");
        }

        private void ResetPosition(object sender, EventArgs e)
        {
            if (targetGameObject == null || localPositionList == null || localPositionList.Count == 0)
                return;

            // Choose a random position from the list
            Vector3 chosenLocalPosition = localPositionList[UnityEngine.Random.Range(0, localPositionList.Count)];

            // Apply the local position relative to the parent
            targetGameObject.transform.position = chosenLocalPosition;
        }
    }
}
