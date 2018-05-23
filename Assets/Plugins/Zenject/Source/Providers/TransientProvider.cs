using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class TransientProvider : IProvider
    {
        private readonly DiContainer _container;
        private readonly Type _concreteType;
        private readonly object _concreteIdentifier;
        private readonly List<TypeValuePair> _extraArguments;

        public TransientProvider(
            Type concreteType, DiContainer container,
            List<TypeValuePair> extraArguments, object concreteIdentifier, string bindingContext)
        {
            Assert.That(!concreteType.IsAbstract(),
                "Expected non-abstract type for given binding but instead found type '{0}'{1}",
                concreteType, bindingContext == null ? "" : " when binding '{0}'".Fmt(bindingContext));

            _container = container;
            _concreteType = concreteType;
            _concreteIdentifier = concreteIdentifier;
            _extraArguments = extraArguments;
        }

        public TransientProvider(
            Type concreteType, DiContainer container,
            List<TypeValuePair> extraArguments)
            : this(concreteType, container, extraArguments, null, null)
        {
        }

        public TransientProvider(
            Type concreteType, DiContainer container)
            : this(concreteType, container, new List<TypeValuePair>())
        {
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _concreteType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsNotNull(context);

            bool autoInject = false;

            var instanceType = GetTypeToCreate(context.MemberType);

            var injectArgs = new InjectArgs()
            {
                ExtraArgs = _extraArguments.Concat(args).ToList(),
                Context = context,
                ConcreteIdentifier = _concreteIdentifier,
            };

            var instance = _container.InstantiateExplicit(
                instanceType, autoInject, injectArgs);

            // Return before property/field/method injection to allow circular dependencies
            yield return new List<object>() { instance };

            _container.InjectExplicit(instance, instanceType, injectArgs);
        }

        private Type GetTypeToCreate(Type contractType)
        {
            return ProviderUtil.GetTypeToInstantiate(contractType, _concreteType);
        }
    }
}