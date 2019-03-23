namespace UniEasy.DI
{
    public class FromBinder : ScopeBinder
    {
        protected IBindingFinalizer BindingFinalizer;

        public FromBinder(BindInfo bindInfo, BindFinalizerWrapper finalizerWrapper) : base(bindInfo)
        {
            FinalizerWrapper = finalizerWrapper;
        }

        protected BindFinalizerWrapper FinalizerWrapper
        {
            get;
            private set;
        }

        protected IBindingFinalizer SubFinalizer
        {
            set
            {
                FinalizerWrapper.SubFinalizer = value;
            }
        }

        protected ScopeBinder FromInstanceBase(object instance)
        {
            SubFinalizer = new ScopableBindingFinalizer(BindInfo, (container, concreteType) =>
            {
                return new InstanceProvider(container, concreteType, instance);
            });
            return this;
        }

        public ScopeBinder FromResolve()
        {
            return FromResolve(null);
        }

        public ScopeBinder FromResolve(object identifier)
        {
            SubFinalizer = new ScopableBindingFinalizer(BindInfo,
                (container, concreteType) => new ResolveProvider(concreteType, container, identifier));

            return this;
        }
    }
}
