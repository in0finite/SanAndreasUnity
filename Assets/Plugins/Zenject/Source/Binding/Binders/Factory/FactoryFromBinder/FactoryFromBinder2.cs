using System;
using System.Collections.Generic;

namespace Zenject
{
    public class FactoryFromBinder<TParam1, TParam2, TContract> : FactoryFromBinderBase<TContract>
    {
        public FactoryFromBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindInfo, factoryBindInfo)
        {
        }

        public ConditionCopyNonLazyBinder FromMethod(Func<DiContainer, TParam1, TParam2, TContract> method)
        {
            ProviderFunc =
                (container) => new MethodProviderWithContainer<TParam1, TParam2, TContract>(method);

            return this;
        }

        public ConditionCopyNonLazyBinder FromFactory<TSubFactory>()
            where TSubFactory : IFactory<TParam1, TParam2, TContract>
        {
            ProviderFunc =
                (container) => new FactoryProvider<TParam1, TParam2, TContract, TSubFactory>(container, new List<TypeValuePair>());

            return this;
        }

        public ConditionCopyNonLazyBinder FromIFactoryResolve()
        {
            return FromIFactoryResolve(null);
        }

        public ConditionCopyNonLazyBinder FromIFactoryResolve(object subIdentifier)
        {
            ProviderFunc =
                (container) => new IFactoryResolveProvider<TParam1, TParam2, TContract>(container, subIdentifier);

            return new ConditionCopyNonLazyBinder(BindInfo);
        }

        public FactorySubContainerBinder<TParam1, TParam2, TContract> FromSubContainerResolve()
        {
            return FromSubContainerResolve(null);
        }

        public FactorySubContainerBinder<TParam1, TParam2, TContract> FromSubContainerResolve(object subIdentifier)
        {
            return new FactorySubContainerBinder<TParam1, TParam2, TContract>(
                BindInfo, FactoryBindInfo, subIdentifier);
        }
    }
}