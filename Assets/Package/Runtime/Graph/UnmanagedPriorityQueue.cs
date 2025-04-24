using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
#nullable enable

namespace TSKT.Graphs
{
    public struct UnmanagedPriorityQueue<T> : IDisposable where T : unmanaged
    {
        NativeList<T> items;
        NativeList<ulong> keys;
        public readonly int Count => keys.Length;

        public UnmanagedPriorityQueue(int initialCapacity, Allocator allocator)
        {
            items = new(initialCapacity, allocator);
            keys = new(initialCapacity, allocator);
        }

        public void Enqueue(float primaryKey, float secondaryKey, T item)
        {
            Enqueue(OrderKeyConvert.Combine(primaryKey, secondaryKey), item);
        }
        public void Enqueue(ulong key, T item)
        {
            // Dequeueの時に末尾からとりたいのでキーを補数にしておく
            var index = keys.BinarySearch(~key);
            if (index < 0)
            {
                index = ~index;
            }
            items.InsertRange(index, 1);
            items[index] = item;

            keys.InsertRange(index, 1);
            keys[index] = ~key;
        }

        public readonly T Peek => items[^1];

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

    public class UnmanagedDoublePriorityQueue<T> : IDisposable where T : unmanaged
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
