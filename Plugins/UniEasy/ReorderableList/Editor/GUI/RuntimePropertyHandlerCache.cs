using System.Collections.Generic;

namespace UniEasy.Editor
{
    public class RuntimePropertyHandlerCache
    {
        protected Dictionary<int, RuntimePropertyHandler> PropertyHandlers = new Dictionary<int, RuntimePropertyHandler>();

        #region Methods

        public RuntimePropertyHandler GetHandler(RuntimeSerializedProperty property)
        {
            RuntimePropertyHandler handler;
            int key = GetPropertyHash(property);
            if (PropertyHandlers.TryGetValue(key, out handler))
            {
                return handler;
            }
            return null;
        }

        public void SetHandler(RuntimeSerializedProperty property, RuntimePropertyHandler handler)
        {
            int key = GetPropertyHash(property);
            PropertyHandlers[key] = handler;
        }

        private static int GetPropertyHash(RuntimeSerializedProperty property)
        {
            if (property.RuntimeSerializedObject == null || property.RuntimeSerializedObject.Target == null)
            {
                return 0;
            }

            // For efficiency, ignore indices inside brackets [] in order to make array elements share handlers.
            int key = property.RuntimeSerializedObject.Target.GetHashCode() ^ property.PropertyPath.GetHashCode();
            var obj = property.RuntimeSerializedObject.Target as UnityEngine.Object;
            if (obj != null)
            {
                key ^= obj.GetInstanceID();
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
