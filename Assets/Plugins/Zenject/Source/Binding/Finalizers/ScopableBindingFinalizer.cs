using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class ScopableBindingFinalizer : ProviderBindingFinalizer
    {
        private readonly SingletonTypes _singletonType;
        private readonly Func<DiContainer, Type, IProvider> _providerFactory;
        private readonly object _singletonSpecificId;

        public ScopableBindingFinalizer(
            BindInfo bindInfo,
            SingletonTypes singletonType, object singletonSpecificId,
            Func<DiContainer, Type, IProvider> providerFactory)
            : base(bindInfo)
        {
            _singletonType = singletonType;
            _providerFactory = providerFactory;
            _singletonSpecificId = singletonSpecificId;
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
            if (concreteTypes.IsEmpty())
            {
                // This can be common when using convention based bindings
                return;
            }

            switch (GetScope())
            {
                case ScopeTypes.Singleton:
                    {
                        RegisterProvidersForAllContractsPerConcreteType(
                            container,
                            concreteTypes,
                            (_, concreteType) => container.SingletonProviderCreator.CreateProviderStandard(
                                new StandardSingletonDeclaration(
                                    concreteType,
                                    BindInfo.ConcreteIdentifier,
                                    BindInfo.Arguments,
                                    _singletonType,
                                    _singletonSpecificId),
                                _providerFactory));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        RegisterProvidersForAllContractsPerConcreteType(
                            container, concreteTypes, _providerFactory);
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        RegisterProvidersForAllContractsPerConcreteType(
                            container,
                            concreteTypes,
                            (_, concreteType) =>
                                new CachedProvider(
                                    _providerFactory(container, concreteType)));
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
                            (_, contractType) => container.SingletonProviderCreator.CreateProviderStandard(
                                new StandardSingletonDeclaration(
                                    contractType,
                                    BindInfo.ConcreteIdentifier,
                                    BindInfo.Arguments,
                                    _singletonType,
                                    _singletonSpecificId),
                                _providerFactory));
                        break;
                    }
                case ScopeTypes.Transient:
                    {
                        RegisterProviderPerContract(container, _providerFactory);
                        break;
                    }
                case ScopeTypes.Cached:
                    {
                        RegisterProviderPerContract(
                            container,
                            (_, contractType) =>
                                new CachedProvider(
                                    _providerFactory(container, contractType)));
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