using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
#nullable enable

namespace TSKT.Graphs
{
    // いずれも二分ヒープ。キーが同じ要素同士の取り出し順は不定。

    public struct UnmanagedPriorityQueue<T> : IDisposable where T : unmanaged
    {
        NativeList<T> items;
        NativeList<ulong> keys;
        public readonly int Count => keys.Length;
        public readonly T Peek => Count > 0 ? items[0] : throw new InvalidOperationException();

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
            // 末尾に置いて、親の方が大きい間は親を下ろしていく
            var index = keys.Length;
            keys.Add(key);
            items.Add(item);
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                var parentKey = keys[parent];
                if (parentKey <= key)
                {
                    break;
                }
                keys[index] = parentKey;
                items[index] = items[parent];
                index = parent;
            }
            keys[index] = key;
            items[index] = item;
        }
        public T Dequeue()
        {
            if (keys.Length == 0)
            {
                throw new InvalidOperationException();
            }

            var result = items[0];

            // 末尾の要素を根に置いたものとして、小さい方の子を引き上げながら下ろしていく
            var count = keys.Length - 1;
            var key = keys[count];
            var item = items[count];
            keys.RemoveAt(count);
            items.RemoveAt(count);

            var index = 0;
            while (true)
            {
                var child = index * 2 + 1;
                if (child >= count)
                {
                    break;
                }
                var childKey = keys[child];
                if (child + 1 < count)
                {
                    var rightKey = keys[child + 1];
                    if (rightKey < childKey)
                    {
                        ++child;
                        childKey = rightKey;
                    }
                }
                if (key <= childKey)
                {
                    break;
                }
                keys[index] = childKey;
                items[index] = items[child];
                index = child;
            }
            if (count > 0)
            {
                keys[index] = key;
                items[index] = item;
            }

            return result;
        }
        public void Dispose()
        {
            items.Dispose();
            keys.Dispose();
        }
    }


    public struct UnmanagedFloatPriorityQueue<T> : IDisposable where T : unmanaged
    {
        NativeList<T> items;
        NativeList<float> keys;
        public readonly int Count => keys.Length;
        public readonly T Peek => Count > 0 ? items[0] : throw new InvalidOperationException();

        public UnmanagedFloatPriorityQueue(int initialCapacity, Allocator allocator)
        {
            items = new(initialCapacity, allocator);
            keys = new(initialCapacity, allocator);
        }

        public void Enqueue(float key, T item)
        {
            var index = keys.Length;
            keys.Add(key);
            items.Add(item);
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                var parentKey = keys[parent];
                if (parentKey <= key)
                {
                    break;
                }
                keys[index] = parentKey;
                items[index] = items[parent];
                index = parent;
            }
            keys[index] = key;
            items[index] = item;
        }
        public T Dequeue()
        {
            return DequeueKeyAndValue().value;
        }
        public (float key, T value) DequeueKeyAndValue()
        {
            if (keys.Length == 0)
            {
                throw new InvalidOperationException();
            }

            var result = (keys[0], items[0]);

            var count = keys.Length - 1;
            var key = keys[count];
            var item = items[count];
            keys.RemoveAt(count);
            items.RemoveAt(count);

            var index = 0;
            while (true)
            {
                var child = index * 2 + 1;
                if (child >= count)
                {
                    break;
                }
                var childKey = keys[child];
                if (child + 1 < count)
                {
                    var rightKey = keys[child + 1];
                    if (rightKey < childKey)
                    {
                        ++child;
                        childKey = rightKey;
                    }
                }
                if (key <= childKey)
                {
                    break;
                }
                keys[index] = childKey;
                items[index] = items[child];
                index = child;
            }
            if (count > 0)
            {
                keys[index] = key;
                items[index] = item;
            }

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
        public int Count => keys.Length;
        public T Peek => Count > 0 ? items[0] : throw new InvalidOperationException();

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

            var index = keys.Length;
            keys.Add(key);
            items.Add(item);
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                var parentKey = keys[parent];
                if (parentKey.CompareTo(key) <= 0)
                {
                    break;
                }
                keys[index] = parentKey;
                items[index] = items[parent];
                index = parent;
            }
            keys[index] = key;
            items[index] = item;
        }

        public T Dequeue()
        {
            if (keys.Length == 0)
            {
                throw new InvalidOperationException();
            }

            var result = items[0];

            var count = keys.Length - 1;
            var key = keys[count];
            var item = items[count];
            keys.RemoveAt(count);
            items.RemoveAt(count);

            var index = 0;
            while (true)
            {
                var child = index * 2 + 1;
                if (child >= count)
                {
                    break;
                }
                var childKey = keys[child];
                if (child + 1 < count)
                {
                    var rightKey = keys[child + 1];
                    if (rightKey < childKey)
                    {
                        ++child;
                        childKey = rightKey;
                    }
                }
                if (key.CompareTo(childKey) <= 0)
                {
                    break;
                }
                keys[index] = childKey;
                items[index] = items[child];
                index = child;
            }
            if (count > 0)
            {
                keys[index] = key;
                items[index] = item;
            }

            return result;
        }

        public void Dispose()
        {
            items.Dispose();
            keys.Dispose();
        }
    }
}
