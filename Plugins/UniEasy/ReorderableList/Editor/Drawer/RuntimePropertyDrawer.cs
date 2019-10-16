using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class RuntimePropertyDrawer : GUIDrawer
    {
        #region Fields

        public FieldInfo FieldInfo;
        public PropertyAttribute Attribute;

        #endregion

        #region Methods

        public virtual bool CanCacheInspectorGUI(RuntimeSerializedProperty property)
        {
            return true;
        }

        internal bool CanCacheInspectorGUISafe(RuntimeSerializedProperty property)
        {
            RuntimeScriptAttributeUtility.s_DrawerStack.Push(this);
            bool result = this.CanCacheInspectorGUI(property);
            RuntimeScriptAttributeUtility.s_DrawerStack.Pop();
            return result;
        }

        public virtual float GetPropertyHeight(RuntimeSerializedProperty property, GUIContent label)
        {
            return 16f;
        }

        internal float GetPropertyHeightSafe(RuntimeSerializedProperty property, GUIContent label)
        {
            RuntimeScriptAttributeUtility.s_DrawerStack.Push(this);
            float propertyHeight = this.GetPropertyHeight(property, label);
            RuntimeScriptAttributeUtility.s_DrawerStack.Pop();
            return propertyHeight;
        }

        public virtual void OnGUI(Rect position, RuntimeSerializedProperty property, GUIContent label)
        {
            RuntimeEasyGUI.DefaultPropertyField(position, property, label);
            EditorGUI.LabelField(position, label, EditorGUIUtilityHelper.TempContent("No GUI Implemented"));
        }

        internal void OnGUISafe(Rect position, RuntimeSerializedProperty property, GUIContent label)
        {
            RuntimeScriptAttributeUtility.s_DrawerStack.Push(this);
            this.OnGUI(position, property, label);
            RuntimeScriptAttributeUtility.s_DrawerStack.Pop();
        }

        #endregion
    }
}
