using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    public static class OrderKeyConvert
    {
        public static uint ToUint32(float v)
        {
            if (double.IsNaN(v))
            {
                throw new System.ArgumentException();
            }

            unsafe
            {
                var result = *(uint*)&v;
                if (v < 0f)
                {
                    result = ~result;
                }
                else
                {
                    result ^= 0b10000000_00000000_00000000_00000000;
                }
                return result;
            }
        }

        public static uint ToUint32(int v)
        {
            return (uint)(v - int.MinValue);
        }

        public static ulong ToUint64(double v)
        {
            if (double.IsNaN(v))
            {
                throw new System.ArgumentException();
            }

            unsafe
            {
                var result = *(ulong*)&v;
                if (v < 0.0)
                {
                    result = ~result;
                }
                else
                {
                    result ^= 0b10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
                }
                return result;
            }
        }

        public static ulong Combine(uint primary, uint secondary)
        {
            return primary * (ulong)0x100000000 + secondary;
        }

        public static ulong Combine(float primary, float secondary)
        {
            var p = ToUint32(primary);
            var s = ToUint32(secondary);
            return Combine(p, s);
        }

        public static ulong Combine(int primary, int secondary)
        {
            var p = ToUint32(primary);
            var s = ToUint32(secondary);
            return Combine(p, s);
        }
    }
}
