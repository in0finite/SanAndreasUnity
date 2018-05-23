using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    // Zero parameters

    public class SubContainerCreatorByMethod : ISubContainerCreator
    {
        private readonly Action<DiContainer> _installMethod;
        private readonly DiContainer _container;

        public SubContainerCreatorByMethod(
            DiContainer container,
            Action<DiContainer> installMethod)
        {
            _installMethod = installMethod;
            _container = container;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context)
        {
            Assert.IsEmpty(args);

            var subContainer = _container.CreateSubContainer();

            _installMethod(subContainer);

            subContainer.ResolveDependencyRoots();
            subContainer.FlushInjectQueue();

            if (subContainer.IsValidating)
            {
                // The root-level Container has its ValidateValidatables method
                // called explicitly - however, this is not so for sub-containers
                // so call it here instead
                subContainer.ValidateValidatables();
            }

            return subContainer;
        }
    }

    // One parameters

    public class SubContainerCreatorByMethod<TParam1> : ISubContainerCreator
    {
        private readonly Action<DiContainer, TParam1> _installMethod;
        private readonly DiContainer _container;

        public SubContainerCreatorByMethod(
            DiContainer container,
            Action<DiContainer, TParam1> installMethod)
        {
            _installMethod = installMethod;
            _container = container;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context)
        {
            Assert.IsEqual(args.Count, 1);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());

            var subContainer = _container.CreateSubContainer();

            _installMethod(subContainer, (TParam1)args[0].Value);

            subContainer.ResolveDependencyRoots();
            subContainer.FlushInjectQueue();

            if (subContainer.IsValidating)
            {
                // The root-level Container has its ValidateValidatables method
                // called explicitly - however, this is not so for sub-containers
                // so call it here instead
                subContainer.ValidateValidatables();
            }

            return subContainer;
        }
    }

    // Two parameters

    public class SubContainerCreatorByMethod<TParam1, TParam2> : ISubContainerCreator
    {
        private readonly Action<DiContainer, TParam1, TParam2> _installMethod;
        private readonly DiContainer _container;

        public SubContainerCreatorByMethod(
            DiContainer container,
            Action<DiContainer, TParam1, TParam2> installMethod)
        {
            _installMethod = installMethod;
            _container = container;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context)
        {
            Assert.IsEqual(args.Count, 2);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());

            var subContainer = _container.CreateSubContainer();

            _installMethod(
                subContainer,
                (TParam1)args[0].Value,
                (TParam2)args[1].Value);

            subContainer.ResolveDependencyRoots();
            subContainer.FlushInjectQueue();

            if (subContainer.IsValidating)
            {
                // The root-level Container has its ValidateValidatables method
                // called explicitly - however, this is not so for sub-containers
                // so call it here instead
                subContainer.ValidateValidatables();
            }

            return subContainer;
        }
    }

    // Three parameters

    public class SubContainerCreatorByMethod<TParam1, TParam2, TParam3> : ISubContainerCreator
    {
        private readonly Action<DiContainer, TParam1, TParam2, TParam3> _installMethod;
        private readonly DiContainer _container;

        public SubContainerCreatorByMethod(
            DiContainer container,
            Action<DiContainer, TParam1, TParam2, TParam3> installMethod)
        {
            _installMethod = installMethod;
            _container = container;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context)
        {
            Assert.IsEqual(args.Count, 3);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());

            var subContainer = _container.CreateSubContainer();

            _installMethod(
                subContainer,
                (TParam1)args[0].Value,
                (TParam2)args[1].Value,
                (TParam3)args[2].Value);

            subContainer.ResolveDependencyRoots();
            subContainer.FlushInjectQueue();

            if (subContainer.IsValidating)
            {
                // The root-level Container has its ValidateValidatables method
                // called explicitly - however, this is not so for sub-containers
                // so call it here instead
                subContainer.ValidateValidatables();
            }

            return subContainer;
        }
    }

    // Four parameters

    public class SubContainerCreatorByMethod<TParam1, TParam2, TParam3, TParam4> : ISubContainerCreator
    {
        private readonly ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4> _installMethod;
        private readonly DiContainer _container;

        public SubContainerCreatorByMethod(
            DiContainer container,
            ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4> installMethod)
        {
            _installMethod = installMethod;
            _container = container;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context)
        {
            Assert.IsEqual(args.Count, 4);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());
            Assert.That(args[3].Type.DerivesFromOrEqual<TParam4>());

            var subContainer = _container.CreateSubContainer();

            _installMethod(
                subContainer,
                (TParam1)args[0].Value,
                (TParam2)args[1].Value,
                (TParam3)args[2].Value,
                (TParam4)args[3].Value);

            subContainer.ResolveDependencyRoots();
            subContainer.FlushInjectQueue();

            if (subContainer.IsValidating)
            {
                // The root-level Container has its ValidateValidatables method
                // called explicitly - however, this is not so for sub-containers
                // so call it here instead
                subContainer.ValidateValidatables();
            }

            return subContainer;
        }
    }

    // Five parameters

    public class SubContainerCreatorByMethod<TParam1, TParam2, TParam3, TParam4, TParam5> : ISubContainerCreator
    {
        private readonly ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5> _installMethod;
        private readonly DiContainer _container;

        public SubContainerCreatorByMethod(
            DiContainer container,
            ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5> installMethod)
        {
            _installMethod = installMethod;
            _container = container;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context)
        {
            Assert.IsEqual(args.Count, 5);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());
            Assert.That(args[3].Type.DerivesFromOrEqual<TParam4>());
            Assert.That(args[4].Type.DerivesFromOrEqual<TParam5>());

            var subContainer = _container.CreateSubContainer();

            _installMethod(
                subContainer,
                (TParam1)args[0].Value,
                (TParam2)args[1].Value,
                (TParam3)args[2].Value,
                (TParam4)args[3].Value,
                (TParam5)args[4].Value);

            subContainer.ResolveDependencyRoots();
            subContainer.FlushInjectQueue();

            if (subContainer.IsValidating)
            {
                // The root-level Container has its ValidateValidatables method
                // called explicitly - however, this is not so for sub-containers
                // so call it here instead
                subContainer.ValidateValidatables();
            }

            return subContainer;
        }
    }
}