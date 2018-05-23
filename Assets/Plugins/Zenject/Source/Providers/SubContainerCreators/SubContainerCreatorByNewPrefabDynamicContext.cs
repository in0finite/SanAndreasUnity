#if !NOT_UNITY3D

using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public abstract class SubContainerCreatorByNewPrefabDynamicContext : ISubContainerCreator
    {
        private readonly GameObjectCreationParameters _gameObjectBindInfo;
        private readonly IPrefabProvider _prefabProvider;
        private readonly DiContainer _container;

        public SubContainerCreatorByNewPrefabDynamicContext(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _prefabProvider = prefabProvider;
            _container = container;
        }

        public DiContainer CreateSubContainer(
            List<TypeValuePair> args, InjectContext parentContext)
        {
            var prefab = _prefabProvider.GetPrefab();

            bool shouldMakeActive;

            var gameObj = _container.CreateAndParentPrefab(
                prefab, _gameObjectBindInfo, null, out shouldMakeActive);

            if (gameObj.GetComponent<GameObjectContext>() != null)
            {
                throw Assert.CreateException(
                    "Found GameObjectContext already attached to prefab with name '{0}'!  When using ByNewPrefabMethod, the GameObjectContext is added to the prefab dynamically", prefab.name);
            }

            var context = gameObj.AddComponent<GameObjectContext>();

            AddInstallers(args, context);

            _container.Inject(context);

            if (shouldMakeActive)
            {
                gameObj.SetActive(true);
            }

            // Note: We don't need to call ValidateValidatables here because GameObjectContext does this for us

            return context.Container;
        }

        protected abstract void AddInstallers(List<TypeValuePair> args, GameObjectContext context);
    }

    public class SubContainerCreatorByNewPrefabInstaller : SubContainerCreatorByNewPrefabDynamicContext
    {
        private readonly Type _installerType;
        private readonly List<TypeValuePair> _extraArgs;

        public SubContainerCreatorByNewPrefabInstaller(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo,
            Type installerType, List<TypeValuePair> extraArgs)
            : base(container, prefabProvider, gameObjectBindInfo)
        {
            _installerType = installerType;
            _extraArgs = extraArgs;

            Assert.That(installerType.DerivesFrom<InstallerBase>(),
                "Invalid installer type given during bind command.  Expected type '{0}' to derive from 'Installer<>'", installerType);
        }

        protected override void AddInstallers(List<TypeValuePair> args, GameObjectContext context)
        {
            context.AddNormalInstaller(
                new ActionInstaller((subContainer) =>
                    {
                        var installer = (InstallerBase)subContainer.InstantiateExplicit(
                            _installerType, args.Concat(_extraArgs).ToList());
                        installer.InstallBindings();
                    }));
        }
    }

    public class SubContainerCreatorByNewPrefabMethod : SubContainerCreatorByNewPrefabDynamicContext
    {
        private readonly Action<DiContainer> _installerMethod;

        public SubContainerCreatorByNewPrefabMethod(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo,
            Action<DiContainer> installerMethod)
            : base(container, prefabProvider, gameObjectBindInfo)
        {
            _installerMethod = installerMethod;
        }

        protected override void AddInstallers(List<TypeValuePair> args, GameObjectContext context)
        {
            Assert.That(args.IsEmpty());
            context.AddNormalInstaller(
                new ActionInstaller(_installerMethod));
        }
    }

    public class SubContainerCreatorByNewPrefabMethod<TParam1> : SubContainerCreatorByNewPrefabDynamicContext
    {
        private readonly Action<DiContainer, TParam1> _installerMethod;

        public SubContainerCreatorByNewPrefabMethod(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo,
            Action<DiContainer, TParam1> installerMethod)
            : base(container, prefabProvider, gameObjectBindInfo)
        {
            _installerMethod = installerMethod;
        }

        protected override void AddInstallers(List<TypeValuePair> args, GameObjectContext context)
        {
            Assert.IsEqual(args.Count, 1);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());

            context.AddNormalInstaller(
                new ActionInstaller((subContainer) =>
                    {
                        _installerMethod(subContainer, (TParam1)args[0].Value);
                    }));
        }
    }

    public class SubContainerCreatorByNewPrefabMethod<TParam1, TParam2> : SubContainerCreatorByNewPrefabDynamicContext
    {
        private readonly Action<DiContainer, TParam1, TParam2> _installerMethod;

        public SubContainerCreatorByNewPrefabMethod(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo,
            Action<DiContainer, TParam1, TParam2> installerMethod)
            : base(container, prefabProvider, gameObjectBindInfo)
        {
            _installerMethod = installerMethod;
        }

        protected override void AddInstallers(List<TypeValuePair> args, GameObjectContext context)
        {
            Assert.IsEqual(args.Count, 2);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());

            context.AddNormalInstaller(
                new ActionInstaller((subContainer) =>
                    {
                        _installerMethod(subContainer,
                            (TParam1)args[0].Value,
                            (TParam2)args[1].Value);
                    }));
        }
    }

    public class SubContainerCreatorByNewPrefabMethod<TParam1, TParam2, TParam3> : SubContainerCreatorByNewPrefabDynamicContext
    {
        private readonly Action<DiContainer, TParam1, TParam2, TParam3> _installerMethod;

        public SubContainerCreatorByNewPrefabMethod(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo,
            Action<DiContainer, TParam1, TParam2, TParam3> installerMethod)
            : base(container, prefabProvider, gameObjectBindInfo)
        {
            _installerMethod = installerMethod;
        }

        protected override void AddInstallers(List<TypeValuePair> args, GameObjectContext context)
        {
            Assert.IsEqual(args.Count, 3);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());

            context.AddNormalInstaller(
                new ActionInstaller((subContainer) =>
                    {
                        _installerMethod(subContainer,
                            (TParam1)args[0].Value,
                            (TParam2)args[1].Value,
                            (TParam3)args[2].Value);
                    }));
        }
    }

    public class SubContainerCreatorByNewPrefabMethod<TParam1, TParam2, TParam3, TParam4> : SubContainerCreatorByNewPrefabDynamicContext
    {
        private readonly ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4> _installerMethod;

        public SubContainerCreatorByNewPrefabMethod(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo,
            ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4> installerMethod)
            : base(container, prefabProvider, gameObjectBindInfo)
        {
            _installerMethod = installerMethod;
        }

        protected override void AddInstallers(List<TypeValuePair> args, GameObjectContext context)
        {
            Assert.IsEqual(args.Count, 4);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());
            Assert.That(args[3].Type.DerivesFromOrEqual<TParam4>());

            context.AddNormalInstaller(
                new ActionInstaller((subContainer) =>
                    {
                        _installerMethod(subContainer,
                            (TParam1)args[0].Value,
                            (TParam2)args[1].Value,
                            (TParam3)args[2].Value,
                            (TParam4)args[3].Value);
                    }));
        }
    }

    public class SubContainerCreatorByNewPrefabMethod<TParam1, TParam2, TParam3, TParam4, TParam5> : SubContainerCreatorByNewPrefabDynamicContext
    {
        private readonly ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5> _installerMethod;

        public SubContainerCreatorByNewPrefabMethod(
            DiContainer container, IPrefabProvider prefabProvider,
            GameObjectCreationParameters gameObjectBindInfo,
            ModestTree.Util.Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5> installerMethod)
            : base(container, prefabProvider, gameObjectBindInfo)
        {
            _installerMethod = installerMethod;
        }

        protected override void AddInstallers(List<TypeValuePair> args, GameObjectContext context)
        {
            Assert.IsEqual(args.Count, 5);
            Assert.That(args[0].Type.DerivesFromOrEqual<TParam1>());
            Assert.That(args[1].Type.DerivesFromOrEqual<TParam2>());
            Assert.That(args[2].Type.DerivesFromOrEqual<TParam3>());
            Assert.That(args[3].Type.DerivesFromOrEqual<TParam4>());
            Assert.That(args[4].Type.DerivesFromOrEqual<TParam5>());

            context.AddNormalInstaller(
                new ActionInstaller((subContainer) =>
                    {
                        _installerMethod(subContainer,
                            (TParam1)args[0].Value,
                            (TParam2)args[1].Value,
                            (TParam3)args[2].Value,
                            (TParam4)args[3].Value,
                            (TParam5)args[4].Value);
                    }));
        }
    }
}

#endif