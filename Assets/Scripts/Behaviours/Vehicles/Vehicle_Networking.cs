using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facepunch.Networking;
using ProtoBuf.Vehicle;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {

#if PROTOBUF
        
        package ProtoBuf.Vehicle;

        message VehicleInfo
        {
            required int32 VehicleId = 1;
            repeated int32 Colors = 2;
        }
        
        //:baseclass = INetworkableSpawnMessage
        message VehicleSpawn
        {
            required ProtoBuf.NetworkableInfo Networkable = 1;
            required VehicleInfo Info = 2;
            required VehicleState State = 3;
        }
        
        //:baseclass = ISnapshot, INetworkableMessage
        message VehicleState
        {
            optional ProtoBuf.NetworkableInfo Networkable = 1;
            required int64 Timestamp = 2;

            //:interpolate
            //:smoothing = 0.5
            optional UnityEngine.Vector3 Position = 3;
        
            //:interpolate
            //:smoothing = 0.5
            optional UnityEngine.Quaternion Rotation = 4;

            //:interpolate
            optional UnityEngine.Vector3 Velocity = 5;
        
            //:interpolate
            optional UnityEngine.Vector3 AngularVelocity = 6;
        }

#endif

#if CLIENT

        [ClientSpawnMethod]
        private static Vehicle Spawn(VehicleSpawn message)
        {
            return new GameObject().AddComponent<Vehicle>();
        }


        [MessageHandler(Domain.Client)]
        private void OnSpawn(IRemote sender, VehicleSpawn message)
        {
            if (IsServer) return;

            var def = Item.GetDefinition<VehicleDef>(message.Info.VehicleId);

            Debug.LogFormat("Spawning a {0}", def.GameName);

            Initialize(def, message.Info.Colors.ToArray());
            UpdateFromSnapshot(message.State);
        }

#endif

        private VehicleState GetSnapshot()
        {
            return new VehicleState {
                Position = transform.position,
                Rotation = transform.rotation,
                Velocity = _rigidBody.velocity,
                AngularVelocity = _rigidBody.angularVelocity
            };
        }

        private void UpdateFromSnapshot(VehicleState state)
        {
            transform.position = state.Position;
            transform.rotation = state.Rotation;

            _rigidBody.velocity = state.Velocity;
            _rigidBody.angularVelocity = state.AngularVelocity;
        }

        protected override void OnFirstObserve(IEnumerable<IRemote> clients)
        {
            SendToClients(new VehicleSpawn {
                Info = new VehicleInfo {
                    VehicleId = Definition.Id,
                    Colors = _colors.ToList()
                },
                State = GetSnapshot()
            }, clients, DeliveryMethod.ReliableOrdered, 2);
        }
    }
}
