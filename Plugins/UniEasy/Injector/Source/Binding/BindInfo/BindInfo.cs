using System.Collections.Generic;
using System;

namespace UniEasy.DI
{
    public enum ScopeTypes
    {
        Transient,
        Singleton,
    }

    public class BindInfo
    {
        public BindInfo(List<Type> contractTypes)
        {
            Identifier = null;
            ContractTypes = contractTypes;
            ToTypes = new List<Type>();
            NonLazy = false;
            Scope = ScopeTypes.Transient;
        }

        public BindInfo(Type contractType) : this(new List<Type>() { contractType })
        {
        }

        public object Identifier
        {
            get;
            set;
        }

        public bool NonLazy
        {
            get;
            set;
        }

        public List<Type> ContractTypes
        {
            get;
            set;
        }

        public List<Type> ToTypes
        {
            get;
            set;
        }

        public BindingCondition Condition
        {
            get;
            set;
        }

        public ScopeTypes Scope
        {
            get;
            set;
        }

        public object ConcreteIdentifier
        {
            get;
            set;
        }
    }
}
