using System.Collections.Generic;
using UnityEngine;
using System;

namespace UniEasy
{
    [Serializable]
    public class EasyDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        protected List<TKey> keys;
        [SerializeField]
        protected List<TValue> values;
        protected Dictionary<TKey, TValue> target;

        public EasyDictionary()
        {
            target = new Dictionary<TKey, TValue>();
        }

        public EasyDictionary(Dictionary<TKey, TValue> dictionary)
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
