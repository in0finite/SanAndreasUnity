using System;
using System.Linq;
using System.Collections.Generic;
using ProtoBuf;

namespace Facepunch.Networking
{
    public sealed class Group : IDisposable
    {

#if PROTOBUF

        package ProtoBuf;

        //:baseclass = INetworkMessage
        message NetworkablesRemoved
        {
            repeated NetworkableInfo Networkables = 1;
        }

#endif

        private readonly Dictionary<uint, Networkable> _networkables;
        private readonly HashSet<IRemote> _subscribers;

        public Server Server { get; private set; }

        public uint UniqueId { get; private set; }

        public IEnumerable<IRemote> Subscribers { get { return _subscribers; } }

        internal Group(Server server, uint uniqueId)
        {
            _networkables = new Dictionary<uint,Networkable>();
            _subscribers = new HashSet<IRemote>();

            Server = server;
            UniqueId = uniqueId;
        }

        public void Add(Networkable networkable)
        {
            if (networkable.Group == this) return;

            var firstObservers = Subscribers;

            if (networkable.Group != null) {
                firstObservers = Subscribers.Where(x => !networkable.Group.IsSubscribed(x));
                networkable.Group.RemoveInternal(networkable);
            } else {
                networkable.InitializeServerside(Server);
            }

            _networkables.Add(networkable.UniqueId, networkable);
            networkable.SetGroupInternal(this);

            networkable.FirstObserve(firstObservers);
        }

        public bool IsSubscribed(IRemote client)
        {
            return _subscribers.Contains(client);
        }

        public void AddSubscriber(IRemote client)
        {
            if (!_subscribers.Add(client)) return;

            var arr = new [] { client };
            foreach (var networkable in _networkables.Values) {
                networkable.FirstObserve(arr);
            }
        }

        public void RemoveSubscriber(IRemote client)
        {
            _subscribers.Remove(client);

            Server.Net.SendMessage(new NetworkablesRemoved {
                Networkables = _networkables.Select(x => x.Value.Info).ToList()
            }, Subscribers, DeliveryMethod.ReliableOrdered, 0);
        }

        private bool RemoveInternal(Networkable networkable)
        {
            if (networkable.Group != this) return false;

            if (_networkables[networkable.UniqueId] != networkable) {
                throw new Exception("Networkable identifier conflict.");
            }

            _networkables.Remove(networkable.UniqueId);
            networkable.SetGroupInternal(null);

            return true;
        }

        public void Remove(Networkable networkable)
        {
            if (!RemoveInternal(networkable)) return;

            Server.Net.SendMessage(new NetworkablesRemoved {
                Networkables = new List<NetworkableInfo> { networkable.Info }
            }, Subscribers, DeliveryMethod.ReliableOrdered, 0);
        }

        internal void Update()
        {
            if (!Subscribers.Any()) return;

            foreach (var networkable in _networkables.Values) {
                networkable.ServerNetworkingUpdate(Subscribers);
            }
        }

        public void Dispose()
        {
            if (UniqueId == 0) return;

            Server.DisposeGroup(this);
            UniqueId = 0;
        }
    }
}
