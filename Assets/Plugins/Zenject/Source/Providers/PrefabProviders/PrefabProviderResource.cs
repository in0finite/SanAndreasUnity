#if !NOT_UNITY3D

using ModestTree;
using UnityEngine;

namespace Zenject
{
    public class PrefabProviderResource : IPrefabProvider
    {
        private readonly string _resourcePath;

        public PrefabProviderResource(string resourcePath)
        {
            _resourcePath = resourcePath;
        }

        public UnityEngine.Object GetPrefab()
        {
            var prefab = (GameObject)Resources.Load(_resourcePath);

            Assert.That(prefab != null,
                "Expected to find prefab at resource path '{0}'", _resourcePath);

            return prefab;
        }
    }
}

#endif