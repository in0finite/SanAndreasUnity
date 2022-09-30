using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.WorldSystem
{
    public class AreaChangeDetector : MonoBehaviour
    {
        public float timeIntervalToCheckArea = 1f;

        private IWorldSystem _worldSystem;

        private AreaIndex _lastAreaIndex;

        public event System.Action<AreaIndex, AreaIndex> onAreaChanged = delegate {};


        public void Init(IWorldSystem worldSystem)
        {
            _worldSystem = worldSystem;
            _lastAreaIndex = worldSystem.GetAreaIndex(this.transform.position);
            this.CancelInvoke(nameof(this.CheckArea));
            this.InvokeRepeating(nameof(this.CheckArea), this.timeIntervalToCheckArea, this.timeIntervalToCheckArea);
        }

        private void CheckArea()
        {
            AreaIndex newAreaIndex = _worldSystem.GetAreaIndex(this.transform.position);
            if (!newAreaIndex.IsEqualTo(_lastAreaIndex))
            {
                var oldAreaIndex = _lastAreaIndex;
                _lastAreaIndex = newAreaIndex;
                F.InvokeEventExceptionSafe(this.onAreaChanged, oldAreaIndex, newAreaIndex);
            }
        }
    }
}
