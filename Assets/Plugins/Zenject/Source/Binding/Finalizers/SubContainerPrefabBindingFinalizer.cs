#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class SubContainerPrefabBindingFinalizer : ProviderBindingFinalizer
    {
        private readonly UnityEngine.Object _prefab;
        private readonly object _subIdentifier;
        private readonly GameObjectCreationParameters _gameObjectBindInfo;

        public SubContainerPrefabBindingFinalizer(
            BindInfo bindInfo,
            GameObjectCreationParameters gameObjectBindInfo,
            UnityEngine.Object prefab,
            object subIdentifier)
            : base(bindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _prefab = prefab;
            _subIdentifier = subIdentifier;
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
                            (_, concreteType) => container.SingletonProviderCreator.CreateProviderForSubContainerPrefab(
                                concreteType,
                                BindInfo.ConcreteIdentifier,
                                _gameObjectBindInfo,
                                _prefab,
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
                                    container, new PrefabProvider(_prefab), _gameObjectBindInfo)));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var containerCreator = new SubContainerCreatorCached(
                            new SubContainerCreatorByNewPrefab(
                                container, new PrefabProvider(_prefab), _gameObjectBindInfo));

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
                            (_, contractType) => container.SingletonProviderCreator.CreateProviderForSubContainerPrefab(
                                contractType,
                                BindInfo.ConcreteIdentifier,
                                _gameObjectBindInfo,
                                _prefab,
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
                                    container, new PrefabProvider(_prefab), _gameObjectBindInfo)));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var containerCreator = new SubContainerCreatorCached(
                            new SubContainerCreatorByNewPrefab(
                                container, new PrefabProvider(_prefab), _gameObjectBindInfo));

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