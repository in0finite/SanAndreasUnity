using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class MethodProvider<TReturn> : IProvider
    {
        private readonly DiContainer _container;
        private readonly Func<InjectContext, TReturn> _method;

        public MethodProvider(
            Func<InjectContext, TReturn> method,
            DiContainer container)
        {
            _container = container;
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

            if (_container.IsValidating && !DiContainer.CanCreateOrInjectDuringValidation(context.MemberType))
            {
                yield return new List<object>() { new ValidationMarker(typeof(TReturn)) };
            }
            else
            {
                yield return new List<object>() { _method(context) };
            }
        }
    }
}