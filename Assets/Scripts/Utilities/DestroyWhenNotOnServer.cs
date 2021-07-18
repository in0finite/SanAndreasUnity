using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SanAndreasUnity.Utilities
{
    public class DestroyWhenNotOnServer : MonoBehaviour
    {
        public Component[] componentsToDestroy = Array.Empty<Component>();

        private void Awake()
        {
            if (!NetUtils.IsServer)
            {
                foreach (var component in this.componentsToDestroy)
                {
                    Object.Destroy(component);
                }
            }
        }
    }
}
