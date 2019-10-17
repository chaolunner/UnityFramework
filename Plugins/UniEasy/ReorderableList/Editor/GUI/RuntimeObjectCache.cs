using System.Collections.Generic;
using UnityEditor;

namespace UniEasy.Editor
{
    public class RuntimeObjectCache
    {
        private static readonly Dictionary<int, object> RuntimeObjectDict = new Dictionary<int, object>();

        private static int GetHashCode(SerializedProperty property)
        {
            return property.serializedObject.targetObject.GetInstanceID() ^ property.propertyPath.GetHashCode();
        }

        public static object GetRuntimeObject(SerializedProperty property, string json)
        {
            var hashCode = GetHashCode(property);
            if (!RuntimeObjectDict.ContainsKey(hashCode))
            {
                RuntimeObjectDict.Add(hashCode, RuntimeObject.FromJson(json));
            }

            return RuntimeObjectDict[hashCode];
        }

        public static object GetRuntimeObject(RuntimeSerializedProperty property, string json)
        {
            var hashCode = property.HashCodeForPropertyPath();
            if (!RuntimeObjectDict.ContainsKey(hashCode))
            {
                RuntimeObjectDict.Add(hashCode, RuntimeObject.FromJson(json));
            }

            return RuntimeObjectDict[hashCode];
        }
    }
}
