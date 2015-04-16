using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Lidgren.Network;
using UnityEngine;
using ProtoBuf;

namespace Facepunch.Networking.Lidgren
{
    using Peer = NetClient;

    public class RemoteServerImpl : RemoteServer
    {
        public const String AppIdentifier = "Arcade";

        private NetConnection Connection { get { return Peer.Connections.FirstOrDefault(); } }

        public override Thread NetThread
        {
            get { return Peer.Thread; }
        }

        public override IPEndPoint RemoteEndPoint
        {
            get { return Connection == null ? null : Connection.RemoteEndPoint; }
        }

        public override ConnectionStatus ConnectionStatus
        {
            get { return Connection == null ? ConnectionStatus.None : (ConnectionStatus) Connection.Status; }
        }

        public override float AverageRoundTripTime
        {
            get { return Connection == null ? float.PositiveInfinity : Connection.AverageRoundtripTime; }
        }

        private static Peer CreatePeer()
        {
            var config = new NetPeerConfiguration(AppIdentifier);

            return new Peer(config);
        }

        protected Peer Peer { get; private set; }

        /// HACK: Not thread safe
        private readonly MemoryStream _writerStream;
        private readonly MemoryStream _readerStream;

        public override NetStatus NetStatus
        {
            get { return (NetStatus) Peer.Status; }
        }

        private RemoteServerImpl(Peer peer)
        {
            Peer = peer;
            Peer.Start();

            _writerStream = new MemoryStream();
            _readerStream = new MemoryStream();
        }

        public RemoteServerImpl(String hostname, int port)
            : this(CreatePeer())
        {
            Peer.Connect(hostname, port);
        }

        public override void SendMessage(INetworkMessage message, DeliveryMethod method, int sequenceChannel)
        {
            if (ConnectionStatus != Networking.ConnectionStatus.Connected) return;

            _writerStream.Clear();
            MessageTable.Serialize(_writerStream, message);

            var length = (int) _writerStream.Length;

            var msg = Peer.CreateMessage(length);
            msg.Write(_writerStream.GetBuffer(), 0, length);

            Peer.SendMessage(msg, (NetDeliveryMethod) method, sequenceChannel);
        }

        public override bool CheckForMessages()
        {
            var received = new List<NetIncomingMessage>();
            Peer.ReadMessages(received);

            foreach (var msg in received) {
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

                        Log(LogType.Log, "New status: {0} (Reason: {1})", status, reason);

                        if (status == NetConnectionStatus.Disconnected) {
                            OnDisconnect("");
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        try {
                            _readerStream.ClearWriteReset(x => x.Write(msg.Data, 0, msg.LengthBytes));
                            var value = MessageTable.Deserialize(_readerStream);
                            HandleMessage(this, value);
                        } catch (Exception e) {
                            Log(LogType.Error, e.ToString());
                        }
                        break;
                    default:
                        Log(LogType.Error, "Unhandled type: {0}", msg.MessageType);
                        break;
                }
            }

            Peer.Recycle(received);

            return received.Count > 0;
        }

        public override void FlushSentMessages()
        {
            Peer.FlushSendQueue();
        }

        public override void Disconnect(string message)
        {
            Peer.Disconnect(message);
            OnDisconnect(message);
        }

        protected override void OnShutdown()
        {
            Log(LogType.Log, "Shutting down");

            base.OnShutdown();

            switch (ConnectionStatus) {
                case ConnectionStatus.Disconnected:
                case ConnectionStatus.Disconnecting:
                case ConnectionStatus.None:
                    break;
                default:
                    Disconnect("Closing");
                    break;
            }

            Peer.Shutdown("Closing");
        }
    }
}
