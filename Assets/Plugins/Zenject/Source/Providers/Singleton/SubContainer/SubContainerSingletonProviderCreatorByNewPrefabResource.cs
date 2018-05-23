#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class SubContainerSingletonProviderCreatorByNewPrefabResource
    {
        private readonly SingletonMarkRegistry _markRegistry;
        private readonly DiContainer _container;

        private readonly Dictionary<CustomSingletonId, CreatorInfo> _subContainerCreators =
            new Dictionary<CustomSingletonId, CreatorInfo>();

        public SubContainerSingletonProviderCreatorByNewPrefabResource(
            DiContainer container,
            SingletonMarkRegistry markRegistry)
        {
            _markRegistry = markRegistry;
            _container = container;
        }

        public IProvider CreateProvider(
            Type resultType, object concreteIdentifier, string resourcePath, object identifier, GameObjectCreationParameters gameObjectBindInfo)
        {
            _markRegistry.MarkSingleton(
                resultType, concreteIdentifier,
                SingletonTypes.FromSubContainerPrefabResource);

            var customSingletonId = new CustomSingletonId(
                concreteIdentifier, resourcePath);

            CreatorInfo creatorInfo;

            if (_subContainerCreators.TryGetValue(customSingletonId, out creatorInfo))
            {
                Assert.IsEqual(creatorInfo.GameObjectCreationParameters, gameObjectBindInfo,
                    "Ambiguous creation parameters (game object name / parent info) when using ToSubContainerPrefab with AsSingle");
            }
            else
            {
                var creator = new SubContainerCreatorCached(
                    new SubContainerCreatorByNewPrefab(
                        _container, new PrefabProviderResource(resourcePath), gameObjectBindInfo));

                creatorInfo = new CreatorInfo(
                    gameObjectBindInfo, creator);

                _subContainerCreators.Add(customSingletonId, creatorInfo);
            }

            return new SubContainerDependencyProvider(
                resultType, identifier, creatorInfo.Creator);
        }

        private class CustomSingletonId : IEquatable<CustomSingletonId>
        {
            public readonly object ConcreteIdentifier;
            public readonly string ResourcePath;

            public CustomSingletonId(object concreteIdentifier, string resourcePath)
            {
                Assert.IsNotNull(resourcePath);

                ConcreteIdentifier = concreteIdentifier;
                ResourcePath = resourcePath;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 29 + (this.ConcreteIdentifier == null ? 0 : this.ConcreteIdentifier.GetHashCode());
                    hash = hash * 29 + this.ResourcePath.GetHashCode();
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
                return object.Equals(left.ResourcePath, right.ResourcePath)
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
                GameObjectCreationParameters creationParameters, ISubContainerCreator creator)
            {
                GameObjectCreationParameters = creationParameters;
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