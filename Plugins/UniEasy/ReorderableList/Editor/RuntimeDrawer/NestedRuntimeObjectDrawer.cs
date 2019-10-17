using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    [RuntimeCustomPropertyDrawer(typeof(RuntimeObjectAttribute))]
    public class NestedRuntimeObjectDrawer : RuntimePropertyDrawer
    {
        public override void OnGUI(Rect position, RuntimeSerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(property.StringValue))
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

        public override float GetPropertyHeight(RuntimeSerializedProperty property, GUIContent label)
        {
            var height = 0f;
            if (string.IsNullOrEmpty(property.StringValue))
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
            var indentLevel = EditorGUI.indentLevel;

            EditorGUI.indentLevel = 1;
            prop.IsExpanded = EditorGUI.Foldout(headerPosition, prop.IsExpanded, prop.DisplayName);
            RuntimeEasyGUI.PropertyField(headerPosition, prop, null);

            if (prop.IsExpanded)
            {
                var y = RuntimeEasyGUI.GetPropertyHeight(prop, null);
                EditorGUI.indentLevel++;
                while (prop.NextVisible(false))
                {
                    height = RuntimeEasyGUI.GetPropertyHeight(prop, new GUIContent(prop.DisplayName), prop.IsExpanded, null);
                    RuntimeEasyGUI.PropertyField(new Rect(position.x, position.y + y, position.width, height), prop, new GUIContent(prop.DisplayName), prop.IsExpanded, null);
                    y += height;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel = indentLevel;
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
                    height += RuntimeEasyGUI.GetPropertyHeight(prop, new GUIContent(prop.DisplayName), prop.IsExpanded, null);
                }
            }
            return height;
        }
    }
}
