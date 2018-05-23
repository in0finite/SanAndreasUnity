namespace Zenject
{
    public class FactoryToChoiceIdBinder<TParam1, TParam2, TParam3, TParam4, TContract> : FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TContract>
    {
        public FactoryToChoiceIdBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindInfo, factoryBindInfo)
        {
        }

        public FactoryToChoiceBinder<TParam1, TParam2, TParam3, TParam4, TContract> WithId(object identifier)
        {
            BindInfo.Identifier = identifier;
            return this;
        }
    }
}