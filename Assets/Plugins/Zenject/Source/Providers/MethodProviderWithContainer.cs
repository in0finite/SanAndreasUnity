using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    // Zero params

    public class MethodProviderWithContainer<TValue> : IProvider
    {
        private readonly Func<DiContainer, TValue> _method;

        public MethodProviderWithContainer(Func<DiContainer, TValue> method)
        {
            _method = method;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TValue);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEmpty(args);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));

            if (context.Container.IsValidating)
            {
                // Don't do anything when validating, we can't make any assumptions on the given method
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>() { _method(context.Container) };
            }
        }
    }

    // One params

    public class MethodProviderWithContainer<TParam1, TValue> : IProvider
    {
        private readonly Func<DiContainer, TParam1, TValue> _method;

        public MethodProviderWithContainer(Func<DiContainer, TParam1, TValue> method)
        {
            _method = method;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TValue);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 1);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual(typeof(TParam1)));

            if (context.Container.IsValidating)
            {
                // Don't do anything when validating, we can't make any assumptions on the given method
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    _method(
                        context.Container,
                        (TParam1)args[0].Value)
                };
            }
        }
    }

    // Two params

    public class MethodProviderWithContainer<TParam1, TParam2, TValue> : IProvider
    {
        private readonly Func<DiContainer, TParam1, TParam2, TValue> _method;

        public MethodProviderWithContainer(Func<DiContainer, TParam1, TParam2, TValue> method)
        {
            _method = method;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TValue);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 2);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual(typeof(TParam1)));
            Assert.That(args[1].Type.DerivesFromOrEqual(typeof(TParam2)));

            if (context.Container.IsValidating)
            {
                // Don't do anything when validating, we can't make any assumptions on the given method
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    _method(
                        context.Container,
                        (TParam1)args[0].Value,
                        (TParam2)args[1].Value)
                };
            }
        }
    }

    // Three params

    public class MethodProviderWithContainer<TParam1, TParam2, TParam3, TValue> : IProvider
    {
        private readonly Func<DiContainer, TParam1, TParam2, TParam3, TValue> _method;

        public MethodProviderWithContainer(Func<DiContainer, TParam1, TParam2, TParam3, TValue> method)
        {
            _method = method;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TValue);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 3);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual(typeof(TParam1)));
            Assert.That(args[1].Type.DerivesFromOrEqual(typeof(TParam2)));
            Assert.That(args[2].Type.DerivesFromOrEqual(typeof(TParam3)));

            if (context.Container.IsValidating)
            {
                // Don't do anything when validating, we can't make any assumptions on the given method
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    _method(
                        context.Container,
                        (TParam1)args[0].Value,
                        (TParam2)args[1].Value,
                        (TParam3)args[2].Value)
                };
            }
        }
    }

    // Four params

    public class MethodProviderWithContainer<TParam1, TParam2, TParam3, TParam4, TValue> : IProvider
    {
        private readonly ModestTree.Util.Func<DiContainer, TParam1, TParam2, TParam3, TParam4, TValue> _method;

        public MethodProviderWithContainer(ModestTree.Util.Func<DiContainer, TParam1, TParam2, TParam3, TParam4, TValue> method)
        {
            _method = method;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TValue);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 4);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual(typeof(TParam1)));
            Assert.That(args[1].Type.DerivesFromOrEqual(typeof(TParam2)));
            Assert.That(args[2].Type.DerivesFromOrEqual(typeof(TParam3)));
            Assert.That(args[3].Type.DerivesFromOrEqual(typeof(TParam4)));

            if (context.Container.IsValidating)
            {
                // Don't do anything when validating, we can't make any assumptions on the given method
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    _method(
                        context.Container,
                        (TParam1)args[0].Value,
                        (TParam2)args[1].Value,
                        (TParam3)args[2].Value,
                        (TParam4)args[3].Value)
                };
            }
        }
    }

    // Five params

    public class MethodProviderWithContainer<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> : IProvider
    {
        private readonly ModestTree.Util.Func<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5, TValue> _method;

        public MethodProviderWithContainer(ModestTree.Util.Func<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5, TValue> method)
        {
            _method = method;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(TValue);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsEqual(args.Count, 5);
            Assert.IsNotNull(context);

            Assert.That(typeof(TValue).DerivesFromOrEqual(context.MemberType));
            Assert.That(args[0].Type.DerivesFromOrEqual(typeof(TParam1)));
            Assert.That(args[1].Type.DerivesFromOrEqual(typeof(TParam2)));
            Assert.That(args[2].Type.DerivesFromOrEqual(typeof(TParam3)));
            Assert.That(args[3].Type.DerivesFromOrEqual(typeof(TParam4)));
            Assert.That(args[4].Type.DerivesFromOrEqual(typeof(TParam5)));

            if (context.Container.IsValidating)
            {
                // Don't do anything when validating, we can't make any assumptions on the given method
                yield return new List<object>() { new ValidationMarker(typeof(TValue)) };
            }
            else
            {
                yield return new List<object>()
                {
                    _method(
                        context.Container,
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