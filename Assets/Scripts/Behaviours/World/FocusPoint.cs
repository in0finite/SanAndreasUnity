using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    [DisallowMultipleComponent]
    public class FocusPoint : MonoBehaviour
    {
        public FocusPointParameters parameters = FocusPointParameters.Default;


        public static FocusPoint Create(GameObject targetGameObject, FocusPointParameters parameters)
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
