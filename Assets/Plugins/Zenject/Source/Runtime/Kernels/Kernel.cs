using System;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class Kernel : IInitializable, IDisposable, ITickable, ILateTickable, IFixedTickable, ILateDisposable
    {
        [InjectLocal]
        private TickableManager _tickableManager = null;

        [InjectLocal]
        private InitializableManager _initializableManager = null;

        [InjectLocal]
        private DisposableManager _disposablesManager = null;

        public virtual void Initialize()
        {
            _initializableManager.Initialize();
        }

        public virtual void Dispose()
        {
            _disposablesManager.Dispose();
        }

        public virtual void LateDispose()
        {
            _disposablesManager.LateDispose();
        }

        public virtual void Tick()
        {
            _tickableManager.Update();
        }

        public virtual void LateTick()
        {
            _tickableManager.LateUpdate();
        }

        public virtual void FixedTick()
        {
            _tickableManager.FixedUpdate();
        }
    }
}