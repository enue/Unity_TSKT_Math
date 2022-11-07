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
        public int Count => keys.Count;

        public void Enqueue(ulong key, T item)
        {
            // Dequeueの時に末尾からとりたいのでキーを補数にしておく
            var index = keys.BinarySearch(~key);
            if (index < 0)
            {
                index = ~index;
            }
            items.Insert(index, item);
            keys.Insert(index, ~key);
        }

        public T Peek => items[items.Count - 1];

        public T Dequeue()
        {
            var index = items.Count - 1;
            var item = items[index];
            items.RemoveAt(index);
            keys.RemoveAt(index);
            return item;
        }
    }

    public class DoublePriorityQueue<T>
    {
        readonly List<T> items = new List<T>();
        readonly List<OrderKey2> keys = new List<OrderKey2>();
        public int Count => keys.Count;
        public T Peek => items[^1];

        public void Enqueue(double primaryKey, double secondaryKey, T item)
        {
            Enqueue(
                OrderKeyConvert.ToUint64(primaryKey),
                OrderKeyConvert.ToUint64(secondaryKey),
                item);
        }
        public void Enqueue(ulong primaryKey, ulong secondaryKey, T item)
        {
            // Dequeueの時に末尾からとりたいのでキーを補数にしておく
            var key = new OrderKey2(
                ~primaryKey,
                ~secondaryKey);
            var index = keys.BinarySearch(key);
            if (index < 0)
            {
                index = ~index;
            }
            items.Insert(index, item);
            keys.Insert(index, key);
        }

        public T Dequeue()
        {
            var index = items.Count - 1;
            var item = items[index];
            items.RemoveAt(index);
            keys.RemoveAt(index);
            return item;
        }
    }
}
