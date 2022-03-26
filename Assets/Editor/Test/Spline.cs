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
        [TestCase(100)]
        public void SolvePoints(int pointCount)
        {
            var points = new List<(float time, float value)>();
            var endTime = 0f;
            for (int i = 0; i < pointCount; ++i)
            {
                endTime += Random.value;
                points.Add((endTime, Random.value));
            }
            var spline = new TSKT.Spline(points.ToArray());
            Validate(spline, points);
        }

        static void Validate(TSKT.Spline spline, List<(float time, float value)> points)
        {
            for (int j = 0; j < spline.EndTimes.Length - 1; ++j)
            {
                var t = spline.EndTimes[j];
                var f1 = spline.Intervals[j];
                var f2 = spline.Intervals[j + 1];
                Assert.AreEqual(f1.Evaluate(t), f2.Evaluate(t), 0.00001);
            }
            foreach (var it in points)
            {
                Assert.AreEqual(it.value, spline.Evaluate(it.time), 0.00001);
            }
            for (int i = 0; i < spline.EndTimes.Length - 1; i++)
            {
                var a = spline.Intervals[i];
                var b = spline.Intervals[i + 1];
                var t = spline.EndTimes[i];

                Assert.AreEqual(a.Evaluate(t), b.Evaluate(t));
                Assert.AreEqual(a.Acceleration(t), b.Acceleration(t));
                Assert.AreEqual(a.Velocity(t), b.Velocity(t));
            }
        }
    }
}

