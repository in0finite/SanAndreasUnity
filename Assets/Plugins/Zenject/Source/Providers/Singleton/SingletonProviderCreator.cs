using System;
using System.Collections.Generic;

namespace Zenject
{
    public class SingletonProviderCreator
    {
        private readonly StandardSingletonProviderCreator _standardProviderCreator;
        private readonly SubContainerSingletonProviderCreatorByMethod _subContainerMethodProviderCreator;
        private readonly SubContainerSingletonProviderCreatorByInstaller _subContainerInstallerProviderCreator;

#if !NOT_UNITY3D
        private readonly SubContainerSingletonProviderCreatorByNewPrefab _subContainerPrefabProviderCreator;
        private readonly SubContainerSingletonProviderCreatorByNewPrefabResource _subContainerPrefabResourceProviderCreator;

        private readonly PrefabSingletonProviderCreator _prefabProviderCreator;
        private readonly PrefabResourceSingletonProviderCreator _prefabResourceProviderCreator;
#endif

        public SingletonProviderCreator(
            DiContainer container, SingletonMarkRegistry markRegistry)
        {
            _standardProviderCreator = new StandardSingletonProviderCreator(container, markRegistry);

            _subContainerMethodProviderCreator = new SubContainerSingletonProviderCreatorByMethod(container, markRegistry);
            _subContainerInstallerProviderCreator = new SubContainerSingletonProviderCreatorByInstaller(container, markRegistry);

#if !NOT_UNITY3D
            _subContainerPrefabProviderCreator = new SubContainerSingletonProviderCreatorByNewPrefab(container, markRegistry);
            _subContainerPrefabResourceProviderCreator = new SubContainerSingletonProviderCreatorByNewPrefabResource(container, markRegistry);

            _prefabProviderCreator = new PrefabSingletonProviderCreator(container, markRegistry);
            _prefabResourceProviderCreator = new PrefabResourceSingletonProviderCreator(container, markRegistry);
#endif
        }

        public IProvider CreateProviderStandard(
            StandardSingletonDeclaration dec, Func<DiContainer, Type, IProvider> providerCreator)
        {
            return _standardProviderCreator.GetOrCreateProvider(dec, providerCreator);
        }

        public IProvider CreateProviderForSubContainerMethod(
            Type resultType, object concreteIdentifier,
            Action<DiContainer> installMethod, object identifier)
        {
            return _subContainerMethodProviderCreator.CreateProvider(
                resultType, concreteIdentifier, installMethod, identifier);
        }

        public IProvider CreateProviderForSubContainerInstaller(
            Type resultType, object concreteIdentifier,
            Type installerType, object identifier)
        {
            return _subContainerInstallerProviderCreator.CreateProvider(
                resultType, concreteIdentifier, installerType, identifier);
        }

#if !NOT_UNITY3D

        public IProvider CreateProviderForPrefab(
            UnityEngine.Object prefab, Type resultType, GameObjectCreationParameters gameObjectBindInfo,
            List<TypeValuePair> extraArguments, object concreteIdentifier, Func<Type, IPrefabInstantiator, IProvider> providerFactory)
        {
            return _prefabProviderCreator.CreateProvider(
                prefab, resultType, gameObjectBindInfo,
                extraArguments, concreteIdentifier, providerFactory);
        }

        public IProvider CreateProviderForPrefabResource(
            string resourcePath, Type resultType, GameObjectCreationParameters gameObjectBindInfo,
            List<TypeValuePair> extraArguments, object concreteIdentifier, Func<Type, IPrefabInstantiator, IProvider> providerFactory)
        {
            return _prefabResourceProviderCreator.CreateProvider(
                resourcePath, resultType, gameObjectBindInfo, extraArguments, concreteIdentifier, providerFactory);
        }

        public IProvider CreateProviderForSubContainerPrefab(
            Type resultType, object concreteIdentifier, GameObjectCreationParameters gameObjectBindInfo,
            UnityEngine.Object prefab, object identifier)
        {
            return _subContainerPrefabProviderCreator.CreateProvider(
                resultType, concreteIdentifier, prefab, identifier, gameObjectBindInfo);
        }

        public IProvider CreateProviderForSubContainerPrefabResource(
            Type resultType, object concreteIdentifier, GameObjectCreationParameters gameObjectBindInfo,
            string resourcePath, object identifier)
        {
            return _subContainerPrefabResourceProviderCreator.CreateProvider(
                resultType, concreteIdentifier, resourcePath, identifier, gameObjectBindInfo);
        }

#endif
    }
}