using System.Collections.Generic;

namespace UniEasy
{
    public interface IFastScrollView<T1, T2> where T1 : IFastScrollElement<T2> where T2 : IFastScrollData
    {
        List<T1> Elements { get; set; }
        int GetElementCount();
        void SetContentSize(float value);
        void Scroll(T2[] list, float value, float elementSize);
    }
}
