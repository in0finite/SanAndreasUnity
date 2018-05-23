using ModestTree;
using System;

namespace Zenject
{
    public class FactorySubContainerBinderWithParams<TContract> : FactorySubContainerBinderBase<TContract>
    {
        public FactorySubContainerBinderWithParams(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo, object subIdentifier)
            : base(bindInfo, factoryBindInfo, subIdentifier)
        {
        }

#if !NOT_UNITY3D

        public NameTransformConditionCopyNonLazyBinder ByNewPrefab<TInstaller>(UnityEngine.Object prefab)
            where TInstaller : IInstaller
        {
            return ByNewPrefab(typeof(TInstaller), prefab);
        }

        public NameTransformConditionCopyNonLazyBinder ByNewPrefab(Type installerType, UnityEngine.Object prefab)
        {
            BindingUtil.AssertIsValidPrefab(prefab);

            Assert.That(installerType.DerivesFrom<MonoInstaller>(),
                "Invalid installer type given during bind command.  Expected type '{0}' to derive from 'MonoInstaller'", installerType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabWithParams(
                        installerType,
                        container,
                        new PrefabProvider(prefab),
                        gameObjectInfo));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder ByNewPrefabResource<TInstaller>(string resourcePath)
            where TInstaller : IInstaller
        {
            return ByNewPrefabResource(typeof(TInstaller), resourcePath);
        }

        public NameTransformConditionCopyNonLazyBinder ByNewPrefabResource(
            Type installerType, string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabWithParams(
                        installerType,
                        container,
                        new PrefabProviderResource(resourcePath),
                        gameObjectInfo));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

#endif
    }
}