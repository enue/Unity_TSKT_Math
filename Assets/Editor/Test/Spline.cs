using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class Spline
    {
        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(10)]
        [TestCase(25)]
        [TestCase(37)]
        [TestCase(40)]
        [TestCase(50)]
        [TestCase(100)]
        public void SolvePoints(int pointCount)
        {
            var points = new List<(double time, double value)>();
            var endTime = 0d;
            for (int i = 0; i < pointCount; ++i)
            {
                endTime += Random.value + 1d;
                points.Add((endTime, Random.value));
            }
            var spline = new TSKT.Spline(points.ToArray());
            Validate(spline, points);
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(10)]
        [TestCase(25)]
        [TestCase(37)]
        [TestCase(40)]
        [TestCase(50)]
        [TestCase(100)]
        public void SolvePointse(int pointCount)
        {
            var points = new List<(double time, double value)>();
            var endTime = 0d;
            for (int i = 0; i < pointCount; ++i)
            {
                endTime += 1f;
                points.Add((endTime, Random.value + 1f));
            }
            var spline = new TSKT.Spline(points.ToArray());
            Validate(spline, points);
        }

        static void Validate(TSKT.Spline spline, List<(double time, double value)> points)
        {
            foreach (var it in points)
            {
                Assert.AreEqual(it.value, spline.Evaluate(it.time), 0.00001);
            }
            for (int i = 0; i < spline.EndTimes.Length - 1; i++)
            {
                var t = spline.EndTimes[i];
                var f1 = spline.Intervals[i];
                var f2 = spline.Intervals[i + 1];

                Assert.AreEqual(f1.Evaluate(t), f2.Evaluate(t), 0.00001);
                Assert.AreEqual(f1.Acceleration(t), f2.Acceleration(t), 0.00001);
                Assert.AreEqual(f1.Velocity(t), f2.Velocity(t), 0.00001);
            }
        }
    }
}

