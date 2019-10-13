using System.Reflection;
using UnityEditor;

namespace UniEasy.Editor
{
    public class SerializedPropertyHelper
    {
        #region Static Fields

        private static PropertyInfo hashCodeForPropertyPathPropertyInfo;

        #endregion

        #region Static Methods

        public static int GetHashCodeForPropertyPath(SerializedProperty property)
        {
            if (hashCodeForPropertyPathPropertyInfo == null)
            {
                hashCodeForPropertyPathPropertyInfo = typeof(SerializedProperty).GetProperty("hashCodeForPropertyPath", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (hashCodeForPropertyPathPropertyInfo != null)
            {
                return (int)hashCodeForPropertyPathPropertyInfo.GetValue(property);
            }
            return -1;
        }

        #endregion
    }
}
