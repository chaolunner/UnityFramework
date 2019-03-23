using System;

namespace UniEasy.DI
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method
    | AttributeTargets.Parameter | AttributeTargets.Property
    | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class InjectAttribute : Attribute
    {
        public object Id
        {
            get;
            set;
        }
    }
}
