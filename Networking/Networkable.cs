using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using JetBrains.Annotations;
using ProtoBuf;
using UnityEngine;

namespace ProtoBuf
{
    /// <summary>
    /// Root message types (ones that aren't sent within another message)
    /// that are sent by a Networkable must implement this interface.
    /// </summary>
    public interface INetworkableMessage : INetworkMessage
    {
        NetworkableInfo Networkable { get; set; }
    }

    /// <summary>
    /// Root message types (ones that aren't sent within another message)
    /// that are sent to initialize a Networkable on the client must
    /// implement this interface.
    /// </summary>
    public interface INetworkableSpawnMessage : INetworkableMessage { }

#if PROTOBUF

    package ProtoBuf;

    message NetworkableInfo
    {
        required uint32 Ident = 1;
    }

    //:baseclass = INetworkableMessage
    message NetworkableRemoved
    {
        required NetworkableInfo Networkable = 1;
    }

    message Transform
    {
        optional UnityEngine.Vector3 Position = 1;
        optional UnityEngine.Quaternion Rotation = 2;
        optional UnityEngine.Vector3 Scale = 3;
    }

    message NetworkableSave
    {
        required MessageTableSchema Schema = 1;
        
        repeated bytes Entries = 2;
    }

#endif

}

namespace Facepunch.Networking
{
    /// <summary>
    /// Component for game objects that send data over the network.
    /// </summary>
    public abstract class Networkable : MonoBehaviour
    {
        /// <remarks>
        /// Assuming the prefab is named "Prefabs/Networkable/TNetworkable".
        /// </remarks>
        public static TNetworkable SpawnFromPrefab<TNetworkable>()
            where TNetworkable : Networkable
        {
            return SpawnFromPrefab<TNetworkable>(typeof(TNetworkable).Name);
        }

        /// <param name="path">Relative to "Prefabs/Networkable/".</param>
        public static TNetworkable SpawnFromPrefab<TNetworkable>(String path)
            where TNetworkable : Networkable
        {
            path = String.Format("Prefabs/Networkable/{0}", path);

            var prefab = Resources.Load<GameObject>(path);
            var inst = Instantiate(prefab);
            var netw = inst.GetComponent<TNetworkable>();

            netw.EditorId = 0;

            return netw;
        }

#if CLIENT
        [AttributeUsage(AttributeTargets.Method), MeansImplicitUse]
        protected sealed class ClientSpawnMethodAttribute : Attribute { }

        internal static Networkable ClientSpawn(Client client, INetworkableSpawnMessage message)
        {
            Networkable inst = null;

            if (NetConfig.IsServer) {
                var sv = FindObjectOfType<Server>();
                inst = sv.GetNetworkable(message.Networkable.Ident);
            }

            if (inst == null) {
                inst = MethodDispatcher<ClientSpawnMethodAttribute, Networkable, INetworkableSpawnMessage>.Dispatch(message);
            }

            inst.InitializeClientside(client, message.Networkable.Ident);

            return inst;
        }
#endif

        #region Message Handler Binding

        [AttributeUsage(AttributeTargets.Method), MeansImplicitUse]
        protected sealed class MessageHandlerAttribute : Attribute
        {
            public Domain Domain { get; private set; }

            public MessageHandlerAttribute(Domain domain)
            {
                Domain = domain;
            }
        }

        private static readonly Dictionary<Domain, Dictionary<Type, Action<Networkable>[]>> _sHandlerCache
            = new Dictionary<Domain, Dictionary<Type, Action<Networkable>[]>> {
#if CLIENT
                { Domain.Client, new Dictionary<Type, Action<Networkable>[]>() },
#endif
                { Domain.Server, new Dictionary<Type, Action<Networkable>[]>() }
            };

        private static IEnumerable<Action<Networkable>> GetHandlerBindersCacheMiss(Domain domain, Type type)
        {
            const BindingFlags bFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var delegType = typeof(MessageHandler<INetworkableMessage>).GetMethod("Invoke");
            var bindMethodGeneric = typeof(Networkable).GetMethod("BindHandler", BindingFlags.Instance | BindingFlags.NonPublic);
            var preboundTypeGeneric = typeof(PreboundMessageHandler<>);

            foreach (var method in type.GetMethods(bFlags)) {
                if (method.IsAbstract) continue;

                var attrib = method.GetAttribute<MessageHandlerAttribute>(true);
                if (attrib == null) continue;
                if (attrib.Domain != domain) continue;

                if (!method.Matches(delegType)) {
                    Debug.LogWarningFormat("Method '{0}.{1}' is not a valid MessageHandler.",
                        type.FullName, method.Name);
                    continue;
                }

                var messageType = method.GetParameters()[1].ParameterType;
                var preboundType = preboundTypeGeneric.MakeGenericType(messageType);

                var pTarget = Expression.Parameter(typeof(Networkable), "target");
                var pSender = Expression.Parameter(typeof(IRemote), "sender");
                var pMessage = Expression.Parameter(messageType, "message");
                var cTarget = Expression.Convert(pTarget, type);

                var call = Expression.Call(cTarget, method, pSender, pMessage);

                var deleg = Expression.Lambda(preboundType, call, pTarget, pSender, pMessage).Compile();

                var cDeleg = Expression.Constant(deleg);
                var cDomain = Expression.Constant(domain);
                var bindMethod = bindMethodGeneric.MakeGenericMethod(messageType);

                pTarget = Expression.Parameter(typeof(Networkable), "target");

                call = Expression.Call(pTarget, bindMethod, cDomain, cDeleg);

                yield return Expression.Lambda<Action<Networkable>>(call, pTarget).Compile();
            }
        }

        private static IEnumerable<Action<Networkable>> GetHandlerBinders(Domain domain, Type type)
        {
            if (!_sHandlerCache[domain].ContainsKey(type)) {
                _sHandlerCache[domain].Add(type, GetHandlerBindersCacheMiss(domain, type).ToArray());
            }

            return _sHandlerCache[domain][type];
        }

        private delegate void PreboundMessageHandler<in TMessage>(Networkable target, IRemote sender, TMessage message)
            where TMessage : INetworkableMessage;

        // ReSharper disable once UnusedMember.Local
        private void BindHandler<TMessage>(Domain domain, PreboundMessageHandler<TMessage> handler)
            where TMessage : INetworkableMessage
        {
            MessageHandler<TMessage> bound = (sender, message) => handler(this, sender, message);

            switch (domain) {
#if CLIENT
                case Domain.Client:
                    RegisterClientHandler(bound);
                    return;
#endif
                case Domain.Server:
                    RegisterServerHandler(bound);
                    return;
            }
        }

        private Domain _discoveredDomains = Domain.None;

        protected void DiscoverMessageHandlers(Domain domain)
        {
            if ((_discoveredDomains & domain) == domain) {
                return;
            }

            _discoveredDomains |= domain;

            var prebound = GetHandlerBinders(domain, GetType());

            foreach (var binder in prebound) {
                binder(this);
            }
        }
        #endregion

#if CLIENT
        private readonly MessageHandlerCollection<INetworkableMessage> _clientHandlers;
#endif

        private readonly MessageHandlerCollection<INetworkableMessage> _serverHandlers;

        private Server _server;
        protected Server Server { get { return IsServer ? _server : null; } }
        protected bool IsServer { get { return _server != null && _server.isActiveAndEnabled; } }

#if CLIENT
        private Client _client;
        protected Client Client { get { return IsClient ? _client : null; } }
#endif

        public long ServerTime { get { return IsServer ? Server.Time : IsClient ? Client.Time : 0; } }

        protected bool IsClient
        {
            get
            {
#if CLIENT
                return _client != null && _client.isActiveAndEnabled;
#else
                return false;
#endif
            }
        }

        [SerializeField, HideInInspector]
        private int _editorIdSerialized = 0;
        public uint EditorId
        {
            get { return (uint) _editorIdSerialized; }
            set { _editorIdSerialized = (int) value; }
        }

        public uint UniqueId { get; private set; }

        private readonly NetworkableInfo _info = new NetworkableInfo();
        public NetworkableInfo Info
        {
            get
            {
                _info.Ident = UniqueId;
                return _info;
            }
        }

        public Group Group { get; private set; }

        protected Networkable()
        {
#if CLIENT
            _clientHandlers = new MessageHandlerCollection<INetworkableMessage>();

            RegisterClientHandler<NetworkableRemoved>(ReceiveRemovedMessage);
#endif

            _serverHandlers = new MessageHandlerCollection<INetworkableMessage>();
        }

#if CLIENT
        private void ReceiveRemovedMessage(IRemote sender, NetworkableRemoved message)
        {
            if (IsServer) return;
            Destroy(gameObject);
        }
#endif

        internal void SetGroupInternal(Group group)
        {
            if (Server == null) return;

            Group = group;
        }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            if (EditorId != 0) StartCoroutine(GlobalNetworkableInit());

            OnAwake();
        }

        private IEnumerator GlobalNetworkableInit()
        {
            yield return null;

            var server = FindObjectOfType<Server>();

            if (server != null && server.isActiveAndEnabled) {
                while (server.NetStatus != NetStatus.Running) yield return null;

                server.GlobalGroup.Add(this);
            }

#if CLIENT
            var client = FindObjectOfType<Client>();

            if (client == null || !client.isActiveAndEnabled) yield break;

            while (client.ConnectionStatus != ConnectionStatus.Connected) yield return null;

            InitializeClientside(client, EditorId);
#endif
        }

        protected virtual void OnAwake() { }


#if CLIENT
        internal void InitializeClientside(Client client, uint uniqueId)
        {
            if (_client != null) return;

            _client = client;

            UniqueId = uniqueId;
            _client.RegisterNetworkable(this);

            OnClientInitialize();
        }

        protected virtual void OnClientInitialize()
        {
            DiscoverMessageHandlers(Domain.Client);
        }

        internal void HandleMessageFromServer(IRemote sender, INetworkableMessage message)
        {
            _clientHandlers.Handle(sender, message);
        }

        public void RegisterClientHandler<TMessage>(MessageHandler<TMessage> handler)
            where TMessage : INetworkableMessage
        {
            _clientHandlers.Add(handler);
        }

        public void SendToServer(INetworkableMessage message, DeliveryMethod deliveryMethod, int sequenceChannel)
        {
            message.Networkable = Info;
            Client.Net.SendMessage(message, deliveryMethod, sequenceChannel);
        }
#endif

        internal void InitializeServerside(Server server)
        {
            if (_server != null) return;

            _server = server;

            UniqueId = server.RegisterNetworkable(this);

            OnServerInitialize();

#if CLIENT
            if (Server.LocalClient != null) {
                InitializeClientside(Server.LocalClient, UniqueId);
            }
#endif
        }

        protected virtual void OnServerInitialize()
        {
            DiscoverMessageHandlers(Domain.Server);
        }

        internal void HandleMessageFromClient(IRemote sender, INetworkableMessage message)
        {
            _serverHandlers.Handle(sender, message);
        }

        public void RegisterServerHandler<TMessage>(MessageHandler<TMessage> handler)
            where TMessage : INetworkableMessage
        {
            _serverHandlers.Add(handler);
        }

        public void SendToClients(INetworkableMessage message, DeliveryMethod deliveryMethod, int sequenceChannel)
        {
            SendToClients(message, Group.Subscribers, deliveryMethod, sequenceChannel);
        }

        public void SendToClient(INetworkableMessage message, IRemote client, DeliveryMethod deliveryMethod,
            int sequenceChannel)
        {
            message.Networkable = Info;
            Server.Net.SendMessage(message, client, deliveryMethod, sequenceChannel);
        }


        public void SendToClients(INetworkableMessage message, IEnumerable<IRemote> clients, DeliveryMethod deliveryMethod, int sequenceChannel)
        {
            message.Networkable = Info;
            Server.Net.SendMessage(message, clients, deliveryMethod, sequenceChannel);
        }

        internal void FirstObserve(IEnumerable<IRemote> clients)
        {
            OnFirstObserve(clients);
        }

        /// <summary>
        /// Serverside method called when this Networkable should be spawned on one or
        /// more remote clients.
        /// </summary>
        protected virtual void OnFirstObserve(IEnumerable<IRemote> clients) { }

        public ProtoBuf.Transform TransformSnapshot
        {
            get
            {
                return new ProtoBuf.Transform {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    Scale = transform.localScale
                };
            }

            set
            {
                transform.position = value.Position;
                transform.rotation = value.Rotation;
                transform.localScale = value.Scale;
            }
        }

        public void UpdateTransform(ProtoBuf.Transform trans)
        {
            transform.position = trans.Position;
            transform.rotation = trans.Rotation;
            transform.localScale = trans.Scale;
        }

        public void UpdateTransform(ProtoBuf.Transform from, ProtoBuf.Transform to, float t)
        {
            t = Mathf.Clamp01(t);
            var s = 1 - t;

            transform.position = s * from.Position + t * to.Position;
            transform.rotation = Quaternion.Lerp(from.Rotation, to.Rotation, t);
            transform.localScale = s * from.Scale + t * to.Scale;
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            OnUpdate();
        }

        protected virtual void OnUpdate() { }

#if CLIENT
        internal void ClientNetworkingUpdate()
        {
            // TODO: Batch?
            OnClientNetworkingUpdate();
        }

        protected virtual void OnClientNetworkingUpdate() { }
#endif

        internal void ServerNetworkingUpdate(IEnumerable<IRemote> subscribers)
        {
            // TODO: Batch?
            OnServerNetworkingUpdate(subscribers);
        }

        protected virtual void OnServerNetworkingUpdate(IEnumerable<IRemote> subscribers) { }

        // ReSharper disable once UnusedMember.Local
        private void OnDestroy()
        {
            OnDestroyed();

            if (UniqueId == 0) return;

#if CLIENT
            if (IsClient) {
                Client.ForgetNetworkable(this);
            }
#endif

            if (IsServer) {
                Server.RemoveNetworkable(this);
            }

            UniqueId = 0;
        }

        protected virtual void OnDestroyed() { }
    }

    public class Networkable<TClient, TServer> : Networkable
        where TClient : Client
        where TServer : Server
    {
        protected new TServer Server { get { return (TServer) base.Server; } }

#if CLIENT
        protected new TClient Client { get { return (TClient) base.Client; } }
#endif
    }
}
