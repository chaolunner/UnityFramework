using System.Collections.Generic;
using UnityEditor;

namespace UniEasy.Editor
{
    public class RuntimeSerializedObjectCache
    {
        private static readonly Dictionary<int, RuntimeSerializedObject> RuntimeSerializedObjectDict = new Dictionary<int, RuntimeSerializedObject>();

        public static int GetHashCode(SerializedProperty serializedProperty, object o)
        {
            return serializedProperty.serializedObject.targetObject.GetInstanceID() ^ o.GetHashCode();
        }

        public static RuntimeSerializedObject LoadFromCache(int instanceID)
        {
            if (RuntimeSerializedObjectDict.ContainsKey(instanceID))
            {
                return RuntimeSerializedObjectDict[instanceID];
            }
            return null;
        }

        public static RuntimeSerializedObject GetRuntimeSerializedObject(SerializedProperty serializedProperty)
        {
            return GetRuntimeSerializedObject(serializedProperty, RuntimeObjectCache.GetRuntimeObject(serializedProperty, serializedProperty.stringValue));
        }

        public static RuntimeSerializedObject GetRuntimeSerializedObject(SerializedProperty serializedProperty, object target)
        {
            var runtimeSerializedObject = LoadFromCache(GetHashCode(serializedProperty, target));
            if (runtimeSerializedObject == null)
            {
                var id = GetHashCode(serializedProperty, target);
                runtimeSerializedObject = new RuntimeSerializedObject(serializedProperty, target, id);
                RuntimeSerializedObjectDict.Add(id, runtimeSerializedObject);
            }
            runtimeSerializedObject.Owner = serializedProperty.serializedObject;
            return runtimeSerializedObject;
        }
    }
}
