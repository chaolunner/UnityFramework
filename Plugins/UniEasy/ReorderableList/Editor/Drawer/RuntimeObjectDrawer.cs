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
                var obj = RuntimeObject.FromJson(property.stringValue);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(property.stringValue))
            {
                return EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return 0;
            }
        }
    }
}
