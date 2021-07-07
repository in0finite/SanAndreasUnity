using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SanAndreasUnity.Utilities
{
    public class DestroyWhenInHeadlessMode : MonoBehaviour
    {
        public Component[] componentsToDestroy = Array.Empty<Component>();

        private void Start() // use Start() to avoid problems with script execution order
        {
            if (F.IsInHeadlessMode)
            {
                foreach (var component in this.componentsToDestroy)
                {
                    Object.Destroy(component);
                }
            }
        }
    }
}
