using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class InspectableProperty : System.IDisposable
    {
        #region Fields

        private InspectablePropertyBase propertyBase;

        private static char StopChar = '.';
        private static string StopStr = ".";
        private static string GetLayerMaskNamesStr = "GetLayerMaskNames";

        #endregion

        #region Static Fields

        private static MethodInfo getLayerMaskNamesMethodInfo;

        #endregion

        #region Constructors

        public InspectableProperty(InspectablePropertyBase inspectablePropertyBase)
        {
            propertyBase = inspectablePropertyBase;
        }

        ~InspectableProperty()
        {
            Dispose();
        }

        #endregion

        #region Static Methods

        public static bool EqualContents(InspectableProperty x, InspectableProperty y)
        {
            return x.InspectableObject == y.InspectableObject && x.PropertyPath == y.PropertyPath;
        }

        private static bool EqualContents(InspectablePropertyBase x, InspectablePropertyBase y)
        {
            return x.InspectableObject == y.InspectableObject && x.PropertyPath == y.PropertyPath;
        }

        #endregion

        #region Methods

        public bool Next(bool enterChildren = false)
        {
            if (enterChildren && HasChildren)
            {
                propertyBase = propertyBase.Properties[0];
                return true;
            }
            else
            {
                string path = PropertyPath;
                string lastPath = null;
                while (path != lastPath)
                {
                    var properties = GetPropertiesAtPath(path);
                    for (int i = 0; i < properties.Count - 1; i++)
                    {
                        if (Depth > properties[i].Depth && PropertyPath.StartsWith(properties[i].PropertyPath + StopStr))
                        {
                            propertyBase = properties[i + 1];
                            return true;
                        }
                        else if (EqualContents(properties[i], propertyBase))
                        {
                            propertyBase = properties[i + 1];
                            return true;
                        }
                    }
                    lastPath = path;
                    int length = path.LastIndexOf(StopChar);
                    if (length > 0)
                    {
                        path = path.Substring(0, length);
                    }
                }
            }
            return false;
        }

        public bool NextVisible(bool enterChildren = false)
        {
            if (enterChildren && HasVisibleChildren)
            {
                propertyBase = propertyBase.VisibleProperties[0];
                return true;
            }
            else
            {
                string path = PropertyPath;
                string lastPath = null;
                while (path != lastPath)
                {
                    var properties = GetVisiblePropertiesAtPath(path);
                    for (int i = 0; i < properties.Count - 1; i++)
                    {
                        if (Depth > properties[i].Depth && PropertyPath.StartsWith(properties[i].PropertyPath + StopStr))
                        {
                            propertyBase = properties[i + 1];
                            return true;
                        }
                        else if (EqualContents(properties[i], propertyBase))
                        {
                            propertyBase = properties[i + 1];
                            return true;
                        }
                    }
                    lastPath = path;
                    int length = path.LastIndexOf(StopChar);
                    if (length > 0)
                    {
                        path = path.Substring(0, length);
                    }
                }
            }
            return false;
        }

        private List<InspectablePropertyBase> GetPropertiesAtPath(string path)
        {
            int length = path.LastIndexOf(StopChar);
            if (length > 0)
            {
                string parentPath = path.Substring(0, length);
                var property = FindProperty(parentPath);
                if (property != null)
                {
                    return property.propertyBase.Properties;
                }
            }
            return InspectableObject.Properties;
        }

        private List<InspectablePropertyBase> GetVisiblePropertiesAtPath(string path)
        {
            int length = path.LastIndexOf(StopChar);
            if (length > 0)
            {
                string parentPath = path.Substring(0, length);
                var property = FindProperty(parentPath);
                if (property != null)
                {
                    return property.propertyBase.VisibleProperties;
                }
            }
            return InspectableObject.VisibleProperties;
        }

        public void AppendFoldoutPPtrValue(Object obj)
        {
            if (IsArray)
            {
                int length = ArraySize;
                ArraySize++;
                GetArrayElementAtIndex(length).Value = GetSuitableObjectReference(obj);
            }
            else
            {
                Value = GetSuitableObjectReference(obj);
            }
        }

        public void ClearArray()
        {
            ArraySize = 0;
        }

        public InspectableProperty Copy()
        {
            InspectableProperty property = new InspectableProperty(propertyBase);
            return property;
        }

        public void DeleteArrayElementAtIndex(int index)
        {
            var length = ArraySize;
            if (IsArray && index < length)
            {
                for (int i = index; i < length; i++)
                {
                    MoveArrayElement(i, i + 1);
                }
                ArraySize = length - 1;
            }
        }

        public bool DeleteCommand()
        {
            var length = PropertyPath.LastIndexOf(StopStr);
            var parentPath = PropertyPath.Substring(0, length);
            var property = FindProperty(parentPath);
            var property2 = property.Copy();

            if (property != null && property.IsArray)
            {
                int index = 0;
                // Skip Array Size Property
                property2.NextVisible(true);
                while (property2.NextVisible(false))
                {
                    if (InspectableProperty.EqualContents(property2, this))
                    {
                        property.DeleteArrayElementAtIndex(index);
                        return true;
                    }
                    index++;
                }
            }
            return false;
        }

        public void Dispose()
        {
            // TODO: do something?
        }

        public bool DuplicateCommand()
        {
            var length = PropertyPath.LastIndexOf(StopStr);
            var parentPath = PropertyPath.Substring(0, length);
            var property = FindProperty(parentPath);
            var property2 = property.Copy();

            if (property != null && property.IsArray)
            {
                int index = 0;
                // Skip Array Size Property
                property2.NextVisible(true);
                while (property2.NextVisible(false))
                {
                    if (InspectableProperty.EqualContents(property2, this))
                    {
                        object o = property.GetArrayElementAtIndex(index).Value;
                        property.InsertArrayElementAtIndex(index);
                        property.GetArrayElementAtIndex(index).Value = o;
                        return true;
                    }
                    index++;
                }
            }
            return false;
        }

        public InspectableProperty FindProperty(string propertyPath)
        {
            InspectableProperty property = null;
            if (!string.IsNullOrEmpty(propertyPath))
            {
                string[] paths = propertyPath.Split(StopChar);
                if (paths.Length > 0)
                {
                    foreach (var child in InspectableObject.Properties)
                    {
                        if (child.Name == paths[0])
                        {
                            property = new InspectableProperty(child);
                            if (paths.Length == 1)
                            {
                                return property;
                            }
                            else
                            {
                                int startIndex = paths[0].Length + 1;
                                string path = propertyPath.Substring(startIndex, propertyPath.Length - startIndex);
                                property = property.FindPropertyRelative(path);
                                if (property != null)
                                {
                                    return property;
                                }
                            }
                        }
                    }
                }
            }
            return property;
        }

        public InspectableProperty FindPropertyRelative(string relativePropertyPath)
        {
            InspectableProperty property = null;
            if (!string.IsNullOrEmpty(relativePropertyPath))
            {
                string[] paths = relativePropertyPath.Split(StopChar);
                if (paths.Length > 0 && propertyBase.Properties != null)
                {
                    foreach (var child in propertyBase.Properties)
                    {
                        if (child.Name == paths[0])
                        {
                            property = new InspectableProperty(child);
                            if (paths.Length == 1)
                            {
                                return property;
                            }
                            else
                            {
                                int startIndex = paths[0].Length + 1;
                                string path = relativePropertyPath.Substring(startIndex, relativePropertyPath.Length - startIndex);
                                property = property.FindPropertyRelative(path);
                                if (property != null)
                                {
                                    return property;
                                }
                            }
                        }
                    }
                }
            }
            return property;
        }

        public InspectableProperty GetArrayElementAtIndex(int index)
        {
            InspectableProperty property = Copy();
            InspectableProperty result = null;
            if (index >= 0 && index < property.ArraySize)
            {
                int i = 0;
                if (property.NextVisible(true))
                {
                    property.NextVisible(false);
                    do
                    {
                        if (i == index)
                        {
                            result = property;
                            break;
                        }
                        i++;
                    } while (property.NextVisible(false));
                }
            }
            return result;
        }

        public InspectableProperty GetEndProperty(bool includeInVisible = false)
        {
            InspectableProperty property = Copy();
            if (includeInVisible)
            {
                property.Next(false);
                return property;
            }
            property.NextVisible(false);
            return property;
        }

        public IEnumerator GetEnumerator()
        {
            return new Iterator() { target = this };
        }

        public InspectableProperty GetFixedBufferElementAtIndex(int index)
        {
            InspectableProperty property = Copy();
            InspectableProperty result = null;
            if (index >= 0 && index < property.FixedBufferSize)
            {
                int i = 0;
                if (property.NextVisible(true))
                {
                    property.NextVisible(false);
                    do
                    {
                        if (i == index)
                        {
                            result = property;
                            break;
                        }
                        i++;
                    } while (property.NextVisible(false));
                }
            }
            else
            {
                result = null;
            }
            return result;
        }

        public void InsertArrayElementAtIndex(int index)
        {
            int length = ArraySize;
            if (IsArray && index <= length)
            {
                length++;
                ArraySize = length;
                for (int i = length - 1; i > index; i--)
                {
                    MoveArrayElement(i, i - 1);
                }
                var property = GetArrayElementAtIndex(index);
                property.Value = PropertyType.GetDefaultValue();
            }
        }

        public bool MoveArrayElement(int srcIndex, int dstIndex)
        {
            if (IsArray && srcIndex < ArraySize && dstIndex < ArraySize)
            {

                var srcProperty = GetArrayElementAtIndex(srcIndex);
                var dstProperty = GetArrayElementAtIndex(dstIndex);
                var srcValue = srcProperty.Value;
                var dstValue = dstProperty.Value;

                srcProperty.Value = dstValue;
                dstProperty.Value = srcValue;

                return true;
            }
            return false;
        }

        public bool ValidateObjectReferenceValue(Object obj)
        {
            return GetSuitableObjectReference(obj) != null;
        }

        public bool ValidateObjectReferenceValueExact(Object obj)
        {
            // I don't know what's the difference with ValidateObjectReferenceValue()
            // So now just keep it same as ValidateObjectReferenceValue()
            return obj.GetType().IsSameOrSubclassOf(IsArray ? ArrayElementType : PropertyType);
        }

        private Object GetSuitableObjectReference(Object obj)
        {
            var result = obj.GetType().IsSameOrSubclassOf(IsArray ? ArrayElementType : PropertyType);
            if (result)
            {
                return obj;
            }
            else if (obj is GameObject)
            {
                var go = obj as GameObject;

                foreach (var component in go.GetComponents<Component>())
                {
                    if (component.GetType().IsSameOrSubclassOf(IsArray ? ArrayElementType : PropertyType))
                    {
                        return component;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Static Methods

        public static string[] GetLayerMaskNames(uint layers)
        {
            if (getLayerMaskNamesMethodInfo == null)
            {
                getLayerMaskNamesMethodInfo = typeof(SerializedProperty).GetMethod(GetLayerMaskNamesStr, BindingFlags.Static | BindingFlags.NonPublic);
            }
            if (getLayerMaskNamesMethodInfo != null)
            {
                return (string[])getLayerMaskNamesMethodInfo.Invoke(null, new object[] { layers });
            }
            return new string[32];
        }

        #endregion

        #region Properties

        public bool Editable
        {
            get
            {
                return propertyBase.Editable;
            }
            set
            {
                propertyBase.Editable = value;
            }
        }

        public bool IsExpanded
        {
            get
            {
                return propertyBase.IsExpanded;
            }
            set
            {
                propertyBase.IsExpanded = value;
            }
        }

        public bool IsFixedBuffer
        {
            get
            {
                return propertyBase.IsFixedBuffer;
            }
        }

        public bool PrefabOverride
        {
            get
            {
                return propertyBase.PrefabOverride;
            }
            set
            {
                propertyBase.PrefabOverride = value;
            }
        }

        public int Depth
        {
            get
            {
                return propertyBase.Depth;
            }
        }

        public string Name
        {
            get
            {
                return propertyBase.Name;
            }
        }

        public string DisplayName
        {
            get
            {
                return propertyBase.DisplayName;
            }
            set
            {
                propertyBase.DisplayName = value;
            }
        }

        public string Tooltip
        {
            get
            {
                return propertyBase.Tooltip;
            }
            set
            {
                propertyBase.Tooltip = value;
            }
        }

        public string Type
        {
            get
            {
                return propertyBase.Type;
            }
        }

        public string PropertyPath
        {
            get
            {
                return propertyBase.PropertyPath;
            }
        }

        public System.Type PropertyType
        {
            get
            {
                return propertyBase.PropertyType;
            }
        }

        public InspectableObject InspectableObject
        {
            get
            {
                return propertyBase.InspectableObject;
            }
        }

        public object Value
        {
            get
            {
                return propertyBase.Value;
            }
            set
            {
                propertyBase.Value = value;
            }
        }

        public int ChildCount
        {
            get
            {
                return propertyBase.ChildCount;
            }
        }

        public int ArraySize
        {
            get
            {
                return propertyBase.ArraySize;
            }
            set
            {
                propertyBase.ArraySize = value;
            }
        }

        public int FixedBufferSize
        {
            get
            {
                if (PropertyType == InspectablePropertyType.FixedBufferSize)
                {
                    return IntValue;
                }
                else if (IsFixedBuffer)
                {
                    return Mathf.Clamp(ChildCount - 1, 0, int.MaxValue);
                }
                return 0;
            }
        }

        public System.Type ArrayElementType
        {
            get
            {
                return propertyBase.ArrayElementType;
            }
        }

        public bool HasChildren
        {
            get
            {
                return propertyBase.HasChildren;
            }
        }

        public bool HasVisibleChildren
        {
            get
            {
                return propertyBase.HasVisibleChildren;
            }
        }

        public bool IsArray
        {
            get
            {
                return propertyBase.IsArray;
            }
        }

        public bool IsInstantiatedPrefab
        {
            get
            {
                if (InspectableObject != null && InspectableObject.SerializedObject != null && InspectableObject.SerializedObject.targetObject != null)
                {
                    PrefabType type = PrefabUtility.GetPrefabType(InspectableObject.SerializedObject.targetObject);
                    return (type == PrefabType.PrefabInstance);
                }
                return false;
            }
        }

        public bool BoolValue
        {
            get
            {
                return (bool)Value;
            }
            set
            {
                Value = value;
            }
        }

        public byte ByteValue
        {
            get
            {
                return (byte)Value;
            }
            set
            {
                Value = value;
            }
        }

        public char CharValue
        {
            get
            {
                return (char)Value;
            }
            set
            {
                Value = value;
            }
        }

        public short ShortValue
        {
            get
            {
                return (short)Value;
            }
            set
            {
                Value = value;
            }
        }

        public int IntValue
        {
            get
            {
                return (int)Value;
            }
            set
            {
                Value = value;
            }
        }

        public long LongValue
        {
            get
            {
                return (long)Value;
            }
            set
            {
                Value = value;
            }
        }

        public sbyte sByteValue
        {
            get
            {
                return (sbyte)Value;
            }
            set
            {
                Value = value;
            }
        }

        public ushort uShortValue
        {
            get
            {
                return (ushort)Value;
            }
            set
            {
                Value = value;
            }
        }

        public uint uIntValue
        {
            get
            {
                return (uint)Value;
            }
            set
            {
                Value = value;
            }
        }

        public ulong uLongValue
        {
            get
            {
                return (ulong)Value;
            }
            set
            {
                Value = value;
            }
        }

        public float FloatValue
        {
            get
            {
                return (float)Value;
            }
            set
            {
                Value = value;
            }
        }

        public double DoubleValue
        {
            get
            {
                return (double)Value;
            }
            set
            {
                Value = value;
            }
        }

        public string StringValue
        {
            get
            {
                return (string)Value;
            }
            set
            {
                Value = value;
            }
        }

        public Color ColorValue
        {
            get
            {
                return (Color)Value;
            }
            set
            {
                Value = value;
            }
        }

        public Object ObjectReference
        {
            get
            {
                return (Object)Value;
            }
            set
            {
                var data = InspectableObject.ParentProperty.GetInspectableObjectData();
                if (value != null)
                {
                    if (data.ToDictionary().ContainsKey(value.GetInstanceID()))
                    {
                        data.ToDictionary()[value.GetInstanceID()] = value;
                    }
                    else
                    {
                        data.ToDictionary().Add(value.GetInstanceID(), value);
                    }
                }
                Value = value;
            }
        }

        public LayerMask LayerMaskValue
        {
            get
            {
                return (LayerMask)Value;
            }
            set
            {
                Value = value;
            }
        }

        public System.Enum EnumValue
        {
            get
            {
                return (System.Enum)Value;
            }
            set
            {
                Value = value;
            }
        }

        public string[] EnumDisplayNames
        {
            get
            {
                return System.Enum.GetNames(PropertyType);
            }
        }

        public Vector2 Vector2Value
        {
            get
            {
                return (Vector2)Value;
            }
            set
            {
                Value = value;
            }
        }

        public Vector3 Vector3Value
        {
            get
            {
                return (Vector3)Value;
            }
            set
            {
                Value = value;
            }
        }

        public Vector4 Vector4Value
        {
            get
            {
                return (Vector4)Value;
            }
            set
            {
                Value = value;
            }
        }

        public Rect RectValue
        {
            get
            {
                return (Rect)Value;
            }
            set
            {
                Value = value;
            }
        }

        public AnimationCurve AnimationCurveValue
        {
            get
            {
                return (AnimationCurve)Value;
            }
            set
            {
                Value = value;
            }
        }

        public Bounds BoundsValue
        {
            get
            {
                return (Bounds)Value;
            }
            set
            {
                Value = value;
            }
        }

        public Gradient GradientValue
        {
            get
            {
                return (Gradient)Value;
            }
            set
            {
                Value = value;
            }
        }

        public Vector2Int Vector2IntValue
        {
            get
            {
                return (Vector2Int)Value;
            }
            set
            {
                Value = value;
            }
        }

        public Vector3Int Vector3IntValue
        {
            get
            {
                return (Vector3Int)Value;
            }
            set
            {
                Value = value;
            }
        }

        public RectInt RectIntValue
        {
            get
            {
                return (RectInt)Value;
            }
            set
            {
                Value = value;
            }
        }

        public BoundsInt BoundsIntValue
        {
            get
            {
                return (BoundsInt)Value;
            }
            set
            {
                Value = value;
            }
        }

        #endregion

        #region Iterators

        public class Iterator : IEnumerator, System.IDisposable, IEnumerator<object>
        {
            public int index;
            public InspectableProperty target;
            public InspectableProperty end;
            public object current;
            public bool disposing;
            public int position;

            object IEnumerator<object>.Current
            {
                get
                {
                    return current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return current;
                }
            }

            public Iterator()
            {
            }

            public void Dispose()
            {
                disposing = true;
                position = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint)position;
                position = -1;
                switch (num)
                {
                    case 0:
                        if (!target.IsArray)
                        {
                            end = target.GetEndProperty();
                            if (target.NextVisible(true) && !InspectableProperty.EqualContents(target, end))
                            {
                                current = target;
                                if (!disposing)
                                {
                                    position = 2;
                                }
                                return true;
                            }
                            position = -1;
                            return false;
                        }
                        index = 0;
                        break;
                    case 1:
                        index++;
                        break;
                    case 2:
                        if (target.NextVisible(true) && !InspectableProperty.EqualContents(target, end))
                        {
                            current = target;
                            if (!disposing)
                            {
                                position = 2;
                            }
                            return true;
                        }
                        position = -1;
                        return false;
                    default:
                        return false;
                }
                if (index >= target.ArraySize)
                {
                    position = -1;
                    return false;
                }
                current = target.GetArrayElementAtIndex(index);
                if (!disposing)
                {
                    position = 1;
                }
                return true;
            }

            public void Reset()
            {
                throw new System.NotSupportedException();
            }
        }

        #endregion
    }
}
