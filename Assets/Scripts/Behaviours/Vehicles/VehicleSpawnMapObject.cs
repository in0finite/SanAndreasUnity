using SanAndreasUnity.Importing.Items.Placements;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public class VehicleSpawnMapObject : MapObject
    {
        public static VehicleSpawnMapObject Create(ParkedVehicle info)
        {
            var vs = new GameObject().AddComponent<VehicleSpawnMapObject>();
            vs.Initialize(info);
            return vs;
        }

        public ParkedVehicle Info { get; private set; }

        public void Initialize(ParkedVehicle info)
        {
            Info = info;

            name = string.Format("Vehicle Spawn ({0})", info.CarId);

            Initialize(info.Position, Quaternion.AngleAxis(info.Angle, Vector3.up));

            this.SetDrawDistance(100f);

            gameObject.SetActive(false);
            gameObject.isStatic = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position + Vector3.up * 128f, new Vector3(1f, 256f, 1f));
        }

        protected override void OnLoad()
        {
            Vehicle.Create(this);
        }
    }
}