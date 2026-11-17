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
        public void Managed()
        {
            var queue = new TSKT.Graphs.PriorityQueue<float>();
            var floatQueue = new TSKT.Graphs.FloatPriorityQueue<float>();
            var doubleQueue = new TSKT.Graphs.DoublePriorityQueue<float>();
            var expected = new List<float>();

            // 出し入れを混ぜても常に最小の要素が出てくる
            for (int i = 0; i < 2000; ++i)
            {
                if (expected.Count > 0 && UnityEngine.Random.Range(0, 3) == 0)
                {
                    var e = expected.Min();
                    expected.Remove(e);
                    Assert.AreEqual(e, queue.Peek);
                    Assert.AreEqual(e, queue.Dequeue());
                    Assert.AreEqual((e, e), floatQueue.PeekKeyAndValue);
                    Assert.AreEqual((e, e), floatQueue.DequeueKeyAndValue());
                    Assert.AreEqual(e, doubleQueue.Peek);
                    Assert.AreEqual(e, doubleQueue.Dequeue());
                }
                else
                {
                    var r = UnityEngine.Random.Range(-1f, 1f);
                    queue.Enqueue(r, -r, r);
                    floatQueue.Enqueue(r, r);
                    doubleQueue.Enqueue(r, -r, r);
                    expected.Add(r);
                }
                Assert.AreEqual(expected.Count, queue.Count);
                Assert.AreEqual(expected.Count, floatQueue.Count);
                Assert.AreEqual(expected.Count, doubleQueue.Count);
            }

            expected.Sort();
            foreach (var e in expected)
            {
                Assert.AreEqual(e, queue.Dequeue());
                Assert.AreEqual(e, floatQueue.Dequeue());
                Assert.AreEqual(e, doubleQueue.Dequeue());
            }
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(0, floatQueue.Count);
            Assert.AreEqual(0, doubleQueue.Count);
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
            Assert.Throws<InvalidOperationException>(() => floatQueue.Dequeue());
            Assert.Throws<InvalidOperationException>(() => doubleQueue.Dequeue());
        }

        [Test]
        public void Unmanaged()
        {
            using var queue = new UnmanagedPriorityQueue<float>(4, Allocator.Temp);
            using var floatQueue = new UnmanagedFloatPriorityQueue<float>(4, Allocator.Temp);
            using var doubleQueue = new UnmanagedDoublePriorityQueue<float>();
            var expected = new List<float>();

            // 出し入れを混ぜても常に最小の要素が出てくる
            for (int i = 0; i < 2000; ++i)
            {
                if (expected.Count > 0 && UnityEngine.Random.Range(0, 3) == 0)
                {
                    var e = expected.Min();
                    expected.Remove(e);
                    Assert.AreEqual(e, queue.Peek);
                    Assert.AreEqual(e, queue.Dequeue());
                    Assert.AreEqual(e, floatQueue.Peek);
                    Assert.AreEqual((e, e), floatQueue.DequeueKeyAndValue());
                    Assert.AreEqual(e, doubleQueue.Peek);
                    Assert.AreEqual(e, doubleQueue.Dequeue());
                }
                else
                {
                    var r = UnityEngine.Random.Range(-1f, 1f);
                    queue.Enqueue(r, -r, r);
                    floatQueue.Enqueue(r, r);
                    doubleQueue.Enqueue(r, -r, r);
                    expected.Add(r);
                }
                Assert.AreEqual(expected.Count, queue.Count);
                Assert.AreEqual(expected.Count, floatQueue.Count);
                Assert.AreEqual(expected.Count, doubleQueue.Count);
            }

            expected.Sort();
            foreach (var e in expected)
            {
                Assert.AreEqual(e, queue.Dequeue());
                Assert.AreEqual(e, floatQueue.Dequeue());
                Assert.AreEqual(e, doubleQueue.Dequeue());
            }
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(0, floatQueue.Count);
            Assert.AreEqual(0, doubleQueue.Count);
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
