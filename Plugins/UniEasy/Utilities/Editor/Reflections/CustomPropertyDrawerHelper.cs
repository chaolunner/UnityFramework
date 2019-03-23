using System.Reflection;
using UnityEditor;
using System;

namespace UniEasy.Editor
{
    public class CustomPropertyDrawerHelper
    {
        #region Static Fields

        private static FieldInfo typeFieldInfo;
        private static FieldInfo useForChildrenFieldInfo;

        #endregion

        #region Static Methods

        public static Type GetType(CustomPropertyDrawer drawer)
        {
            if (typeFieldInfo == null)
            {
                typeFieldInfo = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (typeFieldInfo != null)
            {
                return (Type)typeFieldInfo.GetValue(drawer);
            }
            return null;
        }

        public static bool UseForChildren(CustomPropertyDrawer drawer)
        {
            if (useForChildrenFieldInfo == null)
            {
                useForChildrenFieldInfo = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (useForChildrenFieldInfo != null)
            {
                return (bool)useForChildrenFieldInfo.GetValue(drawer);
            }
            return false;
        }

        #endregion
    }
}
