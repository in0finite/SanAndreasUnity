#if !NOT_UNITY3D

namespace Zenject
{
    // For some platforms, it's desirable to be able to add dependencies to Zenject before
    // Unity even starts up (eg. WSA as described here https://github.com/modesttree/Zenject/issues/118)
    // In those cases you can call StaticContext.Container.BindX to add dependencies
    // Anything you add there will then be injected everywhere, since all other contexts
    // should be children of StaticContext
    public static class StaticContext
    {
        private static DiContainer _container;

        static StaticContext()
        {
            _container = new DiContainer();
        }

        public static DiContainer Container
        {
            get { return _container; }
        }
    }
}

#endif