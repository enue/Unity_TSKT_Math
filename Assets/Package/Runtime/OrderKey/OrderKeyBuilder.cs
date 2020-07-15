using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public class OrderKeyBuilder
    {
        readonly List<OrderKeyCombine> items = new List<OrderKeyCombine>();

        void AppendUint64(ulong key, int index, int count)
        {
            if (items.Count == 0)
            {
                items.Add(new OrderKeyCombine());
            }
            var last = items[items.Count - 1];
            var remainingCapacity = OrderKeyCombine.capacity - last.Length;
            var capacityOverCount = count - remainingCapacity;
            if (capacityOverCount > 0)
            {
                last.Append(key, index, remainingCapacity);
                items[items.Count - 1] = last;

                var newItem = new OrderKeyCombine();
                newItem.Append(key, index + remainingCapacity, capacityOverCount);
                items.Add(newItem);
            }
            else
            {
                last.Append(key, index, count);
                items[items.Count - 1] = last;
            }
        }

        public void AppendUint64(ulong key)
        {
            AppendUint64(key, 0, 64);
        }

        public void AppendUint32(uint key)
        {
            AppendUint64(key, 64 - 32, 32);
        }

        public void AppendUint16(ushort key)
        {
            AppendUint64(key, 64 - 16, 16);
        }

        public void AppendUint8(byte key)
        {
            AppendUint64(key, 64 - 8, 8);
        }

        public void AppendBool(bool key)
        {
            AppendUint64((ulong)(key ? 1 : 0), 64 - 1, 1);
        }

        public OrderKey3 ToOrderKey3()
        {
            if (items.Count > 3)
            {
                throw new System.Exception();
            }
            return new OrderKey3(
                items.Count > 0 ? items[0].Result : 0,
                items.Count > 1 ? items[1].Result : 0,
                items.Count > 2 ? items[2].Result : 0);
        }
    }
}
