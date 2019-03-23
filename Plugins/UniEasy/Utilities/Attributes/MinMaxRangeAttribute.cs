using UnityEngine;

namespace UniEasy
{
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public float Min = 0;
        public float Max = 0;

        public MinMaxRangeAttribute()
        {
            Min = float.MinValue;
            Max = float.MaxValue;
        }

        public MinMaxRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
