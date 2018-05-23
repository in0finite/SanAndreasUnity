#if !NOT_UNITY3D

using System;
using UnityEngine;

namespace Zenject
{
    // We'd prefer to make this abstract but Unity 5.3.5 has a bug where references
    // can get lost during compile errors for classes that are abstract
    [System.Diagnostics.DebuggerStepThrough]
    public class MonoInstallerBase : MonoBehaviour, IInstaller
    {
        [Inject]
        private DiContainer _container = null;

        protected DiContainer Container
        {
            get { return _container; }
        }

        public virtual bool IsEnabled
        {
            get { return this.enabled; }
        }

        public virtual void Start()
        {
            // Define this method so we expose the enabled check box
        }

        public virtual void InstallBindings()
        {
            throw new NotImplementedException();
        }
    }
}

#endif