using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class SubContainerCreatorByInstaller : ISubContainerCreator
    {
        private readonly Type _installerType;
        private readonly DiContainer _container;
        private readonly List<TypeValuePair> _extraArgs;

        public SubContainerCreatorByInstaller(
            DiContainer container, Type installerType, List<TypeValuePair> extraArgs)
        {
            _installerType = installerType;
            _container = container;
            _extraArgs = extraArgs;

            Assert.That(installerType.DerivesFrom<InstallerBase>(),
                "Invalid installer type given during bind command.  Expected type '{0}' to derive from 'Installer<>'", installerType);
        }

        public SubContainerCreatorByInstaller(
            DiContainer container, Type installerType)
            : this(container, installerType, new List<TypeValuePair>())
        {
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context)
        {
            var subContainer = _container.CreateSubContainer();

            var installer = (InstallerBase)subContainer.InstantiateExplicit(
                _installerType, args.Concat(_extraArgs).ToList());
            installer.InstallBindings();

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