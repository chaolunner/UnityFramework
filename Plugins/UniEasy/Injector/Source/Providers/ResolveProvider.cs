using System.Collections.Generic;
using System.Linq;
using System;

namespace UniEasy.DI
{
    public class ResolveProvider : IProvider
    {
        readonly object identifier;
        readonly DiContainer container;
        readonly Type contractType;

        public ResolveProvider(Type contractType, DiContainer container, object identifier)
        {
            this.contractType = contractType;
            this.identifier = identifier;
            this.container = container;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return contractType;
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context)
        {
            yield return container.ResolveAll(GetContext(context)).Cast<object>().ToList();
        }

        InjectContext GetContext(InjectContext context)
        {
            return context.CreateContext(contractType, identifier);
        }
    }
}
