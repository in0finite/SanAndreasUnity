using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoBuf;
using UnityEngine;

namespace Facepunch.Networking
{
    public static class MethodDispatcher<TAttrib, TReturnBase, TMessageBase>
        where TReturnBase : class
        where TAttrib : Attribute
        where TMessageBase : class, IMessage
    {
        public delegate TReturn Delegate<out TReturn, in TMessage>(TMessage message)
            where TReturn : TReturnBase
            where TMessage : TMessageBase;

        public delegate TReturnBase Delegate(TMessageBase message);

        private static bool _searchedForMethods = false;

        private static readonly Dictionary<Type, Delegate> _delegates
            = new Dictionary<Type, Delegate>();

        public static void RegisterMethod<TReturn, TMessage>(Delegate<TReturn, TMessage> handler)
            where TReturn : TReturnBase
            where TMessage : TMessageBase
        {
            RegisterMethod(typeof(TMessage), message => handler((TMessage) message));
        }

        public static void RegisterMethod(Type messageType, Delegate handler)
        {
            if (!messageType.GetInterfaces().Contains(typeof(TMessageBase))) {
                throw new Exception(String.Format("Type '{0}' does not implement "
                    + "'{1}'.", messageType, typeof(TMessageBase)));
            }

            if (_delegates.ContainsKey(messageType)) {
                Debug.LogWarningFormat("Handler method for messages of type "
                    + "'{0}' already registered.", messageType);

                return;
            }

            _delegates.Add(messageType, handler);
        }

        private static void SearchForServerSpawnMethods()
        {
            _searchedForMethods = true;

            var thisType = typeof(MethodDispatcher<TAttrib, TReturnBase, TMessageBase>);
            var baseType = typeof(TReturnBase);
            var delegMethod = typeof(Delegate).GetMethod("Invoke");

            var registerMethodGeneric = thisType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == "RegisterMethod" && x.IsGenericMethodDefinition);

            var delegTypeGeneric = typeof (Delegate<TReturnBase, TMessageBase>)
                .GetGenericTypeDefinition();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
                if (!baseType.IsAssignableFrom(type)) continue;

                foreach (var method in type.GetMethods(BindingFlags.Static
                    | BindingFlags.Public | BindingFlags.NonPublic)) {

                    if (method.DeclaringType != type) continue;
                    if (method.GetAttribute<TAttrib>(false) == null) continue;

                    if (!method.Matches(delegMethod)) {
                        Debug.LogWarningFormat("Method '{0}.{1}' is not a valid '{2}'.",
                            type.FullName, method.Name, delegTypeGeneric);
                        continue;
                    }

                    var networkableType = method.ReturnType;
                    var messageType = method.GetParameters()[0].ParameterType;
                    var delegType = delegTypeGeneric.MakeGenericType(thisType.GetGenericArguments()
                        .Concat(new [] {networkableType, messageType}).ToArray());

                    var deleg = System.Delegate.CreateDelegate(delegType, method);

                    var registerMethod = registerMethodGeneric.MakeGenericMethod(networkableType, messageType);
                    registerMethod.Invoke(null, new object[] { deleg });
                }
            }
        }

        public static TReturnBase Dispatch(TMessageBase message)
        {
            if (!_searchedForMethods) {
                SearchForServerSpawnMethods();
            }

            var type = message.GetType();

            if (!_delegates.ContainsKey(type)) {
                throw new Exception(String.Format("Unknown message type '{0}'.", type));
            }

            return _delegates[type](message);
        }
    }
}
