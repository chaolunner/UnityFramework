using System.Collections;
using System.Reflection;
using UnityEditor;
using System.Linq;
using System;

namespace UniEasy.Editor
{
    public static partial class SerializedPropertyExtensions
    {
        #region Static Fields

        private static MethodInfo getHandler;
        private static PropertyInfo propertyDrawer;

        private static string InspectableObjectDataType = "InspectableObjectData";
        private static string InstanceIDStr = "\"instanceID\":{0}";
        private static string PropertyDrawerStr = "propertyDrawer";
        private static string ArrayDataStr = ".Array.data[";
        private static string GetHandlerStr = "GetHandler";
        private static string RightBracketStr = "]";
        private static string ValuesStr = "values";
        private static string LeftBracketStr = "[";
        private static string TypeStr = "Type";
        private static string DataStr = "Data";
        private static string KeysStr = "keys";
        private static string EmptyStr = "";

        private static char StopChar = '.';

        #endregion

        #region Static Methods

        private static object GetValue(object source, string name)
        {
            if (source == null)
            {
                return null;
            }
            Type type = source.GetType();
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null)
                {
                    return null;
                }
                return property.GetValue(source, null);
            }
            return field.GetValue(source);
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            if (enumerable == null)
            {
                return null;
            }
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
            {
                enm.MoveNext();
            }
            return enm.Current;
        }

        public static object GetValue<T>(this SerializedProperty property)
        {
            var path = property.propertyPath.Replace(ArrayDataStr, LeftBracketStr);
            object obj = property.serializedObject.targetObject;
            var elements = path.Split(StopChar);
            foreach (var element in elements)
            {
                if (element.Contains(LeftBracketStr))
                {
                    var elementName = element.Substring(0, element.IndexOf(LeftBracketStr));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf(LeftBracketStr)).Replace(LeftBracketStr, EmptyStr).Replace(RightBracketStr, EmptyStr));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            if (obj is T)
            {
                return (T)obj;
            }
            return null;
        }

        public static T GetParent<T>(this SerializedProperty property)
        {
            var path = property.propertyPath.Replace(ArrayDataStr, LeftBracketStr);
            var obj = (object)property.serializedObject.targetObject;
            var elements = path.Split(StopChar);
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains(LeftBracketStr))
                {
                    var elementName = element.Substring(0, element.IndexOf(LeftBracketStr));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf(LeftBracketStr)).Replace(LeftBracketStr, EmptyStr).Replace(RightBracketStr, EmptyStr));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return (T)obj;
        }

        public static Type GetTypeReflection(this SerializedProperty property)
        {
            object obj = GetParent<object>(property);
            if (obj == null)
            {
                return null;
            }
            Type type = obj.GetType();
            const BindingFlags bindingFlags = BindingFlags.GetField
                                              | BindingFlags.GetProperty
                                              | BindingFlags.Instance
                                              | BindingFlags.NonPublic
                                              | BindingFlags.Public;
            FieldInfo field = type.GetField(property.name, bindingFlags);
            if (field == null)
            {
                return null;
            }
            return field.FieldType;
        }

        public static string GetRootPath(this SerializedProperty property)
        {
            var rootPath = property.propertyPath;
            var firstDot = property.propertyPath.IndexOf(StopChar);
            if (firstDot > 0)
            {
                rootPath = property.propertyPath.Substring(0, firstDot);
            }
            return rootPath;
        }

        public static object[] GetAttributes<T>(this SerializedProperty property)
        {
            object obj = GetParent<object>(property);
            if (obj == null)
            {
                return null;
            }

            Type attrType = typeof(T);
            Type type = obj.GetType();
            const BindingFlags bindingFlags = BindingFlags.GetField
                                              | BindingFlags.GetProperty
                                              | BindingFlags.Instance
                                              | BindingFlags.NonPublic
                                              | BindingFlags.Public;
            FieldInfo field = type.GetField(property.name, bindingFlags);
            if (field != null)
            {
                return field.GetCustomAttributes(attrType, true);
            }
            return null;
        }

        public static bool HasAttribute<T>(this SerializedProperty property)
        {
            object[] attrs = GetAttributes<T>(property);
            if (attrs != null)
            {
                return attrs.Length > 0;
            }
            return false;
        }

        public static object GetHandler(this SerializedProperty property)
        {
            if (getHandler == null)
            {
                getHandler = TypeHelper.ScriptAttributeUtilityType.GetMethod(GetHandlerStr, BindingFlags.Static | BindingFlags.NonPublic);
            }
            return getHandler.Invoke(null, new object[] { property });
        }

        public static PropertyDrawer TryGetPropertyDrawer(this SerializedProperty property)
        {
            if (propertyDrawer == null)
            {
                propertyDrawer = TypeHelper.PropertyHandlerType.GetProperty(PropertyDrawerStr, BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return (PropertyDrawer)propertyDrawer.GetValue(GetHandler(property), null);
        }

        public static bool IsInspectableObjectDataArrayOrList(this SerializedProperty property)
        {
            return property.isArray && property.type == InspectableObjectDataType;
        }

        public static bool IsInspectableObjectData(this SerializedProperty property)
        {
            return !property.isArray && property.type == InspectableObjectDataType;
        }

        public static InspectableObjectData GetInspectableObjectData(this SerializedProperty property)
        {
            if (IsInspectableObjectData(property))
            {
                return property.GetValue<InspectableObjectData>() as InspectableObjectData;
            }
            return null;
        }

        public static bool SetInspectableObjectData(this SerializedProperty property, string data)
        {
            var target = property.GetInspectableObjectData();
            target.Data = data;
            return SetInspectableObjectData(property, target);
        }

        public static bool SetInspectableObjectData(this SerializedProperty property, InspectableObjectData data)
        {
            if (IsInspectableObjectData(property))
            {
                var property2 = property.Copy();
                if (property2.NextVisible(true))
                {
                    foreach (var o in data.ToDictionary().Select(kvp => kvp.Value).ToArray())
                    {
                        if (o != null && !data.ToDictionary().ContainsKey(o.GetInstanceID()))
                        {
                            data.ToDictionary().Add(o.GetInstanceID(), o);
                        }
                    }
                    do
                    {
                        if (property2.name == TypeStr)
                        {
                            property2.stringValue = data.Type;
                        }
                        else if (property2.name == DataStr)
                        {
                            property2.stringValue = data.Data;
                        }
                        else if (property2.name == KeysStr)
                        {
                            var keys = data.ToDictionary().Keys.Where(key => data.Data.Contains(string.Format(InstanceIDStr, key.ToString()))).ToArray();
                            property2.arraySize = keys.Length;
                            for (int i = 0; i < keys.Length; i++)
                            {
                                property2.GetArrayElementAtIndex(i).intValue = keys[i];
                            }
                        }
                        else if (property2.name == ValuesStr)
                        {
                            var values = data.ToDictionary().Where(kvp => data.Data.Contains(string.Format(InstanceIDStr, kvp.Key.ToString()))).Select(kvp => kvp.Value).ToArray();
                            property2.arraySize = values.Length;
                            for (int i = 0; i < values.Length; i++)
                            {
                                property2.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                            }
                        }
                    } while (property2.NextVisible(false));
                }
                return true;
            }
            return false;
        }

        public static int HashCodeForPropertyPathWithoutArrayIndex(this SerializedProperty property)
        {
            return SerializedPropertyHelper.GetHashCodeForPropertyPathWithoutArrayIndex(property);
        }

        #endregion
    }
}
