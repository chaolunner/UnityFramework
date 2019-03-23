using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class PropertyDrawerHelper
    {
        #region Static Fields

        private static FieldInfo attributeFieldInfo;
        private static FieldInfo fieldInfoFieldInfo;
        private static MethodInfo onGUISafeMethodInfo;
        private static MethodInfo getPropertyHeightSafeMethodInfo;

        #endregion

        #region Static Methods

        public static PropertyAttribute GetAttribute(PropertyDrawer drawer)
        {
            if (attributeFieldInfo == null)
            {
                attributeFieldInfo = typeof(PropertyDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (attributeFieldInfo != null)
            {
                return (PropertyAttribute)attributeFieldInfo.GetValue(drawer);
            }
            return null;
        }

        public static void SetAttribute(PropertyDrawer drawer, PropertyAttribute attr)
        {
            if (attributeFieldInfo == null)
            {
                attributeFieldInfo = typeof(PropertyDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (attributeFieldInfo != null)
            {
                attributeFieldInfo.SetValue(drawer, attr);
            }
        }

        public static FieldInfo GetFieldInfo(PropertyDrawer drawer)
        {
            if (fieldInfoFieldInfo == null)
            {
                fieldInfoFieldInfo = typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (fieldInfoFieldInfo != null)
            {
                return (FieldInfo)fieldInfoFieldInfo.GetValue(drawer);
            }
            return null;
        }

        public static void SetFieldInfo(PropertyDrawer drawer, FieldInfo info)
        {
            if (fieldInfoFieldInfo == null)
            {
                fieldInfoFieldInfo = typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (fieldInfoFieldInfo != null)
            {
                fieldInfoFieldInfo.SetValue(drawer, info);
            }
        }

        public static void OnGUISafe(PropertyDrawer drawer, Rect position, SerializedProperty property, GUIContent label)
        {
            if (onGUISafeMethodInfo == null)
            {
                onGUISafeMethodInfo = typeof(PropertyDrawer).GetMethod("OnGUISafe", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (onGUISafeMethodInfo != null)
            {
                onGUISafeMethodInfo.Invoke(drawer, new object[] { position, property, label });
            }
        }

        public static float GetPropertyHeightSafe(PropertyDrawer drawer, SerializedProperty property, GUIContent label)
        {
            if (getPropertyHeightSafeMethodInfo == null)
            {
                getPropertyHeightSafeMethodInfo = typeof(PropertyDrawer).GetMethod("GetPropertyHeightSafe", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (getPropertyHeightSafeMethodInfo != null)
            {
                return (float)getPropertyHeightSafeMethodInfo.Invoke(drawer, new object[] { property, label });
            }
            return 0;
        }

        #endregion
    }
}
