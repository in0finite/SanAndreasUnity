using ModestTree;

namespace Zenject
{
    public class ScopeNonLazyBinder : NonLazyBinder
    {
        public ScopeNonLazyBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        public NonLazyBinder AsSingle()
        {
            return AsSingle(null);
        }

        public NonLazyBinder AsSingle(object concreteIdentifier)
        {
            Assert.IsNull(BindInfo.ConcreteIdentifier);

            BindInfo.Scope = ScopeTypes.Singleton;
            BindInfo.ConcreteIdentifier = concreteIdentifier;
            return this;
        }

        public NonLazyBinder AsCached()
        {
            BindInfo.Scope = ScopeTypes.Cached;
            return this;
        }

        // Note that this is the default so it's not necessary to call this
        public NonLazyBinder AsTransient()
        {
            BindInfo.Scope = ScopeTypes.Transient;
            return this;
        }
    }
}