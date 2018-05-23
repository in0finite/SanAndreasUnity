using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class SubContainerInstallerBindingFinalizer : ProviderBindingFinalizer
    {
        private readonly object _subIdentifier;
        private readonly Type _installerType;

        public SubContainerInstallerBindingFinalizer(
            BindInfo bindInfo, Type installerType, object subIdentifier)
            : base(bindInfo)
        {
            _subIdentifier = subIdentifier;
            _installerType = installerType;
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

        private ISubContainerCreator CreateContainerCreator(DiContainer container)
        {
            return new SubContainerCreatorCached(
                new SubContainerCreatorByInstaller(container, _installerType));
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
                                container.SingletonProviderCreator.CreateProviderForSubContainerInstaller(
                                    concreteType, BindInfo.ConcreteIdentifier, _installerType, _subIdentifier));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        RegisterProvidersForAllContractsPerConcreteType(
                            container,
                            concreteTypes,
                            (_, concreteType) =>
                                new SubContainerDependencyProvider(
                                    concreteType, _subIdentifier, CreateContainerCreator(container)));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var containerCreator = CreateContainerCreator(container);

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
                            (_, contractType) => container.SingletonProviderCreator.CreateProviderForSubContainerInstaller(
                                contractType, BindInfo.ConcreteIdentifier, _installerType, _subIdentifier));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        RegisterProviderPerContract(
                            container,
                            (_, contractType) => new SubContainerDependencyProvider(
                                contractType, _subIdentifier, CreateContainerCreator(container)));
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        var containerCreator = CreateContainerCreator(container);

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