using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    // Responsibilities:
    // - Run Initialize() on all Iinitializable's, in the order specified by InitPriority
    public class InitializableManager
    {
        private List<InitializableInfo> _initializables;

        private bool _hasInitialized;

        [Inject]
        public InitializableManager(
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<IInitializable> initializables,
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ModestTree.Util.ValuePair<Type, int>> priorities)
        {
            _initializables = new List<InitializableInfo>();

            for (int i = 0; i < initializables.Count; i++)
            {
                var initializable = initializables[i];

                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                var matches = priorities.Where(x => initializable.GetType().DerivesFromOrEqual(x.First)).Select(x => x.Second).ToList();
                int priority = matches.IsEmpty() ? 0 : matches.Distinct().Single();

                _initializables.Add(new InitializableInfo(initializable, priority));
            }
        }

        public void Initialize()
        {
            Assert.That(!_hasInitialized);
            _hasInitialized = true;

            _initializables = _initializables.OrderBy(x => x.Priority).ToList();

#if UNITY_EDITOR
            foreach (var initializable in _initializables.Select(x => x.Initializable).GetDuplicates())
            {
                Assert.That(false, "Found duplicate IInitializable with type '{0}'".Fmt(initializable.GetType()));
            }
#endif

            foreach (var initializable in _initializables)
            {
                try
                {
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
                    using (ProfileBlock.Start("{0}.Initialize()", initializable.Initializable.GetType()))
#endif
                    {
                        initializable.Initializable.Initialize();
                    }
                }
                catch (Exception e)
                {
                    throw Assert.CreateException(
                        e, "Error occurred while initializing IInitializable with type '{0}'", initializable.Initializable.GetType());
                }
            }
        }

        private class InitializableInfo
        {
            public IInitializable Initializable;
            public int Priority;

            public InitializableInfo(IInitializable initializable, int priority)
            {
                Initializable = initializable;
                Priority = priority;
            }
        }
    }
}