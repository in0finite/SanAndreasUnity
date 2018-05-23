using ModestTree;
using System.Collections.Generic;

namespace Zenject
{
    public static class IProviderExtensions
    {
        public static IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            this IProvider creator, InjectContext context)
        {
            return creator.GetAllInstancesWithInjectSplit(
                context, new List<TypeValuePair>());
        }

        public static List<object> GetAllInstances(
            this IProvider creator, InjectContext context)
        {
            return creator.GetAllInstances(context, new List<TypeValuePair>());
        }

        public static List<object> GetAllInstances(
            this IProvider creator, InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsNotNull(context);

            var runner = creator.GetAllInstancesWithInjectSplit(context, args);

            // First get instance
            bool hasMore = runner.MoveNext();

            var instances = runner.Current;

            Assert.IsNotNull(instances, "Null value returned from creator '{0}'", creator.GetType());

            // Now do injection
            while (hasMore)
            {
                hasMore = runner.MoveNext();
            }

            return instances;
        }

        public static object TryGetInstance(
            this IProvider creator, InjectContext context)
        {
            return creator.TryGetInstance(context, new List<TypeValuePair>());
        }

        public static object TryGetInstance(
            this IProvider creator, InjectContext context, List<TypeValuePair> args)
        {
            var allInstances = creator.GetAllInstances(context, args);

            if (allInstances.IsEmpty())
            {
                return null;
            }

            Assert.That(allInstances.Count == 1,
                "Provider returned multiple instances when one or zero was expected");

            return allInstances[0];
        }

        public static object GetInstance(
            this IProvider creator, InjectContext context)
        {
            return creator.GetInstance(context, new List<TypeValuePair>());
        }

        public static object GetInstance(
            this IProvider creator, InjectContext context, List<TypeValuePair> args)
        {
            var allInstances = creator.GetAllInstances(context, args);

            Assert.That(!allInstances.IsEmpty(),
                "Provider returned zero instances when one was expected when looking up type '{0}'", context.MemberType);

            Assert.That(allInstances.Count == 1,
                "Provider returned multiple instances when only one was expected when looking up type '{0}'", context.MemberType);

            return allInstances[0];
        }
    }
}