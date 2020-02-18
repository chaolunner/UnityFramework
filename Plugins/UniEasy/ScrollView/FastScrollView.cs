using System.Collections.Generic;
using UnityEngine;
using System;

namespace UniEasy
{
    public abstract class FastScrollView<T1, T2> : MonoBehaviour, IFastScrollView<T1, T2> where T1 : IFastScrollElement<T2> where T2 : IFastScrollData
    {
        public float ElementSize = 40;
        public int ConstraintCount = 1;
        public List<T1> Elements { get; set; } = new List<T1>();
        public List<T2> Data = new List<T2>();

        public event Action<T1, int, T2, float, int, bool> OnScroll;

        public abstract int GetElementCount();

        public abstract void SetContentSize(float value);

        public void Scroll(T2[] list, float value, float elementSize)
        {
            SetContentSize(list.Length * elementSize / ConstraintCount);
            int index = Mathf.FloorToInt(Mathf.Clamp(list.Length - Elements.Count, 0, list.Length) * (1 - value));
            for (int i = 0; i < Elements.Count; i++)
            {
                T2 data = index < list.Length ? list[index] : default;
                Elements[i].Scroll(index, data, elementSize, ConstraintCount, index < list.Length);
                OnScroll?.Invoke(Elements[i], index, data, elementSize, ConstraintCount, index < list.Length);
                index++;
            }
        }
    }
}
