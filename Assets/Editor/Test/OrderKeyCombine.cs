using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class OrderKeyCombineTest
    {
        [Test]
        public void DoubleCapacity()
        {
            var combine = new OrderKeyCombine();
            combine.Append(double.MaxValue);
            Assert.AreEqual(OrderKeyCombine.capacity, combine.Length);
            Assert.Catch(() => combine.Append(double.MaxValue));
            Assert.Catch(() => combine.Append(true));
        }
        [Test]
        public void ByteCapacity()
        {
            var combine = new OrderKeyCombine();
            combine.Append(byte.MaxValue);
            combine.Append(byte.MaxValue);
            combine.Append(byte.MaxValue);
            combine.Append(byte.MaxValue);
            combine.Append(byte.MaxValue);
            combine.Append(byte.MaxValue);
            combine.Append(byte.MaxValue);
            combine.Append(byte.MaxValue);
            Assert.AreEqual(ulong.MaxValue, combine.Result);
            Assert.AreEqual(OrderKeyCombine.capacity, combine.Length);
            Assert.Catch(() => combine.Append(true));
        }
        [Test]
        public void UshortCapacity()
        {
            var combine = new OrderKeyCombine();
            combine.Append(ushort.MaxValue);
            combine.Append(ushort.MaxValue);
            combine.Append(ushort.MaxValue);
            combine.Append(ushort.MaxValue);
            Assert.AreEqual(ulong.MaxValue, combine.Result);
            Assert.AreEqual(OrderKeyCombine.capacity, combine.Length);
            Assert.Catch(() => combine.Append(true));
        }

        [Test]
        public void UintCapacity()
        {
            var combine = new OrderKeyCombine();
            combine.Append(uint.MaxValue);
            combine.Append(uint.MaxValue);
            Assert.AreEqual(ulong.MaxValue, combine.Result);
            Assert.AreEqual(OrderKeyCombine.capacity, combine.Length);
            Assert.Catch(() => combine.Append(true));
        }

        [Test]
        public void IntCapacity()
        {
            var combine = new OrderKeyCombine();
            combine.Append(int.MaxValue);
            combine.Append(int.MaxValue);
            Assert.AreEqual(ulong.MaxValue, combine.Result);
            Assert.AreEqual(OrderKeyCombine.capacity, combine.Length);
            Assert.Catch(() => combine.Append(true));
        }

        [Test]
        public void FloatCapacity()
        {
            var combine = new OrderKeyCombine();
            combine.Append(float.MaxValue);
            combine.Append(float.MaxValue);
            Assert.AreEqual(OrderKeyCombine.capacity, combine.Length);
            Assert.Catch(() => combine.Append(true));
        }

        [Test]
        public void BoolCapacity()
        {
            var combine = new OrderKeyCombine();
            for (int i = 0; i < 64; ++i)
            {
                combine.Append(true);
            }
            Assert.AreEqual(ulong.MaxValue, combine.Result);
            Assert.AreEqual(OrderKeyCombine.capacity, combine.Length);
            Assert.Catch(() => combine.Append(true));
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        [TestCase(32)]
        public void Append(int splitCount)
        {
            var k = OrderKeyConvert.ToUint64(Random.Range(0.0, double.MaxValue));
            var combine = new OrderKeyCombine();

            var splitSize = 64 / splitCount;
            for (int i = 0; i < splitCount; ++i)
            {
                combine.Append(k, splitSize * i, splitSize);
            }

            Assert.AreEqual(k, combine.Result);
            Assert.AreEqual(OrderKeyCombine.capacity, combine.Length);
            Assert.Catch(() => combine.Append(true));
        }
    }
}
