#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zenject
{
    public class AddToCurrentGameObjectComponentProvider : IProvider
    {
        private readonly object _concreteIdentifier;
        private readonly Type _componentType;
        private readonly DiContainer _container;
        private readonly List<TypeValuePair> _extraArguments;

        public AddToCurrentGameObjectComponentProvider(
            DiContainer container, Type componentType,
            object concreteIdentifier, List<TypeValuePair> extraArguments)
        {
            Assert.That(componentType.DerivesFrom<Component>());

            _concreteIdentifier = concreteIdentifier;
            _extraArguments = extraArguments;
            _componentType = componentType;
            _container = container;
        }

        protected DiContainer Container
        {
            get { return _container; }
        }

        protected Type ComponentType
        {
            get { return _componentType; }
        }

        protected object ConcreteIdentifier
        {
            get { return _concreteIdentifier; }
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _componentType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsNotNull(context);

            Assert.That(context.ObjectType.DerivesFrom<Component>(),
                "Object '{0}' can only be injected into MonoBehaviour's since it was bound with 'FromNewComponentSibling'. Attempted to inject into non-MonoBehaviour '{1}'",
                context.MemberType, context.ObjectType);

            object instance;

            if (!_container.IsValidating || DiContainer.CanCreateOrInjectDuringValidation(_componentType))
            {
                var gameObj = ((Component)context.ObjectInstance).gameObject;

                instance = gameObj.GetComponent(_componentType);

                if (instance != null)
                {
                    yield return new List<object>() { instance };
                    yield break;
                }

                instance = gameObj.AddComponent(_componentType);
            }
            else
            {
                instance = new ValidationMarker(_componentType);
            }

            // Note that we don't just use InstantiateComponentOnNewGameObjectExplicit here
            // because then circular references don't work
            yield return new List<object>() { instance };

            var injectArgs = new InjectArgs()
            {
                ExtraArgs = _extraArguments.Concat(args).ToList(),
                Context = context,
                ConcreteIdentifier = _concreteIdentifier,
            };

            _container.InjectExplicit(instance, _componentType, injectArgs);

            Assert.That(injectArgs.ExtraArgs.IsEmpty());
        }
    }
}

#endif