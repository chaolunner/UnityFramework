using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    [RuntimeCustomPropertyDrawer(typeof(RangeAttribute))]
    public class RuntimeRangeDrawer : RuntimePropertyDrawer
    {
        public override void OnGUI(Rect position, RuntimeSerializedProperty property, GUIContent label)
        {
            RangeAttribute rangeAttribute = Attribute as RangeAttribute;
            if (property.PropertyType == RuntimeSerializedPropertyType.Float)
            {
                RuntimeEasyGUI.Slider(position, property, rangeAttribute.min, rangeAttribute.max, label);
            }
            else if (property.PropertyType == RuntimeSerializedPropertyType.Integer)
            {
                RuntimeEasyGUI.IntSlider(position, property, (int)rangeAttribute.min, (int)rangeAttribute.max, label);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
            }
        }
    }
}
