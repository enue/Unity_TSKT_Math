using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Graphs
{
    public class PriorityQueue<T>
    {
        readonly List<T> items = new List<T>();
        readonly List<double> keys = new List<double>();
        public int Count => keys.Count;

        public void Enqueue(double key, T item)
        {
            // Dequeueの時に末尾からとりたいのでキーをマイナスにしておく
            var index = keys.BinarySearch(-key);
            if (index < 0)
            {
                index = ~index;
            }
            items.Insert(index, item);
            keys.Insert(index, -key);
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
        public readonly struct Key
        {
            public class Comparer : IComparer<Key>
            {
                public int Compare(Key x, Key y)
                {
                    if (x.primary > y.primary)
                    {
                        return 1;
                    }
                    if (x.primary < y.primary)
                    {
                        return -1;
                    }
                    if (x.secondary > y.secondary)
                    {
                        return 1;
                    }
                    if (x.secondary < y.secondary)
                    {
                        return -1;
                    }
                    return 0;
                }
            }

            public readonly double primary;
            public readonly double secondary;

            public Key(double primary, double secondary)
            {
                this.primary = primary;
                this.secondary = secondary;
            }
        }

        readonly List<T> items = new List<T>();
        readonly List<Key> keys = new List<Key>();
        readonly Key.Comparer keyComparer = new Key.Comparer();
        public int Count => keys.Count;

        public void Enqueue(double primaryKey, double secondaryKey, T item)
        {
            // Dequeueの時に末尾からとりたいのでキーをマイナスにしておく
            var key = new Key(-primaryKey, -secondaryKey);
            var index = keys.BinarySearch(key, keyComparer);
            if (index < 0)
            {
                index = ~index;
            }
            items.Insert(index, item);
            keys.Insert(index, key);
        }

        public (double primaryKey, double secondaryKey, T item) Dequeue()
        {
            var index = items.Count - 1;
            var item = items[index];
            var key = keys[index];
            items.RemoveAt(index);
            keys.RemoveAt(index);
            return (-key.primary, -key.secondary, item);
        }
    }

}
