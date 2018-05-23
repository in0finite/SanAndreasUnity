using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class MethodProviderUntyped : IProvider
    {
        private readonly DiContainer _container;
        private readonly Func<InjectContext, object> _method;

        public MethodProviderUntyped(
            Func<InjectContext, object> method,
            DiContainer container)
        {
            _container = container;
            _method = method;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return context.MemberType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEmpty(args);
            Assert.IsNotNull(context);

            if (_container.IsValidating && !DiContainer.CanCreateOrInjectDuringValidation(context.MemberType))
            {
                yield return new List<object>() { new ValidationMarker(context.MemberType) };
            }
            else
            {
                var result = _method(context);

                if (result == null)
                {
#if !UNITY_WSA
                    Assert.That(context.MemberType.IsPrimitive,
                        "Invalid value returned from FromMethod.  Expected non-null.");
#endif
                }
                else
                {
                    Assert.That(result.GetType().DerivesFromOrEqual(context.MemberType));
                }

                yield return new List<object>() { result };
            }
        }
    }
}