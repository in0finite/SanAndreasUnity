using System;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public class StartupSingleton<T> : MonoBehaviour
        where T : StartupSingleton<T>
    {
        public static T Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton != null)
            {
                throw new Exception($"Awake() method called twice for singleton of type {this.GetType().Name}");
            }

            Singleton = (T) this;

            this.OnSingletonAwake();
        }

        protected virtual void OnSingletonAwake()
        {
        }

        private void Start()
        {
            if (this != Singleton)
                return;

            this.OnSingletonStart();
        }

        protected virtual void OnSingletonStart()
        {
        }
    }
}
