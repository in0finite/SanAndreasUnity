using ModestTree;

namespace Zenject
{
    public class MemoryPoolBindingFinalizer<TContract> : ProviderBindingFinalizer
    {
        private readonly MemoryPoolBindInfo _poolBindInfo;
        private readonly FactoryBindInfo _factoryBindInfo;

        public MemoryPoolBindingFinalizer(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo, MemoryPoolBindInfo poolBindInfo)
            : base(bindInfo)
        {
            // Note that it doesn't derive from MemoryPool<TContract>
            // when used with To<>, so we can only check IMemoryPoolBase
            Assert.That(factoryBindInfo.FactoryType.DerivesFrom<IMemoryPool>());

            _factoryBindInfo = factoryBindInfo;
            _poolBindInfo = poolBindInfo;
        }

        protected override void OnFinalizeBinding(DiContainer container)
        {
            var factory = new FactoryProviderWrapper<TContract>(
                _factoryBindInfo.ProviderFunc(container), new InjectContext(container, typeof(TContract)));

            var settings = new MemoryPoolSettings()
            {
                InitialSize = _poolBindInfo.InitialSize,
                ExpandMethod = _poolBindInfo.ExpandMethod,
            };

            RegisterProviderForAllContracts(
                container,
                new CachedProvider(
                    new TransientProvider(
                        _factoryBindInfo.FactoryType,
                        container,
                        InjectUtil.CreateArgListExplicit(factory, settings),
                        null,
                        BindInfo.ContextInfo)));
        }
    }
}