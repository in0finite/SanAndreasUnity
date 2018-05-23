namespace Zenject
{
    public class CopyNonLazyBinder : NonLazyBinder
    {
        public CopyNonLazyBinder(BindInfo bindInfo)
            : base(bindInfo)
        {
        }

        public NonLazyBinder CopyIntoAllSubContainers()
        {
            BindInfo.CopyIntoAllSubContainers = true;
            return this;
        }

        // Would these variations be useful?

        // Only copy the binding into children and not grandchildren
        //public NonLazyBinder CopyIntoDirectSubContainers()

        // Do not apply the binding on the current container
        //public NonLazyBinder MoveIntoAllSubContainers()
        //public NonLazyBinder MoveIntoDirectSubContainers()
    }
}