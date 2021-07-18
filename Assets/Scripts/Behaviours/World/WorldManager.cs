using UnityEngine;
using Object = UnityEngine.Object;

namespace SanAndreasUnity.Behaviours.World
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Singleton { get; private set; }

        public const float MinMaxDrawDistance = 250f;
        public const float MaxMaxDrawDistance = 1500f;

        [SerializeField]
        [Range(MinMaxDrawDistance, MaxMaxDrawDistance)]
        private float _defaultMaxDrawDistance = 1500f;

        [SerializeField]
        [Range(MinMaxDrawDistance, MaxMaxDrawDistance)]
        private float _defaultMaxDrawDistanceOnMobile = 1000f;

        private float _maxDrawDistance = 0f;
        public float MaxDrawDistance
        {
            get => _maxDrawDistance;
            set
            {
                if (value == _maxDrawDistance)
                    return;

                _maxDrawDistance = value;

                this.ApplyMaxDrawDistance(Cell.Instance);
            }
        }


        private void Awake()
        {
            Singleton = this;

            _maxDrawDistance = Application.isMobilePlatform ? _defaultMaxDrawDistanceOnMobile : _defaultMaxDrawDistance;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (s1, s2) => OnActiveSceneChanged();
        }

        void OnActiveSceneChanged()
        {
            // apply settings

            // we need to find Cell with FindObjectOfType(), because it's Awake() method may have not been called yet
            Cell cell = Object.FindObjectOfType<Cell>();
            this.ApplyMaxDrawDistance(cell);

        }

        void ApplyMaxDrawDistance(Cell cell)
        {
            if (cell != null)
                cell.MaxDrawDistance = this.MaxDrawDistance;

            var cam = Camera.main;
            if (cam != null)
                cam.farClipPlane = this.MaxDrawDistance;

            cam = Camera.current;
            if (cam != null)
                cam.farClipPlane = this.MaxDrawDistance;
        }
    }
}
