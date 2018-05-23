using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zenject
{
    public class UntypedFactoryProvider : IProvider
    {
        private readonly List<TypeValuePair> _factoryArgs;
        private readonly DiContainer _container;
        private readonly Type _factoryType;
        private readonly Type _concreteType;
        private readonly MethodInfo _createMethod;

        public UntypedFactoryProvider(
            Type factoryType, DiContainer container, List<TypeValuePair> factoryArgs)
        {
            Assert.That(factoryType.DerivesFrom<IFactory>());

            _concreteType = LookupConcreteType(factoryType);
            _factoryType = factoryType;
            _container = container;
            _factoryArgs = factoryArgs;

            _createMethod = factoryType
                .DeclaredInstanceMethods().Where(x => x.Name == "Create").Single();

            Assert.That(_createMethod.ReturnType == _concreteType);
        }

        private Type LookupConcreteType(Type factoryType)
        {
            // We assume here that the concrete type is the last generic argument to the IFactory class
            return factoryType.Interfaces().Where(x => x.Interfaces().OnlyOrDefault() == typeof(IFactory))
                .Single().GenericArguments().Last();
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _concreteType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            // Do this even when validating in case it has its own dependencies
            var factory = _container.InstantiateExplicit(_factoryType, _factoryArgs);

            if (_container.IsValidating)
            {
                // In case users define a custom IFactory that needs to be validated
                if (factory is IValidatable)
                {
                    ((IValidatable)factory).Validate();
                }

                // We assume here that we are creating a user-defined factory so there's
                // nothing else we can validate here
                yield return new List<object>() { new ValidationMarker(_concreteType) };
            }
            else
            {
                var result = _createMethod.Invoke(factory, args.Select(x => x.Value).ToArray());

                yield return new List<object>() { result };
            }
        }
    }
}