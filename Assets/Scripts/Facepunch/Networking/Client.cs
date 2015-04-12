using System;
using ProtoBuf;
// ReSharper disable once RedundantUsingDirective
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Facepunch.Networking
{
    public class Client : EndPoint<Client, IRemoteServer>
    {
        public delegate String UsernameResolver();
        public delegate ulong UserIdResolver();

        public static event UsernameResolver ResolveUsername;
        public static event UserIdResolver ResolveUserId;

#if PROTOBUF
        
        package ProtoBuf;

        //:baseclass = INetworkMessage
        //:ident = 1
        message ConnectRequest
        {
            required int32 Protocol = 1;
            required uint64 UserId = 2;
            required string Username = 3;
            required string Os = 4;

            optional bytes Data = 5;
        }

#endif

#if CLIENT
        private readonly Dictionary<uint, Networkable> _networkables;

        private int _tickRate = 30;

        public String ServerName { get; private set; }

        private long _timeDiff;

        public override long Time
        {
            get { return SystemTime - _timeDiff; }
        }

        public ConnectionStatus ConnectionStatus
        {
            get { return Net == null ? ConnectionStatus.None : Net.ConnectionStatus; }
        }

        protected override bool ShouldActivate
        {
            get { return NetConfig.IsClient; }
        }

        protected override int UpdateRate
        {
            get { return _tickRate; }
        }

        protected override string LogPrefix
        {
            get { return "net-cl"; }
        }

        public Client()
        {
            _networkables = new Dictionary<uint,Networkable>();

            ServerName = String.Empty;
        }

        protected override IRemoteServer CreateInstance()
        {
            return NetProvider.Connect(NetConfig.Hostname, NetConfig.Port);
        }

        protected override void OnNetworkingAwake()
        {
            base.OnNetworkingAwake();

            Net.RegisterHandler<ConnectResponse>(OnReceiveMessage);
            Net.RegisterHandler<NetworkablesRemoved>(OnReceiveMessage);

            StartCoroutine(SendConnectionRequest());
        }

        protected override void OnUpdate()
        {
            foreach (var networkable in _networkables.Values) {
                networkable.ClientNetworkingUpdate();
            }
        }

        protected virtual void OnPrepareConnectRequest(ConnectRequest request) { }

        private IEnumerator SendConnectionRequest()
        {
            while (Net.ConnectionStatus != ConnectionStatus.Connected) yield return null;

            var request = new ConnectRequest {
                Os = Environment.OSVersion.VersionString,
                Protocol = Protocol,
                UserId = ResolveUserId(),
                Username = ResolveUsername()
            };

            OnPrepareConnectRequest(request);

            Net.SendMessage(request, DeliveryMethod.ReliableOrdered, 0);
        }

        private void OnReceiveMessage(IRemote sender, ConnectResponse message)
        {
            if (message.Flags != ConnectResponseFlags.Accept) {
                Debug.LogFormat("Failed to connect: {0}", message.Message ?? "Rejected by server");

                Net.Shutdown();
                return;
            }

            ServerName = message.HostName;
            _tickRate = message.UpdateRate > 0 ? message.UpdateRate : _tickRate;
            _timeDiff = SystemTime - message.ServerTime;

            Net.LoadMessageTableFromSchema(message.MessageTable);
        }

        private void OnReceiveMessage(IRemote sender, NetworkablesRemoved message)
        {
            foreach (var info in message.Networkables) {
                var networkable = GetNetworkable(info.Ident);
                if (networkable == null) continue;
                networkable.DestroyClientSide(true);
            }
        }

        internal void RegisterNetworkable(Networkable networkable)
        {
            Debug.LogFormat("[cl] Registering {0} ({1})", networkable.UniqueId, networkable.GetType());
            _networkables.Add(networkable.UniqueId, networkable);
        }

        internal void ForgetNetworkable(Networkable networkable)
        {
            _networkables.Remove(networkable.UniqueId);
        }

        public override Networkable GetNetworkable(uint id)
        {
            return _networkables.ContainsKey(id) ? _networkables[id] : null;
        }

        protected virtual void OnDispatchNetworkableMessage(Networkable target, IRemote sender, INetworkableMessage message)
        {
            target.HandleMessageFromServer(sender, message);
        }

        protected override void OnReceiveMessage(IRemote sender, INetworkableMessage message)
        {
            Networkable target;

            if (message.Networkable == null) return;
            if (!_networkables.ContainsKey(message.Networkable.Ident)) {
                var spawn = message as INetworkableSpawnMessage;
                if (spawn == null) return;

                target = Networkable.ClientSpawn(this, spawn);
            } else {
                target = _networkables[message.Networkable.Ident];
            }

            OnDispatchNetworkableMessage(target, sender, message);
        }
#endif
    }
}
