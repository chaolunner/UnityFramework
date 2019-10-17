using UnityEngine;
using UnityEditor;

namespace UniEasy.Editor
{
    [RuntimeCustomPropertyDrawer(typeof(EnumMaskAttribute))]
    public class RuntimeEnumMaskDrawer : RuntimePropertyDrawer
    {
        public override void OnGUI(Rect position, RuntimeSerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var index = EditorGUI.MaskField(position, property.DisplayName, property.IntValue, property.EnumDisplayNames);
            if (EditorGUI.EndChangeCheck())
            {
                property.IntValue = index;
            }
        }
    }
}
