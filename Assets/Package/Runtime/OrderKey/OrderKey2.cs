using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public readonly struct OrderKey2 : System.IComparable<OrderKey2>
    {
        public readonly ulong primaryKey;
        public readonly ulong secondaryKey;

        public OrderKey2(ulong key1, ulong key2)
        {
            primaryKey = key1;
            secondaryKey = key2;
        }

        public int CompareTo(OrderKey2 other)
        {
            if (primaryKey > other.primaryKey)
            {
                return 1;
            }
            if (primaryKey < other.primaryKey)
            {
                return -1;
            }

            if (secondaryKey > other.secondaryKey)
            {
                return 1;
            }
            if (secondaryKey < other.secondaryKey)
            {
                return -1;
            }

            return 0;
        }

        public static bool operator >(OrderKey2 left, OrderKey2 right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(OrderKey2 left, OrderKey2 right)
        {
            return left.CompareTo(right) < 0;
        }
    }
}
