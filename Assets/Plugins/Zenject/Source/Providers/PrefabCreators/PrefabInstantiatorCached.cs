#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zenject
{
    public class PrefabInstantiatorCached : IPrefabInstantiator
    {
        private readonly IPrefabInstantiator _subInstantiator;

        private GameObject _gameObject;

        public PrefabInstantiatorCached(IPrefabInstantiator subInstantiator)
        {
            _subInstantiator = subInstantiator;
        }

        public List<TypeValuePair> ExtraArguments
        {
            get { return _subInstantiator.ExtraArguments; }
        }

        public Type ArgumentTarget
        {
            get { return _subInstantiator.ArgumentTarget; }
        }

        public GameObjectCreationParameters GameObjectCreationParameters
        {
            get { return _subInstantiator.GameObjectCreationParameters; }
        }

        public UnityEngine.Object GetPrefab()
        {
            return _subInstantiator.GetPrefab();
        }

        public IEnumerator<GameObject> Instantiate(List<TypeValuePair> args)
        {
            // We can't really support arguments if we are using the cached value since
            // the arguments might change when called after the first time
            Assert.IsEmpty(args);

            if (_gameObject != null)
            {
                yield return _gameObject;
                yield break;
            }

            var runner = _subInstantiator.Instantiate(new List<TypeValuePair>());

            // First get instance
            bool hasMore = runner.MoveNext();

            _gameObject = runner.Current;

            yield return _gameObject;

            // Now do injection
            while (hasMore)
            {
                hasMore = runner.MoveNext();
            }
        }
    }
}

#endif