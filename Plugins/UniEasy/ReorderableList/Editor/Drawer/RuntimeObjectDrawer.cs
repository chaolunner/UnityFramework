using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    [CustomPropertyDrawer(typeof(RuntimeObjectAttribute))]
    public class RuntimeObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(property.stringValue))
            {
                var style = new GUIStyle();
                style.richText = true;
                EditorGUI.LabelField(position, "<color=red>Missing</color>", style);
            }
            else
            {
                var runtimeSerializedObject = RuntimeSerializedObjectCache.GetRuntimeSerializedObject(property);

                DrawRuntimeObject(position, runtimeSerializedObject);

                runtimeSerializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0f;
            if (string.IsNullOrEmpty(property.stringValue))
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            else
            {
                var runtimeSerializedObject = RuntimeSerializedObjectCache.GetRuntimeSerializedObject(property);

                height += GetRuntimeObjectHeight(runtimeSerializedObject);
            }
            return height;
        }

        private void DrawRuntimeObject(Rect position, RuntimeSerializedObject runtimeSerializedObject)
        {
            var prop = runtimeSerializedObject.GetIterator();
            var height = RuntimeEasyGUI.GetSinglePropertyHeight(prop, new GUIContent(prop.DisplayName));
            var headerPosition = new Rect(position.x, position.y, position.width, height);

            prop.IsExpanded = EditorGUI.Foldout(headerPosition, prop.IsExpanded, prop.DisplayName);
            RuntimeEasyGUI.PropertyField(headerPosition, prop);

            if (prop.IsExpanded)
            {
                var y = RuntimeEasyGUI.GetPropertyHeight(prop);
                EditorGUI.indentLevel++;
                while(prop.NextVisible(false))
                {
                    height = RuntimeEasyGUI.GetPropertyHeight(prop, new GUIContent(prop.DisplayName), prop.IsExpanded);
                    RuntimeEasyGUI.PropertyField(new Rect(position.x, position.y + y, position.width, height), prop, new GUIContent(prop.DisplayName), prop.IsExpanded);
                    y += height;
                }
                EditorGUI.indentLevel--;
            }
        }

        private float GetRuntimeObjectHeight(RuntimeSerializedObject runtimeSerializedObject)
        {
            var height = 0f;
            var prop = runtimeSerializedObject.GetIterator();

            height += RuntimeEasyGUI.GetSinglePropertyHeight(prop, new GUIContent(prop.DisplayName));
            if (prop.IsExpanded)
            {
                while (prop.NextVisible(false))
                {
                    height += RuntimeEasyGUI.GetPropertyHeight(prop, new GUIContent(prop.DisplayName), prop.IsExpanded);
                }
            }
            return height;
        }
    }
}
