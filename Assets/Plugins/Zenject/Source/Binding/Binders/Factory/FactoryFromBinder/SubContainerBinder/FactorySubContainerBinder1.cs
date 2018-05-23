using System;

namespace Zenject
{
    public class FactorySubContainerBinder<TParam1, TContract>
        : FactorySubContainerBinderWithParams<TContract>
    {
        public FactorySubContainerBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo, object subIdentifier)
            : base(bindInfo, factoryBindInfo, subIdentifier)
        {
        }

        public ConditionCopyNonLazyBinder ByMethod(Action<DiContainer, TParam1> installerMethod)
        {
            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByMethod<TParam1>(
                        container, installerMethod));

            return new ConditionCopyNonLazyBinder(BindInfo);
        }

#if !NOT_UNITY3D

        public NameTransformConditionCopyNonLazyBinder ByNewPrefabMethod(
            UnityEngine.Object prefab, Action<DiContainer, TParam1> installerMethod)
        {
            BindingUtil.AssertIsValidPrefab(prefab);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabMethod<TParam1>(
                        container,
                        new PrefabProvider(prefab),
                        gameObjectInfo, installerMethod));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder ByNewPrefabResourceMethod(
            string resourcePath, Action<DiContainer, TParam1> installerMethod)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabMethod<TParam1>(
                        container,
                        new PrefabProviderResource(resourcePath),
                        gameObjectInfo, installerMethod));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

#endif
    }
}