using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class InstanceProvider : IProvider
    {
        private readonly object _instance;
        private readonly Type _instanceType;
        private readonly DiContainer _container;

        public InstanceProvider(
            Type instanceType, object instance, DiContainer container)
        {
            _instanceType = instanceType;
            _instance = instance;
            _container = container;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _instanceType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEmpty(args);
            Assert.IsNotNull(context);

            Assert.That(_instanceType.DerivesFromOrEqual(context.MemberType));

            yield return new List<object>() { _instance };

            _container.LazyInject(_instance);
        }
    }
}