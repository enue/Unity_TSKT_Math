using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT.Graphs
{
    public class PriorityQueue<T>
    {
        readonly List<T> items = new();
        readonly List<ulong> keys = new();
        public int Count => keys.Count - position;
        int position;
        public T Peek => items[position];

        public void Enqueue(float primaryKey, float secondaryKey, T item)
        {
            Enqueue(OrderKeyConvert.Combine(primaryKey, secondaryKey), item);
        }

        public void Enqueue(ulong key, T item)
        {
            var index = keys.BinarySearch(position, keys.Count - position, key, null);
            if (index < 0)
            {
                index = ~index;
            }
            if (index == 0 && position > 0)
            {
                --position;
                items[position] = item;
                keys[position] = key;
            }
            else
            {
                items.Insert(index, item);
                keys.Insert(index, key);
            }
        }

        public T Dequeue()
        {
            var index = position;
            ++position;
            return items[index];
        }
    }

    public class FloatPriorityQueue<T>
    {
        readonly List<T> items = new();
        readonly List<float> keys = new();
        public int Count => keys.Count - position;
        int position;
        public T Peek => items[position];
        public (float key, T value) PeekKeyAndValue => (keys[position], items[position]);


        public void Enqueue(float key, T item)
        {
            var index = keys.BinarySearch(position, keys.Count - position, key, null);
            if (index < 0)
            {
                index = ~index;
            }
            if (index == 0 && position > 0)
            {
                --position;
                items[position] = item;
                keys[position] = key;
            }
            else
            {
                items.Insert(index, item);
                keys.Insert(index, key);
            }
        }

        public T Dequeue()
        {
            var index = position;
            ++position;
            return items[index];
        }
        public (float key, T value) DequeueKeyAndValue()
        {
            var index = position;
            ++position;
            return (keys[index], items[index]);
        }
    }

    public class DoublePriorityQueue<T>
    {
        readonly List<T> items = new();
        readonly List<OrderKey2> keys = new();
        public int Count => keys.Count - position;
        int position;
        public T Peek => items[position];

        public void Enqueue(double primaryKey, double secondaryKey, T item)
        {
            Enqueue(
                OrderKeyConvert.ToUint64(primaryKey),
                OrderKeyConvert.ToUint64(secondaryKey),
                item);
        }
        public void Enqueue(ulong primaryKey, ulong secondaryKey, T item)
        {
            var key = new OrderKey2(
                primaryKey,
                secondaryKey);
            var index = keys.BinarySearch(position, keys.Count - position, key, null);
            if (index < 0)
            {
                index = ~index;
            }
            if (index == 0 && position > 0)
            {
                --position;
                items[position] = item;
                keys[position] = key;
            }
            else
            {
                items.Insert(index, item);
                keys.Insert(index, key);
            }
        }

        public T Dequeue()
        {
            var index = position;
            ++position;
            return items[index];
        }
    }
}
