#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;
using Zenject.Internal;

namespace Zenject
{
    public class SubContainerSingletonProviderCreatorByNewPrefab
    {
        private readonly SingletonMarkRegistry _markRegistry;
        private readonly DiContainer _container;

        private readonly Dictionary<CustomSingletonId, CreatorInfo> _subContainerCreators =
            new Dictionary<CustomSingletonId, CreatorInfo>();

        public SubContainerSingletonProviderCreatorByNewPrefab(
            DiContainer container,
            SingletonMarkRegistry markRegistry)
        {
            _markRegistry = markRegistry;
            _container = container;
        }

        public IProvider CreateProvider(
            Type resultType, object concreteIdentifier, UnityEngine.Object prefab, object identifier,
            GameObjectCreationParameters gameObjectBindInfo)
        {
            _markRegistry.MarkSingleton(
                resultType, concreteIdentifier,
                SingletonTypes.FromSubContainerPrefab);

            var customSingletonId = new CustomSingletonId(
                concreteIdentifier, prefab);

            CreatorInfo creatorInfo;

            if (_subContainerCreators.TryGetValue(customSingletonId, out creatorInfo))
            {
                Assert.IsEqual(creatorInfo.GameObjectCreationParameters, gameObjectBindInfo,
                    "Ambiguous creation parameters (game object name/parent info) when using ToSubContainerPrefab with AsSingle");
            }
            else
            {
                var creator = new SubContainerCreatorCached(
                    new SubContainerCreatorByNewPrefab(_container, new PrefabProvider(prefab), gameObjectBindInfo));

                creatorInfo = new CreatorInfo(gameObjectBindInfo, creator);

                _subContainerCreators.Add(customSingletonId, creatorInfo);
            }

            return new SubContainerDependencyProvider(
                resultType, identifier, creatorInfo.Creator);
        }

        private class CustomSingletonId : IEquatable<CustomSingletonId>
        {
            public readonly object ConcreteIdentifier;
            public readonly UnityEngine.Object Prefab;

            public CustomSingletonId(object concreteIdentifier, UnityEngine.Object prefab)
            {
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
                if (other is CustomSingletonId)
                {
                    CustomSingletonId otherId = (CustomSingletonId)other;
                    return otherId == this;
                }
                else
                {
                    return false;
                }
            }

            public bool Equals(CustomSingletonId that)
            {
                return this == that;
            }

            public static bool operator ==(CustomSingletonId left, CustomSingletonId right)
            {
                return object.Equals(left.Prefab, right.Prefab)
                    && object.Equals(left.ConcreteIdentifier, right.ConcreteIdentifier);
            }

            public static bool operator !=(CustomSingletonId left, CustomSingletonId right)
            {
                return !left.Equals(right);
            }
        }

        private class CreatorInfo
        {
            public CreatorInfo(
                GameObjectCreationParameters gameObjectBindInfo, ISubContainerCreator creator)
            {
                GameObjectCreationParameters = gameObjectBindInfo;
                Creator = creator;
            }

            public GameObjectCreationParameters GameObjectCreationParameters
            {
                get;
                private set;
            }

            public ISubContainerCreator Creator
            {
                get;
                private set;
            }
        }
    }
}

#endif