using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniEasy.Editor
{
    public static partial class ObjectExtensions
    {
        private static readonly Dictionary<Object, SerializedObjectData> serializedObjectDataIndex = new Dictionary<Object, SerializedObjectData>();

        public static SerializedObjectData GetSerializedObjectData(this Object o)
        {
            SerializedObjectData data = null;
            if (o != null)
            {
                if (serializedObjectDataIndex.ContainsKey(o))
                {
                    data = serializedObjectDataIndex[o];
                    data.Object.UpdateIfRequiredOrScript();
                }
                if (data == null)
                {
                    data = new SerializedObjectData(new SerializedObject(o), o.name, o.GetType().Name, false);
                    serializedObjectDataIndex.Add(o, data);
                }
            }
            return data;
        }
    }
}
