using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class InspectableDrawer : GUIDrawer
    {
        #region Fields

        public FieldInfo FieldInfo;
        public PropertyAttribute Attribute;

        #endregion

        #region Methods

        public virtual bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return true;
        }

        internal bool CanCacheInspectorGUISafe(SerializedProperty property)
        {
            InspectableAttributeUtility.s_DrawerStack.Push(this);
            bool result = this.CanCacheInspectorGUI(property);
            InspectableAttributeUtility.s_DrawerStack.Pop();
            return result;
        }

        public virtual float GetPropertyHeight(InspectableProperty property, GUIContent label)
        {
            return 16f;
        }

        internal float GetPropertyHeightSafe(InspectableProperty property, GUIContent label)
        {
            InspectableAttributeUtility.s_DrawerStack.Push(this);
            float propertyHeight = this.GetPropertyHeight(property, label);
            InspectableAttributeUtility.s_DrawerStack.Pop();
            return propertyHeight;
        }

        public virtual void OnGUI(Rect position, InspectableProperty property, GUIContent label)
        {
            EasyGUI.DefaultPropertyField(position, property, label);
            EditorGUI.LabelField(position, label, EditorGUIUtilityHelper.TempContent("No GUI Implemented"));
        }

        internal void OnGUISafe(Rect position, InspectableProperty property, GUIContent label)
        {
            InspectableAttributeUtility.s_DrawerStack.Push(this);
            this.OnGUI(position, property, label);
            InspectableAttributeUtility.s_DrawerStack.Pop();
        }

        #endregion
    }
}
