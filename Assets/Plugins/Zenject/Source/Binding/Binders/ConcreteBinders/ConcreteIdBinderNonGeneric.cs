namespace Zenject
{
    public class ConcreteIdBinderNonGeneric : ConcreteBinderNonGeneric
    {
        public ConcreteIdBinderNonGeneric(
            BindInfo bindInfo,
            BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo, finalizerWrapper)
        {
        }

        public ConcreteBinderNonGeneric WithId(object identifier)
        {
            BindInfo.Identifier = identifier;
            return this;
        }
    }
}