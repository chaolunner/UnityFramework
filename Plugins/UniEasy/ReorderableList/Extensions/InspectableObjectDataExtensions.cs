using System.Collections.Generic;
using UnityEngine;

namespace UniEasy
{
    public static class InspectableObjectDataExtensions
    {
        private static readonly Dictionary<object, object> inspectableObjectIndex = new Dictionary<object, object>();
        private static System.Type ScriptableObjectType = typeof(ScriptableObject);
        private static string format = "\"instanceID\":{0}";

        public static object CreateInstance(this InspectableObjectData data, bool cached = true)
        {
            object component = null;
            var type = data.Type.GetTypeFromCached();
            var json = data.Data;
            if (cached && inspectableObjectIndex.ContainsKey(data))
            {
                component = inspectableObjectIndex[data];
            }
            if (component == null || component.GetType() != type)
            {
                if (type == null)
                {

                }
                else if (type.IsSameOrSubclassOf(ScriptableObjectType))
                {
                    component = ScriptableObject.CreateInstance(type);
                }
                else
                {
                    component = System.Activator.CreateInstance(type);
                }
            }
            if (!Application.isPlaying || (Application.isPlaying && !inspectableObjectIndex.ContainsKey(data)))
            {
                foreach (var kvp in data.ToDictionary())
                {
                    if (kvp.Value != null)
                    {
                        json = json.Replace(string.Format(format, kvp.Key.ToString()), string.Format(format, kvp.Value.GetInstanceID().ToString()));
                    }
                }
                if (component != null)
                {
                    JsonUtility.FromJsonOverwrite(json, component);
                }
            }
            if (inspectableObjectIndex.ContainsKey(data))
            {
                inspectableObjectIndex[data] = component;
            }
            else
            {
                inspectableObjectIndex.Add(data, component);
            }
            return component;
        }

        public static bool IsScriptableObject(this InspectableObjectData data)
        {
            return data.Type.GetTypeWithAssembly().IsSameOrSubclassOf(ScriptableObjectType);
        }
    }
}
