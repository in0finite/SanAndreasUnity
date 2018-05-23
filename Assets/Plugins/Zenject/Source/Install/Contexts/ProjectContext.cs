#if !NOT_UNITY3D

using ModestTree;

using System.Collections.Generic;
using Zenject.Internal;

#if UNITY_EDITOR
#endif

using UnityEngine;

namespace Zenject
{
    public class ProjectContext : Context
    {
        public const string ProjectContextResourcePath = "ProjectContext";
        public const string ProjectContextResourcePathOld = "ProjectCompositionRoot";

        private static ProjectContext _instance;

        private DiContainer _container;

        public override DiContainer Container
        {
            get { return _container; }
        }

        public static bool HasInstance
        {
            get { return _instance != null; }
        }

        public static ProjectContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    InstantiateAndInitialize();
                    Assert.IsNotNull(_instance);
                }

                return _instance;
            }
        }

#if UNITY_EDITOR

        public static bool ValidateOnNextRun
        {
            get;
            set;
        }

#endif

        public override IEnumerable<GameObject> GetRootGameObjects()
        {
            return new[] { this.gameObject };
        }

        public static GameObject TryGetPrefab()
        {
            var prefab = (GameObject)Resources.Load(ProjectContextResourcePath);

            if (prefab == null)
            {
                prefab = (GameObject)Resources.Load(ProjectContextResourcePathOld);
            }

            return prefab;
        }

        private static void InstantiateAndInitialize()
        {
            Assert.That(GameObject.FindObjectsOfType<ProjectContext>().IsEmpty(),
                "Tried to create multiple instances of ProjectContext!");

            var prefab = TryGetPrefab();

            bool shouldMakeActive = false;

            if (prefab == null)
            {
                _instance = new GameObject("ProjectContext")
                    .AddComponent<ProjectContext>();
            }
            else
            {
                var wasActive = prefab.activeSelf;

                shouldMakeActive = wasActive;

                if (wasActive)
                {
                    prefab.SetActive(false);
                }

                try
                {
                    _instance = GameObject.Instantiate(prefab).GetComponent<ProjectContext>();
                }
                finally
                {
                    if (wasActive)
                    {
                        // Always make sure to reset prefab state otherwise this change could be saved
                        // persistently
                        prefab.SetActive(true);
                    }
                }

                Assert.IsNotNull(_instance,
                    "Could not find ProjectContext component on prefab 'Resources/{0}.prefab'", ProjectContextResourcePath);
            }

            // Note: We use Initialize instead of awake here in case someone calls
            // ProjectContext.Instance while ProjectContext is initializing
            _instance.Initialize();

            if (shouldMakeActive)
            {
                // We always instantiate it as disabled so that Awake and Start events are triggered after inject
                _instance.gameObject.SetActive(true);
            }
        }

        public void EnsureIsInitialized()
        {
            // Do nothing - Initialize occurs in Instance property
        }

        public void Awake()
        {
            if (Application.isPlaying)
            // DontDestroyOnLoad can only be called when in play mode and otherwise produces errors
            // ProjectContext is created during design time (in an empty scene) when running validation
            // and also when running unit tests
            // In these cases we don't need DontDestroyOnLoad so just skip it
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Initialize()
        {
            Assert.IsNull(_container);

            bool isValidating = false;

#if UNITY_EDITOR
            isValidating = ValidateOnNextRun;

            // Reset immediately to ensure it doesn't get used in another run
            ValidateOnNextRun = false;
#endif

            _container = new DiContainer(
                new DiContainer[] { StaticContext.Container }, isValidating);

            var injectableMonoBehaviours = new List<MonoBehaviour>();
            GetInjectableMonoBehaviours(injectableMonoBehaviours);

            foreach (var instance in injectableMonoBehaviours)
            {
                _container.QueueForInject(instance);
            }

            _container.IsInstalling = true;

            try
            {
                InstallBindings(injectableMonoBehaviours);
            }
            finally
            {
                _container.IsInstalling = false;
            }

            _container.ResolveDependencyRoots();

            _container.FlushInjectQueue();
        }

        protected override void GetInjectableMonoBehaviours(List<MonoBehaviour> monoBehaviours)
        {
            ZenUtilInternal.GetInjectableMonoBehaviours(this.gameObject, monoBehaviours);
        }

        private void InstallBindings(List<MonoBehaviour> injectableMonoBehaviours)
        {
            _container.DefaultParent = this.transform;

            // Note that adding GuiRenderableManager here doesn't instantiate it by default
            // You still have to add GuiRenderer manually
            // We could add the contents of GuiRenderer into MonoKernel, but this adds
            // undesirable per-frame allocations.  See comment in IGuiRenderable.cs for usage
            //
            // Short answer is if you want to use IGuiRenderable then
            // you need to include the following in project context installer:
            // `Container.Bind<GuiRenderer>().FromNewComponentOnNewGameObject().AsSingle().CopyIntoAllSubContainers().NonLazy();`
            _container.Bind(typeof(TickableManager), typeof(InitializableManager), typeof(DisposableManager), typeof(GuiRenderableManager))
                .ToSelf().AsSingle().CopyIntoAllSubContainers();

            _container.Bind<SignalManager>().AsSingle();
            _container.Bind<Context>().FromInstance(this);

            _container.Bind(typeof(ProjectKernel), typeof(MonoKernel))
                .To<ProjectKernel>().FromNewComponentOn(this.gameObject).AsSingle().NonLazy();

            _container.Bind<SceneContextRegistry>().AsSingle();

            InstallSceneBindings(injectableMonoBehaviours);
            InstallInstallers();
        }
    }
}

#endif