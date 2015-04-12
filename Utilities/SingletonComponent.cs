using System.Linq;

namespace UnityEngine
{
    public abstract class SingletonComponent<TComponent> : MonoBehaviour
        where TComponent : SingletonComponent<TComponent>
    {
        private static TComponent _sInstance;

        public static TComponent Instance
        {
            get { return _sInstance ?? (_sInstance = FindObjectOfType<TComponent>()); }
        }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            if (FindObjectsOfType<TComponent>().Any(x => x != this && x.isActiveAndEnabled)) {
                DestroyImmediate(this);
                return;
            }

            _sInstance = (TComponent) this;

            DontDestroyOnLoad(gameObject);

            OnAwake();
        }

        protected virtual void OnAwake() { }
    }
}
