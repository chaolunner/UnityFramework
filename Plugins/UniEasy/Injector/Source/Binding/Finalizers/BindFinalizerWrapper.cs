namespace UniEasy.DI
{
    public class BindFinalizerWrapper : IBindingFinalizer
    {
        IBindingFinalizer subFinalizer;

        public IBindingFinalizer SubFinalizer
        {
            set
            {
                subFinalizer = value;
            }
        }

        public void FinalizeBinding(DiContainer container)
        {
            if (subFinalizer != null)
            {
                subFinalizer.FinalizeBinding(container);
            }
        }
    }
}
