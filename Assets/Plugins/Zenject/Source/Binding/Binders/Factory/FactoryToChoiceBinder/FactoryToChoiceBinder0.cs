using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class FactoryToChoiceBinder<TContract> : FactoryFromBinder<TContract>
    {
        public FactoryToChoiceBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindInfo, factoryBindInfo)
        {
        }

        // Note that this is the default, so not necessary to call
        public FactoryFromBinder<TContract> ToSelf()
        {
            Assert.IsEqual(BindInfo.ToChoice, ToChoices.Self);
            return this;
        }

        public FactoryFromBinder<TConcrete> To<TConcrete>()
            where TConcrete : TContract
        {
            BindInfo.ToChoice = ToChoices.Concrete;
            BindInfo.ToTypes = new List<Type>()
            {
                typeof(TConcrete)
            };

            return new FactoryFromBinder<TConcrete>(BindInfo, FactoryBindInfo);
        }
    }
}