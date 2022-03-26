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
            Assert.AreEqual(v1, f.Evaluate(t1), 0.000001);
            Assert.AreEqual(v2, f.Evaluate(t2), 0.000001);
            Assert.AreEqual(0f, f.a, 0.000001);
            Assert.AreEqual(0f, f.b, 0.000001);
        }

        [Test]
        [TestCase(53f, 136f, 164f, 321f, 8f, 1f, 213f, 9f)]
        public void Solve2PointsAnd2Velocities(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve2PointsAnd2Velocities((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            Assert.AreEqual(v1, f.Evaluate(t1), 0.000001);
            Assert.AreEqual(v2, f.Evaluate(t2), 0.000001);
            Assert.AreEqual(v3, f.Velocity(t3), 0.000001);
            Assert.AreEqual(v4, f.Velocity(t4), 0.000001);
        }

        [Test]
        [TestCase(1.5f, 1f, 1f, 2f, 4f)]
        public void Solve2PointsAndConstantAccel(float t1, float v1, float t2, float v2, float a)
        {
            var f = TSKT.CubicFunction.Solve2PointsAndConstantAccel((t1, v1), (t2, v2), a);
            Assert.AreEqual(v1, f.Evaluate(t1), 0.000001);
            Assert.AreEqual(v2, f.Evaluate(t2), 0.000001);
            Assert.AreEqual(a, f.Acceleration(0f), 0.000001);
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(53f, 136f, 164f, 321f, 8f, 1f)]
        public void Solve2PointsAnd2Velocities(float t1, float v1, float t2, float v2, float t3, float v3)
        {
            var f = TSKT.CubicFunction.Solve2PointsAndVelocity((t1, v1), (t2, v2), (t3, v3));
            Assert.AreEqual(v1, f.Evaluate(t1), 0.000001);
            Assert.AreEqual(v2, f.Evaluate(t2), 0.000001);
            Assert.AreEqual(v3, f.Velocity(t3), 0.000001);
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(1f, 2f, 3f, 4f, 5f)]
        public void SolvePointAndVelocityAndConstantAccel(float t1, float p1, float t2, float v2, float a)
        {
            var f = TSKT.CubicFunction.SolvePointAndVelocityAndConstantAccel((t1, p1), (t2, v2), a);
            Assert.AreEqual(p1, f.Evaluate(t1), 0.000001);
            Assert.AreEqual(v2, f.Velocity(t2), 0.000001);
            Assert.AreEqual(a, f.Acceleration(0f), 0.000001);
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(3f, 5f, 6f, 2f, 0f, 1f, 3f, 2f)]
        public void Solve2PointsVelocityAndAccel(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve2PointsVelocityAndAccel((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            Assert.AreEqual(v1, f.Evaluate(t1), 0.000001);
            Assert.AreEqual(v2, f.Evaluate(t2), 0.000001);
            Assert.AreEqual(v3, f.Velocity(t3), 0.000001);
            Assert.AreEqual(v4, f.Acceleration(t4), 0.000001);
        }

        [Test]
        [TestCase(3f, 5f, 6f, 2f, 0f, 1f)]
        public void Solve3Points(float t1, float v1, float t2, float v2, float t3, float v3)
        {
            var f = TSKT.CubicFunction.Solve3Points((t1, v1), (t2, v2), (t3, v3));
            Assert.AreEqual(v1, f.Evaluate(t1), 0.000001);
            Assert.AreEqual(v2, f.Evaluate(t2), 0.000001);
            Assert.AreEqual(v3, f.Evaluate(t3), 0.000001);
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(3f, 3f, 6f, 2f, 0f, 1f, 3f, 2f)]
        public void Solve3PointsAndVelocity(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve3PointsAndVelocity((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            Assert.AreEqual(v1, f.Evaluate(t1), 0.000001);
            Assert.AreEqual(v2, f.Evaluate(t2), 0.000001);
            Assert.AreEqual(v3, f.Evaluate(t3), 0.000001);
            Assert.AreEqual(v4, f.Velocity(t4), 0.000001);
        }

        [Test]
        [TestCase(2.6f, 1f, 2f, 1.5f, 1f, 0.9f, 0f, 2.7f)]
        public void Solve4Points(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve4Points((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            Assert.AreEqual(v1, f.Evaluate(t1), 0.000001);
            Assert.AreEqual(v2, f.Evaluate(t2), 0.000001);
            Assert.AreEqual(v3, f.Evaluate(t3), 0.000001);
            Assert.AreEqual(v4, f.Evaluate(t4), 0.000001);
        }
    }
}

