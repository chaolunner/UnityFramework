using UnityEngine;
using UnityEditor;

namespace UniEasy.Editor
{
    [InspectablePropertyDrawer(typeof(EnumMaskAttribute))]
    public class ExtendEnumMaskDrawer : InspectableDrawer
    {
        public override void OnGUI(Rect position, InspectableProperty property, GUIContent label)
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
