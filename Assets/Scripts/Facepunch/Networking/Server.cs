using System;
using System.Linq;
using Facepunch.ConCommands;
using Facepunch.RCon;
using ProtoBuf;
using UnityEngine;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Facepunch.Networking
{
    public class Server : EndPoint<Server, ILocalServer>
    {
#if PROTOBUF

        package ProtoBuf;

        enum ConnectResponseFlags
        {
            Reject = 0;
            Accept = 1;
        }

        //:baseclass = INetworkMessage
        //:ident = 2
        message ConnectResponse
        {
            required ConnectResponseFlags Flags = 1;

            optional string Message = 2;
            optional string HostName = 3;

            optional int32 UpdateRate = 5;
            optional double ServerTime = 6;

            optional MessageTableSchema MessageTable = 4;
        }

#endif

        private RConServer _rcon;

        private readonly IdentifierSet _identifiers;

        private readonly Dictionary<uint, Group> _groups;
        private readonly Dictionary<uint, Networkable> _networkables;

#if CLIENT
        private Client _localClient;

        public Client LocalClient
        {
            get { return _localClient != null && _localClient.isActiveAndEnabled ? _localClient : null; }
        }

#endif

        public String ServerName;
        public int TickRate = 20;

        public Group GlobalGroup { get; private set; }

        private double _startTime;

        public override double Time
        {
            get { return SystemTime - _startTime; }
        }

        protected override bool ShouldActivate
        {
            get { return NetConfig.IsServer; }
        }

        protected override int UpdateRate
        {
            get { return TickRate; }
        }

        protected override string LogPrefix
        {
            get { return "net-sv"; }
        }

        protected override ILocalServer CreateInstance()
        {
            return NetProvider.CreateServer(NetConfig.Port, NetConfig.MaxConnections);
        }

        public Server()
        {
            _identifiers = new IdentifierSet(1);
            _groups = new Dictionary<uint, Group>();
            _networkables = new Dictionary<uint, Networkable>();

            GlobalGroup = CreateGroup();
        }

        public Group CreateGroup()
        {
            var group = new Group(this, _identifiers.Allocate());
            _groups.Add(group.UniqueId, group);
            return group;
        }

        internal void DisposeGroup(Group group)
        {
            if (_identifiers.TryFree(group.UniqueId))
            {
                _groups.Remove(group.UniqueId);
            }
        }

#if UNITY_EDITOR

        public static void AssignEditorIds()
        {
            var networkables = FindObjectsOfType<Networkable>()
                .OrderBy(x => x.EditorId);

            var usedIds = new IdentifierSet(0x0100);

            foreach (var networkable in networkables)
            {
                var id = usedIds.Allocate();
                if (networkable.EditorId == id) continue;

                networkable.EditorId = id;
                EditorUtility.SetDirty(networkable);
            }
        }

#endif

        protected override void OnAwake()
        {
            base.OnAwake();

            if (!NetConfig.RconEnabled) return;

            _rcon = new RConServer(NetConfig.RconPort);
            _rcon.VerifyCredentials += OnVerifyRconCredentials;
            _rcon.ExecuteCommand += (creds, command) => ConCommand.RunServer(command);

            UnityEngine.Application.logMessageReceived += _rcon.BroadcastLog;

            _rcon.Start();
        }

        protected override void OnNetworkingAwake()
        {
            base.OnNetworkingAwake();

            if (string.IsNullOrEmpty(ServerName))
            {
                ServerName = NetConfig.ServerName;
            }

            _startTime = SystemTime;

#if CLIENT
            var client = FindObjectOfType<Client>();
            if (client != null)
            {
                _localClient = client;
            }
#endif

            Net.RegisterHandler<ConnectRequest>(OnReceiveMessage);
        }

        protected virtual bool OnVerifyRconCredentials(RConCredentials creds)
        {
            return creds.Password.Equals(NetConfig.RconPassword);
        }

        protected override void OnUpdate()
        {
            ConCommand.PerformMainThreadTasks();

            foreach (var group in _groups)
            {
                group.Value.Update();
            }
        }

        protected virtual void OnAcceptClient(IRemote client, ConnectRequest request)
        {
            var resp = new ConnectResponse
            {
                Flags = ConnectResponseFlags.Accept,
                HostName = ServerName,
                UpdateRate = UpdateRate,
                ServerTime = Time,
                MessageTable = Net.GetMessageTableSchema()
            };

            Net.SendMessage(resp, client, DeliveryMethod.ReliableOrdered, 0);
        }

        private void AcceptClient(IRemote client, ConnectRequest request)
        {
            Net.Log(LogType.Log, "Accepting client '{0}'", request.Username);

            OnAcceptClient(client, request);
        }

        private void RejectClient(IRemote client, String response)
        {
            var resp = new ConnectResponse
            {
                Flags = ConnectResponseFlags.Reject,
                HostName = ServerName,
                Message = response,
            };

            Net.SendMessage(resp, client, DeliveryMethod.ReliableOrdered, 0);

            client.Disconnect(response ?? "Rejected by server");
        }

        protected virtual bool OnClientConnect(IRemote client, ConnectRequest request, out String message)
        {
            message = null;

            var protocol = request.Protocol;
            if (protocol == Protocol) return true;

            message = string.Format("{0} protocol out of date, expected {1}, received {2}", (Protocol < protocol) ? "Server" : "Client", Protocol, protocol);

            return false;
        }

        private void OnReceiveMessage(IRemote sender, ConnectRequest message)
        {
            String response;
            if (OnClientConnect(sender, message, out response))
            {
                AcceptClient(sender, message);
            }
            else
            {
                RejectClient(sender, response);
            }
        }

        protected override void OnReceiveMessage(IRemote sender, INetworkableMessage message)
        {
            if (message.Networkable == null) return;
            if (!_networkables.ContainsKey(message.Networkable.Ident)) return;

            _networkables[message.Networkable.Ident].HandleMessageFromClient(sender, message);
        }

        internal uint RegisterNetworkable(Networkable networkable)
        {
            if (networkable.EditorId == 0)
            {
                var id = _identifiers.Allocate();

                Debug.LogFormat("[sv] Registering {0} ({1})", id, networkable.GetType());

                _networkables.Add(id, networkable);
                return id;
            }

            if (!_identifiers.TryAssign(networkable.EditorId))
            {
                throw new Exception("Networkable identifier conflict");
            }

            Debug.LogFormat("[sv] Registering {0} ({1})", networkable.EditorId, networkable.GetType());

            _networkables.Add(networkable.EditorId, networkable);

            return networkable.EditorId;
        }

        internal void RemoveNetworkable(Networkable networkable)
        {
            if (networkable.UniqueId == 0) return;

            if (networkable.Group != null)
            {
                networkable.Group.Remove(networkable);
            }

            _networkables.Remove(networkable.UniqueId);
            _identifiers.TryFree(networkable.UniqueId);
        }

        public override Networkable GetNetworkable(uint id)
        {
            return _networkables.ContainsKey(id) ? _networkables[id] : null;
        }

        public override IEnumerable<Networkable> GetNetworkables()
        {
            return _networkables.Values;
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            if (_rcon != null)
            {
                UnityEngine.Application.logMessageReceived -= _rcon.BroadcastLog;
                _rcon.Stop();
            }
        }
    }
}