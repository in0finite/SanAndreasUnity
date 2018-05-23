#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zenject
{
    public class AddToExistingGameObjectComponentProvider : AddToGameObjectComponentProviderBase
    {
        private readonly GameObject _gameObject;

        public AddToExistingGameObjectComponentProvider(
            GameObject gameObject, DiContainer container, Type componentType,
            object concreteIdentifier, List<TypeValuePair> extraArguments)
            : base(container, componentType, concreteIdentifier, extraArguments)
        {
            _gameObject = gameObject;
        }

        // This will cause [Inject] to be triggered after awake / start
        // We could return true, but what if toggling active has other negative repercussions?
        // For now let's just not do anything
        protected override bool ShouldToggleActive
        {
            get { return false; }
        }

        protected override GameObject GetGameObject(InjectContext context)
        {
            return _gameObject;
        }
    }

    public class AddToExistingGameObjectComponentProviderGetter : AddToGameObjectComponentProviderBase
    {
        private readonly Func<InjectContext, GameObject> _gameObjectGetter;

        public AddToExistingGameObjectComponentProviderGetter(
            Func<InjectContext, GameObject> gameObjectGetter, DiContainer container, Type componentType,
            object concreteIdentifier, List<TypeValuePair> extraArguments)
            : base(container, componentType, concreteIdentifier, extraArguments)
        {
            _gameObjectGetter = gameObjectGetter;
        }

        // This will cause [Inject] to be triggered after awake / start
        // We could return true, but what if toggling active has other negative repercussions?
        // For now let's just not do anything
        protected override bool ShouldToggleActive
        {
            get { return false; }
        }

        protected override GameObject GetGameObject(InjectContext context)
        {
            var gameObj = _gameObjectGetter(context);
            Assert.IsNotNull(gameObj, "Provided Func<InjectContext, GameObject> returned null value for game object when using FromComponentOn");
            return gameObj;
        }
    }
}

#endif