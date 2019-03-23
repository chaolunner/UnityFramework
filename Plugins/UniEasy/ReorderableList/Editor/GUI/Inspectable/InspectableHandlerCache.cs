using System.Collections.Generic;

namespace UniEasy.Editor
{
    public class InspectableHandlerCache
    {
        protected Dictionary<int, InspectableHandler> InspectableHandlers = new Dictionary<int, InspectableHandler>();

        public InspectableHandler GetHandler(InspectableProperty property)
        {
            InspectableHandler handler;
            int key = GetPropertyHash(property);
            if (InspectableHandlers.TryGetValue(key, out handler))
            {
                return handler;
            }
            return null;
        }

        public void SetHandler(InspectableProperty property, InspectableHandler handler)
        {
            int key = GetPropertyHash(property);
            InspectableHandlers[key] = handler;
        }

        private static int GetPropertyHash(InspectableProperty property)
        {
            if (property.InspectableObject == null || property.InspectableObject.Object == null)
            {
                return 0;
            }

            // For efficiency, ignore indices inside brackets [] in order to make array elements share handlers.
            int key = property.InspectableObject.Object.GetHashCode() ^ property.PropertyPath.GetHashCode();
            var obj = property.InspectableObject.Object as UnityEngine.Object;
            if (obj != null)
            {
                key ^= obj.GetInstanceID();
            }
            return key;
        }

        public void Clear()
        {
            InspectableHandlers.Clear();
        }
    }
}
