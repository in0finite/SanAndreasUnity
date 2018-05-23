using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class SubContainerDependencyProvider : IProvider
    {
        private readonly ISubContainerCreator _subContainerCreator;
        private readonly Type _dependencyType;
        private readonly object _identifier;

        // if concreteType is null we use the contract type from inject context
        public SubContainerDependencyProvider(
            Type dependencyType,
            object identifier,
            ISubContainerCreator subContainerCreator)
        {
            _subContainerCreator = subContainerCreator;
            _dependencyType = dependencyType;
            _identifier = identifier;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _dependencyType;
        }

        private InjectContext CreateSubContext(
            InjectContext parent, DiContainer subContainer)
        {
            var subContext = parent.CreateSubContext(_dependencyType, _identifier);

            subContext.Container = subContainer;

            // This is important to avoid infinite loops
            subContext.SourceType = InjectSources.Local;

            return subContext;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context, List<TypeValuePair> args)
        {
            Assert.IsNotNull(context);

            var subContainer = _subContainerCreator.CreateSubContainer(args, context);

            var subContext = CreateSubContext(context, subContainer);

            yield return subContainer.ResolveAll(subContext).Cast<object>().ToList();
        }
    }
}