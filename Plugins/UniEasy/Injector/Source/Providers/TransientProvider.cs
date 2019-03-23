using System.Collections.Generic;
using System;

namespace UniEasy.DI
{
    public class TransientProvider : IProvider
    {
        readonly DiContainer container;
        readonly Type concreteType;

        public TransientProvider(Type concreteType, DiContainer container)
        {
            this.container = container;
            this.concreteType = concreteType;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return concreteType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context)
        {
            bool autoInject = false;

            var instanceType = GetTypeToCreate(context.MemberType);

            var instance = container.Instantiate(instanceType, autoInject);

            yield return new List<object>() { instance };

            container.Inject(instance);
        }

        Type GetTypeToCreate(Type contractType)
        {
            return ProviderUtility.GetTypeToInstantiate(contractType, concreteType);
        }
    }
}
