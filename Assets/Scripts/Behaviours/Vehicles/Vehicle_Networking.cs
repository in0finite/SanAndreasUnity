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
            optional UnityEngine.Quaternion Rotation = 4;

            //:interpolate
            optional UnityEngine.Vector3 Velocity = 5;
        
            //:interpolate
            optional float Steering = 6;
        
            //:interpolate
            optional float Accelerator = 7;
        
            //:interpolate
            optional float Braking = 8;
        }

#endif

#if CLIENT

        private readonly SnapshotBuffer<VehicleState> _snapshots
            = new SnapshotBuffer<VehicleState>(.25);

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

            Initialize(def, message.Info.Colors.ToArray());

            UpdateFromSnapshot(message.State);
        }

        protected override void OnClientNetworkingUpdate()
        {
            if (!IsControlling) return;

            SendToServer(GetSnapshot(), DeliveryMethod.UnreliableSequenced, 2);
        }

        [MessageHandler(Domain.Client)]
        private void OnReceiveMessageFromServer(IRemote sender, VehicleState message)
        {
            if (IsControlling) return;

            _snapshots.Add(message);
        }

#endif

        private VehicleState GetSnapshot()
        {
            return new VehicleState {
                Position = transform.position,
                Rotation = transform.rotation,
                Velocity = _rigidBody != null ? _rigidBody.velocity : Vector3.zero,
                Steering = Steering,
                Accelerator = Accelerator,
                Braking = Braking,
                Timestamp = ServerTime
            };
        }

        private void UpdateFromSnapshot(VehicleState state)
        {
            transform.position = state.Position;
            transform.rotation = state.Rotation;

            Steering = state.Steering;
            Accelerator = state.Accelerator;
            Braking = state.Braking;

            if (_rigidBody != null) {
                _rigidBody.velocity = state.Velocity;
                _rigidBody.angularVelocity = Vector3.zero;
            }
        }

        protected override void OnFirstObserve(IEnumerable<IRemote> clients)
        {
            SendToClients(new VehicleSpawn {
                Info = new VehicleInfo {
                    VehicleId = Definition.Id,
                    Colors = _colors.ToList()
                },
                State = GetSnapshot()
            }, clients, DeliveryMethod.ReliableOrdered, 1);
        }

        private void NetworkingFixedUpdate()
        {
            if (!IsClient) return;
            if (!IsControlling && _snapshots.Update()) {
                UpdateFromSnapshot(_snapshots.Current);
            }
        }

        [MessageHandler(Domain.Server)]
        private void OnReceiveMessageFromClient(IRemote sender, VehicleState message)
        {
            // TODO: Check to see if controller is driver
            
            SendToClients(message, Group.Subscribers.Where(x => x != sender),
                DeliveryMethod.UnreliableSequenced, 2);
        }
    }
}
