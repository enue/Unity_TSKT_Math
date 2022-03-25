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
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(10)]
        public void SolvePoints(int pointCount)
        {
            var points = new List<(double time, double value)>();
            var endTime = 0f;
            for (int i = 0; i < pointCount; ++i)
            {
                endTime += Random.value;
                points.Add((endTime, Random.value));
            }
            var spline = TSKT.Spline.SolvePoints(points.ToArray());
            Validate(spline, points);
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(10)]
        public void SolveVelocityAndPoints(int pointCount)
        {
            for (int i = 0; i < pointCount; ++i)
            {
                var points = new List<(double time, double value)>();
                var endTime = 0f;
                for (int j = 0; j < pointCount; ++j)
                {
                    endTime += Random.value;
                    points.Add((endTime, Random.value));
                }
                var tt = Mathf.Lerp(-1, endTime + 1f, (float)i / pointCount);
                var spline = TSKT.Spline.SolvedVelocityAndPoints(tt, Random.value, points.ToArray());
                Validate(spline, points);
            }
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(6)]
        [TestCase(10)]
        public void SolveAccelAndVelocityAndPoints(int pointCount)
        {
            var points = new List<(double time, double value)>();
            var endTime = 0f;
            for (int i = 0; i < pointCount; ++i)
            {
                endTime += 1f;
                points.Add((endTime, Random.value));
            }

            for (int i = 0; i < points.Count + 3; ++i)
            {
                var tt = Mathf.Lerp(-1, endTime + 1f, (float)i / points.Count);
                var a = Random.value;
                var v = Random.value;
                var spline = TSKT.Spline.SolveAccelAndVelocityAndPoints(tt, a, v, points.ToArray());
                Validate(spline, points);
                Assert.AreEqual(spline.SortedKeys.ToArray(), points.Skip(1).Select(_ => _.time).ToArray());
                Assert.AreEqual(spline.SortedKeys.ToArray(), spline.SortedKeys.ToArray().OrderBy(_ => _).ToArray());
                Assert.AreEqual(points.Count - 1, spline.SortedKeys.Length);
            }
        }

        static void Validate(TSKT.Spline spline, List<(double time, double value)> points)
        {
            for (int j = 0; j < spline.SortedKeys.Length - 1; ++j)
            {
                var t = spline.SortedKeys[j];
                var f1 = spline.Functions[j];
                var f2 = spline.Functions[j + 1];
                Assert.AreEqual(f1.Evaluate(t), f2.Evaluate(t), 0.00001);
            }
            foreach (var it in points)
            {
                Assert.AreEqual(it.value, spline.Evaluate(it.time), 0.00001);
            }
        }
    }
}

