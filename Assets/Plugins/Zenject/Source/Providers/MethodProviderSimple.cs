using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class MethodProviderSimple<TReturn> : IProvider
    {
        private readonly Func<TReturn> _method;

        public MethodProviderSimple(Func<TReturn> method)
        {
            _method = method;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TReturn);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEmpty(args);
            Assert.IsNotNull(context);

            Assert.That(typeof(TReturn).DerivesFromOrEqual(context.MemberType));

            yield return new List<object>() { _method() };
        }
    }
}