using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public class RuntimeSerializedObjectCache
    {
        private static readonly Dictionary<int, RuntimeSerializedObject> RuntimeSerializedObjectDict = new Dictionary<int, RuntimeSerializedObject>();

        private static RuntimeSerializedObject GetRuntimeSerializedObject(object obj, object target, int id)
        {
            if (RuntimeSerializedObjectDict.ContainsKey(id))
            {
                return RuntimeSerializedObjectDict[id];
            }
            var runtimeSerializedObject = new RuntimeSerializedObject(obj, target, id);
            RuntimeSerializedObjectDict.Add(id, runtimeSerializedObject);
            //Debug.Log("Cached RuntimeSerializedObjects : " + RuntimeSerializedObjectDict.Count);
            //foreach (var kvp in RuntimeSerializedObjectDict)
            //{
            //    Debug.Log(kvp.Key + " : " + kvp.Value.Name);
            //}
            return runtimeSerializedObject;
        }

        public static int GetHashCode(SerializedProperty serializedProperty, object o)
        {
            return serializedProperty.serializedObject.targetObject.GetInstanceID() ^ o.GetHashCode();
        }

        public static RuntimeSerializedObject GetRuntimeSerializedObject(SerializedProperty serializedProperty)
        {
            return GetRuntimeSerializedObject(serializedProperty, RuntimeObjectCache.GetRuntimeObject(serializedProperty, serializedProperty.stringValue));
        }

        public static RuntimeSerializedObject GetRuntimeSerializedObject(SerializedProperty serializedProperty, object target)
        {
            var runtimeSerializedObject = GetRuntimeSerializedObject(serializedProperty, target, GetHashCode(serializedProperty, target));
            runtimeSerializedObject.SerializedObject = serializedProperty.serializedObject;
            return runtimeSerializedObject;
        }

        public static int GetHashCode(RuntimeSerializedProperty runtimeSerializedProperty, object o)
        {
            return runtimeSerializedProperty.HashCodeForPropertyPath() ^ o.GetHashCode();
        }

        public static RuntimeSerializedObject GetRuntimeSerializedObject(RuntimeSerializedProperty runtimeSerializedProperty)
        {
            return GetRuntimeSerializedObject(runtimeSerializedProperty, RuntimeObjectCache.GetRuntimeObject(runtimeSerializedProperty, runtimeSerializedProperty.StringValue));
        }

        public static RuntimeSerializedObject GetRuntimeSerializedObject(RuntimeSerializedProperty runtimeSerializedProperty, object target)
        {
            var runtimeSerializedObject = GetRuntimeSerializedObject(runtimeSerializedProperty, target, GetHashCode(runtimeSerializedProperty, target));
            runtimeSerializedObject.SerializedObject = runtimeSerializedProperty.RuntimeSerializedObject.SerializedObject;
            return runtimeSerializedObject;
        }
    }
}
