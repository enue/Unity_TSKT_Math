using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class OrderKeyConvertTest
    {
        [Test]
        [TestCase(1.0, 0.0)]
        [TestCase(2.0, 1.0)]
        [TestCase(1.0, -1.0)]
        [TestCase(-2.0, -1.0)]
        [TestCase(double.MinValue, -1.0)]
        [TestCase(double.MinValue, double.MaxValue)]
        public void CompareDouble(double a, double b)
        {

            var x = OrderKeyConvert.ToUint64(a);
            var y = OrderKeyConvert.ToUint64(b);
            Assert.AreEqual(a > b, x > y);
            Assert.AreEqual(a < b, x < y);
        }

        [Test]
        [TestCase(ulong.MaxValue, uint.MaxValue, uint.MaxValue)]
        [TestCase(uint.MaxValue, (uint)0, uint.MaxValue)]
        [TestCase((ulong)uint.MaxValue + 1, (uint)1, (uint)0)]
        public void Combine(ulong expected, uint primary, uint secondary)
        {
            var v = OrderKeyConvert.Combine(primary, secondary);
            Assert.AreEqual(expected, v);
        }

        [Test]
        [TestCase(ulong.MaxValue, int.MaxValue, int.MaxValue)]
        [TestCase(ulong.MinValue, int.MinValue, int.MinValue)]
        public void Combine(ulong expected, int primary, int secondary)
        {
            var v = OrderKeyConvert.Combine(primary, secondary);
            Assert.AreEqual(expected, v);
        }

        [Test]
        [TestCase(0f, 1f, 0f, 0f)]
        [TestCase(0f, 2f, 0f, 1f)]
        [TestCase(0f, 1f, 0f, -1f)]
        [TestCase(0f, -1f, 0f, -2f)]
        [TestCase(1f, 0f, 0f, 0f)]
        [TestCase(2f, 0f, 1f, 0f)]
        [TestCase(1f, 0f, -1f, 0f)]
        [TestCase(-1f, 0f, -2f, 0f)]
        [TestCase(float.MaxValue, float.MaxValue, float.MaxValue, 0f)]
        [TestCase(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue)]
        public void Greater(float primary1, float secondary1, float primary2, float secondary2)
        {
            var a = OrderKeyConvert.Combine(primary1, secondary1);
            var b = OrderKeyConvert.Combine(primary2, secondary2);
            Assert.Greater(a, b);
        }

        [Test]
        [TestCase(0, 1, 0, 0)]
        [TestCase(0, 2, 0, 1)]
        [TestCase(0, 1, 0, -1)]
        [TestCase(0, -1, 0, -2)]
        [TestCase(1, 0, 0, 0)]
        [TestCase(2, 0, 1, 0)]
        [TestCase(1, 0, -1, 0)]
        [TestCase(-1, 0, -2, 0)]
        [TestCase(int.MaxValue, int.MaxValue, int.MaxValue, 0)]
        [TestCase(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue)]
        public void Greater(int primary1, int secondary1, int primary2, int secondary2)
        {
            var a = OrderKeyConvert.Combine(primary1, secondary1);
            var b = OrderKeyConvert.Combine(primary2, secondary2);
            Assert.Greater(a, b);
        }
    }
}