using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class FactoryToChoiceBinder<TParam1, TContract> : FactoryFromBinder<TParam1, TContract>
    {
        public FactoryToChoiceBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindInfo, factoryBindInfo)
        {
        }

        // Note that this is the default, so not necessary to call
        public FactoryFromBinder<TParam1, TContract> ToSelf()
        {
            Assert.IsEqual(BindInfo.ToChoice, ToChoices.Self);
            return this;
        }

        public FactoryFromBinder<TParam1, TConcrete> To<TConcrete>()
            where TConcrete : TContract
        {
            BindInfo.ToChoice = ToChoices.Concrete;
            BindInfo.ToTypes = new List<Type>()
            {
                typeof(TConcrete)
            };

            return new FactoryFromBinder<TParam1, TConcrete>(BindInfo, FactoryBindInfo);
        }
    }
}