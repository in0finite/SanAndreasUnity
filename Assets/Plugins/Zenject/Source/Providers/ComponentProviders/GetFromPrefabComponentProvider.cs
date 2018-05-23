#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class GetFromPrefabComponentProvider : IProvider
    {
        private readonly IPrefabInstantiator _prefabInstantiator;
        private readonly Type _componentType;

        // if concreteType is null we use the contract type from inject context
        public GetFromPrefabComponentProvider(
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

            // NOTE: Need to set includeInactive to true here, because prefabs are always
            // instantiated as disabled until injection occurs, so that Awake / OnEnabled is executed
            // after injection has occurred
            var allComponents = gameObject.GetComponentsInChildren(_componentType, true);

            Assert.That(allComponents.Length >= 1,
                "Expected to find at least one component with type '{0}' on prefab '{1}'",
                _componentType, _prefabInstantiator.GetPrefab().name);

            yield return allComponents.Cast<object>().ToList();

            // Now do injection
            while (hasMore)
            {
                hasMore = gameObjectRunner.MoveNext();
            }
        }
    }
}

#endif