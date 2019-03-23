using UniEasy.ECS;
using UnityEditor;
using UnityEngine;
using UniRx;

namespace UniEasy.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Feature), true)]
    public class FeatureDrawer : ReorderableListDrawer
    {
        private static string IsOn = "IsOn";

        protected override bool DoElementHeader(Rect position, GUIContent label, InspectableObject inspectableObject)
        {
            EditorGUI.BeginChangeCheck();
            var property = inspectableObject.FindProperty(IsOn);
            var boolReactive = (BoolReactiveProperty)property.Value;
            var togglePosition = new Rect(position);
            togglePosition.xMin -= 8;
            togglePosition.width = 15;
            var isOn = EditorGUI.Toggle(togglePosition, boolReactive.Value);
            if (EditorGUI.EndChangeCheck())
            {
                boolReactive.Value = isOn;
                property.Value = boolReactive;
                inspectableObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel++;
            return base.DoElementHeader(position, label, inspectableObject);
        }

        protected override void DoElementFooter(Rect position, InspectableObject inspectableObject)
        {
            EditorGUI.indentLevel--;
        }
    }
}
