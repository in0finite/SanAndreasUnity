using ModestTree;
using System;
using System.Linq;

namespace Zenject
{
    public class FactoryFromBinder<TContract> : FactoryFromBinderBase<TContract>
    {
        public FactoryFromBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindInfo, factoryBindInfo)
        {
        }

        public ConditionCopyNonLazyBinder FromResolveGetter<TObj>(Func<TObj, TContract> method)
        {
            return FromResolveGetter<TObj>(null, method);
        }

        public ConditionCopyNonLazyBinder FromResolveGetter<TObj>(
            object subIdentifier, Func<TObj, TContract> method)
        {
            FactoryBindInfo.ProviderFunc =
                (container) => new GetterProvider<TObj, TContract>(subIdentifier, method, container);

            return this;
        }

        public ConditionCopyNonLazyBinder FromMethod(Func<DiContainer, TContract> method)
        {
            ProviderFunc =
                (container) => new MethodProviderWithContainer<TContract>(method);

            return this;
        }

        public ConditionCopyNonLazyBinder FromInstance(object instance)
        {
            BindingUtil.AssertInstanceDerivesFromOrEqual(instance, AllParentTypes);

            ProviderFunc =
                (container) => new InstanceProvider(ContractType, instance, container);

            return this;
        }

        public ArgConditionCopyNonLazyBinder FromFactory<TSubFactory>()
            where TSubFactory : IFactory<TContract>
        {
            ProviderFunc =
                (container) => new FactoryProvider<TContract, TSubFactory>(
                    container, BindInfo.Arguments);

            return new ArgConditionCopyNonLazyBinder(BindInfo);
        }

        public ConditionCopyNonLazyBinder FromIFactoryResolve()
        {
            return FromIFactoryResolve(null);
        }

        public ConditionCopyNonLazyBinder FromIFactoryResolve(object subIdentifier)
        {
            ProviderFunc =
                (container) => new IFactoryResolveProvider<TContract>(container, subIdentifier);

            return new ConditionCopyNonLazyBinder(BindInfo);
        }

        public FactorySubContainerBinder<TContract> FromSubContainerResolve()
        {
            return FromSubContainerResolve(null);
        }

        public FactorySubContainerBinder<TContract> FromSubContainerResolve(object subIdentifier)
        {
            return new FactorySubContainerBinder<TContract>(
                BindInfo, FactoryBindInfo, subIdentifier);
        }

#if !NOT_UNITY3D

        public ConditionCopyNonLazyBinder FromComponentInHierarchy()
        {
            BindingUtil.AssertIsInterfaceOrComponent(ContractType);

            return FromMethod((container) =>
                {
                    var matches = container.Resolve<Context>().GetRootGameObjects()
                        .SelectMany(x => x.GetComponentsInChildren<TContract>()).ToList();

                    Assert.That(!matches.IsEmpty(),
                        "Found zero matches when looking up type '{0}' using FromComponentInHierarchy for factory", ContractType);

                    Assert.That(matches.Count() == 1,
                        "Found multiple matches when looking up type '{0}' using FromComponentInHierarchy for factory.  Only expected to find one!", ContractType);

                    return matches.Single();
                });
        }

        public ConditionCopyNonLazyBinder FromResource(string resourcePath)
        {
            BindingUtil.AssertDerivesFromUnityObject(ContractType);

            ProviderFunc =
                (container) => new ResourceProvider(resourcePath, ContractType);

            return this;
        }

#endif
    }
}