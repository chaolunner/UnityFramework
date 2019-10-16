using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;

namespace UniEasy.Editor
{
    public class RuntimeNativeSerializedProperty
    {
        #region Fields

        public bool Editable;
        public bool IsExpanded;
        public bool IsFixedBuffer;
        public bool PrefabOverride;
        public int Depth;
        public string Name;
        public string DisplayName;
        public string Tooltip;
        public string Type;
        public string PropertyPath;
        public System.Type PropertyType;
        public RuntimeSerializedObject RuntimeSerializedObject;
        public List<RuntimeNativeSerializedProperty> Properties;
        public List<RuntimeNativeSerializedProperty> VisibleProperties;

        private MethodInfo getMethod;
        private MethodInfo setMethod;
        private MethodInfo addMethod;
        private MethodInfo removerangeMethod;
        private MethodInfo getCountMethod;

        private event System.Func<object> GetValueCallback;
        private event System.Action<object> SetValueCallback;
        private event System.Action<int> ArraySizeChanged;

        private const string AddStr = "add";
        private const string SpaceStr = " ";
        private const string GetStr = "get";
        private const string SetStr = "set";
        private const string SizeStr = "Size";
        private const string ElementStr = "Element";
        private const string Get_ItemStr = "get_item";
        private const string Set_ItemStr = "set_item";
        private const string Get_CountStr = "get_count";
        private const string Get_LengthStr = "get_length";
        private const string RemoveRangeStr = "removerange";
        private const string CombinePathLayout = "{0}.{1}";
        private const string CombineNameLayout = "{0} {1}";
        private const string UppercasePattern = @"\b[a-z]\w*";
        private const string SeparatePattern = @"[A-Z]+[0-9_a-z]*";

        #endregion

        #region Constructors

        public RuntimeNativeSerializedProperty(RuntimeSerializedObject runtimeSerializedObject, FieldInfo propertyField, System.Type type, string propertyPath, string name, int depth, System.Func<object> getValueCallback, System.Action<object> setValueCallback)
        {
            Editable = true;

            RuntimeSerializedObject = runtimeSerializedObject;
            PropertyType = type;
            PropertyPath = propertyPath;
            Name = name;
            DisplayName = name;
            DisplayName = Regex.Replace(DisplayName, UppercasePattern, match => { var str = match.ToString(); return char.ToUpperInvariant(str[0]) + str.Substring(1); });
            DisplayName = Regex.Replace(DisplayName, SeparatePattern, match => { var str = match.ToString(); return str + SpaceStr; });
            Depth = depth;

            Type = type.ToString();

            this.GetValueCallback += getValueCallback;
            this.SetValueCallback += setValueCallback;

            if (propertyField != null)
            {
                var attrs = propertyField.GetCustomAttributes(typeof(FixedBufferAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    var attr = (FixedBufferAttribute)attrs[0];
                    var property = new RuntimeNativeSerializedProperty(RuntimeSerializedObject, null, RuntimeSerializedPropertyType.FixedBufferSize, string.Format(CombinePathLayout, PropertyPath, SizeStr), SizeStr, Depth + 1, () =>
                    {
                        return attr.Length;
                    }, null);

                    IsFixedBuffer = true;
                    property.Editable = false;
                    property.IsFixedBuffer = true;

                    Properties = new List<RuntimeNativeSerializedProperty>() { property };
                    VisibleProperties = new List<RuntimeNativeSerializedProperty>() { property };

                    var elementType = attr.ElementType;

                    for (int i = 0; i < attr.Length; i++)
                    {
                        var index = i;
                        var elementName = string.Format(CombineNameLayout, ElementStr, i);
                        var element = new RuntimeNativeSerializedProperty(RuntimeSerializedObject, null, elementType, string.Format(CombinePathLayout, PropertyPath, elementName), elementName, Depth + 1, () =>
                        {
                            return FixedBufferUtility.GetValue(Value, attr, index);
                        }, (val) =>
                        {
                            Value = FixedBufferUtility.SetValue(Value, PropertyType, attr, index, val);
                        });

                        Properties.Add(element);
                        VisibleProperties.Add(element);
                    }
                }
            }

            if (!IsFixedBuffer && PropertyType.HasChildren())
            {
                var fields = PropertyType.GetAllInstanceFieldsFromCached();
                if (fields != null)
                {
                    Properties = new List<RuntimeNativeSerializedProperty>();
                    foreach (var field in fields)
                    {
                        var property = new RuntimeNativeSerializedProperty(RuntimeSerializedObject, field, field.FieldType, string.Format(CombinePathLayout, PropertyPath, field.Name), field.Name, Depth + 1, () =>
                        {
                            return field.GetValue(Value);
                        }, (val) =>
                        {
                            var obj = Value;
                            field.SetValue(obj, val);
                            Value = obj;
                        });

                        Properties.Add(property);

                        if (field.IsVisible())
                        {
                            if (VisibleProperties == null)
                            {
                                VisibleProperties = new List<RuntimeNativeSerializedProperty>();
                            }
                            VisibleProperties.Add(property);
                        }
                    }
                }
            }

            if (IsArray)
            {
                ArraySizeChanged += OnArraySizeChanged;
                var property = new RuntimeNativeSerializedProperty(RuntimeSerializedObject, null, RuntimeSerializedPropertyType.ArraySize, string.Format(CombinePathLayout, PropertyPath, SizeStr), SizeStr, Depth + 1, () =>
                {
                    return (int)getCountMethod.Invoke(Value, null);
                }, (val) =>
                {
                    ArraySizeChanged.Invoke((int)val);
                });
                property.ArraySizeChanged += OnArraySizeChanged;

                foreach (var method in PropertyType.GetMethods())
                {
                    if (PropertyType.IsArray)
                    {
                        if (method.Name.ToLower() == Get_LengthStr)
                        {
                            getCountMethod = method;
                            property.getCountMethod = method;
                        }
                        else if (method.Name.ToLower() == GetStr)
                        {
                            getMethod = method;
                            property.getMethod = method;
                        }
                        else if (method.Name.ToLower() == SetStr)
                        {
                            setMethod = method;
                            property.setMethod = method;
                        }
                    }
                    else
                    {
                        if (method.Name.ToLower() == Get_CountStr)
                        {
                            getCountMethod = method;
                            property.getCountMethod = method;
                        }
                        else if (method.Name.ToLower() == Get_ItemStr)
                        {
                            getMethod = method;
                            property.getMethod = method;
                        }
                        else if (method.Name.ToLower() == Set_ItemStr)
                        {
                            setMethod = method;
                            property.setMethod = method;
                        }
                        else if (method.Name.ToLower() == AddStr)
                        {
                            addMethod = method;
                            property.addMethod = method;
                        }
                        else if (method.Name.ToLower() == RemoveRangeStr)
                        {
                            removerangeMethod = method;
                            property.removerangeMethod = method;
                        }
                    }
                }

                Properties = new List<RuntimeNativeSerializedProperty>() { property };
                VisibleProperties = new List<RuntimeNativeSerializedProperty>() { property };

                ApplyModifiedArray();
            }
        }

        #endregion

        #region Methods

        private void ApplyModifiedArray()
        {
            if (Properties.Count > 1)
            {
                Properties.RemoveRange(1, Properties.Count - 1);
            }
            if (VisibleProperties.Count > 1)
            {
                VisibleProperties.RemoveRange(1, VisibleProperties.Count - 1);
            }

            var array = Value;
            if (array == null && PropertyType.IsArray)
            {
                array = System.Array.CreateInstance(ArrayElementType, 0);
                Value = array;
            }
            for (int i = 0; i < ArraySize; i++)
            {
                var index = i;
                var elementName = string.Format(CombineNameLayout, ElementStr, i);
                var element = new RuntimeNativeSerializedProperty(RuntimeSerializedObject, null, ArrayElementType, string.Format(CombinePathLayout, PropertyPath, elementName), elementName, Depth + 1, () =>
                {
                    return getMethod.Invoke(Value, new object[] { index });
                }, (val) =>
                {
                    setMethod.Invoke(Value, new object[] { index, val });
                    Value = array;
                });

                Properties.Add(element);
                VisibleProperties.Add(element);
            }
        }

        private void OnArraySizeChanged(int size)
        {
            var length = ArraySize;
            if (length != size)
            {
                var array = Value;
                if (PropertyType.IsArray)
                {
                    var newArray = System.Array.CreateInstance(ArrayElementType, size);
                    if (length < size)
                    {
                        System.Array.Copy(array as System.Array, newArray, length);
                        if (length > 0)
                        {
                            for (int i = length; i < size; i++)
                            {
                                System.Array.Copy(array as System.Array, length - 1, newArray, i, 1);
                            }
                        }
                    }
                    else
                    {
                        System.Array.Copy(array as System.Array, newArray, size);
                    }
                    Value = newArray;
                }
                else
                {
                    if (length < size)
                    {
                        object lastObject = null;
                        if (length > 0)
                        {
                            lastObject = getMethod.Invoke(array, new object[] { length - 1 });
                        }
                        else
                        {
                            lastObject = ArrayElementType.GetDefaultValue();
                        }
                        for (int i = length; i < size; i++)
                        {
                            addMethod.Invoke(array, new object[] { lastObject });
                        }
                    }
                    else
                    {
                        removerangeMethod.Invoke(array, new object[] { size, length - size });
                    }
                    Value = array;
                }
            }
            ApplyModifiedArray();
        }

        #endregion

        #region Properties

        public bool IsArray
        {
            get
            {
                return PropertyType.IsArrayOrList();
            }
        }

        public bool HasChildren
        {
            get
            {
                return (Properties != null && Properties.Count > 0);
            }
        }

        public bool HasVisibleChildren
        {
            get
            {
                return (VisibleProperties != null && VisibleProperties.Count > 0);
            }
        }

        public int ArraySize
        {
            get
            {
                if (PropertyType == RuntimeSerializedPropertyType.ArraySize)
                {
                    return (int)Value;
                }
                else if (getCountMethod != null && IsArray)
                {
                    return (int)getCountMethod.Invoke(Value, null);
                }
                return 0;
            }
            set
            {
                if (ArraySizeChanged != null)
                {
                    ArraySizeChanged.Invoke(value);
                }
            }
        }

        public int ChildCount
        {
            get
            {
                if (HasChildren)
                {
                    return Properties.Count;
                }
                return 0;
            }
        }

        public System.Type ArrayElementType
        {
            get
            {
                return PropertyType.GetArrayOrListElementType();
            }
        }

        public object Value
        {
            get
            {
                if (GetValueCallback != null)
                {
                    return GetValueCallback();
                }
                return null;
            }
            set
            {
                if (SetValueCallback != null)
                {
                    SetValueCallback.Invoke(value);
                }
            }
        }

        #endregion
    }
}
