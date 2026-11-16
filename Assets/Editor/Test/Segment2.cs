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

        [Test]
        // 点と点
        [TestCase(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, true)]
        [TestCase(0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, false)]
        // 点と離れた線分
        [TestCase(0f, 0f, 0f, 0f, 5f, 5f, 6f, 9f, false)]
        // 点と、その点を通る線分
        [TestCase(0f, 0f, 0f, 0f, -1f, 0f, 1f, 0f, true)]
        // 点と、延長線上にその点がある線分
        [TestCase(0f, 0f, 0f, 0f, 1f, 0f, 2f, 0f, false)]
        // 点と、その点を端点とする線分
        [TestCase(0f, 0f, 0f, 0f, 0f, 0f, 2f, 0f, true)]
        public void IntersectWithPoint(
            float fromX, float fromY, float toX, float toY,
            float targetFromX, float targetFromY, float targetToX, float targetToY,
            bool expected)
        {
            var a = new TSKT.Segment2(new Vector2(fromX, fromY), new Vector2(toX, toY));
            var b = new TSKT.Segment2(new Vector2(targetFromX, targetFromY), new Vector2(targetToX, targetToY));

            Assert.AreEqual(expected, a.Intersect(b));
            Assert.AreEqual(expected, b.Intersect(a));
        }
    }
}

