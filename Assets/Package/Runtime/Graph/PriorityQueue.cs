using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT.Graphs
{
    // いずれも二分ヒープ。キーが同じ要素同士の取り出し順は不定。

    public class PriorityQueue<T>
    {
        T[] items = new T[16];
        ulong[] keys = new ulong[16];
        public int Count { get; private set; }
        public T Peek => Count > 0 ? items[0] : throw new System.InvalidOperationException();

        public void Enqueue(float primaryKey, float secondaryKey, T item)
        {
            Enqueue(OrderKeyConvert.Combine(primaryKey, secondaryKey), item);
        }

        public void Enqueue(ulong key, T item)
        {
            if (Count == keys.Length)
            {
                System.Array.Resize(ref keys, Count * 2);
                System.Array.Resize(ref items, Count * 2);
            }

            // 末尾に置いて、親の方が大きい間は親を下ろしていく
            var index = Count;
            ++Count;
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                if (keys[parent] <= key)
                {
                    break;
                }
                keys[index] = keys[parent];
                items[index] = items[parent];
                index = parent;
            }
            keys[index] = key;
            items[index] = item;
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new System.InvalidOperationException();
            }

            var result = items[0];

            // 末尾の要素を根に置いたものとして、小さい方の子を引き上げながら下ろしていく
            --Count;
            var key = keys[Count];
            var item = items[Count];
            items[Count] = default!;

            var index = 0;
            while (true)
            {
                var child = index * 2 + 1;
                if (child >= Count)
                {
                    break;
                }
                if (child + 1 < Count && keys[child + 1] < keys[child])
                {
                    ++child;
                }
                if (key <= keys[child])
                {
                    break;
                }
                keys[index] = keys[child];
                items[index] = items[child];
                index = child;
            }
            if (Count > 0)
            {
                keys[index] = key;
                items[index] = item;
            }

            return result;
        }
    }

    public class FloatPriorityQueue<T>
    {
        T[] items = new T[16];
        float[] keys = new float[16];
        public int Count { get; private set; }
        public T Peek => Count > 0 ? items[0] : throw new System.InvalidOperationException();
        public (float key, T value) PeekKeyAndValue => Count > 0 ? (keys[0], items[0]) : throw new System.InvalidOperationException();

        public void Enqueue(float key, T item)
        {
            if (Count == keys.Length)
            {
                System.Array.Resize(ref keys, Count * 2);
                System.Array.Resize(ref items, Count * 2);
            }

            var index = Count;
            ++Count;
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                if (keys[parent] <= key)
                {
                    break;
                }
                keys[index] = keys[parent];
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
            if (Count == 0)
            {
                throw new System.InvalidOperationException();
            }

            var result = (keys[0], items[0]);

            --Count;
            var key = keys[Count];
            var item = items[Count];
            items[Count] = default!;

            var index = 0;
            while (true)
            {
                var child = index * 2 + 1;
                if (child >= Count)
                {
                    break;
                }
                if (child + 1 < Count && keys[child + 1] < keys[child])
                {
                    ++child;
                }
                if (key <= keys[child])
                {
                    break;
                }
                keys[index] = keys[child];
                items[index] = items[child];
                index = child;
            }
            if (Count > 0)
            {
                keys[index] = key;
                items[index] = item;
            }

            return result;
        }
    }

    public class DoublePriorityQueue<T>
    {
        T[] items = new T[16];
        OrderKey2[] keys = new OrderKey2[16];
        public int Count { get; private set; }
        public T Peek => Count > 0 ? items[0] : throw new System.InvalidOperationException();

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

            if (Count == keys.Length)
            {
                System.Array.Resize(ref keys, Count * 2);
                System.Array.Resize(ref items, Count * 2);
            }

            var index = Count;
            ++Count;
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                if (keys[parent].CompareTo(key) <= 0)
                {
                    break;
                }
                keys[index] = keys[parent];
                items[index] = items[parent];
                index = parent;
            }
            keys[index] = key;
            items[index] = item;
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new System.InvalidOperationException();
            }

            var result = items[0];

            --Count;
            var key = keys[Count];
            var item = items[Count];
            items[Count] = default!;

            var index = 0;
            while (true)
            {
                var child = index * 2 + 1;
                if (child >= Count)
                {
                    break;
                }
                if (child + 1 < Count && keys[child + 1] < keys[child])
                {
                    ++child;
                }
                if (key.CompareTo(keys[child]) <= 0)
                {
                    break;
                }
                keys[index] = keys[child];
                items[index] = items[child];
                index = child;
            }
            if (Count > 0)
            {
                keys[index] = key;
                items[index] = item;
            }

            return result;
        }
    }
}
