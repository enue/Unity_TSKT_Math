using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public static class CombineOrderKey
    {
        public static ulong Combine(uint primary, uint secondary)
        {
            return primary * (ulong)0x100000000 + secondary;
        }

        static uint Convert(float v)
        {
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

        public static ulong Combine(float primary, float secondary)
        {
            var p = Convert(primary);
            var s = Convert(secondary);
            return Combine(p, s);
        }

        static uint Convert(int v)
        {
            return (uint)(v - int.MinValue);
        }

        public static ulong Combine(int primary, int secondary)
        {
            var p = Convert(primary);
            var s = Convert(secondary);
            return Combine(p, s);
        }
    }
}
