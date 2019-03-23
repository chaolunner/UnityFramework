using System.Collections.Generic;
using UnityEngine;
using System;

namespace UniEasy
{
    [Serializable]
    public class EasyHashSet<T> : ISerializationCallbackReceiver
    {
        [SerializeField]
        [Reorderable(elementName: null)]
        protected List<T> inner;
        protected HashSet<T> target;

        public HashSet<T> ToHashSet()
        {
            return target;
        }

        public EasyHashSet()
        {
            target = new HashSet<T>();
        }

        public EasyHashSet(HashSet<T> hashSet)
        {
            target = hashSet;
        }

        public EasyHashSet(params T[] values)
        {
            target = new HashSet<T>();
            foreach (var value in values)
            {
                target.Add(value);
            }
        }

        public void OnBeforeSerialize()
        {
            inner = new List<T>(target);
        }

        public void OnAfterDeserialize()
        {
            target = new HashSet<T>();
            for (var i = 0; i < inner.Count; ++i)
            {
                target.Add(inner[i]);
            }
        }
    }
}
