using ModestTree;

namespace Zenject
{
    public class ScopeConditionCopyNonLazyBinder : ConditionCopyNonLazyBinder
    {
        public ScopeConditionCopyNonLazyBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        public ConditionCopyNonLazyBinder AsSingle()
        {
            return AsSingle(null);
        }

        public ConditionCopyNonLazyBinder AsSingle(object concreteIdentifier)
        {
            Assert.IsNull(BindInfo.ConcreteIdentifier);

            BindInfo.Scope = ScopeTypes.Singleton;
            BindInfo.ConcreteIdentifier = concreteIdentifier;
            return this;
        }

        public ConditionCopyNonLazyBinder AsCached()
        {
            BindInfo.Scope = ScopeTypes.Cached;
            return this;
        }

        // Note that this is the default so it's not necessary to call this
        public ConditionCopyNonLazyBinder AsTransient()
        {
            BindInfo.Scope = ScopeTypes.Transient;
            return this;
        }
    }
}