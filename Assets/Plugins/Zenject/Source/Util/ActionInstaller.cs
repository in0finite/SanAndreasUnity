using System;

namespace Zenject
{
    public class ActionInstaller : Installer<ActionInstaller>
    {
        private readonly Action<DiContainer> _installMethod;

        public ActionInstaller(Action<DiContainer> installMethod)
        {
            _installMethod = installMethod;
        }

        public override void InstallBindings()
        {
            _installMethod(Container);
        }
    }
}