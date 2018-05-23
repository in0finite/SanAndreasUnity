namespace Zenject
{
    public class FactorySubContainerBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TContract>
        : FactorySubContainerBinderWithParams<TContract>
    {
        public FactorySubContainerBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo, object subIdentifier)
            : base(bindInfo, factoryBindInfo, subIdentifier)
        {
        }

        public ConditionCopyNonLazyBinder ByMethod(ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5> installerMethod)
        {
            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByMethod<TParam1, TParam2, TParam3, TParam4, TParam5>(
                        container, installerMethod));

            return new ConditionCopyNonLazyBinder(BindInfo);
        }

#if !NOT_UNITY3D

        public NameTransformConditionCopyNonLazyBinder ByNewPrefabMethod(
            UnityEngine.Object prefab, ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5> installerMethod)
        {
            BindingUtil.AssertIsValidPrefab(prefab);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabMethod<TParam1, TParam2, TParam3, TParam4, TParam5>(
                        container,
                        new PrefabProvider(prefab),
                        gameObjectInfo, installerMethod));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder ByNewPrefabResourceMethod(
            string resourcePath, ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5> installerMethod)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabMethod<TParam1, TParam2, TParam3, TParam4, TParam5>(
                        container,
                        new PrefabProviderResource(resourcePath),
                        gameObjectInfo, installerMethod));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

#endif
    }
}