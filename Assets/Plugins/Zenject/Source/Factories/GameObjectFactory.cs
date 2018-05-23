using UnityEngine;

namespace Zenject
{
    // This factory type can be useful if you want to instantiate from a prefab but there isn't
    // a particular component that you want to grab from it
    // You could use GameObject.Instantiate directly but then any MonoBehaviour's that might be
    // on it would not be injected unlike if you use GameObjectFactory
    //
    // This is also nicer because any using code doesn't need access to the prefab and you can
    // just bind it in an installer instead
    //
    // Example usage:
    //
    // public class ExplosionFactory : GameObjectFactory
    // {
    // }
    //
    // Then in an installer:
    //
    // Container.Bind<ExplosionFactory>().AsSingle().WithArguments(explosionPrefab)

    public class GameObjectFactory : IFactory<GameObject>
    {
        private DiContainer _container;
        private UnityEngine.Object _prefab;

        [Inject]
        public void Construct(
            UnityEngine.Object prefab,
            DiContainer container)
        {
            _container = container;
            _prefab = prefab;
        }

        public GameObject Create()
        {
            return _container.InstantiatePrefab(_prefab);
        }
    }
}