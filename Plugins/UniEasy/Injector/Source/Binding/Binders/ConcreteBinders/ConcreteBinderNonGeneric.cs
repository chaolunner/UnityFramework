using System.Collections.Generic;
using System.Linq;
using System;

namespace UniEasy.DI
{
    public class ConcreteBinderNonGeneric : FromBinderNonGeneric
    {
        public ConcreteBinderNonGeneric(BindInfo bindInfo, BindFinalizerWrapper finalizerWrapper) : base(bindInfo, finalizerWrapper)
        {
            ToSelf();
        }

        public FromBinderNonGeneric ToSelf()
        {
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo, (container, type) => new TransientProvider(
                type, container));

            return this;
        }

        public FromBinderNonGeneric To<TConcrete>()
        {
            return To(typeof(TConcrete));
        }

        public FromBinderNonGeneric To(params Type[] concreteTypes)
        {
            return To((IEnumerable<Type>)concreteTypes);
        }

        public FromBinderNonGeneric To(IEnumerable<Type> concreteTypes)
        {
            BindInfo.ToTypes = concreteTypes.ToList();
            return this;
        }
    }
}
