using System.Collections.Generic;
using System;

namespace UniEasy.DI
{
    public class InstanceProvider : IProvider
    {
        private DiContainer container;
        private Type instanceType;
        private object instance;

        public InstanceProvider(DiContainer container, Type instanceType, object instance)
        {
            this.container = container;
            this.instanceType = instanceType;
            this.instance = instance;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return instanceType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context)
        {
            container.Inject(instance);
            yield return new List<object>() { instance };
        }
    }
}
