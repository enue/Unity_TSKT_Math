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
        public readonly int Count => keys.Length - position;
        int position;
        public readonly T Peek => items[position];

        public UnmanagedPriorityQueue(int initialCapacity, Allocator allocator)
        {
            items = new(initialCapacity, allocator);
            keys = new(initialCapacity, allocator);
            position = 0;
        }

        public void Enqueue(float primaryKey, float secondaryKey, T item)
        {
            Enqueue(OrderKeyConvert.Combine(primaryKey, secondaryKey), item);
        }
        public void Enqueue(ulong key, T item)
        {
            var index = keys.AsArray().GetSubArray(position, keys.Length - position).BinarySearch(key);
            if (index < 0)
            {
                index = ~index;
            }
            if (index == 0 && position > 0)
            {
                --position;
                keys[position] = key;
                items[position] = item;
            }
            else
            {
                items.InsertRange(index + position, 1);
                items[index + position] = item;

                keys.InsertRange(index + position, 1);
                keys[index + position] = key;
            }
        }
        public T Dequeue()
        {
            return DequeueKeyAndValue().value;
        }
        public (float key, T value) DequeueKeyAndValue()
        {
            var index = position;
            ++position;
            var result = (keys[index], items[index]);
            return result;
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
        public int Count => keys.Length - position;
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

            var index = keys.AsArray().GetSubArray(position, keys.Length - position).BinarySearch(key);
            if (index < 0)
            {
                index = ~index;
            }
            if (index == 0 && position > 0)
            {
                --position;
                keys[position] = key;
                items[position] = item;
            }
            else
            {
                items.InsertRange(index + position, 1);
                items[index + position] = item;

                keys.InsertRange(index + position, 1);
                keys[index + position] = key;
            }
        }

        public T Dequeue()
        {
            var index = position;
            ++position;
            return items[index];
        }

        public void Dispose()
        {
            items.Dispose();
            keys.Dispose();
        }
    }
}
