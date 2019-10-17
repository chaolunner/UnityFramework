using System.Collections.Generic;
using UnityEngine;

namespace UniEasy.Editor
{
    public class RuntimePropertyHandlerCache
    {
        protected Dictionary<int, RuntimePropertyHandler> PropertyHandlers = new Dictionary<int, RuntimePropertyHandler>();

        #region Methods

        public RuntimePropertyHandler GetHandler(RuntimeSerializedProperty property, List<PropertyAttribute> attributes)
        {
            RuntimePropertyHandler handler;
            int key = GetPropertyHash(property, attributes);
            if (PropertyHandlers.TryGetValue(key, out handler))
            {
                return handler;
            }
            return null;
        }

        public void SetHandler(RuntimeSerializedProperty property, RuntimePropertyHandler handler, List<PropertyAttribute> attributes)
        {
            int key = GetPropertyHash(property, attributes);
            PropertyHandlers[key] = handler;
        }

        private static int GetPropertyHash(RuntimeSerializedProperty property, List<PropertyAttribute> attributes)
        {
            if (property.RuntimeSerializedObject == null || property.RuntimeSerializedObject.Target == null)
            {
                return 0;
            }

            // For efficiency, ignore indices inside brackets [] in order to make array elements share handlers.
            int key = property.HashCodeForPropertyPath();

            if (attributes != null && attributes.Count > 0)
            {
                foreach (var attr in attributes)
                {
                    key ^= attr.GetHashCode();
                }
            }

            return key;
        }

        public void Clear()
        {
            PropertyHandlers.Clear();
        }

        #endregion
    }
}
