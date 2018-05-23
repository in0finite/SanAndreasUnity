namespace Zenject
{
    public class SignalBinder : ConditionCopyNonLazyBinder
    {
        private readonly SignalSettings _signalSettings;

        public SignalBinder(
            BindInfo bindInfo, SignalSettings signalSettings)
            : base(bindInfo)
        {
            _signalSettings = signalSettings;
        }

        public ConditionCopyNonLazyBinder RequireHandler()
        {
            _signalSettings.RequiresHandler = true;
            return this;
        }
    }

    public class SignalBinderWithId : SignalBinder
    {
        public SignalBinderWithId(
            BindInfo bindInfo, SignalSettings signalSettings)
            : base(bindInfo, signalSettings)
        {
        }

        public SignalBinder WithId(object identifier)
        {
            this.BindInfo.Identifier = identifier;
            return this;
        }
    }
}