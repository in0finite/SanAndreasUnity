namespace Zenject
{
    public class NullBindingFinalizer : IBindingFinalizer
    {
        public bool CopyIntoAllSubContainers
        {
            get { return false; }
        }

        public void FinalizeBinding(DiContainer container)
        {
            // Do nothing
        }
    }
}