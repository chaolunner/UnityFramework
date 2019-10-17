using System.Reflection;
using UnityEditor;

namespace UniEasy.Editor
{
    public class SerializedPropertyHelper
    {
        #region Static Fields

        private static PropertyInfo hashCodeForPropertyPathWithoutArrayIndexPropertyInfo;

        #endregion

        #region Static Methods

        public static int GetHashCodeForPropertyPathWithoutArrayIndex(SerializedProperty property)
        {
            if (hashCodeForPropertyPathWithoutArrayIndexPropertyInfo == null)
            {
                hashCodeForPropertyPathWithoutArrayIndexPropertyInfo = typeof(SerializedProperty).GetProperty("hashCodeForPropertyPathWithoutArrayIndex", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (hashCodeForPropertyPathWithoutArrayIndexPropertyInfo != null)
            {
                return (int)hashCodeForPropertyPathWithoutArrayIndexPropertyInfo.GetValue(property);
            }
            return -1;
        }

        #endregion
    }
}
