using ModestTree;

namespace Zenject
{
    public class ScopeArgConditionCopyNonLazyBinder : ArgConditionCopyNonLazyBinder
    {
        public ScopeArgConditionCopyNonLazyBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        public ArgConditionCopyNonLazyBinder AsSingle()
        {
            return AsSingle(null);
        }

        public ArgConditionCopyNonLazyBinder AsSingle(object concreteIdentifier)
        {
            Assert.IsNull(BindInfo.ConcreteIdentifier);

            BindInfo.Scope = ScopeTypes.Singleton;
            BindInfo.ConcreteIdentifier = concreteIdentifier;
            return this;
        }

        public ArgConditionCopyNonLazyBinder AsCached()
        {
            BindInfo.Scope = ScopeTypes.Cached;
            return this;
        }

        // Note that this is the default so it's not necessary to call this
        public ArgConditionCopyNonLazyBinder AsTransient()
        {
            BindInfo.Scope = ScopeTypes.Transient;
            return this;
        }
    }
}