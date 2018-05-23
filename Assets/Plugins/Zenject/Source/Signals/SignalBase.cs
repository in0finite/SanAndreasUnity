namespace Zenject
{
    public interface ISignalBase
    {
        int NumHandlers
        {
            get;
        }

        bool HasHandler
        {
            get;
        }
    }

    public class SignalSettings
    {
        public bool RequiresHandler;
    }

    public abstract class SignalBase : ISignalBase
    {
        private SignalManager _manager;

        [Inject]
        private void Construct(SignalManager manager, SignalSettings settings, BindInfo bindInfo)
        {
            _manager = manager;

            SignalId = new BindingId(this.GetType(), bindInfo.Identifier);
            Settings = settings;
        }

        protected BindingId SignalId
        {
            get;
            private set;
        }

        protected SignalSettings Settings
        {
            get;
            private set;
        }

        protected SignalManager Manager
        {
            get { return _manager; }
        }

        public int NumHandlers
        {
            get { return _manager.GetNumHandlers(SignalId); }
        }

        public bool HasHandler
        {
            get { return _manager.IsHandlerRegistered(SignalId); }
        }
    }
}