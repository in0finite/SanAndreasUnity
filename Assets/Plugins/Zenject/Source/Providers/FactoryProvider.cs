using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public abstract class FactoryProviderBase<TValue, TFactory> : IProvider
        where TFactory : IFactory
    {
        private readonly List<TypeValuePair> _factoryArgs;
        private object _factory;

        public FactoryProviderBase(DiContainer container, List<TypeValuePair> factoryArgs)
        {
            Container = container;
            _factoryArgs = factoryArgs;
        }

        protected DiContainer Container
        {
            get;
            private set;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TValue);
        }

        public abstract IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args);

        protected object CreateFactory()
        {
            if (_factory == null)
            {
                _factory = Container.InstantiateExplicit(typeof(TFactory), _factoryArgs);
            }

            return _factory;
        }
    }

    // Zero parameters

    public class FactoryProvider<TValue, TFactory> : FactoryProviderBase<TValue, TFactory>
        where TFactory : IFactory<TValue>
    {
        public FactoryProvider(DiContainer container, List<TypeValuePair> factoryArgs)
            : base(container, factoryArgs)
        {
        }

        public override IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEmpty(args);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));

            // Do this even when validating in case it has its own dependencies
            var factory = CreateFactory();

            if (Container.IsValidating)
            {
                // In case users define a custom IFactory that needs to be validated
                if (factory is IValidatable)
                {
                    ((IValidatable)factory).Validate();
                }

                // We assume here that we are creating a user-defined factory so there's
                // nothing else we can validate here
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>() { ((TFactory)factory).Create() };
            }
        }
    }

    // One parameters

    public class FactoryProvider<TParam1, TValue, TFactory> : FactoryProviderBase<TValue, TFactory>
        where TFactory : IFactory<TParam1, TValue>
    {
        public FactoryProvider(DiContainer container, List<TypeValuePair> factoryArgs)
            : base(container, factoryArgs)
        {
        }

        public override IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 1);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());

            // Do this even when validating in case it has its own dependencies
            var factory = CreateFactory();

            if (Container.IsValidating)
            {
                // In case users define a custom IFactory that needs to be validated
                if (factory is IValidatable)
                {
                    ((IValidatable)factory).Validate();
                }

                // We assume here that we are creating a user-defined factory so there's
                // nothing else we can validate here
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    ((TFactory)factory).Create((TParam1)args[0].Value)
                };
            }
        }
    }

    // Two parameters

    public class FactoryProvider<TParam1, TParam2, TValue, TFactory> : FactoryProviderBase<TValue, TFactory>
        where TFactory : IFactory<TParam1, TParam2, TValue>
    {
        public FactoryProvider(DiContainer container, List<TypeValuePair> factoryArgs)
            : base(container, factoryArgs)
        {
        }

        public override IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 2);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());

            // Do this even when validating in case it has its own dependencies
            var factory = CreateFactory();

            if (Container.IsValidating)
            {
                // In case users define a custom IFactory that needs to be validated
                if (factory is IValidatable)
                {
                    ((IValidatable)factory).Validate();
                }

                // We assume here that we are creating a user-defined factory so there's
                // nothing else we can validate here
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    ((TFactory)factory).Create(
                        (TParam1)args[0].Value,
                        (TParam2)args[1].Value)
                };
            }
        }
    }

    // Three parameters

    public class FactoryProvider<TParam1, TParam2, TParam3, TValue, TFactory> : FactoryProviderBase<TValue, TFactory>
        where TFactory : IFactory<TParam1, TParam2, TParam3, TValue>
    {
        public FactoryProvider(DiContainer container, List<TypeValuePair> factoryArgs)
            : base(container, factoryArgs)
        {
        }

        public override IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 3);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());

            // Do this even when validating in case it has its own dependencies
            var factory = CreateFactory();

            if (Container.IsValidating)
            {
                // In case users define a custom IFactory that needs to be validated
                if (factory is IValidatable)
                {
                    ((IValidatable)factory).Validate();
                }

                // We assume here that we are creating a user-defined factory so there's
                // nothing else we can validate here
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    ((TFactory)factory).Create(
                        (TParam1)args[0].Value,
                        (TParam2)args[1].Value,
                        (TParam3)args[2].Value)
                };
            }
        }
    }

    // Four parameters

    public class FactoryProvider<TParam1, TParam2, TParam3, TParam4, TValue, TFactory> : FactoryProviderBase<TValue, TFactory>
        where TFactory : IFactory<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        public FactoryProvider(DiContainer container, List<TypeValuePair> factoryArgs)
            : base(container, factoryArgs)
        {
        }

        public override IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 4);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());
            Assert.That(args[3].Type.DerivesFromOrEqual<TParam4>());

            // Do this even when validating in case it has its own dependencies
            var factory = CreateFactory();

            if (Container.IsValidating)
            {
                // In case users define a custom IFactory that needs to be validated
                if (factory is IValidatable)
                {
                    ((IValidatable)factory).Validate();
                }

                // We assume here that we are creating a user-defined factory so there's
                // nothing else we can validate here
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    ((TFactory)factory).Create(
                        (TParam1)args[0].Value,
                        (TParam2)args[1].Value,
                        (TParam3)args[2].Value,
                        (TParam4)args[3].Value)
                };
            }
        }
    }

    // Five parameters

    public class FactoryProvider<TParam1, TParam2, TParam3, TParam4, TParam5, TValue, TFactory> : FactoryProviderBase<TValue, TFactory>
        where TFactory : IFactory<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        public FactoryProvider(DiContainer container, List<TypeValuePair> factoryArgs)
            : base(container, factoryArgs)
        {
        }

        public override IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 5);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());
            Assert.That(args[3].Type.DerivesFromOrEqual<TParam4>());
            Assert.That(args[4].Type.DerivesFromOrEqual<TParam5>());

            // Do this even when validating in case it has its own dependencies
            var factory = CreateFactory();

            if (Container.IsValidating)
            {
                // In case users define a custom IFactory that needs to be validated
                if (factory is IValidatable)
                {
                    ((IValidatable)factory).Validate();
                }

                // We assume here that we are creating a user-defined factory so there's
                // nothing else we can validate here
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    ((TFactory)factory).Create(
                        (TParam1)args[0].Value,
                        (TParam2)args[1].Value,
                        (TParam3)args[2].Value,
                        (TParam4)args[3].Value,
                        (TParam5)args[4].Value)
                };
            }
        }
    }
}