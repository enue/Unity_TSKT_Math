using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    public struct OrderKeyCombine
    {
        public ulong Result { readonly get; private set; }
        public int Length { readonly get; private set; }
        public const int capacity = 64;

        public void Clear()
        {
            Result = 0;
            Length = 0;
        }

        public void Append(double k)
        {
            if (Length > 0)
            {
                throw new System.ArgumentException();
            }
            Result = OrderKeyConvert.ToUint64(k);
            Length = 64;
        }

        public void Append(float k)
        {
            var v = OrderKeyConvert.ToUint32(k);
            Append(v);
        }
        public void Append(int k)
        {
            var v = OrderKeyConvert.ToUint32(k);
            Append(v);
        }

        public void Append(uint k)
        {
            if (Length > capacity - 32)
            {
                throw new System.ArgumentException();
            }
            Result |= (ulong)k << (capacity - Length - 32);
            Length += 32;
        }

        public void Append(ulong k, int index, int count)
        {
            if (Length > capacity - count)
            {
                throw new System.ArgumentException();
            }

            ulong mask = 0;
            for (int i = 0; i < count; ++i)
            {
                mask |= (ulong)1 << (63 - index - i);
            }
            var offset = 64 - index - count;
            var v = (k & mask) >> offset;

            Result |= v << (capacity - Length - count);
            Length += count;
        }

        public void Append(bool k)
        {
            if (Length > capacity - 1)
            {
                throw new System.ArgumentException();
            }
            Result |= (ulong)(k ? 1 : 0) << (capacity - Length - 1);
            Length += 1;
        }

        public void Append(byte k)
        {
            if (Length > capacity - 8)
            {
                throw new System.ArgumentException();
            }
            Result |= (ulong)k << (capacity - Length - 8);
            Length += 8;
        }
        public void Append(ushort k)
        {
            if (Length > capacity - 16)
            {
                throw new System.ArgumentException();
            }
            Result |= (ulong)k << (capacity - Length - 16);
            Length += 16;
        }

        bool Append(ulong k, int index, int count, out ulong filled)
        {
            var remainingCapacity = capacity - Length;
            if (remainingCapacity >= count)
            {
                Append(k, index, count);
                filled = default;
                return false;
            }

            Append(k, index, remainingCapacity);
            filled = Result;

            Result = 0;
            Length = 0;
            Append(k, index + remainingCapacity, count - remainingCapacity);

            return true;
        }

        public bool Append(ulong k, out ulong filled)
        {
            return Append(k, 0, 64, out filled);
        }

        public bool Append(uint k, out ulong filled)
        {
            return Append(k, 32, 32, out filled);
        }

        public bool Append(ushort k, out ulong filled)
        {
            return Append(k, 64 - 16, 16, out filled);
        }

        public bool Append(byte k, out ulong filled)
        {
            return Append(k, 64 - 8, 8, out filled);
        }

        public bool Append(bool k, out ulong filled)
        {
            return Append(k ? 1UL : 0UL, 64 - 1, 1, out filled);
        }

        public bool Append(double k, out ulong filled)
        {
            var v = OrderKeyConvert.ToUint64(k);
            return Append(v, out filled);
        }
        public bool Append(float k, out ulong filled)
        {
            var v = OrderKeyConvert.ToUint32(k);
            return Append(v, out filled);
        }
        public bool Append(int k, out ulong filled)
        {
            var v = OrderKeyConvert.ToUint32(k);
            return Append(v, out filled);
        }

    }
}
