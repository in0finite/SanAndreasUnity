using System;
using UnityEditor;

namespace Zenject
{
    public abstract class ZenjectEditorWindow : EditorWindow
    {
        [Inject]
        [NonSerialized]
        TickableManager _tickableManager = null;

        [Inject]
        [NonSerialized]
        InitializableManager _initializableManager = null;

        [Inject]
        [NonSerialized]
        DisposableManager _disposableManager = null;

        [Inject]
        [NonSerialized]
        GuiRenderableManager _guiRenderableManager = null;

        [NonSerialized]
        DiContainer _container;

        protected DiContainer Container
        {
            get { return _container; }
        }

        public virtual void OnEnable()
        {
            _container = new DiContainer(new DiContainer[] { StaticContext.Container });

            // Make sure we don't create any game objects since editor windows don't have a scene
            _container.AssertOnNewGameObjects = true;

            _container.Bind<TickableManager>().AsSingle();
            _container.Bind<InitializableManager>().AsSingle();
            _container.Bind<DisposableManager>().AsSingle();
            _container.Bind<GuiRenderableManager>().AsSingle();

            InstallBindings();

            _container.Inject(this);

            _initializableManager.Initialize();
        }

        public virtual void OnDisable()
        {
            if (_disposableManager != null)
            {
                _disposableManager.Dispose();
                _disposableManager = null;
            }
        }

        public virtual void Update()
        {
            if (_tickableManager != null)
            {
                _tickableManager.Update();
            }

            // We might also consider only calling Repaint when changes occur
            Repaint();
        }

        public virtual void OnGUI()
        {
            if (_guiRenderableManager != null)
            {
                _guiRenderableManager.OnGui();
            }
        }

        public abstract void InstallBindings();
    }
}
