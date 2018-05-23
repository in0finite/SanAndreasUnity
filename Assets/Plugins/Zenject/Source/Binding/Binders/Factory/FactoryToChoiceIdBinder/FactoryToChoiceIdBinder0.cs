namespace Zenject
{
    public class FactoryToChoiceIdBinder<TContract> : FactoryToChoiceBinder<TContract>
    {
        public FactoryToChoiceIdBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindInfo, factoryBindInfo)
        {
        }

        public FactoryToChoiceBinder<TContract> WithId(object identifier)
        {
            BindInfo.Identifier = identifier;
            return this;
        }
    }
}