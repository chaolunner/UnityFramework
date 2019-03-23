namespace UniEasy.DI
{
    public class ScopeBinder : ConditionBinder
    {
        public ScopeBinder(BindInfo bindInfo) : base(bindInfo)
        {
        }

        public ConditionBinder AsSingle()
        {
            return AsSingle(null);
        }

        public ConditionBinder AsSingle(object concreteIdentifier)
        {
            BindInfo.Scope = ScopeTypes.Singleton;
            BindInfo.ConcreteIdentifier = concreteIdentifier;
            return this;
        }
    }
}
