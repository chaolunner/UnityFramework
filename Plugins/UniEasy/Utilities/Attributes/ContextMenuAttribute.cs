using System;

namespace UniEasy
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | 
        AttributeTargets.Parameter | AttributeTargets.Property | 
        AttributeTargets.Field | AttributeTargets.Interface, 
        AllowMultiple = true, Inherited = false)]
    public class ContextMenuAttribute : Attribute
    {
        public string MenuItem;
        public bool Validate;
        public int Priority;

        public ContextMenuAttribute(string itemName) : this(itemName, false)
        {
        }

        public ContextMenuAttribute(string itemName, bool isValidateFunction) : this(itemName, isValidateFunction, 0)
        {
        }

        public ContextMenuAttribute(string itemName, bool isValidateFunction, int priority)
        {
            MenuItem = itemName;
            Validate = isValidateFunction;
            Priority = priority;
        }
    }
}
