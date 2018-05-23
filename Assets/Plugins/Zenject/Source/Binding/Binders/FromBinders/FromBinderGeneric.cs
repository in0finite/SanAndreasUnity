using System;
using System.Collections.Generic;
using ModestTree;
using System.Linq;

#if !NOT_UNITY3D

using UnityEngine;

#endif

namespace Zenject
{
    public class FromBinderGeneric<TContract> : FromBinder
    {
        public FromBinderGeneric(
            BindInfo bindInfo,
            BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo, finalizerWrapper)
        {
            BindingUtil.AssertIsDerivedFromTypes(typeof(TContract), BindInfo.ContractTypes);
        }

        public ScopeArgConditionCopyNonLazyBinder FromFactory<TFactory>()
            where TFactory : IFactory<TContract>
        {
            return FromFactoryBase<TContract, TFactory>();
        }

        public ScopeArgConditionCopyNonLazyBinder FromMethod(Func<InjectContext, TContract> method)
        {
            return FromMethodBase<TContract>(method);
        }

        public ScopeArgConditionCopyNonLazyBinder FromMethodMultiple(Func<InjectContext, IEnumerable<TContract>> method)
        {
            return FromMethodMultipleBase<TContract>(method);
        }

        public ScopeConditionCopyNonLazyBinder FromResolveGetter<TObj>(Func<TObj, TContract> method)
        {
            return FromResolveGetter<TObj>(null, method);
        }

        public ScopeConditionCopyNonLazyBinder FromResolveGetter<TObj>(object identifier, Func<TObj, TContract> method)
        {
            return FromResolveGetterBase<TObj, TContract>(identifier, method);
        }

        public ScopeConditionCopyNonLazyBinder FromInstance(TContract instance)
        {
            return FromInstanceBase(instance);
        }

#if !NOT_UNITY3D

        public ScopeArgConditionCopyNonLazyBinder FromComponentInChildren(
            Func<TContract, bool> predicate, bool includeInactive = false)
        {
            return FromComponentInChildren(false, predicate, includeInactive);
        }

        public ScopeArgConditionCopyNonLazyBinder FromComponentInChildren(
            bool excludeSelf = false, Func<TContract, bool> predicate = null, bool includeInactive = false)
        {
            BindingUtil.AssertIsInterfaceOrComponent(AllParentTypes);

            return FromMethodMultiple((ctx) =>
            {
                Assert.That(ctx.ObjectType.DerivesFromOrEqual<MonoBehaviour>());
                Assert.IsNotNull(ctx.ObjectInstance);

                var res = ((MonoBehaviour)ctx.ObjectInstance).GetComponentsInChildren<TContract>(includeInactive)
                    .Where(x => !ReferenceEquals(x, ctx.ObjectInstance));

                if (excludeSelf)
                {
                    res = res.Where(x => (x as Component).gameObject != (ctx.ObjectInstance as Component).gameObject);
                }

                if (predicate != null)
                {
                    res = res.Where(predicate);
                }

                return res;
            });
        }

        public ScopeArgConditionCopyNonLazyBinder FromComponentInParents(bool excludeSelf = false)
        {
            BindingUtil.AssertIsInterfaceOrComponent(AllParentTypes);

            return FromMethodMultiple((ctx) =>
                {
                    Assert.That(ctx.ObjectType.DerivesFromOrEqual<MonoBehaviour>());
                    Assert.IsNotNull(ctx.ObjectInstance);

                    var res = ((MonoBehaviour)ctx.ObjectInstance).GetComponentsInParent<TContract>()
                        .Where(x => !ReferenceEquals(x, ctx.ObjectInstance));

                    if (excludeSelf)
                    {
                        res = res.Where(x => (x as Component).gameObject != (ctx.ObjectInstance as Component).gameObject);
                    }

                    return res;
                });
        }

        public ScopeArgConditionCopyNonLazyBinder FromComponentSibling()
        {
            BindingUtil.AssertIsInterfaceOrComponent(AllParentTypes);

            return FromMethodMultiple((ctx) =>
                {
                    Assert.That(ctx.ObjectType.DerivesFromOrEqual<MonoBehaviour>());
                    Assert.IsNotNull(ctx.ObjectInstance);

                    return ((MonoBehaviour)ctx.ObjectInstance).GetComponents<TContract>()
                        .Where(x => !ReferenceEquals(x, ctx.ObjectInstance));
                });
        }

        public ScopeArgConditionCopyNonLazyBinder FromComponentInHierarchy(
            Func<TContract, bool> predicate = null, bool includeInactive = false)
        {
            BindingUtil.AssertIsInterfaceOrComponent(AllParentTypes);

            return FromMethodMultiple((ctx) =>
            {
                var res = ctx.Container.Resolve<Context>().GetRootGameObjects()
                    .SelectMany(x => x.GetComponentsInChildren<TContract>(includeInactive))
                    .Where(x => !ReferenceEquals(x, ctx.ObjectInstance));

                if (predicate != null)
                {
                    res = res.Where(predicate);
                }

                return res;
            });
        }

#endif
    }
}