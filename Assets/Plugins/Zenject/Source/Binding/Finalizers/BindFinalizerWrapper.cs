using ModestTree;

namespace Zenject
{
    public class BindFinalizerWrapper : IBindingFinalizer
    {
        private IBindingFinalizer _subFinalizer;

        public IBindingFinalizer SubFinalizer
        {
            set { _subFinalizer = value; }
        }

        public bool CopyIntoAllSubContainers
        {
            get { return _subFinalizer.CopyIntoAllSubContainers; }
        }

        public void FinalizeBinding(DiContainer container)
        {
            Assert.IsNotNull(_subFinalizer,
                "Unfinished binding! Finalizer was not given.");

            _subFinalizer.FinalizeBinding(container);
        }
    }
}