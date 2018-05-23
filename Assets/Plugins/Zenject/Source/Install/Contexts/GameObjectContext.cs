#if !NOT_UNITY3D

using ModestTree;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject.Internal;

#pragma warning disable 649

namespace Zenject
{
    public class GameObjectContext : RunnableContext
    {
        [SerializeField]
        [Tooltip("Note that this field is optional and can be ignored in most cases.  This is really only needed if you want to control the 'Script Execution Order' of your subcontainer.  In this case, define a new class that derives from MonoKernel, add it to this game object, then drag it into this field.  Then you can set a value for 'Script Execution Order' for this new class and this will control when all ITickable/IInitializable classes bound within this subcontainer get called.")]
        [FormerlySerializedAs("_facade")]
        private MonoKernel _kernel;

        private DiContainer _container;

        public override DiContainer Container
        {
            get { return _container; }
        }

        public override IEnumerable<GameObject> GetRootGameObjects()
        {
            return new[] { this.gameObject };
        }

        [Inject]
        public void Construct(
            DiContainer parentContainer)
        {
            Assert.IsNull(_container);

            _container = parentContainer.CreateSubContainer();

            Initialize();
        }

        protected override void RunInternal()
        {
            var injectableMonoBehaviours = new List<MonoBehaviour>();

            GetInjectableMonoBehaviours(injectableMonoBehaviours);

            foreach (var instance in injectableMonoBehaviours)
            {
                if (instance is MonoKernel)
                {
                    Assert.That(ReferenceEquals(instance, _kernel),
                        "Found MonoKernel derived class that is not hooked up to GameObjectContext.  If you use MonoKernel, you must indicate this to GameObjectContext by dragging and dropping it to the Kernel field in the inspector");
                }

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

            if (_container.IsValidating)
            {
                // The root-level Container has its ValidateValidatables method
                // called explicitly - however, this is not so for sub-containers
                // so call it here instead
                _container.ValidateValidatables();
            }

            // Normally, the IInitializable.Initialize method would be called during MonoKernel.Start
            // However, this behaviour is undesirable for dynamically created objects, since Unity
            // has the strange behaviour of waiting until the end of the frame to call Start() on
            // dynamically created objects, which means that any GameObjectContext that is created
            // dynamically via a factory cannot be used immediately after calling Create(), since
            // it will not have been initialized
            // So we have chosen to diverge from Unity behaviour here and trigger IInitializable.Initialize
            // immediately - but only when the GameObjectContext is created dynamically.  For any
            // GameObjectContext's that are placed in the scene, we still want to execute
            // IInitializable.Initialize during Start()
            if (gameObject.scene.isLoaded && !_container.IsValidating)
            {
                _kernel = _container.Resolve<MonoKernel>();
                _kernel.Initialize();
            }
        }

        protected override void GetInjectableMonoBehaviours(List<MonoBehaviour> monoBehaviours)
        {
            // We inject on all components on the root except ourself
            foreach (var monoBehaviour in GetComponents<MonoBehaviour>())
            {
                if (monoBehaviour == null)
                {
                    // Missing script
                    continue;
                }

                if (!ZenUtilInternal.IsInjectableMonoBehaviourType(monoBehaviour.GetType()))
                {
                    continue;
                }

                if (monoBehaviour == this)
                {
                    continue;
                }

                monoBehaviours.Add(monoBehaviour);
            }

            for (int i = 0; i < this.transform.childCount; i++)
            {
                var child = this.transform.GetChild(i);

                if (child != null)
                {
                    ZenUtilInternal.GetInjectableMonoBehaviours(
                        child.gameObject, monoBehaviours);
                }
            }
        }

        private void InstallBindings(List<MonoBehaviour> injectableMonoBehaviours)
        {
            _container.DefaultParent = this.transform;

            _container.Bind<Context>().FromInstance(this);
            _container.Bind<GameObjectContext>().FromInstance(this);

            if (_kernel == null)
            {
                _container.Bind<MonoKernel>()
                    .To<DefaultGameObjectKernel>().FromNewComponentOn(this.gameObject).AsSingle().NonLazy();
            }
            else
            {
                _container.Bind<MonoKernel>().FromInstance(_kernel).AsSingle().NonLazy();
            }

            InstallSceneBindings(injectableMonoBehaviours);
            InstallInstallers();
        }
    }
}

#endif