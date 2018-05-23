#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject.Internal;

namespace Zenject
{
    public class PrefabSingletonProviderCreator
    {
        private readonly SingletonMarkRegistry _markRegistry;
        private readonly DiContainer _container;

        private readonly Dictionary<PrefabId, IPrefabInstantiator> _prefabCreators =
            new Dictionary<PrefabId, IPrefabInstantiator>();

        public PrefabSingletonProviderCreator(
            DiContainer container,
            SingletonMarkRegistry markRegistry)
        {
            _markRegistry = markRegistry;
            _container = container;
        }

        public IProvider CreateProvider(
            UnityEngine.Object prefab, Type resultType, GameObjectCreationParameters gameObjectBindInfo,
            List<TypeValuePair> extraArguments, object concreteIdentifier, Func<Type, IPrefabInstantiator, IProvider> providerFactory)
        {
            IPrefabInstantiator creator;

            var prefabId = new PrefabId(concreteIdentifier, prefab);

            _markRegistry.MarkSingleton(
                resultType, concreteIdentifier, SingletonTypes.FromPrefab);

            if (_prefabCreators.TryGetValue(prefabId, out creator))
            {
                // TODO: Check the arguments are the same?
                Assert.That(creator.ExtraArguments.IsEmpty() && extraArguments.IsEmpty(),
                    "Ambiguous creation parameters (arguments) when using ToPrefab with AsSingle");

                Assert.IsEqual(creator.GameObjectCreationParameters, gameObjectBindInfo,
                    "Ambiguous creation parameters (game object naming/parent info) when using ToPrefab with AsSingle");
            }
            else
            {
                creator = new PrefabInstantiatorCached(
                    new PrefabInstantiator(
                        _container, gameObjectBindInfo, resultType, extraArguments, new PrefabProvider(prefab)));

                _prefabCreators.Add(prefabId, creator);
            }

            if (resultType == typeof(GameObject))
            {
                return new PrefabGameObjectProvider(creator);
            }

            return providerFactory(resultType, creator);
        }

        private class PrefabId : IEquatable<PrefabId>
        {
            public readonly object ConcreteIdentifier;
            public readonly UnityEngine.Object Prefab;

            public PrefabId(object concreteIdentifier, UnityEngine.Object prefab)
            {
                Assert.IsNotNull(prefab);

                ConcreteIdentifier = concreteIdentifier;
                Prefab = prefab;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 29 + (this.ConcreteIdentifier == null ? 0 : this.ConcreteIdentifier.GetHashCode());
                    hash = hash * 29 + (ZenUtilInternal.IsNull(this.Prefab) ? 0 : this.Prefab.GetHashCode());
                    return hash;
                }
            }

            public override bool Equals(object other)
            {
                if (other is PrefabId)
                {
                    PrefabId otherId = (PrefabId)other;
                    return otherId == this;
                }
                else
                {
                    return false;
                }
            }

            public bool Equals(PrefabId that)
            {
                return this == that;
            }

            public static bool operator ==(PrefabId left, PrefabId right)
            {
                return object.Equals(left.Prefab, right.Prefab) && object.Equals(left.ConcreteIdentifier, right.ConcreteIdentifier);
            }

            public static bool operator !=(PrefabId left, PrefabId right)
            {
                return !left.Equals(right);
            }
        }
    }
}

#endif