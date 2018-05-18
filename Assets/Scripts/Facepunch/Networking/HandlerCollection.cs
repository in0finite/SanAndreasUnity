using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Facepunch.Networking
{
    public class MessageHandlerCollection<TBaseMessage>
        where TBaseMessage : class, INetworkMessage
    {
        // ReSharper disable once StaticFieldInGenericType
        private static readonly Dictionary<Type, Type[]> _sHandlableTypeCache = new Dictionary<Type, Type[]>();

        private readonly Dictionary<Type, List<MessageHandler<TBaseMessage>>> _messageHandlers;

        public MessageHandlerCollection()
        {
            _messageHandlers = new Dictionary<Type, List<MessageHandler<TBaseMessage>>>();
        }

        public bool CanHandle(Type type)
        {
            return _messageHandlers.ContainsKey(type);
        }

        public void Handle(IRemote sender, TBaseMessage message)
        {
            if (message == null) return;

            var type = message.GetType();
            if (!CanHandle(type)) return;

            foreach (var handler in _messageHandlers[type])
            {
                handler(sender, message);
            }
        }

        private static IEnumerable<Type> GetHandlableTypesCacheMiss(Type type)
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(type.IsAssignableFrom);
        }

        private static IEnumerable<Type> GetHandlableTypes(Type type)
        {
            if (!_sHandlableTypeCache.ContainsKey(type))
            {
                _sHandlableTypeCache.Add(type, GetHandlableTypesCacheMiss(type).ToArray());
            }

            return _sHandlableTypeCache[type];
        }

        public void Add<TMessage>(MessageHandler<TMessage> handler)
            where TMessage : TBaseMessage
        {
            MessageHandler<TBaseMessage> action = (sender, message) => handler(sender, (TMessage)message);

            foreach (var type in GetHandlableTypes(typeof(TMessage)))
            {
                if (!_messageHandlers.ContainsKey(type))
                {
                    _messageHandlers.Add(type, new List<MessageHandler<TBaseMessage>>());
                }

                _messageHandlers[type].Add(action);
            }
        }

        public void Clear()
        {
            _messageHandlers.Clear();
        }
    }
}