namespace Zenject
{
    public class IdScopeConditionCopyNonLazyBinder : ScopeConditionCopyNonLazyBinder
    {
        public IdScopeConditionCopyNonLazyBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        public ScopeConditionCopyNonLazyBinder WithId(object identifier)
        {
            BindInfo.Identifier = identifier;
            return this;
        }
    }
}