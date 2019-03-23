using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace UniEasy
{
    public static partial class TypeExtensions
    {
        #region Static Fields

        private static string UnityUI = "UnityEngine.UI";
        private static Type StringType = typeof(string);
        private static Type ObjectType = typeof(UnityEngine.Object);
        private static Type ScriptableObjectType = typeof(UnityEngine.ScriptableObject);
        private static Dictionary<Type, IEnumerable<FieldInfo>> InstanceFieldsIndex = new Dictionary<Type, IEnumerable<FieldInfo>>();
        private static Dictionary<Type, IEnumerable<FieldInfo>> VisibleInstanceFieldsIndex = new Dictionary<Type, IEnumerable<FieldInfo>>();

        #endregion

        #region Static Types

        public static Type BaseType(this Type type)
        {
            return type.BaseType;
        }

        public static Type GetArrayOrListElementType(this Type listType)
        {
            Type result;
            if (listType.IsArray)
            {
                result = listType.GetElementType();
            }
            else if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
            {
                result = listType.GetGenericArguments()[0];
            }
            else
            {
                result = null;
            }
            return result;
        }

        #endregion

        #region Static Methods

        public static bool IsSameOrSubclassOf(this Type type, Type c)
        {
            return type == c || type.IsSubclassOf(c);
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsOpenGenericType(this Type type)
        {
            return type.IsGenericType() && type == type.GetGenericTypeDefinition();
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        public static bool IsArrayOrList(this Type type)
        {
            return (type.IsArray || (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>))));
        }

        public static bool DerivesFrom<T>(this Type that)
        {
            return DerivesFrom(that, typeof(T));
        }

        // This seems easier to think about than IsAssignableFrom
        public static bool DerivesFrom(this Type right, Type left)
        {
            return left != right && right.DerivesFromOrEqual(left);
        }

        public static bool DerivesFromOrEqual<T>(this Type that)
        {
            return DerivesFromOrEqual(that, typeof(T));
        }

        public static bool DerivesFromOrEqual(this Type right, Type left)
        {
            return left == right || left.IsAssignableFrom(right);
        }

        public static bool HasAttribute(this MemberInfo provider, params Type[] attributeTypes)
        {
            return provider.AllAttributes(attributeTypes).Any();
        }

        public static bool HasAttribute<T>(this MemberInfo provider) where T : Attribute
        {
            return provider.AllAttributes(typeof(T)).Any();
        }

        public static bool IsVisible(this Type type)
        {
            if (!type.IsVisible)
            {
            }
            else if (typeof(ICollection<IDisposable>).IsAssignableFrom(type))
            {
            }
            else if (type.IsInterface)
            {
            }
            else if (type == typeof(Nullable))
            {
            }
            else
            {
                return true;
            }
            return false;
        }

        public static bool HasChildren(this Type type)
        {
            if (!IsVisible(type))
            {
            }
            else if (type.IsPrimitive)
            {
            }
            else if (type.IsEnum)
            {
            }
            else if (type == StringType)
            {
            }
            else if (type.IsArrayOrList())
            {
                return true;
            }
            else if (type.IsClass)
            {
                if (type.IsSealed)
                {
                }
                else if (type.Namespace == UnityUI)
                {
                }
                else if ((type.IsGenericType && !type.GetGenericTypeDefinition().IsSameOrSubclassOf(ScriptableObjectType)) || type.GetParentTypes().Any(t => t.IsGenericType && !t.GetGenericTypeDefinition().IsSameOrSubclassOf(ScriptableObjectType)))
                {
                    return true;
                }
                else if (type.IsSameOrSubclassOf(ObjectType) && !type.IsSameOrSubclassOf(ScriptableObjectType))
                {
                    return true;
                }
                else if (string.IsNullOrEmpty(type.Namespace))
                {
                    return true;
                }
            }
            else if (type.IsValueType)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Static IEnumerable

        // Returns all instance fields, including private and public and also those in base classes
        public static IEnumerable<FieldInfo> GetAllInstanceFields(this Type type)
        {
            var fieldInfos = type.DeclaredInstanceFields();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                yield return fieldInfos[i];
            }

            if (type.BaseType() != null && type.BaseType() != typeof(object))
            {
                var baseFieldInfos = type.BaseType().GetAllInstanceFields().ToArray();
                for (int i = 0; i < baseFieldInfos.Length; i++)
                {
                    yield return baseFieldInfos[i];
                }
            }
        }

        public static IEnumerable<FieldInfo> GetAllInstanceFieldsFromCached(this Type type)
        {
            IEnumerable<FieldInfo> fields = null;
            if (InstanceFieldsIndex.ContainsKey(type))
            {
                fields = InstanceFieldsIndex[type];
            }
            if (fields == null)
            {
                fields = type.GetAllInstanceFields();
                InstanceFieldsIndex.Add(type, fields);
            }
            return fields;
        }

        public static IEnumerable<FieldInfo> GetVisibleInstanceFieldsFromCached(this Type type)
        {
            IEnumerable<FieldInfo> fields = null;
            if (VisibleInstanceFieldsIndex.ContainsKey(type))
            {
                fields = VisibleInstanceFieldsIndex[type];
            }
            if (fields == null)
            {
                fields = type.GetAllInstanceFieldsFromCached();
                if (fields != null)
                {
                    var removedList = new List<FieldInfo>();
                    foreach (var field in fields)
                    {
                        if (!field.IsVisible())
                        {
                            removedList.Add(field);
                        }
                    }
                    fields = fields.Except(removedList);
                    VisibleInstanceFieldsIndex.Add(type, fields);
                }
            }
            return fields;
        }

        // Returns all instance properties, including private and public and also those in base classes
        public static IEnumerable<PropertyInfo> GetAllInstanceProperties(this Type type)
        {
            var propInfos = type.DeclaredInstanceProperties();
            for (int i = 0; i < propInfos.Length; i++)
            {
                yield return propInfos[i];
            }

            if (type.BaseType() != null && type.BaseType() != typeof(object))
            {
                var basePropInfos = type.BaseType().GetAllInstanceProperties().ToArray();
                for (int i = 0; i < basePropInfos.Length; i++)
                {
                    yield return basePropInfos[i];
                }
            }
        }

        public static IEnumerable<T> AllAttributes<T>(this MemberInfo provider) where T : Attribute
        {
            return provider.AllAttributes(typeof(T)).Cast<T>();
        }

        public static IEnumerable<Attribute> AllAttributes(this MemberInfo provider, params Type[] attributeTypes)
        {
            var allAttributes = provider.GetCustomAttributes(true).Cast<Attribute>();

            if (attributeTypes.Length == 0)
            {
                return allAttributes;
            }

            return allAttributes.Where(a => attributeTypes.Any(x => a.GetType().DerivesFromOrEqual(x)));
        }

        public static IEnumerable<T> AllAttributes<T>(this ParameterInfo provider) where T : Attribute
        {
            return provider.AllAttributes(typeof(T)).Cast<T>();
        }

        public static IEnumerable<Attribute> AllAttributes(this ParameterInfo provider, params Type[] attributeTypes)
        {
            var allAttributes = provider.GetCustomAttributes(true).Cast<Attribute>();

            if (attributeTypes.Length == 0)
            {
                return allAttributes;
            }

            return allAttributes.Where(a => attributeTypes.Any(x => a.GetType().DerivesFromOrEqual(x)));
        }

        // Returns all instance methods, including private and public and also those in base classes
        public static IEnumerable<MethodInfo> GetAllInstanceMethods(this Type type)
        {
            var methodInfos = type.DeclaredInstanceMethods();
            for (int i = 0; i < methodInfos.Length; i++)
            {
                yield return methodInfos[i];
            }

            if (type.BaseType() != null && type.BaseType() != typeof(object))
            {
                var instanceMethods = type.BaseType().GetAllInstanceMethods().ToArray();
                for (int j = 0; j < instanceMethods.Length; j++)
                {
                    yield return instanceMethods[j];
                }
            }
        }

        public static IEnumerable<Type> GetParentTypes(this Type type)
        {
            if (type == null || type.BaseType() == null || type == typeof(object) || type.BaseType() == typeof(object))
            {
                yield break;
            }

            yield return type.BaseType();

            var ancestors = type.BaseType().GetParentTypes().ToArray();
            for (int i = 0; i < ancestors.Length; i++)
            {
                yield return ancestors[i];
            }
        }

        #endregion

        #region Static FieldInfo

        public static FieldInfo[] DeclaredInstanceFields(this Type type)
        {
            return type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        #endregion

        #region Static PropertyInfo

        public static PropertyInfo[] DeclaredInstanceProperties(this Type type)
        {
            return type.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        #endregion

        #region Static ConstructorInfo

        public static ConstructorInfo[] Constructors(this Type type)
        {
            return type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        #endregion

        #region Static MethodInfo

        public static MethodInfo[] DeclaredInstanceMethods(this Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        #endregion

        #region Static Instance

        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType())
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        #endregion
    }
}
