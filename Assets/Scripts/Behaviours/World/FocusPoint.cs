using System;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    [DisallowMultipleComponent]
    public class FocusPoint : MonoBehaviour
    {
        [Serializable]
        public struct Parameters
        {
            public bool hasRevealRadius;
            public float revealRadius;
            public float timeToKeepRevealingAfterRemoved;

            public Parameters(bool hasRevealRadius, float revealRadius, float timeToKeepRevealingAfterRemoved)
            {
                this.hasRevealRadius = hasRevealRadius;
                this.revealRadius = revealRadius;
                this.timeToKeepRevealingAfterRemoved = timeToKeepRevealingAfterRemoved;
            }

            public static Parameters Default => new Parameters(true, 150f, 3f);
        }

        public Parameters parameters = Parameters.Default;


        public static FocusPoint Create(GameObject targetGameObject, Parameters parameters)
        {
            var focusPoint = targetGameObject.AddComponent<FocusPoint>();
            focusPoint.parameters = parameters;
            return focusPoint;
        }

        private void Start()
        {
            if (Cell.Instance != null)
                Cell.Instance.RegisterFocusPoint(this.transform, this.parameters);
        }

        private void OnDisable()
        {
            if (Cell.Instance != null)
                Cell.Instance.UnRegisterFocusPoint(this.transform);
        }
    }
}
