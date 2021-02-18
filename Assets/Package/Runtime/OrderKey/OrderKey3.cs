using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public readonly struct OrderKey3 : System.IComparable<OrderKey3>
    {
        public readonly ulong primaryKey;
        public readonly ulong secondaryKey;
        public readonly ulong tertiaryKey;

        public OrderKey3(ulong key1, ulong key2, ulong key3)
        {
            primaryKey = key1;
            secondaryKey = key2;
            tertiaryKey = key3;
        }

        readonly public int CompareTo(OrderKey3 other)
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

            if (tertiaryKey > other.tertiaryKey)
            {
                return 1;
            }
            if (tertiaryKey < other.tertiaryKey)
            {
                return -1;
            }

            return 0;
        }

        public static bool operator >(OrderKey3 left, OrderKey3 right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(OrderKey3 left, OrderKey3 right)
        {
            return left.CompareTo(right) < 0;
        }
    }
}
