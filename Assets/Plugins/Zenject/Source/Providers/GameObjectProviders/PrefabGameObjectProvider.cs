#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zenject
{
    public class PrefabGameObjectProvider : IProvider
    {
        private readonly IPrefabInstantiator _prefabCreator;

        public PrefabGameObjectProvider(
            IPrefabInstantiator prefabCreator)
        {
            _prefabCreator = prefabCreator;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(GameObject);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            var runner = _prefabCreator.Instantiate(args);

            // First get instance
            bool hasMore = runner.MoveNext();

            var instance = runner.Current;

            yield return new List<object>() { instance };

            // Now do injection
            while (hasMore)
            {
                hasMore = runner.MoveNext();
            }
        }
    }
}

#endif