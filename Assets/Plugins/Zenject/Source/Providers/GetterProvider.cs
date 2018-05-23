using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class GetterProvider<TObj, TResult> : IProvider
    {
        private readonly DiContainer _container;
        private readonly object _identifier;
        private readonly Func<TObj, TResult> _method;

        public GetterProvider(
            object identifier, Func<TObj, TResult> method,
            DiContainer container)
        {
            _container = container;
            _identifier = identifier;
            _method = method;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TResult);
        }

        private InjectContext GetSubContext(InjectContext parent)
        {
            var subContext = parent.CreateSubContext(
                typeof(TObj), _identifier);

            subContext.Optional = false;

            return subContext;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEmpty(args);
            Assert.IsNotNull(context);

            Assert.That(typeof(TResult).DerivesFromOrEqual(context.MemberType));

            if (_container.IsValidating)
            {
                // All we can do is validate that the getter object can be resolved
                _container.Resolve(GetSubContext(context));

                yield return new List<object>() { new ValidationMarker(typeof(TResult)) };
            }
            else
            {
                yield return new List<object>() { _method(
                    (TObj)_container.Resolve(GetSubContext(context))) };
            }
        }
    }
}