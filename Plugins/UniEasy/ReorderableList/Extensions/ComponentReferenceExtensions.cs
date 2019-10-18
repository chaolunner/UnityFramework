using System.Collections.Generic;
using UnityEngine;

namespace UniEasy
{
    public static class ComponentReferenceExtensions
    {
        public static T GetComponent<T>(this List<ComponentReference> list, string name) where T : Component
        {
            foreach (var item in list)
            {
                if (item.Name == name && item.Component != null && item.Component is T)
                {
                    return (T)item.Component;
                }
            }
            return default;
        }
    }
}
