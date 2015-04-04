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

        public string CarName;

        public void Initialize(ParkedVehicle info)
        {
            _info = info;

            name = string.Format("Vehicle Spawner ({0})", info.CarId);

            Initialize(info.Position, Quaternion.AngleAxis(info.Angle, Vector3.up));

            gameObject.SetActive(false);
            gameObject.isStatic = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position + Vector3.up * 128f, new Vector3(1f, 256f, 1f));
        }

        protected override float OnRefreshLoadOrder(Vector3 from)
        {
            if (HasLoaded) return float.PositiveInfinity;
            var dist = Vector3.Distance(from, transform.position);
            return dist > 1000f ? float.PositiveInfinity : dist;
        }

        protected override void OnLoad()
        {
            // TODO
            if (_info.CarId == -1) return;

            _vehicle = Cell.GameData.GetDefinition<Vehicle>(_info.CarId);

            CarName = _vehicle.ModelName;

            gameObject.SetActive(true);
        }
    }
}
