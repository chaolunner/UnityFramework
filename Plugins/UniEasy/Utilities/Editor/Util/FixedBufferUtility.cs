using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

namespace UniEasy.Editor
{
    public class FixedBufferUtility
    {
        public static object GetValue(object o, FixedBufferAttribute attr, int index)
        {
            GCHandle handle = GCHandle.Alloc(o, GCHandleType.Pinned);
            IntPtr source = handle.AddrOfPinnedObject();
            int elementSize = Marshal.SizeOf(o) / attr.Length;
            object value = null;
            try
            {
                if (elementSize == 1)
                {
                    if (attr.ElementType == InspectablePropertyType.sByte)
                    {
                        byte byteValue = Marshal.ReadByte(source, index * elementSize);
                        if (byteValue > sbyte.MaxValue)
                        {
                            value = (sbyte)(sbyte.MaxValue - byteValue);
                        }
                        else
                        {
                            value = (sbyte)byteValue;
                        }
                    }
                    else
                    {
                        value = Convert.ChangeType(Marshal.ReadByte(source, index * elementSize), attr.ElementType);
                    }
                }
                else if (elementSize == 2)
                {
                    if (attr.ElementType == InspectablePropertyType.uShort)
                    {
                        short shortValue = Marshal.ReadInt16(source, index * elementSize);
                        if (shortValue < 0)
                        {
                            value = (ushort)(short.MaxValue - shortValue);
                        }
                        else
                        {
                            value = (ushort)shortValue;
                        }
                    }
                    else
                    {
                        value = Convert.ChangeType(Marshal.ReadInt16(source, index * elementSize), attr.ElementType);
                    }
                }
                else if (elementSize == 4)
                {
                    if (attr.ElementType == InspectablePropertyType.uInteger)
                    {
                        int intValue = Marshal.ReadInt32(source, index * elementSize);
                        if (intValue < 0)
                        {
                            value = (uint)(int.MaxValue - intValue);
                        }
                        else
                        {
                            value = (uint)intValue;
                        }
                    }
                    else
                    {
                        value = Convert.ChangeType(Marshal.ReadInt32(source, index * elementSize), attr.ElementType);
                    }
                }
                else if (elementSize == 8)
                {
                    if (attr.ElementType == InspectablePropertyType.uLong)
                    {
                        long longValue = Marshal.ReadInt64(source, index * elementSize);
                        if (longValue < 0)
                        {
                            value = (ulong)(long.MaxValue - longValue);
                        }
                        else
                        {
                            value = (ulong)longValue;
                        }
                    }
                    else
                    {
                        value = Convert.ChangeType(Marshal.ReadInt64(source, index * elementSize), attr.ElementType);
                    }
                }
            }
            finally
            {
                handle.Free();
            }
            return value;
        }

        public static object SetValue(object o, Type type, FixedBufferAttribute attr, int index, object value)
        {
            GCHandle handle = GCHandle.Alloc(o, GCHandleType.Pinned);
            IntPtr source = handle.AddrOfPinnedObject();
            int elementSize = Marshal.SizeOf(o) / attr.Length;
            try
            {
                if (elementSize == 1)
                {
                    if (attr.ElementType == InspectablePropertyType.sByte)
                    {
                        byte byteValue = 0;
                        sbyte sbyteValue = (sbyte)value;
                        if (sbyteValue < 0)
                        {
                            byteValue = (byte)(sbyte.MaxValue - sbyteValue);
                        }
                        else
                        {
                            byteValue = (byte)sbyteValue;
                        }
                        Marshal.WriteByte(source, index * elementSize, byteValue);
                    }
                    else
                    {
                        Marshal.WriteByte(source, index * elementSize, Convert.ToByte(value));
                    }
                }
                else if (elementSize == 2)
                {
                    if (attr.ElementType == InspectablePropertyType.uShort)
                    {
                        short shortValue = 0;
                        ushort ushortValue = (ushort)value;
                        if (ushortValue > short.MaxValue)
                        {
                            shortValue = (short)(short.MaxValue - ushortValue);
                        }
                        else
                        {
                            shortValue = (short)ushortValue;
                        }
                        Marshal.WriteInt16(source, index * elementSize, shortValue);
                    }
                    else
                    {
                        Marshal.WriteInt16(source, index * elementSize, Convert.ToInt16(value));
                    }
                }
                else if (elementSize == 4)
                {
                    if (attr.ElementType == InspectablePropertyType.uInteger)
                    {
                        int intValue = 0;
                        uint uintValue = (uint)value;
                        if (uintValue > int.MaxValue)
                        {
                            intValue = (int)(int.MaxValue - uintValue);
                        }
                        else
                        {
                            intValue = (int)uintValue;
                        }
                        Marshal.WriteInt32(source, index * elementSize, intValue);
                    }
                    else if (attr.ElementType == InspectablePropertyType.Float)
                    {
                        int intValue = 0;
                        float floatValue = (float)value;
                        if (floatValue > int.MaxValue)
                        {
                            intValue = (int)(int.MaxValue - floatValue);
                        }
                        else
                        {
                            intValue = (int)floatValue;
                        }
                        Marshal.WriteInt32(source, index * elementSize, intValue);
                    }
                    else
                    {
                        Marshal.WriteInt32(source, index * elementSize, Convert.ToInt32(value));
                    }
                }
                else if (elementSize == 8)
                {
                    if (attr.ElementType == InspectablePropertyType.uLong)
                    {
                        long longValue = 0;
                        ulong ulongValue = (ulong)value;
                        if (ulongValue > long.MaxValue)
                        {
                            longValue = (long)(long.MaxValue - ulongValue);
                        }
                        else
                        {
                            longValue = (long)ulongValue;
                        }
                        Marshal.WriteInt64(source, index * elementSize, longValue);
                    }
                    else if (attr.ElementType == InspectablePropertyType.Double)
                    {
                        long longValue = 0;
                        double doubleValue = (double)value;
                        if (doubleValue > long.MaxValue)
                        {
                            longValue = (long)(long.MaxValue - doubleValue);
                        }
                        else
                        {
                            longValue = (long)doubleValue;
                        }
                        Marshal.WriteInt64(source, index * elementSize, longValue);
                    }
                    else
                    {
                        Marshal.WriteInt64(source, index * elementSize, Convert.ToInt64(value));
                    }
                }
            }
            finally
            {
                handle.Free();
            }
            return o;
        }
    }
}
