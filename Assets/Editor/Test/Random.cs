using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class RandomTest
    {
        [Test]
        public void RangeInt()
        {
            var a = TSKT.Random.Range(0f, 1f);
            var b = TSKT.Random.Range(0f, 1f);
            Assert.AreNotEqual(a, b);
        }
        [Test]
        public void RangeFloat()
        {
            var a = TSKT.Random.Range(0, 10000);
            var b = TSKT.Random.Range(0, 10000);
            Assert.AreNotEqual(a, b);
        }
        [Test]
        public void RangeDouble()
        {
            var a = TSKT.Random.Range(0.0, 1.0);
            var b = TSKT.Random.Range(0.0, 1.0);
            Assert.AreNotEqual(a, b);
        }
    }
}

