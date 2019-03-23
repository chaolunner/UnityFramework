using System.Text.RegularExpressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

namespace UniEasy.Editor
{
    [InspectablePropertyDrawer(typeof(InspectorDisplayAttribute))]
    [InspectablePropertyDrawer(typeof(IntReactiveProperty))]
    [InspectablePropertyDrawer(typeof(LongReactiveProperty))]
    [InspectablePropertyDrawer(typeof(ByteReactiveProperty))]
    [InspectablePropertyDrawer(typeof(FloatReactiveProperty))]
    [InspectablePropertyDrawer(typeof(DoubleReactiveProperty))]
    [InspectablePropertyDrawer(typeof(StringReactiveProperty))]
    [InspectablePropertyDrawer(typeof(BoolReactiveProperty))]
    [InspectablePropertyDrawer(typeof(Vector2ReactiveProperty))]
    [InspectablePropertyDrawer(typeof(Vector3ReactiveProperty))]
    [InspectablePropertyDrawer(typeof(Vector4ReactiveProperty))]
    [InspectablePropertyDrawer(typeof(ColorReactiveProperty))]
    [InspectablePropertyDrawer(typeof(RectReactiveProperty))]
    [InspectablePropertyDrawer(typeof(AnimationCurveReactiveProperty))]
    [InspectablePropertyDrawer(typeof(BoundsReactiveProperty))]
    [InspectablePropertyDrawer(typeof(QuaternionReactiveProperty))]
    public class InspectorDisplayDrawer : InspectableDrawer
    {
        public override void OnGUI(Rect position, InspectableProperty property, GUIContent label)
        {
            string fieldName;
            bool notifyPropertyChanged;
            {
                var attr = Attribute as InspectorDisplayAttribute;
                fieldName = (attr == null) ? "value" : attr.FieldName;
                notifyPropertyChanged = (attr == null) ? true : attr.NotifyPropertyChanged;
            }

            if (notifyPropertyChanged)
            {
                EditorGUI.BeginChangeCheck();
            }
            var targetInspectableProperty = property.FindPropertyRelative(fieldName);
            if (targetInspectableProperty == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent() { text = "InspectorDisplay can't find target:" + fieldName });
                if (notifyPropertyChanged)
                {
                    EditorGUI.EndChangeCheck();
                }
                return;
            }
            else
            {
                EmitPropertyField(position, targetInspectableProperty, label);
            }

            if (notifyPropertyChanged)
            {
                if (EditorGUI.EndChangeCheck())
                {
                    property.InspectableObject.SerializedObject.ApplyModifiedProperties(); // deserialize to field

                    var paths = property.PropertyPath.Split('.'); // X.Y.Z...
                    var attachedComponent = property.InspectableObject.Object;

                    var targetProp = (paths.Length == 1) ? FieldInfo.GetValue(attachedComponent) : GetValueRecursive(attachedComponent, 0, paths);
                    if (targetProp == null)
                    {
                        return;
                    }
                    var propInfo = targetProp.GetType().GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var modifiedValue = propInfo.GetValue(targetProp, null); // retrieve new value

                    var methodInfo = targetProp.GetType().GetMethod("SetValueAndForceNotify", BindingFlags.IgnoreCase | BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(targetProp, new object[] { modifiedValue });
                    }
                }
                else
                {
                    property.InspectableObject.SerializedObject.ApplyModifiedProperties();
                }
            }
        }

        object GetValueRecursive(object obj, int index, string[] paths)
        {
            var path = paths[index];
            var fieldInfo = obj.GetType().GetField(path, BindingFlags.IgnoreCase | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // If array, path = Array.data[index]
            if (fieldInfo == null && path == "Array")
            {
                try
                {
                    path = paths[++index];
                    var m = Regex.Match(path, @"(.+)\[([0-9]+)*\]");
                    var arrayIndex = int.Parse(m.Groups[2].Value);
                    var arrayValue = (obj as System.Collections.IList)[arrayIndex];
                    if (index < paths.Length - 1)
                    {
                        return GetValueRecursive(arrayValue, ++index, paths);
                    }
                    else
                    {
                        return arrayValue;
                    }
                }
                catch
                {
                    Debug.Log("InspectorDisplayDrawer Exception, objType:" + obj.GetType().Name + " path:" + string.Join(", ", paths));
                    throw;
                }
            }
            else if (fieldInfo == null)
            {
                throw new Exception("Can't decode path, please report to UniRx's GitHub issues:" + string.Join(", ", paths));
            }

            var v = fieldInfo.GetValue(obj);
            if (index < paths.Length - 1)
            {
                return GetValueRecursive(v, ++index, paths);
            }

            return v;
        }

        public override float GetPropertyHeight(InspectableProperty property, GUIContent label)
        {
            var attr = Attribute as InspectorDisplayAttribute;
            var fieldName = (attr == null) ? "value" : attr.FieldName;

            var height = base.GetPropertyHeight(property, label);
            var valueProperty = property.FindPropertyRelative(fieldName);
            if (valueProperty == null)
            {
                return height;
            }

            if (valueProperty.PropertyType == InspectablePropertyType.Rect || valueProperty.PropertyType == InspectablePropertyType.RectInt)
            {
                return height * 2;
            }
            if (valueProperty.PropertyType == InspectablePropertyType.Bounds || valueProperty.PropertyType == InspectablePropertyType.BoundsInt)
            {
                return height * 3;
            }
            if (valueProperty.PropertyType == InspectablePropertyType.String)
            {
                var multilineAttr = GetMultilineAttribute();
                if (multilineAttr != null)
                {
                    return ((!EditorGUIUtility.wideMode) ? 16f : 0f) + 16f + (float)((multilineAttr.Lines - 1) * 13);
                }
                ;
            }

            if (valueProperty.IsExpanded)
            {
                var count = 0;
                var e = valueProperty.GetEnumerator();
                while (e.MoveNext())
                    count++;
                return ((height + 4) * count) + 6; // (Line = 20 + Padding) ?
            }

            return height;
        }

        protected virtual void EmitPropertyField(Rect position, InspectableProperty targetInspectableProperty, GUIContent label)
        {
            var multiline = GetMultilineAttribute();
            if (multiline == null)
            {
                var range = GetRangeAttribute();
                if (range == null)
                {
                    EasyGUI.PropertyField(position, targetInspectableProperty, label, includeChildren: true);
                }
                else
                {
                    if (targetInspectableProperty.PropertyType == InspectablePropertyType.Float)
                    {
                        EasyGUI.Slider(position, targetInspectableProperty, range.Min, range.Max, label);
                    }
                    else if (targetInspectableProperty.PropertyType == InspectablePropertyType.Integer)
                    {
                        EasyGUI.IntSlider(position, targetInspectableProperty, (int)range.Min, (int)range.Max, label);
                    }
                    else
                    {
                        EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
                    }
                }
            }
            else
            {
                var property = targetInspectableProperty;

                label = EasyGUI.BeginProperty(position, label, property);
                var method = typeof(EditorGUI).GetMethod("MultiFieldPrefixLabel", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic);
                position = (Rect)method.Invoke(null, new object[] { position, 0, label, 1 });

                EditorGUI.BeginChangeCheck();
                int indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                var stringValue = EditorGUI.TextArea(position, property.StringValue);
                EditorGUI.indentLevel = indentLevel;
                if (EditorGUI.EndChangeCheck())
                {
                    property.StringValue = stringValue;
                }
                EasyGUI.EndProperty();
            }
        }

        MultilineReactivePropertyAttribute GetMultilineAttribute()
        {
            var fi = FieldInfo;
            if (fi == null)
                return null;
            return fi.GetCustomAttributes(false).OfType<MultilineReactivePropertyAttribute>().FirstOrDefault();
        }

        RangeReactivePropertyAttribute GetRangeAttribute()
        {
            var fi = FieldInfo;
            if (fi == null)
                return null;
            return fi.GetCustomAttributes(false).OfType<RangeReactivePropertyAttribute>().FirstOrDefault();
        }
    }
}
