using UnityEngine;
using UnityEditor;

namespace UniEasy.Editor
{
    [InspectablePropertyDrawer(typeof(RangeAttribute))]
    public class RangeDrawer : InspectableDrawer
    {
        public override void OnGUI(Rect position, InspectableProperty property, GUIContent label)
        {
            RangeAttribute rangeAttribute = (RangeAttribute)base.Attribute;
            if (property.PropertyType == InspectablePropertyType.Float)
            {
                EasyGUI.Slider(position, property, rangeAttribute.min, rangeAttribute.max, label);
            }
            else if (property.PropertyType == InspectablePropertyType.Integer)
            {
                EasyGUI.IntSlider(position, property, (int)rangeAttribute.min, (int)rangeAttribute.max, label);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
            }
        }
    }
}
