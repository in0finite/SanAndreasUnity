using System;
using System.Collections.Generic;

namespace Zenject
{
    public class FromBinderNonGeneric : FromBinder
    {
        public FromBinderNonGeneric(
            BindInfo bindInfo,
            BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo, finalizerWrapper)
        {
        }

        public ScopeArgConditionCopyNonLazyBinder FromFactory<TConcrete, TFactory>()
            where TFactory : IFactory<TConcrete>
        {
            return FromFactoryBase<TConcrete, TFactory>();
        }

        public ScopeArgConditionCopyNonLazyBinder FromMethod<TConcrete>(Func<InjectContext, TConcrete> method)
        {
            return FromMethodBase<TConcrete>(method);
        }

        public ScopeArgConditionCopyNonLazyBinder FromMethodMultiple<TConcrete>(Func<InjectContext, IEnumerable<TConcrete>> method)
        {
            return FromMethodMultipleBase<TConcrete>(method);
        }

        public ScopeConditionCopyNonLazyBinder FromResolveGetter<TObj, TContract>(Func<TObj, TContract> method)
        {
            return FromResolveGetter<TObj, TContract>(null, method);
        }

        public ScopeConditionCopyNonLazyBinder FromResolveGetter<TObj, TContract>(object identifier, Func<TObj, TContract> method)
        {
            return FromResolveGetterBase<TObj, TContract>(identifier, method);
        }

        public ScopeConditionCopyNonLazyBinder FromInstance(object instance)
        {
            return FromInstanceBase(instance);
        }
    }
}