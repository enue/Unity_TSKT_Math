#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Unity.PerformanceTesting;
using System;
using TSKT.Graphs;
using Unity.Collections;

namespace TSKT.Tests
{
    public class PriorityQueue
    {
        [Test]
        public void Test()
        {
            using var queue = new UnmanagedPriorityQueue<float>(1000, Allocator.Temp);
            var expected = new List<float>();
            for (int i = 0; i < 1000; ++i)
            {
                var r = UnityEngine.Random.Range(-1f, 1f);
                queue.Enqueue(r, -r, r);
                expected.Add(r);
            }
            expected.Sort();

            while (queue.Count > 0)
            {
                var e = expected[0];
                expected.RemoveAt(0);
                Assert.AreEqual(e, queue.Dequeue());
            }
        }
        [Test]
        public void TestInt()
        {
            using var queue = new UnmanagedPriorityQueue<int>(1000, Allocator.Temp);
            var expected = new List<int>();
            for (int i = 0; i < 1000; ++i)
            {
                var r = UnityEngine.Random.Range(-2, 2);
                queue.Enqueue(r, -r, r);
                expected.Add(r);
            }
            expected.Sort();

            while (queue.Count > 0)
            {
                var e = expected[0];
                expected.RemoveAt(0);
                Assert.AreEqual(e, queue.Dequeue());
            }
        }
        [Test]
        public void Hoge()
        {
            using var queue = new UnmanagedPriorityQueue<float>(1000, Allocator.Temp);
            queue.Enqueue(1f, 0f, 1f);
            queue.Enqueue(0f, 0f, 0f);
            Assert.AreEqual(0f, queue.Dequeue());
            Assert.AreEqual(1f, queue.Dequeue());

        }
    }
}
