using System;

namespace UniEasy.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RuntimeCustomPropertyDrawer : Attribute
    {
        #region Fields

        public Type Type;
        public bool UseForChildren;

        #endregion

        #region Constructors

        public RuntimeCustomPropertyDrawer(Type type)
        {
            Type = type;
        }

        public RuntimeCustomPropertyDrawer(Type type, bool useForChildren)
        {
            Type = type;
            UseForChildren = useForChildren;
        }

        #endregion
    }
}
