using System;

namespace Zenject
{
    public class FactoryBindInfo
    {
        public FactoryBindInfo(Type factoryType)
        {
            FactoryType = factoryType;
        }

        public Type FactoryType
        {
            get; private set;
        }

        public Func<DiContainer, IProvider> ProviderFunc
        {
            get; set;
        }
    }
}