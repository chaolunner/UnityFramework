using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace UniEasy.Editor
{
    [CustomPropertyDrawer(typeof(ComponentReferenceAttribute))]
    public class ComponentReferenceDrawer : PropertyDrawer
    {
        private readonly Type GameObjectType = typeof(GameObject);
        private readonly Type ComponentType = typeof(Component);
        private const string NameStr = "Name";
        private const string IsExpandedStr = "IsExpanded";
        private const string TargetStr = "Target";
        private const string ComponentStr = "Component";
        private const string ElementStr = "Element";
        private const string NoneStr = "None";
        private const string SearchKeyStr = "Search Key";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var name = property.FindPropertyRelative(NameStr);
            var isExpanded = property.FindPropertyRelative(IsExpandedStr);
            var target = property.FindPropertyRelative(TargetStr);
            var component = property.FindPropertyRelative(ComponentStr);
            int index;
            var parentProperty = property.GetBelongArrayAndIndex(out index);
            var componentReferenceAttributes = index >= 0 ? parentProperty.GetAttributes<ComponentReferenceAttribute>() : property.GetAttributes<ComponentReferenceAttribute>();
            Type type = null;

            if (componentReferenceAttributes != null && componentReferenceAttributes.Length > 0)
            {
                type = (componentReferenceAttributes[0] as ComponentReferenceAttribute).DefaultType;
            }
            if (string.IsNullOrEmpty(name.stringValue) || name.stringValue.StartsWith(ElementStr))
            {
                name.stringValue = index >= 0 ? ElementStr + index : ElementStr;
            }

            EditorGUI.BeginChangeCheck();
            var folderPosition = new Rect(position.x, position.y, 10, EditorGUIUtility.singleLineHeight);
            var targetPosition = new Rect(position.x + 10, position.y, position.width - 10, EditorGUIUtility.singleLineHeight);
            isExpanded.boolValue = EditorGUI.Foldout(folderPosition, isExpanded.boolValue, GUIContent.none);
            target.objectReferenceValue = EditorGUI.ObjectField(targetPosition, GUIContent.none, target.objectReferenceValue, GameObjectType, true);

            if (component.objectReferenceValue != null && (component.objectReferenceValue as Component).gameObject != target.objectReferenceValue)
            {
                component.objectReferenceValue = null;
            }
            if (component.objectReferenceValue == null && type != null && target.objectReferenceValue != null)
            {
                var go = target.objectReferenceValue as GameObject;
                if (go != null)
                {
                    component.objectReferenceValue = go.GetComponent(type);
                }
            }
            if (isExpanded.boolValue)
            {
                var labelPosition = new Rect(position.x + 10, position.y + EditorGUIUtility.singleLineHeight + 5, 140, EditorGUIUtility.singleLineHeight);
                var textPosition = new Rect(position.x + 150, position.y + EditorGUIUtility.singleLineHeight + 5, position.width - 150, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelPosition, SearchKeyStr);
                name.stringValue = EditorGUI.DelayedTextField(textPosition, name.stringValue);
                List<Component> components = null;
                string[] displaysOptions = null;
                if (target.objectReferenceValue != null)
                {
                    var go = target.objectReferenceValue as GameObject;
                    if (go != null)
                    {
                        components = go.GetComponents<Component>().ToList();
                        displaysOptions = new string[components.Count + 1];
                        for (int i = 0; i < components.Count; i++)
                        {
                            displaysOptions[i + 1] = components[i].GetType().Name;
                        }
                    }
                }
                if (displaysOptions == null)
                {
                    displaysOptions = new string[1];
                }
                displaysOptions[0] = NoneStr;

                var selectIndex = component.objectReferenceValue != null ? components.IndexOf(component.objectReferenceValue as Component) + 1 : 0;
                var popupPosition = new Rect(position.x + 10, position.y + 2 * (EditorGUIUtility.singleLineHeight + 5), 140, EditorGUIUtility.singleLineHeight);
                var componentPosition = new Rect(position.x + 150, position.y + 2 * (EditorGUIUtility.singleLineHeight + 5), position.width - 150, EditorGUIUtility.singleLineHeight);
                selectIndex = EditorGUI.Popup(popupPosition, selectIndex, displaysOptions);
                component.objectReferenceValue = selectIndex > 0 ? components[selectIndex - 1] : null;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.ObjectField(componentPosition, GUIContent.none, component.objectReferenceValue, ComponentType, true);
                EditorGUI.EndDisabledGroup();
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var isExpanded = property.FindPropertyRelative(IsExpandedStr);

            if (isExpanded.boolValue)
            {
                height += 2 * (EditorGUIUtility.singleLineHeight + 5);
            }
            return height;
        }
    }
}
