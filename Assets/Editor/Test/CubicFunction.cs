using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class CubicFunction
    {
        // 倍精度で解いて倍精度で評価するので、誤差は相対で1e-15程度に収まる。余裕を見て1e-10まで許容する
        const double relativeTolerance = 1e-10;

        static void AreApproximatelyEqual(double expected, double actual)
        {
            Assert.AreEqual(expected, actual, relativeTolerance * System.Math.Max(1.0, System.Math.Abs(expected)));
        }

        [Test]
        [TestCase(53f, 136584f, 16584f, 12321f)]
        public void Solve2Points(float t1, float v1, float t2, float v2)
        {
            var f = TSKT.CubicFunction.Solve2Points((t1, v1), (t2, v2));
            AreApproximatelyEqual(v1, f.Evaluate(t1));
            AreApproximatelyEqual(v2, f.Evaluate(t2));
            Assert.AreEqual(0.0, f.a);
            Assert.AreEqual(0.0, f.b);
        }

        [Test]
        [TestCase(53f, 136f, 164f, 321f, 8f, 1f, 213f, 9f)]
        public void Solve2PointsAnd2Velocities(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve2PointsAnd2Velocities((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            AreApproximatelyEqual(v1, f.Evaluate(t1));
            AreApproximatelyEqual(v2, f.Evaluate(t2));
            AreApproximatelyEqual(v3, f.Velocity(t3));
            AreApproximatelyEqual(v4, f.Velocity(t4));
        }

        [Test]
        [TestCase(1.5f, 1f, 1f, 2f, 4f)]
        public void Solve2PointsAndConstantAccel(float t1, float v1, float t2, float v2, float a)
        {
            var f = TSKT.CubicFunction.Solve2PointsAndConstantAccel((t1, v1), (t2, v2), a);
            AreApproximatelyEqual(v1, f.Evaluate(t1));
            AreApproximatelyEqual(v2, f.Evaluate(t2));
            AreApproximatelyEqual(a, f.Acceleration(0f));
            Assert.AreEqual(0.0, f.a);
        }

        [Test]
        [TestCase(53f, 136f, 164f, 321f, 8f, 1f)]
        public void Solve2PointsAnd2Velocities(float t1, float v1, float t2, float v2, float t3, float v3)
        {
            var f = TSKT.CubicFunction.Solve2PointsAndVelocity((t1, v1), (t2, v2), (t3, v3));
            AreApproximatelyEqual(v1, f.Evaluate(t1));
            AreApproximatelyEqual(v2, f.Evaluate(t2));
            AreApproximatelyEqual(v3, f.Velocity(t3));
            Assert.AreEqual(0.0, f.a);
        }

        [Test]
        [TestCase(1f, 2f, 3f, 4f, 5f)]
        public void SolvePointAndVelocityAndConstantAccel(float t1, float p1, float t2, float v2, float a)
        {
            var f = TSKT.CubicFunction.SolvePointAndVelocityAndConstantAccel((t1, p1), (t2, v2), a);
            AreApproximatelyEqual(p1, f.Evaluate(t1));
            AreApproximatelyEqual(v2, f.Velocity(t2));
            AreApproximatelyEqual(a, f.Acceleration(0f));
            Assert.AreEqual(0.0, f.a);
        }

        [Test]
        [TestCase(3f, 5f, 6f, 2f, 0f, 1f, 3f, 2f)]
        public void Solve2PointsVelocityAndAccel(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve2PointsVelocityAndAccel((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            AreApproximatelyEqual(v1, f.Evaluate(t1));
            AreApproximatelyEqual(v2, f.Evaluate(t2));
            AreApproximatelyEqual(v3, f.Velocity(t3));
            AreApproximatelyEqual(v4, f.Acceleration(t4));
        }

        [Test]
        [TestCase(3f, 5f, 6f, 2f, 0f, 1f)]
        public void Solve3Points(float t1, float v1, float t2, float v2, float t3, float v3)
        {
            var f = TSKT.CubicFunction.Solve3Points((t1, v1), (t2, v2), (t3, v3));
            AreApproximatelyEqual(v1, f.Evaluate(t1));
            AreApproximatelyEqual(v2, f.Evaluate(t2));
            AreApproximatelyEqual(v3, f.Evaluate(t3));
            Assert.AreEqual(0.0, f.a);
        }

        [Test]
        [TestCase(3f, 3f, 6f, 2f, 0f, 1f, 3f, 2f)]
        public void Solve3PointsAndVelocity(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve3PointsAndVelocity((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            AreApproximatelyEqual(v1, f.Evaluate(t1));
            AreApproximatelyEqual(v2, f.Evaluate(t2));
            AreApproximatelyEqual(v3, f.Evaluate(t3));
            AreApproximatelyEqual(v4, f.Velocity(t4));
        }

        [Test]
        [TestCase(2.6f, 1f, 2f, 1.5f, 1f, 0.9f, 0f, 2.7f)]
        public void Solve4Points(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Solve4Points((t1, v1), (t2, v2), (t3, v3), (t4, v4));
            AreApproximatelyEqual(v1, f.Evaluate(t1));
            AreApproximatelyEqual(v2, f.Evaluate(t2));
            AreApproximatelyEqual(v3, f.Evaluate(t3));
            AreApproximatelyEqual(v4, f.Evaluate(t4));
        }
    }
}

