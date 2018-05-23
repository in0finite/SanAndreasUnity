using ModestTree;

namespace Zenject
{
    public class PlaceholderFactoryBindingFinalizer<TContract> : ProviderBindingFinalizer
    {
        private readonly FactoryBindInfo _factoryBindInfo;

        public PlaceholderFactoryBindingFinalizer(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindInfo)
        {
            // Note that it doesn't derive from Factory<TContract>
            // when used with To<>, so we can only check IPlaceholderFactory
            Assert.That(factoryBindInfo.FactoryType.DerivesFrom<IPlaceholderFactory>());

            _factoryBindInfo = factoryBindInfo;
        }

        protected override void OnFinalizeBinding(DiContainer container)
        {
            var provider = _factoryBindInfo.ProviderFunc(container);

            RegisterProviderForAllContracts(
                container,
                new CachedProvider(
                    new TransientProvider(
                        _factoryBindInfo.FactoryType,
                        container,
                        InjectUtil.CreateArgListExplicit(
                            provider,
                            new InjectContext(container, typeof(TContract))),
                        null,
                        BindInfo.ContextInfo)));
        }
    }
}