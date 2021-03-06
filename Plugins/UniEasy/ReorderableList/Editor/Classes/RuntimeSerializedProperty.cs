﻿using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class RuntimeSerializedProperty : System.IDisposable
    {
        #region Fields

        private RuntimeNativeSerializedProperty nativeProperty;

        private const char StopChar = '.';
        private const string StopStr = ".";
        private const string GetLayerMaskNamesStr = "GetLayerMaskNames";

        #endregion

        #region Static Fields

        private static MethodInfo getLayerMaskNamesMethodInfo;

        #endregion

        #region Constructors

        public RuntimeSerializedProperty(RuntimeNativeSerializedProperty nativeProperty)
        {
            this.nativeProperty = nativeProperty;
        }

        ~RuntimeSerializedProperty()
        {
            Dispose();
        }

        #endregion

        #region Static Methods

        public static bool EqualContents(RuntimeSerializedProperty x, RuntimeSerializedProperty y)
        {
            return x.RuntimeSerializedObject == y.RuntimeSerializedObject && x.PropertyPath == y.PropertyPath;
        }

        private static bool EqualContents(RuntimeNativeSerializedProperty x, RuntimeNativeSerializedProperty y)
        {
            return x.RuntimeSerializedObject == y.RuntimeSerializedObject && x.PropertyPath == y.PropertyPath;
        }

        #endregion

        #region Methods

        public int HashCodeForPropertyPath()
        {
            return RuntimeSerializedObject.TargetObject.GetInstanceID() ^ RuntimeSerializedObject.Target.GetHashCode() ^ PropertyPath.GetHashCode();
        }

        public bool Next(bool enterChildren = false)
        {
            if (enterChildren && HasChildren)
            {
                nativeProperty = nativeProperty.Properties[0];
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
                            nativeProperty = properties[i + 1];
                            return true;
                        }
                        else if (EqualContents(properties[i], nativeProperty))
                        {
                            nativeProperty = properties[i + 1];
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
                nativeProperty = nativeProperty.VisibleProperties[0];
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
                            nativeProperty = properties[i + 1];
                            return true;
                        }
                        else if (EqualContents(properties[i], nativeProperty))
                        {
                            nativeProperty = properties[i + 1];
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

        private List<RuntimeNativeSerializedProperty> GetPropertiesAtPath(string path)
        {
            int length = path.LastIndexOf(StopChar);
            if (length > 0)
            {
                string parentPath = path.Substring(0, length);
                var property = FindProperty(parentPath);
                if (property != null)
                {
                    return property.nativeProperty.Properties;
                }
            }
            return RuntimeSerializedObject.Properties;
        }

        private List<RuntimeNativeSerializedProperty> GetVisiblePropertiesAtPath(string path)
        {
            int length = path.LastIndexOf(StopChar);
            if (length > 0)
            {
                string parentPath = path.Substring(0, length);
                var property = FindProperty(parentPath);
                if (property != null)
                {
                    return property.nativeProperty.VisibleProperties;
                }
            }
            return RuntimeSerializedObject.VisibleProperties;
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

        public RuntimeSerializedProperty Copy()
        {
            RuntimeSerializedProperty property = new RuntimeSerializedProperty(nativeProperty);
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
            var prop = property.Copy();

            if (property != null && property.IsArray)
            {
                int index = 0;
                // Skip Array Size Property
                prop.NextVisible(true);
                while (prop.NextVisible(false))
                {
                    if (EqualContents(prop, this))
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
            var prop = property.Copy();

            if (property != null && property.IsArray)
            {
                int index = 0;
                // Skip Array Size Property
                prop.NextVisible(true);
                while (prop.NextVisible(false))
                {
                    if (EqualContents(prop, this))
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

        public RuntimeSerializedProperty FindProperty(string propertyPath)
        {
            RuntimeSerializedProperty property = null;
            if (!string.IsNullOrEmpty(propertyPath))
            {
                string[] paths = propertyPath.Split(StopChar);
                if (paths.Length > 0)
                {
                    foreach (var child in RuntimeSerializedObject.Properties)
                    {
                        if (child.Name == paths[0])
                        {
                            property = new RuntimeSerializedProperty(child);
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

        public RuntimeSerializedProperty FindPropertyRelative(string relativePropertyPath)
        {
            RuntimeSerializedProperty property = null;
            if (!string.IsNullOrEmpty(relativePropertyPath))
            {
                string[] paths = relativePropertyPath.Split(StopChar);
                if (paths.Length > 0 && nativeProperty.Properties != null)
                {
                    foreach (var child in nativeProperty.Properties)
                    {
                        if (child.Name == paths[0])
                        {
                            property = new RuntimeSerializedProperty(child);
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

        public RuntimeSerializedProperty GetArrayElementAtIndex(int index)
        {
            RuntimeSerializedProperty property = Copy();
            RuntimeSerializedProperty result = null;
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

        public RuntimeSerializedProperty GetEndProperty(bool includeInVisible = false)
        {
            RuntimeSerializedProperty property = Copy();
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
            return new RuntimeSerializedPropertyIterator() { target = this };
        }

        public RuntimeSerializedProperty GetFixedBufferElementAtIndex(int index)
        {
            RuntimeSerializedProperty property = Copy();
            RuntimeSerializedProperty result = null;
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
                return nativeProperty.Editable;
            }
            set
            {
                nativeProperty.Editable = value;
            }
        }

        public bool IsExpanded
        {
            get
            {
                return nativeProperty.IsExpanded;
            }
            set
            {
                nativeProperty.IsExpanded = value;
            }
        }

        public bool IsFixedBuffer
        {
            get
            {
                return nativeProperty.IsFixedBuffer;
            }
        }

        public bool PrefabOverride
        {
            get
            {
                return nativeProperty.PrefabOverride;
            }
            set
            {
                nativeProperty.PrefabOverride = value;
            }
        }

        public int Depth
        {
            get
            {
                return nativeProperty.Depth;
            }
        }

        public string Name
        {
            get
            {
                return nativeProperty.Name;
            }
        }

        public string DisplayName
        {
            get
            {
                return nativeProperty.DisplayName;
            }
            set
            {
                nativeProperty.DisplayName = value;
            }
        }

        public string Tooltip
        {
            get
            {
                return nativeProperty.Tooltip;
            }
            set
            {
                nativeProperty.Tooltip = value;
            }
        }

        public string Type
        {
            get
            {
                return nativeProperty.Type;
            }
        }

        public string PropertyPath
        {
            get
            {
                return nativeProperty.PropertyPath;
            }
        }

        public System.Type PropertyType
        {
            get
            {
                return nativeProperty.PropertyType;
            }
        }

        public RuntimeSerializedObject RuntimeSerializedObject
        {
            get
            {
                return nativeProperty.RuntimeSerializedObject;
            }
        }

        public object Value
        {
            get
            {
                return nativeProperty.Value;
            }
            set
            {
                nativeProperty.Value = value;
            }
        }

        public int ChildCount
        {
            get
            {
                return nativeProperty.ChildCount;
            }
        }

        public int ArraySize
        {
            get
            {
                return nativeProperty.ArraySize;
            }
            set
            {
                nativeProperty.ArraySize = value;
            }
        }

        public int FixedBufferSize
        {
            get
            {
                if (PropertyType == RuntimeSerializedPropertyType.FixedBufferSize)
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
                return nativeProperty.ArrayElementType;
            }
        }

        public bool HasChildren
        {
            get
            {
                return nativeProperty.HasChildren;
            }
        }

        public bool HasVisibleChildren
        {
            get
            {
                return nativeProperty.HasVisibleChildren;
            }
        }

        public bool IsArray
        {
            get
            {
                return nativeProperty.IsArray;
            }
        }

        public bool IsInstantiatedPrefab
        {
            get
            {
                if (RuntimeSerializedObject != null && RuntimeSerializedObject.TargetObject != null)
                {
#if UNITY_2018_3_OR_NEWER
                    return PrefabUtility.IsPartOfPrefabInstance(RuntimeSerializedObject.TargetObject);
#else
                    var type = PrefabUtility.GetPrefabType(RuntimeSerializedObject.TargetObject);
                    return type == PrefabType.PrefabInstance;
#endif
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

        /// <summary>
        /// Reference type is not supported!
        /// </summary>
        public Object ObjectReference
        {
            get
            {
                return null;
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
    }
}
