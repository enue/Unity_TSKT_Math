using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
#nullable enable

namespace TSKT.Graphs
{
    public class PriorityQueueU<T>
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

        public T Peek => items[^1];

        public T Dequeue()
        {
            var index = items.Count - 1;
            var item = items[index];
            items.RemoveAt(index);
            keys.RemoveAt(index);
            return item;
        }
    }

    public class DoublePriorityQueueU<T> : IDisposable where T : unmanaged
    {
        NativeList<T> items = new(Allocator.Temp);
        NativeList<OrderKey2> keys = new(Allocator.Temp);
        public int Count => keys.Length;
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
            items.InsertRange(index, 1);
            items[index] = item;

            keys.InsertRange(index, 1);
            keys[index] = key;
        }

        public T Dequeue()
        {
            var index = items.Length - 1;
            var item = items[index];
            items.RemoveAt(index);
            keys.RemoveAt(index);
            return item;
        }

        public void Dispose()
        {
            items.Dispose();
            keys.Dispose();
        }
    }
}
