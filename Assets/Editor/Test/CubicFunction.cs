using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class CubicFunction
    {
        [Test]
        [TestCase(53f, 136584f, 16584f, 12321f)]
        public void Solve2Points(float t1, float v1, float t2, float v2)
        {
            var f = TSKT.CubicFunction.Solve2Points((t1, v1), (t2, v2));
            Assert.True(Mathf.Approximately(v1, (float)f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, (float)f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.AreEqual(0f, f.a);
            Assert.AreEqual(0f, f.b);
        }

        [Test]
        [TestCase(53f, 136f, 164f, 321f, 8f, 1f, 213f, 9f)]
        public void Solve2PointsAnd2Velocities(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve2PointsAnd2Velocities((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            Assert.True(Mathf.Approximately(v1, (float)f.Evaluate(t1)));
            Assert.True(Mathf.Approximately(v2, (float)f.Evaluate(t2)));
            Assert.True(Mathf.Approximately(v3, (float)f.Velocity(t3)));
            Assert.True(Mathf.Approximately(v4, (float)f.Velocity(t4)));
        }

        [Test]
        [TestCase(1.5f, 1f, 1f, 2f, 4f)]
        public void Solve2PointsAndConstantAccel(float t1, float v1, float t2, float v2, float a)
        {
            var f = TSKT.CubicFunction.Solve2PointsAndConstantAccel((t1, v1), (t2, v2), a);
            Assert.True(Mathf.Approximately(v1, (float)f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, (float)f.Evaluate(t2)));
            Assert.True(Mathf.Approximately(a, (float)f.Acceleration(0f)));
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(53f, 136f, 164f, 321f, 8f, 1f)]
        public void Solve2PointsAnd2Velocities(float t1, float v1, float t2, float v2, float t3, float v3)
        {
            var f = TSKT.CubicFunction.Solve2PointsAndVelocity((t1, v1), (t2, v2), (t3, v3));
            Assert.True(Mathf.Approximately(v1, (float)f.Evaluate(t1)));
            Assert.True(Mathf.Approximately(v2, (float)f.Evaluate(t2)));
            Assert.True(Mathf.Approximately(v3, (float)f.Velocity(t3)));
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(1f, 2f, 3f, 4f, 5f)]
        public void SolvePointAndVelocityAndConstantAccel(float t1, float p1, float t2, float v2, float a)
        {
            var f = TSKT.CubicFunction.SolvePointAndVelocityAndConstantAccel((t1, p1), (t2, v2), a);
            Assert.True(Mathf.Approximately(p1, (float)f.Evaluate(t1)));
            Assert.True(Mathf.Approximately(v2, (float)f.Velocity(t2)));
            Assert.True(Mathf.Approximately(a, (float)f.Acceleration(0f)));
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(3f, 5f, 6f, 2f, 0f, 1f, 3f, 2f)]
        public void Solve2PointsVelocityAndAccel(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve2PointsVelocityAndAccel((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            Assert.True(Mathf.Approximately(v1, (float)f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, (float)f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.True(Mathf.Approximately(v3, (float)f.Velocity(t3)), v3 + ", " + f.Velocity(t3));
            Assert.True(Mathf.Approximately(v4, (float)f.Acceleration(t4)));
        }

        [Test]
        [TestCase(3f, 5f, 6f, 2f, 0f, 1f)]
        public void Solve3Points(float t1, float v1, float t2, float v2, float t3, float v3)
        {
            var f = TSKT.CubicFunction.Solve3Points((t1, v1), (t2, v2), (t3, v3));
            Assert.True(Mathf.Approximately(v1, (float)f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, (float)f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.True(Mathf.Approximately(v3, (float)f.Evaluate(t3)), v2 + ", " + f.Evaluate(t3));
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(3f, 3f, 6f, 2f, 0f, 1f, 3f, 2f)]
        public void Solve3PointsAndVelocity(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve3PointsAndVelocity((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            Assert.True(Mathf.Approximately(v1, (float)f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, (float)f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.True(Mathf.Approximately(v3, (float)f.Evaluate(t3)), v3 + ", " + f.Evaluate(t3));
            Assert.True(Mathf.Approximately(v4, (float)f.Velocity(t4)));
        }

        [Test]
        [TestCase(2.6f, 1f, 2f, 1.5f, 1f, 0.9f, 0f, 2.7f)]
        public void Solve4Points(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve4Points((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            Assert.True(Mathf.Approximately(v1, (float)f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, (float)f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.True(Mathf.Approximately(v3, (float)f.Evaluate(t3)), v3 + ", " + f.Evaluate(t3));
            Assert.True(Mathf.Approximately(v4, (float)f.Evaluate(t4)), v3 + ", " + f.Evaluate(t4));
        }
    }
}

