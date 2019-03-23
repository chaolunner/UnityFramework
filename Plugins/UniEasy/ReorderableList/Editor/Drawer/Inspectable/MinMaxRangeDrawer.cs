using UnityEngine;
using UnityEditor;

namespace UniEasy.Editor
{
    [InspectablePropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class ExtendMinMaxRangeDrawer : InspectableDrawer
    {
        private float minValue;
        private float maxValue;

        private static string Min = "Min";
        private static string Max = "Max";

        public override void OnGUI(Rect position, InspectableProperty property, GUIContent label)
        {
            var range = Attribute as MinMaxRangeAttribute;
            var minProperty = property.FindPropertyRelative(Min);
            var maxProperty = property.FindPropertyRelative(Max);

            if (minProperty != null && maxProperty != null)
            {

                minValue = GetValue(minProperty);
                maxValue = GetValue(maxProperty);

                Rect contentPosition = EditorGUI.PrefixLabel(position, label);
                EditorGUI.BeginChangeCheck();

                int indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                if (range.Min == float.MinValue || range.Max == float.MaxValue)
                {
                    var tempPosition = new Rect(contentPosition);
                    tempPosition.xMax = tempPosition.xMin + 0.2f * contentPosition.width;
                    EditorGUI.LabelField(tempPosition, Min);
                    tempPosition.xMin = tempPosition.xMax;
                    tempPosition.xMax += 0.3f * contentPosition.width;
                    minValue = EditorGUI.FloatField(tempPosition, minValue);
                    tempPosition.xMin = tempPosition.xMax;
                    tempPosition.xMax += 0.2f * contentPosition.width;
                    EditorGUI.LabelField(tempPosition, Max);
                    tempPosition.xMin = tempPosition.xMax;
                    tempPosition.xMax += 0.3f * contentPosition.width;
                    maxValue = EditorGUI.FloatField(tempPosition, maxValue);
                }
                else
                {
                    var tempPosition = new Rect(contentPosition);
                    tempPosition.xMax = tempPosition.xMin + 0.2f * contentPosition.width;
                    minValue = EditorGUI.FloatField(tempPosition, minValue);
                    tempPosition.xMin = tempPosition.xMax;
                    tempPosition.xMax += 0.6f * contentPosition.width;
                    EditorGUI.MinMaxSlider(tempPosition, ref minValue, ref maxValue, range.Min, range.Max);
                    tempPosition.xMin = tempPosition.xMax;
                    tempPosition.xMax += 0.2f * contentPosition.width;
                    maxValue = EditorGUI.FloatField(tempPosition, maxValue);
                }
                EditorGUI.indentLevel = indentLevel;

                if (EditorGUI.EndChangeCheck())
                {
                    SetValue(minProperty, minValue);
                    SetValue(maxProperty, maxValue);
                }
            }
        }

        private float GetValue(InspectableProperty property)
        {
            if (property.PropertyType == InspectablePropertyType.Integer)
            {
                return property.IntValue;
            }
            else if (property.PropertyType == InspectablePropertyType.Float)
            {
                return property.FloatValue;
            }
            return 0;
        }

        private void SetValue(InspectableProperty property, float value)
        {
            if (property.PropertyType == InspectablePropertyType.Integer)
            {
                property.IntValue = (int)value;
            }
            else if (property.PropertyType == InspectablePropertyType.Float)
            {
                property.FloatValue = value;
            }
        }
    }
}
