using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public static partial class PropertyDrawerExtensions
    {
        public static PropertyAttribute GetAttribute(this PropertyDrawer drawer)
        {
            return PropertyDrawerHelper.GetAttribute(drawer);
        }

        public static void SetAttribute(this PropertyDrawer drawer, PropertyAttribute attr)
        {
            PropertyDrawerHelper.SetAttribute(drawer, attr);
        }

        public static FieldInfo GetFieldInfo(this PropertyDrawer drawer)
        {
            return PropertyDrawerHelper.GetFieldInfo(drawer);
        }

        public static void SetFieldInfo(this PropertyDrawer drawer, FieldInfo info)
        {
            PropertyDrawerHelper.SetFieldInfo(drawer, info);
        }

        public static void OnGUISafe(this PropertyDrawer drawer, Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyDrawerHelper.OnGUISafe(drawer, position, property, label);
        }

        public static float GetPropertyHeightSafe(this PropertyDrawer drawer, SerializedProperty property, GUIContent label)
        {
            return PropertyDrawerHelper.GetPropertyHeightSafe(drawer, property, label);
        }
    }
}
