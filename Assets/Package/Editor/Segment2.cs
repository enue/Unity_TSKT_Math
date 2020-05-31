using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class Segment2
    {
        [Test]
        [TestCase(1f, 1f, -1f, -1f)]
        [TestCase(-1f, -1f, 1f, 1f)]
        [TestCase(-1f, 1f, 1f, -1f)]
        public void Bounds(float fromX, float fromY, float toX, float toY)
        {
            var segment = new TSKT.Segment2(new Vector2(fromX, fromY), new Vector2(toX, toY));
            var bounds = segment.Bounds;

            Assert.AreEqual(-1f, bounds.xMin);
            Assert.AreEqual(-1f, bounds.yMin);
            Assert.AreEqual(1f, bounds.xMax);
            Assert.AreEqual(1f, bounds.yMax);
            Assert.AreEqual(2f, bounds.width);
            Assert.AreEqual(2f, bounds.height);
        }
    }
}

