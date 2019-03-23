using System.Collections.Generic;
using UnityEngine;
using System;

namespace UniEasy
{
    [Serializable]
    public class EasyAsset<TKey, TValue> : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys;
        [SerializeField]
        private List<TValue> values;
        private Dictionary<TKey, TValue> target;

        public EasyAsset()
        {
            target = new Dictionary<TKey, TValue>();
        }

        public EasyAsset(Dictionary<TKey, TValue> dictionary)
        {
            target = dictionary;
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(target.Keys);
            values = new List<TValue>(target.Values);
        }

        public void OnAfterDeserialize()
        {
            var count = Math.Min(keys.Count, values.Count);
            target = new Dictionary<TKey, TValue>(count);
            for (var i = 0; i < count; ++i)
            {
                target.Add(keys[i], values[i]);
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            return target;
        }
    }
}
