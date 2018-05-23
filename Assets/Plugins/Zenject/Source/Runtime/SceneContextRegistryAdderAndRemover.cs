using System;

namespace Zenject
{
    public class SceneContextRegistryAdderAndRemover : IInitializable, IDisposable
    {
        private readonly SceneContextRegistry _registry;
        private readonly SceneContext _sceneContext;

        public SceneContextRegistryAdderAndRemover(
            SceneContext sceneContext,
            SceneContextRegistry registry)
        {
            _registry = registry;
            _sceneContext = sceneContext;
        }

        public void Initialize()
        {
            _registry.Add(_sceneContext);
        }

        public void Dispose()
        {
            _registry.Remove(_sceneContext);
        }
    }
}