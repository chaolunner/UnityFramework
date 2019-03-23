using System.Collections.Generic;
using UnityEngine;
using System;

namespace UniEasy.ECS
{
    public static class IEntityExtensions
    {
        public static string Serialize(this IEntity entity, HashSet<Type> includedTypes = null, HashSet<Type> ignoredTypes = null)
        {
            var writer = new EasyDictionary<string, EasyObject>();
            foreach (var component in entity.Components)
            {
                var type = component.GetType();
                if (includedTypes != null && !includedTypes.Contains(type))
                {
                    continue;
                }
                if (type.ShouldIgnore(ignoredTypes))
                {
                    continue;
                }
                writer.Set(type.ToString(), component, type);
            }

#if UNITY_EDITOR
            return JsonUtility.ToJson(writer, true);
#else
			return JsonUtility.ToJson (writer);
#endif
        }

        public static IEntity Deserialize(this IEntity entity, string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var writer = JsonUtility.FromJson<EasyDictionary<string, EasyObject>>(data);

                foreach (var component in entity.Components)
                {
                    var type = component.GetType();
                    var componentType = type.ToString();
                    if (writer.HasKey(componentType))
                    {
                        writer.Get(componentType, component, type);
                    }
                }
            }
            return entity;
        }

        public static bool ShouldIgnore(this Type type, HashSet<Type> ignoredTypes)
        {
            var shouldIgnore = false;

            // Use MonoBehaviour as JsonUtility.ToJson does not support engine types
            if ((!typeof(MonoBehaviour).IsAssignableFrom(type) && !typeof(IComponent).IsAssignableFrom(type)) ||
                type.IsDefined(typeof(NonSerializableAttribute), false))
            {
                shouldIgnore = true;
            }

            if (ignoredTypes != null)
            {
                foreach (var t in ignoredTypes)
                {
                    if (t.IsAssignableFrom(type))
                    {
                        shouldIgnore = true;
                        break;
                    }
                }
            }

            return shouldIgnore;
        }
    }
}
