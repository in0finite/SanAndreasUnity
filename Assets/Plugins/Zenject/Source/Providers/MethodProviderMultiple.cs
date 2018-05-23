using ModestTree;
using ModestTree.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class MethodProviderMultiple<TReturn> : IProvider
    {
        private readonly DiContainer _container;
        private readonly Func<InjectContext, IEnumerable<TReturn>> _method;

        public MethodProviderMultiple(
            Func<InjectContext, IEnumerable<TReturn>> method,
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
                var result = _method(context);

                if (result == null)
                {
                    throw Assert.CreateException(
                        "Method '{0}' returned null when list was expected. Object graph: {1}",
                        _method.ToDebugString(), context.GetObjectGraphString());
                }

                yield return result.Cast<object>().ToList();
            }
        }
    }
}