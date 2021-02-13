using System.Collections.Generic;
using System.Linq;
using Mirror;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Net
{

    public class NetworkedVehicleDetachedPart : NetworkBehaviour
    {
        public NetworkRigidBody NetworkRigidBody { get; private set; }

        [SyncVar] uint m_net_vehicleNetId;
        [SyncVar] int m_net_vehicleModelId;
        [SyncVar] string m_net_vehicleColors;
        [SyncVar] string m_net_frameName;
        [SyncVar] float m_net_mass;

        class VehicleInfo
        {
            public int numReferences;
            public FrameContainer frames;
        }

        static readonly Dictionary<uint, VehicleInfo> s_dummyObjectsPerVehicle = new Dictionary<uint, VehicleInfo>();

        bool m_isReferencingFrame = false;



        void Awake()
        {
            this.NetworkRigidBody = this.GetComponentOrThrow<NetworkRigidBody>();
        }

        void OnDisable()
        {
            if (m_isReferencingFrame)
            {
                m_isReferencingFrame = false;
                if (s_dummyObjectsPerVehicle.TryGetValue(m_net_vehicleNetId, out VehicleInfo vehicleInfo))
                {
                    vehicleInfo.numReferences--;
                    if (vehicleInfo.numReferences <= 0)
                    {
                        // TODO: check if it is null
                        Object.Destroy(vehicleInfo.frames.gameObject);
                        s_dummyObjectsPerVehicle.Remove(m_net_vehicleNetId);
                    }
                }
            }
        }

        public void InitializeOnServer(uint vehicleNetId, int vehicleModelId, int[] vehicleColors, string frameName, float mass, Rigidbody rigidbody)
        {
            NetStatus.ThrowIfNotOnServer();

            m_net_vehicleNetId = vehicleNetId;
            m_net_vehicleModelId = vehicleModelId;
            m_net_vehicleColors = VehicleController.SerializeColors(vehicleColors);
            m_net_frameName = frameName;
            m_net_mass = mass;

            this.NetworkRigidBody.Rigidbody = rigidbody;
            this.NetworkRigidBody.UpdateServer();
        }

        public override void OnStartClient()
        {
            if (NetStatus.IsServer)
                return;

            F.RunExceptionSafe(() => this.OnStartClientInternal());
        }

        void OnStartClientInternal()
        {
            GameObject vehicleGo = NetManager.GetNetworkObjectById(m_net_vehicleNetId);
            if (null == vehicleGo)
            {
                // this should only happen when vehicle has been destroyed before client connected, but it's detached parts are still alive
                // load vehicle's model, and attach it's frames to dummy object, and detach a frame from that dummy object


                
                VehicleDef def = Item.GetDefinitionOrThrow<VehicleDef>(m_net_vehicleModelId);

                int[] colors = VehicleController.DeserializeColors(m_net_vehicleColors);

                var geometryParts = Vehicle.LoadGeometryParts(def);


                VehicleInfo vehicleInfo;
                if (s_dummyObjectsPerVehicle.TryGetValue(m_net_vehicleNetId, out vehicleInfo))
                {
                    // we already created dummy object for this vehicle
                    vehicleInfo.numReferences++;
                }
                else
                {
                    // need to create dummy object for this vehicle, and attach all vehicle's frames to it
                    Transform transformDummy = new GameObject($"{def.GameName}_detached_part_{m_net_frameName}").transform;
                    vehicleInfo = new VehicleInfo();
                    vehicleInfo.frames = geometryParts.AttachFrames(transformDummy, Importing.Conversion.MaterialFlags.Vehicle);
                    SetColors(vehicleInfo.frames, colors);
                    vehicleInfo.numReferences = 1;
                    s_dummyObjectsPerVehicle.Add(m_net_vehicleNetId, vehicleInfo);
                }

                m_isReferencingFrame = true;

                Frame frame = vehicleInfo.frames.FirstOrDefault(f => f.gameObject.name == m_net_frameName);
                if (null == frame)
                    throw new System.Exception($"Failed to find frame by name {m_net_frameName}");

                // detach frame from dummy object
                Vehicle.DetachFrameFromTransformDuringExplosion(vehicleInfo.frames.transform, frame, m_net_mass, this.gameObject, m_net_vehicleNetId, m_net_vehicleModelId, colors);

            }
            else
            {
                Vehicle vehicle = vehicleGo.GetComponentOrThrow<Vehicle>();
                vehicle.DetachFrameDuringExplosion(m_net_frameName, m_net_mass, this.gameObject);
            }

            this.NetworkRigidBody.Rigidbody = this.GetComponentInChildren<Rigidbody>();
            if (this.NetworkRigidBody.Rigidbody != null)
                this.NetworkRigidBody.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            this.NetworkRigidBody.UpdateClient();

        }

        private static void SetColors(FrameContainer frames, int[] colors)
        {
            Vehicle.UpdateMaterials(frames, colors, new []{ 0f, 0f, 0f, 0f }, new MaterialPropertyBlock());
        }
    }

}
