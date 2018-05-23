#if !NOT_UNITY3D

using ModestTree;
using System.Collections.Generic;

namespace Zenject
{
    public class SubContainerCreatorByNewPrefab : ISubContainerCreator
    {
        private readonly GameObjectCreationParameters _gameObjectBindInfo;
        private readonly IPrefabProvider _prefabProvider;
        private readonly DiContainer _container;

        public SubContainerCreatorByNewPrefab(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _prefabProvider = prefabProvider;
            _container = container;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext parentContext)
        {
            Assert.That(args.IsEmpty());

            var prefab = _prefabProvider.GetPrefab();
            var gameObject = _container.InstantiatePrefab(prefab, _gameObjectBindInfo);

            var context = gameObject.GetComponent<GameObjectContext>();

            Assert.That(context != null,
                "Expected prefab with name '{0}' to container a component of type 'GameObjectContext'", prefab.name);

            // Note: We don't need to call ValidateValidatables here because GameObjectContext does this for us

            return context.Container;
        }
    }
}

#endif