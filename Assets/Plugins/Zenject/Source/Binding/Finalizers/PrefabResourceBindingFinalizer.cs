#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class PrefabResourceBindingFinalizer : ProviderBindingFinalizer
    {
        private readonly GameObjectCreationParameters _gameObjectBindInfo;
        private readonly string _resourcePath;
        private readonly Func<Type, IPrefabInstantiator, IProvider> _providerFactory;

        public PrefabResourceBindingFinalizer(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectBindInfo,
            string resourcePath, Func<Type, IPrefabInstantiator, IProvider> providerFactory)
            : base(bindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _resourcePath = resourcePath;
            _providerFactory = providerFactory;
        }

        protected override void OnFinalizeBinding(DiContainer container)
        {
            if (BindInfo.ToChoice == ToChoices.Self)
            {
                Assert.IsEmpty(BindInfo.ToTypes);
                FinalizeBindingSelf(container);
            }
            else
            {
                FinalizeBindingConcrete(container, BindInfo.ToTypes);
            }
        }

        private void FinalizeBindingConcrete(DiContainer container, List<Type> concreteTypes)
        {
            switch (GetScope())
            {
                case ScopeTypes.Singleton:
                    {
                        RegisterProvidersForAllContractsPerConcreteType(
                            container,
                            concreteTypes,
                            (_, concreteType) => container.SingletonProviderCreator.CreateProviderForPrefabResource(
                                _resourcePath,
                                concreteType,
                                _gameObjectBindInfo,
                                BindInfo.Arguments,
                                BindInfo.ConcreteIdentifier, _providerFactory));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        RegisterProvidersForAllContractsPerConcreteType(
                            container,
                            concreteTypes,
                            (_, concreteType) =>
                                _providerFactory(
                                    concreteType,
                                    new PrefabInstantiator(
                                        container,
                                        _gameObjectBindInfo,
                                        concreteType,
                                        BindInfo.Arguments,
                                        new PrefabProviderResource(_resourcePath))));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var argumentTarget = concreteTypes.OnlyOrDefault();

                        if (argumentTarget == null)
                        {
                            Assert.That(BindInfo.Arguments.IsEmpty(),
                                "Cannot provide arguments to prefab instantiator when using more than one concrete type");
                        }

                        var prefabCreator = new PrefabInstantiatorCached(
                            new PrefabInstantiator(
                                container,
                                _gameObjectBindInfo,
                                argumentTarget,
                                BindInfo.Arguments,
                                new PrefabProviderResource(_resourcePath)));

                        RegisterProvidersForAllContractsPerConcreteType(
                            container,
                            concreteTypes,
                            (_, concreteType) => new CachedProvider(
                                _providerFactory(concreteType, prefabCreator)));
                        break;
                    }
                default:
                    {
                        throw Assert.CreateException();
                    }
            }
        }

        private void FinalizeBindingSelf(DiContainer container)
        {
            switch (GetScope())
            {
                case ScopeTypes.Singleton:
                    {
                        RegisterProviderPerContract(
                            container,
                            (_, contractType) => container.SingletonProviderCreator.CreateProviderForPrefabResource(
                                _resourcePath,
                                contractType,
                                _gameObjectBindInfo,
                                BindInfo.Arguments,
                                BindInfo.ConcreteIdentifier, _providerFactory));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        RegisterProviderPerContract(
                            container,
                            (_, contractType) =>
                                _providerFactory(
                                    contractType,
                                    new PrefabInstantiator(
                                        container,
                                        _gameObjectBindInfo,
                                        contractType,
                                        BindInfo.Arguments,
                                        new PrefabProviderResource(_resourcePath))));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var argumentTarget = BindInfo.ContractTypes.OnlyOrDefault();

                        if (argumentTarget == null)
                        {
                            Assert.That(BindInfo.Arguments.IsEmpty(),
                                "Cannot provide arguments to prefab instantiator when using more than one concrete type");
                        }

                        var prefabCreator = new PrefabInstantiatorCached(
                            new PrefabInstantiator(
                                container,
                                _gameObjectBindInfo,
                                argumentTarget,
                                BindInfo.Arguments,
                                new PrefabProviderResource(_resourcePath)));

                        RegisterProviderPerContract(
                            container,
                            (_, contractType) =>
                                new CachedProvider(
                                    _providerFactory(contractType, prefabCreator)));
                        break;
                    }
                default:
                    {
                        throw Assert.CreateException();
                    }
            }
        }
    }
}

#endif