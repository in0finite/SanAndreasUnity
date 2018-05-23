#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class InstantiateOnPrefabComponentProvider : IProvider
    {
        private readonly IPrefabInstantiator _prefabInstantiator;
        private readonly Type _componentType;

        // if concreteType is null we use the contract type from inject context
        public InstantiateOnPrefabComponentProvider(
            Type componentType,
            IPrefabInstantiator prefabInstantiator)
        {
            _prefabInstantiator = prefabInstantiator;
            _componentType = componentType;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _componentType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsNotNull(context);

            var gameObjectRunner = _prefabInstantiator.Instantiate(args);

            // First get instance
            bool hasMore = gameObjectRunner.MoveNext();

            var gameObject = gameObjectRunner.Current;
            var component = gameObject.AddComponent(_componentType);

            yield return new List<object>() { component };

            // Now do injection
            while (hasMore)
            {
                hasMore = gameObjectRunner.MoveNext();
            }
        }
    }
}

#endif