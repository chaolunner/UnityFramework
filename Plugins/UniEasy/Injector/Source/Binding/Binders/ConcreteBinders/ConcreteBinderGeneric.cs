using System.Collections.Generic;
using System.Linq;
using System;

namespace UniEasy.DI
{
    public class ConcreteBinderGeneric<TContract> : FromBinderGeneric<TContract>
    {
        public ConcreteBinderGeneric(BindInfo bindInfo, BindFinalizerWrapper finalizerWrapper) : base(bindInfo, finalizerWrapper)
        {
            ToSelf();
        }

        public FromBinderGeneric<TContract> ToSelf()
        {
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo, (container, type) => new TransientProvider(
                type, container));

            return this;
        }

        public FromBinderGeneric<TConcrete> To<TConcrete>()
            where TConcrete : TContract
        {
            BindInfo.ToTypes = new List<Type>() { typeof(TConcrete) };
            return new FromBinderGeneric<TConcrete>(BindInfo, FinalizerWrapper);
        }

        public FromBinderNonGeneric To(params Type[] concreteTypes)
        {
            return To((IEnumerable<Type>)concreteTypes);
        }

        public FromBinderNonGeneric To(IEnumerable<Type> concreteTypes)
        {
            BindInfo.ToTypes = concreteTypes.ToList();
            return new FromBinderNonGeneric(BindInfo, FinalizerWrapper);
        }
    }
}
