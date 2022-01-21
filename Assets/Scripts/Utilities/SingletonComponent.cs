using System;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public class SingletonComponent<T> : MonoBehaviour
        where T : SingletonComponent<T>
    {
#if !UNITY_EDITOR
        public static T Singleton { get; private set; }
#else
        private static T s_cachedSingleton;
        public static T Singleton
        {
            get
            {
                if (!F.IsAppInEditTime)
                {
                    return s_cachedSingleton;
                }

                if (s_cachedSingleton != null)
                    return s_cachedSingleton;

                T[] objects = FindObjectsOfType<T>();

                if (objects.Length == 0)
                    return null;

                if (objects.Length > 1)
                    throw new Exception($"Found multiple singleton objects of type {typeof(T).Name}. Make sure there is only 1 singleton object created per type.");

                s_cachedSingleton = objects[0];
                return s_cachedSingleton;
            }
            private set
            {
                s_cachedSingleton = value;
            }
        }
#endif

        private void Awake()
        {
            if (Singleton != null)
            {
                throw new Exception($"Awake() method called twice for singleton of type {this.GetType().Name}");
            }

            Singleton = (T)this;

            this.OnSingletonAwake();
        }

        protected virtual void OnSingletonAwake()
        {
        }

        private void OnDisable()
        {
            if (Singleton != this)
                return;

            this.OnSingletonDisable();
        }

        protected virtual void OnSingletonDisable()
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
