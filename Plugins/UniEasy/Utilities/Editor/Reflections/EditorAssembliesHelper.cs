using System.Collections.Generic;
using System.Reflection;
using System;

namespace UniEasy.Editor
{
    public class EditorAssembliesHelper
    {
        #region Static Fields

        private static MethodInfo subclassesOfMethodInfo;

        #endregion

        #region Static Methods

        public static IEnumerable<Type> SubclassesOf(Type parent)
        {
            if (subclassesOfMethodInfo == null)
            {
                subclassesOfMethodInfo = TypeHelper.EditorAssembliesType.GetMethod("SubclassesOf", BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (subclassesOfMethodInfo != null)
            {
                return (IEnumerable<Type>)subclassesOfMethodInfo.Invoke(null, new object[] { parent });
            }
            return null;
        }

        #endregion
    }
}
