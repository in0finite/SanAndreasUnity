#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zenject
{
    public class ResourceProvider : IProvider
    {
        private readonly Type _resourceType;
        private readonly string _resourcePath;

        public ResourceProvider(
            string resourcePath, Type resourceType)
        {
            _resourceType = resourceType;
            _resourcePath = resourcePath;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _resourceType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEmpty(args);

            Assert.IsNotNull(context);

            var objects = Resources.LoadAll(_resourcePath, _resourceType).Cast<object>().ToList();

            Assert.That(!objects.IsEmpty(),
                "Could not find resource at path '{0}' with type '{1}'", _resourcePath, _resourceType);

            yield return objects;

            // Are there any resource types which can be injected?
        }
    }
}

#endif