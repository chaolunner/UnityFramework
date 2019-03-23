using System;

namespace UniEasy
{
    [Serializable]
    public struct RangedFloat
    {
        public float Min;
        public float Max;

        public RangedFloat(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
