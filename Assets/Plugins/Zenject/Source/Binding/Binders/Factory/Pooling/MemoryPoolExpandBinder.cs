namespace Zenject
{
    public class MemoryPoolExpandBinder<TContract> : FactoryToChoiceIdBinder<TContract>
    {
        public MemoryPoolExpandBinder(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo, MemoryPoolBindInfo poolBindInfo)
            : base(bindInfo, factoryBindInfo)
        {
            MemoryPoolBindInfo = poolBindInfo;

            ExpandByOneAtATime();
        }

        protected MemoryPoolBindInfo MemoryPoolBindInfo
        {
            get; private set;
        }

        public FactoryToChoiceIdBinder<TContract> ExpandByOneAtATime()
        {
            MemoryPoolBindInfo.ExpandMethod = PoolExpandMethods.OneAtATime;
            return this;
        }

        public FactoryToChoiceIdBinder<TContract> ExpandByDoubling()
        {
            MemoryPoolBindInfo.ExpandMethod = PoolExpandMethods.Double;
            return this;
        }
    }
}