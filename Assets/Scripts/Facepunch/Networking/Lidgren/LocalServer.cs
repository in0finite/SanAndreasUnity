using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Lidgren.Network;
using ProtoBuf;
using UnityEngine;

namespace Facepunch.Networking.Lidgren
{
    using Peer = NetServer;

    /// <remarks>
    /// This class is super not thread safe
    /// </remarks>
    public class LocalServerImpl : LocalServer
    {
        public const String AppIdentifier = "Arcade";

        public const double ClientCheckPeriod = 1d;

        private static Peer CreatePeer(int port, int maxConnections)
        {
            var config = new NetPeerConfiguration(AppIdentifier) {
                Port = port,
                MaximumConnections = maxConnections,
                AcceptIncomingConnections = true
            };

            return new Peer(config);
        }

        protected Peer Peer { get; private set; }

        public override NetStatus NetStatus
        {
            get { return (NetStatus) Peer.Status; }
        }

        private class Client : IRemote
        {
            private bool _sentDisconnect;

            public NetConnection Connection { get; private set; }

            public IPEndPoint EndPoint { get { return Connection.RemoteEndPoint; } }

            public ConnectionStatus ConnectionStatus { get { return (ConnectionStatus) Connection.Status; } }

            public float AverageRoundTripTime { get { return Connection.AverageRoundtripTime; } }

            public event ClientDisconnectEventHandler Disconnected;

            public string DisconnectMessage = "";

            public Client(NetConnection connection)
            {
                Connection = connection;
            }

            internal void OnDisconnect(string message)
            {
                if (_sentDisconnect) return;
                _sentDisconnect = true;

                if (Disconnected != null) {
                    Disconnected(this, message);
                }
            }

            public void Disconnect(String message)
            {
                Connection.Disconnect(message);

                DisconnectMessage = message;
            }
        }

        /// HACK: Not thread safe
        private readonly MemoryStream _writerStream;
        private readonly MemoryStream _readerStream;

        private readonly Dictionary<NetConnection, Client> _clients;
        private DateTime _lastClientCheck;

        public override IEnumerable<IRemote> Clients
        {
            get
            {
                CheckClients();
                return _clients.Values.Cast<IRemote>();
            }
        }

        private Client GetClient(NetConnection connection)
        {
            if (connection == null) return null;

            CheckClients();

            if (!_clients.ContainsKey(connection)) {
                _clients.Add(connection, new Client(connection));
                ClientConnect(_clients[connection]);
            }

            return _clients[connection];
        }

        private void CheckClients()
        {
            if ((DateTime.Now - _lastClientCheck).TotalSeconds < ClientCheckPeriod) return;

            _lastClientCheck = DateTime.Now;

            var disconnected = _clients.Keys
                .Where(x => x.Status == NetConnectionStatus.Disconnected || !Peer.Connections.Contains(x))
                .ToList();

            foreach (var client in disconnected) {
                ClientDisconnect(_clients[client], _clients[client].DisconnectMessage);
                _clients.Remove(client);
            }
        }
        
        private LocalServerImpl(Peer peer)
        {
            Peer = peer;
            Peer.Start();

            _writerStream = new MemoryStream();
            _readerStream = new MemoryStream();
            
            _clients = new Dictionary<NetConnection, Client>();
            _lastClientCheck = DateTime.Now;
        }

        public LocalServerImpl(int port, int maxConnections)
            : this(CreatePeer(port, maxConnections)) { }

        private NetOutgoingMessage PrepareMessage(INetworkMessage message)
        {
            _writerStream.Clear();
            MessageTable.Serialize(_writerStream, message);

            var length = (int) _writerStream.Length;

            var msg = Peer.CreateMessage(length);
            msg.Write(_writerStream.GetBuffer(), 0, length);

            return msg;
        }

        public override void SendMessage(INetworkMessage message, IRemote recipient, DeliveryMethod method, int sequenceChannel)
        {
            var client = ((Client) recipient).Connection;

            Peer.SendMessage(PrepareMessage(message), client, (NetDeliveryMethod) method, sequenceChannel);
        }

        public override void SendMessage(INetworkMessage message, IEnumerable<IRemote> recipients, DeliveryMethod method, int sequenceChannel)
        {
            if (!recipients.Any()) return;

            var clients = recipients
                .Cast<Client>()
                .Select(x => x.Connection)
                .ToArray();

            Peer.SendMessage(PrepareMessage(message), clients, (NetDeliveryMethod) method, sequenceChannel);
        }

        public override bool CheckForMessages()
        {
            CheckClients();

            var received = new List<NetIncomingMessage>();
            Peer.ReadMessages(received);

            foreach (var msg in received) {
                var client = GetClient(msg.SenderConnection);

                switch (msg.MessageType) {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                        Log(LogType.Log, msg.ReadString());
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        Log(LogType.Warning, msg.ReadString());
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Log(LogType.Error, msg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        var status = (NetConnectionStatus) msg.ReadByte();
                        var reason = msg.ReadString();

                        if (status == NetConnectionStatus.Disconnected) {
                            client.DisconnectMessage = reason;
                        }

                        Log(LogType.Log, "New status: {0} (Reason: {1})", status, reason);
                        break;
                    case NetIncomingMessageType.Data:
                        _readerStream.ClearWriteReset(x => x.Write(msg.Data, 0, msg.LengthBytes));
                        var value = MessageTable.Deserialize(_readerStream);
                        HandleMessage(client, value);
                        break;
                    default:
                        Log(LogType.Error, "Unhandled type: {0}", msg.MessageType);
                        break;
                }

                Peer.Recycle(msg);
            }

            return received.Count > 0;
        }

        public override void FlushSentMessages()
        {
            Peer.FlushSendQueue();
        }

        protected override void OnClientDisconnected(IRemote client, string message)
        {
            ((Client) client).OnDisconnect(message);
        }

        protected override void OnShutdown()
        {
            Log(LogType.Log, "Shutting down");

            base.OnShutdown();

            Peer.Shutdown("Closing");
        }
    }
}
