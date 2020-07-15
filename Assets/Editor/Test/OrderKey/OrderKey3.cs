using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class OrderKey3Test
    {
        [Test]
        [TestCase(1u, 0u)]
        public void Compare(uint big, uint small)
        {
            var a = new OrderKey3(big, small, 0);
            var b = new OrderKey3(small, big, 0);
            Assert.IsTrue(a > b);

            var c = new OrderKey3(0, big, 0);
            var d = new OrderKey3(0, small, 0);
            Assert.IsTrue(c > d);

            var e = new OrderKey3(0, 0, big);
            var f = new OrderKey3(0, 0, small);
            Assert.IsTrue(e > f);
        }

        [Test]
        public void Sort()
        {
            var list = new List<OrderKey3>();
            for (int i = 0; i < 100; ++i)
            {
                var v = Random.Range(int.MinValue, int.MaxValue);
                var k = OrderKeyConvert.ToUint32(v);
                list.Add(new OrderKey3(k, 0, 0));
            }
            list.Sort();
            for (int i = 1; i < list.Count; ++i)
            {
                Assert.LessOrEqual(
                    list[i - 1].primaryKey,
                    list[i].primaryKey);
            }
        }
    }
}
