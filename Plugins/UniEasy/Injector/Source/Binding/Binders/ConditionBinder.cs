using System.Linq;
using System;

namespace UniEasy.DI
{
    public class ConditionBinder : NonLazyBinder
    {
        public ConditionBinder(BindInfo bindInfo) : base(bindInfo)
        {
        }

        public NonLazyBinder When(BindingCondition condition)
        {
            BindInfo.Condition = condition;
            return this;
        }

        public NonLazyBinder WhenInjectedIntoInstance(object instance)
        {
            BindInfo.Condition = r => ReferenceEquals(r.ObjectInstance, instance);
            return this;
        }

        public NonLazyBinder WhenInjectedInto(params Type[] targets)
        {
            BindInfo.Condition = r => targets.Where(x => r.ObjectType != null && !r.ObjectType.DerivesFromOrEqual(x)).IsEmpty();
            return this;
        }

        public NonLazyBinder WhenInjectedInto<T>()
        {
            BindInfo.Condition = r => r.ObjectType != null && r.ObjectType.DerivesFromOrEqual(typeof(T));
            return this;
        }

        public NonLazyBinder WhenNotInjectedInto<T>()
        {
            BindInfo.Condition = r => r.ObjectType == null || !r.ObjectType.DerivesFromOrEqual(typeof(T));
            return this;
        }
    }
}
