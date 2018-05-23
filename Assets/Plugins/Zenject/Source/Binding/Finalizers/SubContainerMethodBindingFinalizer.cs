using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class SubContainerMethodBindingFinalizer : ProviderBindingFinalizer
    {
        private readonly object _subIdentifier;
        private readonly Action<DiContainer> _installMethod;

        public SubContainerMethodBindingFinalizer(
            BindInfo bindInfo, Action<DiContainer> installMethod, object subIdentifier)
            : base(bindInfo)
        {
            _subIdentifier = subIdentifier;
            _installMethod = installMethod;
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
                            (_, concreteType) =>
                                container.SingletonProviderCreator.CreateProviderForSubContainerMethod(
                                    concreteType,
                                    BindInfo.ConcreteIdentifier,
                                    _installMethod,
                                    _subIdentifier));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        // Note: each contract/concrete pair gets its own container here
                        RegisterProvidersPerContractAndConcreteType(
                            container,
                            concreteTypes,
                            (contractType, concreteType) => new SubContainerDependencyProvider(
                                concreteType, _subIdentifier,
                                new SubContainerCreatorByMethod(container, _installMethod)));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var creator = new SubContainerCreatorCached(
                            new SubContainerCreatorByMethod(container, _installMethod));

                        // Note: each contract/concrete pair gets its own container
                        RegisterProvidersForAllContractsPerConcreteType(
                            container,
                            concreteTypes,
                            (_, concreteType) => new SubContainerDependencyProvider(
                                concreteType, _subIdentifier, creator));
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
                            (_, contractType) => container.SingletonProviderCreator.CreateProviderForSubContainerMethod(
                                contractType,
                                BindInfo.ConcreteIdentifier,
                                _installMethod,
                                _subIdentifier));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        RegisterProviderPerContract(
                            container,
                            (_, contractType) => new SubContainerDependencyProvider(
                                contractType, _subIdentifier,
                                new SubContainerCreatorByMethod(
                                    container, _installMethod)));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var containerCreator = new SubContainerCreatorCached(
                            new SubContainerCreatorByMethod(container, _installMethod));

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