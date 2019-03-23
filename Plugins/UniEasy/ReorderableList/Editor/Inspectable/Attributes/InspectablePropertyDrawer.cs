using System;

namespace UniEasy.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class InspectablePropertyDrawer : Attribute
    {
        #region Fields

        public Type Type;

        public bool UseForChildren;

        #endregion

        #region Constructors

        public InspectablePropertyDrawer(Type type)
        {
            Type = type;
        }

        public InspectablePropertyDrawer(Type type, bool useForChildren)
        {
            Type = type;
            UseForChildren = useForChildren;
        }

        #endregion
    }
}
