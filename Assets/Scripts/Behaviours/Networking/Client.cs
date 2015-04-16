using System;
using Facepunch.Networking;
using ProtoBuf;
using ProtoBuf.Player;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Networking
{
    public class Client : Facepunch.Networking.Client
    {
        private static readonly ulong _sUserId;

        protected override ulong UserId
        {
            get { return _sUserId; }
        }

        protected override string Username
        {
            get { return Config.Get<string>("cl_name"); }
        }

        static Client()
        {
            // Risky
            _sUserId = BitConverter.ToUInt64(Guid.NewGuid().ToByteArray(), 8);

            if (Config.Get<bool>("cl_connect")) {
                NetConfig.RemoteHostname = Config.Get<string>("cl_remote_hostname");
                NetConfig.Port = Config.Get<int>("cl_remote_port");
                NetConfig.IsClient = true;

                Debug.LogFormat("Will connect to {0}:{1}", NetConfig.RemoteHostname, NetConfig.Port);
            } else {
                NetConfig.IsClient = false;
            }
        }

#if PROTOBUF
        
        package ProtoBuf;

        message ConnectRequestData
        {
            required int32 ModelId = 1;
        }

#endif

        public Player LocalPlayer { get; private set; }

        protected override void OnPrepareConnectRequest(ProtoBuf.ConnectRequest request)
        {
            request.Data = new ConnectRequestData {
                ModelId = Config.Get<int>("cl_model_id")
            }.ToProtoBytes();
        }

        protected override void OnDispatchNetworkableMessage(Facepunch.Networking.Networkable target, IRemote sender, INetworkableMessage message)
        {
            var plySpawn = message as PlayerSpawn;
            if (plySpawn != null && plySpawn.IsLocal) {
                LocalPlayer = (Player) target;
            }

            base.OnDispatchNetworkableMessage(target, sender, message);
        }
    }
}
