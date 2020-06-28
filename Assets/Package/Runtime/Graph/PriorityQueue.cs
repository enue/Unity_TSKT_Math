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
}
