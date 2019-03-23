using System.Collections.Generic;
using UnityEngine;
using System;

namespace UniEasy
{
    [Serializable]
    public class EasyList<T>
    {
        [SerializeField]
        [Reorderable(elementName: null)]
        protected List<T> inner;

        public List<T> ToList()
        {
            return inner;
        }

        public EasyList()
        {
        }

        public EasyList(List<T> list)
        {
            inner = list;
        }

        public EasyList(params T[] values)
        {
            inner = new List<T>();
            inner.AddRange(values);
        }
    }
}
