using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    [DisallowMultipleComponent]
    public class FocusPoint : MonoBehaviour
    {
        public bool hasRevealRadius = true;
        public float revealRadius = 50f;


        public static FocusPoint Create(GameObject targetGameObject, bool hasRevealRadius, float revealRadius)
        {
            var focusPoint = targetGameObject.AddComponent<FocusPoint>();
            focusPoint.hasRevealRadius = hasRevealRadius;
            focusPoint.revealRadius = revealRadius;
            return focusPoint;
        }

        private void Start()
        {
            if (Cell.Instance != null)
            {
                if (this.hasRevealRadius)
                    Cell.Instance.RegisterFocusPoint(this.transform, this.revealRadius);
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
