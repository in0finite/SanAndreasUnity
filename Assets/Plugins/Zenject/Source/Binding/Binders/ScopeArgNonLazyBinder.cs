using ModestTree;

namespace Zenject
{
    public class ScopeArgNonLazyBinder : ArgNonLazyBinder
    {
        public ScopeArgNonLazyBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        public ArgNonLazyBinder AsSingle()
        {
            return AsSingle(null);
        }

        public ArgNonLazyBinder AsSingle(object concreteIdentifier)
        {
            Assert.IsNull(BindInfo.ConcreteIdentifier);

            BindInfo.Scope = ScopeTypes.Singleton;
            BindInfo.ConcreteIdentifier = concreteIdentifier;
            return this;
        }

        public ArgNonLazyBinder AsCached()
        {
            BindInfo.Scope = ScopeTypes.Cached;
            return this;
        }

        // Note that this is the default so it's not necessary to call this
        public ArgNonLazyBinder AsTransient()
        {
            BindInfo.Scope = ScopeTypes.Transient;
            return this;
        }
    }
}