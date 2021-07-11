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

            public static Parameters Default => new Parameters() { hasRevealRadius = true, revealRadius = 150f };
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
            {
                if (this.parameters.hasRevealRadius)
                    Cell.Instance.RegisterFocusPoint(this.transform, this.parameters.revealRadius);
                else
                    Cell.Instance.RegisterFocusPoint(this.transform);
            }
        }

        private void OnDisable()
        {
            if (Cell.Instance != null)
                Cell.Instance.UnRegisterFocusPoint(this.transform);
        }
    }
}
