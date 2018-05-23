namespace Zenject
{
    public interface IBindingFinalizer
    {
        bool CopyIntoAllSubContainers
        {
            get;
        }

        void FinalizeBinding(DiContainer container);
    }
}