using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class CachedProvider : IProvider
    {
        private readonly IProvider _creator;

        private List<object> _instances;
        private bool _isCreatingInstance;

        public CachedProvider(IProvider creator)
        {
            _creator = creator;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _creator.GetInstanceType(context);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsNotNull(context);

            if (_instances != null)
            {
                yield return _instances;
                yield break;
            }

#if !ZEN_MULTITHREADING
            // This should only happen with constructor injection
            // Field or property injection should allow circular dependencies
            if (_isCreatingInstance)
            {
                throw Assert.CreateException(
                    "Found circular dependency when creating type '{0}'. Object graph: {1}",
                    _creator.GetInstanceType(context), context.GetObjectGraphString());
            }
#endif

            _isCreatingInstance = true;

            var runner = _creator.GetAllInstancesWithInjectSplit(context, args);

            // First get instance
            bool hasMore = runner.MoveNext();

            _instances = runner.Current;
            Assert.IsNotNull(_instances);
            _isCreatingInstance = false;

            yield return _instances;

            // Now do injection
            while (hasMore)
            {
                hasMore = runner.MoveNext();
            }
        }
    }
}