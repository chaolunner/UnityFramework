using UnityEngine;
using System;

namespace UniEasy.Editor
{
    public class InspectablePropertyType
    {
        #region Static Types

        public static Type Bool = typeof(bool);
        public static Type Byte = typeof(byte);
        public static Type Char = typeof(char);
        public static Type Short = typeof(short);
        public static Type Integer = typeof(int);
        public static Type Long = typeof(long);
        public static Type sByte = typeof(sbyte);
        public static Type uShort = typeof(ushort);
        public static Type uInteger = typeof(uint);
        public static Type uLong = typeof(ulong);
        public static Type Float = typeof(float);
        public static Type Double = typeof(double);
        public static Type String = typeof(string);
        public static Type Rect = typeof(Rect);
        public static Type Color = typeof(Color);
        public static Type Object = typeof(UnityEngine.Object);
        public static Type LayerMask = typeof(LayerMask);
        public static Type Vector2 = typeof(Vector2);
        public static Type Vector3 = typeof(Vector3);
        public static Type Vector4 = typeof(Vector4);
        public static Type Bounds = typeof(Bounds);
        public static Type AnimationCurve = typeof(AnimationCurve);
        public static Type Gradient = typeof(Gradient);
        public static Type Vector2Int = typeof(Vector2Int);
        public static Type Vector3Int = typeof(Vector3Int);
        public static Type RectInt = typeof(RectInt);
        public static Type BoundsInt = typeof(BoundsInt);
        public static Type ArraySize = typeof(ArraySizeType);
        public static Type FixedBufferSize = typeof(FixedBufferSizeType);
        public static Type Texture = typeof(Texture);
        public static Type Texture2D = typeof(Texture2D);
        public static Type Sprite = typeof(Sprite);

        #endregion
    }
}
