namespace Zenject
{
    public class FactoryToChoiceIdBinder<TParam1, TContract> : FactoryToChoiceBinder<TParam1, TContract>
    {
        public FactoryToChoiceIdBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindInfo, factoryBindInfo)
        {
        }

        public FactoryToChoiceBinder<TParam1, TContract> WithId(object identifier)
        {
            BindInfo.Identifier = identifier;
            return this;
        }
    }
}