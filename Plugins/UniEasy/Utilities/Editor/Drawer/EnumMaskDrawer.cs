using UnityEngine;
using UnityEditor;

namespace UniEasy.Editor
{
    [CustomPropertyDrawer(typeof(EnumMaskAttribute))]
    public class EnumMaskDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var index = EditorGUI.MaskField(position, property.displayName, property.intValue, property.enumDisplayNames);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = index;
            }
        }
    }
}
