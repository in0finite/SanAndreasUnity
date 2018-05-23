using ModestTree;
using System.Collections.Generic;

namespace Zenject
{
    // If you want to ensure that all items are always returned to the pool, include the following
    // line in an installer on project context:
    // Container.BindInterfaces<PoolCleanupChecker>().To<PoolCleanupChecker>().AsSingle().CopyIntoAllSubContainers().NonLazy()
    public class PoolCleanupChecker : ILateDisposable
    {
        private readonly List<IMemoryPool> _poolFactories;

        public PoolCleanupChecker(
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<IMemoryPool> poolFactories)
        {
            _poolFactories = poolFactories;
        }

        public void LateDispose()
        {
            foreach (var pool in _poolFactories)
            {
                Assert.IsEqual(pool.NumActive, 0,
                    "Found active objects in pool '{0}' during dispose.  Did you forget to despawn an object of type '{1}'?".Fmt(pool.GetType(), pool.ItemType));
            }
        }
    }
}