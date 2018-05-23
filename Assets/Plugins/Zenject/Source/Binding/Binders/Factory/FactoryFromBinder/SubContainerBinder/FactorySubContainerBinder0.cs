using System;

namespace Zenject
{
    public class FactorySubContainerBinder<TContract>
        : FactorySubContainerBinderBase<TContract>
    {
        public FactorySubContainerBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo, object subIdentifier)
            : base(bindInfo, factoryBindInfo, subIdentifier)
        {
        }

        public ConditionCopyNonLazyBinder ByMethod(Action<DiContainer> installerMethod)
        {
            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByMethod(
                        container, installerMethod));

            return new ConditionCopyNonLazyBinder(BindInfo);
        }

#if !NOT_UNITY3D

        public NameTransformConditionCopyNonLazyBinder ByNewPrefabMethod(
            UnityEngine.Object prefab, Action<DiContainer> installerMethod)
        {
            BindingUtil.AssertIsValidPrefab(prefab);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabMethod(
                        container,
                        new PrefabProvider(prefab),
                        gameObjectInfo, installerMethod));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder ByNewPrefabResourceMethod(
            string resourcePath, Action<DiContainer> installerMethod)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabMethod(
                        container,
                        new PrefabProviderResource(resourcePath),
                        gameObjectInfo, installerMethod));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder ByNewPrefab(UnityEngine.Object prefab)
        {
            BindingUtil.AssertIsValidPrefab(prefab);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefab(
                        container,
                        new PrefabProvider(prefab),
                        gameObjectInfo));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder ByNewPrefabResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefab(
                        container,
                        new PrefabProviderResource(resourcePath),
                        gameObjectInfo));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

#endif
    }
}