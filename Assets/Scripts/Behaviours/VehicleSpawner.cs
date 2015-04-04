using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Items.Placements;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class VehicleSpawner : MapObject
    {
        public static VehicleSpawner Create(ParkedVehicle info)
        {
            var vs = new GameObject().AddComponent<VehicleSpawner>();
            vs.Initialize(info);
            return vs;
        }

        private ParkedVehicle _info;
        private Vehicle _vehicle;

        public void Initialize(ParkedVehicle info)
        {
            _info = info;

            name = string.Format("Vehicle Spawner ({0})", info.CarId);

            Initialize(info.Position, Quaternion.AngleAxis(info.Angle, Vector3.up));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position, new Vector3(1f, 1f, 1f));
        }

        protected override float OnRefreshLoadOrder(Vector3 from)
        {
            if (HasLoaded) return float.PositiveInfinity;
            var dist = Vector3.Distance(from, transform.position);
            return dist > 1000f ? float.PositiveInfinity : dist;
        }
    }
}
