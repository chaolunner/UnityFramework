using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    [CustomPropertyDrawer(typeof(RuntimeFeatureAttribute))]
    public class RuntimeFeatureDrawer : RuntimeObjectDrawer
    {
        private const int ToggleSize = 30;
        private const string IsOnValueStr = "IsOn.value";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var togglePosition = new Rect(position.x, position.y, ToggleSize, EditorGUIUtility.singleLineHeight);
            var mainPosition = new Rect(position.x + ToggleSize, position.y, position.width - ToggleSize, position.height);
            base.OnGUI(mainPosition, property, label);

            var prop = runtimeSerializedObject.GetIterator();
            while (prop.Next(true))
            {
                if (prop.PropertyPath == IsOnValueStr)
                {
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();
            var isOn = EditorGUI.Toggle(togglePosition, prop.BoolValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.BoolValue = isOn;
                prop.RuntimeSerializedObject.ApplyModifiedProperties();
            }
        }
    }
}
