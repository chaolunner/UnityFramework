namespace UniEasy.DI
{
    public class FromBinderNonGeneric : FromBinder
    {
        public FromBinderNonGeneric(BindInfo bindInfo, BindFinalizerWrapper finalizerWrapper) : base(bindInfo, finalizerWrapper)
        {
        }

        public ScopeBinder FromInstance(object instance)
        {
            return FromInstanceBase(instance);
        }
    }
}
