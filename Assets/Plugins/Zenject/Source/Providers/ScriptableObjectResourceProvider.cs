#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zenject
{
    public class ScriptableObjectResourceProvider : IProvider
    {
        private readonly DiContainer _container;
        private readonly Type _resourceType;
        private readonly string _resourcePath;
        private readonly List<TypeValuePair> _extraArguments;
        private readonly object _concreteIdentifier;
        private readonly bool _createNew;

        public ScriptableObjectResourceProvider(
            string resourcePath, Type resourceType,
            DiContainer container, object concreteIdentifier, List<TypeValuePair> extraArguments,
            bool createNew)
        {
            _container = container;
            Assert.DerivesFromOrEqual<ScriptableObject>(resourceType);

            _concreteIdentifier = concreteIdentifier;
            _extraArguments = extraArguments;
            _resourceType = resourceType;
            _resourcePath = resourcePath;
            _createNew = createNew;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _resourceType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsNotNull(context);

            List<object> objects;

            if (_createNew)
            {
                objects = Resources.LoadAll(_resourcePath, _resourceType)
                    .Select(x => ScriptableObject.Instantiate(x)).Cast<object>().ToList();
            }
            else
            {
                objects = Resources.LoadAll(_resourcePath, _resourceType)
                    .Cast<object>().ToList();
            }

            Assert.That(!objects.IsEmpty(),
                "Could not find resource at path '{0}' with type '{1}'", _resourcePath, _resourceType);

            yield return objects;

            var injectArgs = new InjectArgs()
            {
                ExtraArgs = _extraArguments.Concat(args).ToList(),
                Context = context,
                ConcreteIdentifier = _concreteIdentifier,
            };

            foreach (var obj in objects)
            {
                _container.InjectExplicit(
                    obj, _resourceType, injectArgs);
            }
        }
    }
}

#endif