using System.Collections.Generic;
using UnityEditor;

namespace UniEasy.Editor
{
    public class PropertyHandlerCache
    {
        protected Dictionary<int, PropertyHandler> PropertyHandlers = new Dictionary<int, PropertyHandler>();

        public PropertyHandler GetHandler(SerializedProperty property)
        {
            PropertyHandler handler;
            int key = GetPropertyHash(property);
            if (PropertyHandlers.TryGetValue(key, out handler))
            {
                return handler;
            }
            return null;
        }

        public void SetHandler(SerializedProperty property, PropertyHandler handler)
        {
            int key = GetPropertyHash(property);
            PropertyHandlers[key] = handler;
        }

        private static int GetPropertyHash(SerializedProperty property)
        {
            if (property.serializedObject.targetObject == null)
            {
                return 0;
            }
            return property.HashCodeForPropertyPath();
        }

        public void Clear()
        {
            PropertyHandlers.Clear();
        }
    }
}
