#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class SubContainerPrefabResourceBindingFinalizer : ProviderBindingFinalizer
    {
        private readonly string _resourcePath;
        private readonly object _subIdentifier;
        private readonly GameObjectCreationParameters _gameObjectBindInfo;

        public SubContainerPrefabResourceBindingFinalizer(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectBindInfo,
            string resourcePath,
            object subIdentifier)
            : base(bindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _subIdentifier = subIdentifier;
            _resourcePath = resourcePath;
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
                            (_, concreteType) => container.SingletonProviderCreator.CreateProviderForSubContainerPrefabResource(
                                concreteType,
                                BindInfo.ConcreteIdentifier,
                                _gameObjectBindInfo,
                                _resourcePath,
                                _subIdentifier));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        RegisterProvidersForAllContractsPerConcreteType(
                            container,
                            concreteTypes,
                            (_, concreteType) => new SubContainerDependencyProvider(
                                concreteType, _subIdentifier,
                                new SubContainerCreatorByNewPrefab(
                                    container, new PrefabProviderResource(_resourcePath), _gameObjectBindInfo)));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var containerCreator = new SubContainerCreatorCached(
                            new SubContainerCreatorByNewPrefab(container, new PrefabProviderResource(_resourcePath), _gameObjectBindInfo));

                        RegisterProvidersForAllContractsPerConcreteType(
                            container,
                            concreteTypes,
                            (_, concreteType) =>
                            new SubContainerDependencyProvider(
                                concreteType, _subIdentifier, containerCreator));
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
                            (_, contractType) => container.SingletonProviderCreator.CreateProviderForSubContainerPrefabResource(
                                contractType,
                                BindInfo.ConcreteIdentifier,
                                _gameObjectBindInfo,
                                _resourcePath,
                                _subIdentifier));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        RegisterProviderPerContract(
                            container,
                            (_, contractType) => new SubContainerDependencyProvider(
                                contractType, _subIdentifier,
                                new SubContainerCreatorByNewPrefab(
                                    container, new PrefabProviderResource(_resourcePath), _gameObjectBindInfo)));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var containerCreator = new SubContainerCreatorCached(
                            new SubContainerCreatorByNewPrefab(
                                container, new PrefabProviderResource(_resourcePath), _gameObjectBindInfo));

                        RegisterProviderPerContract(
                            container,
                            (_, contractType) =>
                            new SubContainerDependencyProvider(
                                contractType, _subIdentifier, containerCreator));
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