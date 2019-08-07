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
        public void Process2Points(float t1, float v1, float t2, float v2)
        {
            var f = TSKT.CubicFunction.Process2Points(t1, v1, t2, v2);
            Assert.True(Mathf.Approximately(v1, f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.AreEqual(0f, f.a);
            Assert.AreEqual(0f, f.b);
        }

        [Test]
        [TestCase(53f, 136f, 164f, 321f, 8f, 1f, 213f, 9f)]
        public void Process2PointsAnd2Velocities(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Process2PointsAnd2Velocities(t1, v1, t2, v2, t3, v3, t4, v4);
            Assert.True(Mathf.Approximately(v1, f.Evaluate(t1)));
            Assert.True(Mathf.Approximately(v2, f.Evaluate(t2)));
            Assert.True(Mathf.Approximately(v3, f.Velocity(t3)));
            Assert.True(Mathf.Approximately(v4, f.Velocity(t4)));
        }

        [Test]
        [TestCase(1.5f, 1f, 1f, 2f, 4f)]
        public void Process2PointsAndConstantAccel(float t1, float v1, float t2, float v2, float a)
        {
            var f = TSKT.CubicFunction.Process2PointsAndConstantAccel(t1, v1, t2, v2, a);
            Assert.True(Mathf.Approximately(v1, f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, f.Evaluate(t2)));
            Assert.True(Mathf.Approximately(a, f.Acceleration(0f)));
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(53f, 136f, 164f, 321f, 8f, 1f)]
        public void Process2PointsAnd2Velocities(float t1, float v1, float t2, float v2, float t3, float v3)
        {
            var f = TSKT.CubicFunction.Process2PointsAndVelocity(t1, v1, t2, v2, t3, v3);
            Assert.True(Mathf.Approximately(v1, f.Evaluate(t1)));
            Assert.True(Mathf.Approximately(v2, f.Evaluate(t2)));
            Assert.True(Mathf.Approximately(v3, f.Velocity(t3)));
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(3f, 5f, 6f, 2f, 0f, 1f, 3f, 2f)]
        public void Process2PointsVelocityAndAccel(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Process2PointsVelocityAndAccel(t1, v1, t2, v2, t3, v3, t4, v4);
            Assert.True(Mathf.Approximately(v1, f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.True(Mathf.Approximately(v3, f.Velocity(t3)), v3 + ", " + f.Velocity(t3));
            Assert.True(Mathf.Approximately(v4, f.Acceleration(t4)));
        }

        [Test]
        [TestCase(3f, 5f, 6f, 2f, 0f, 1f)]
        public void Process3Points(float t1, float v1, float t2, float v2, float t3, float v3)
        {
            var f = TSKT.CubicFunction.Process3Points(t1, v1, t2, v2, t3, v3);
            Assert.True(Mathf.Approximately(v1, f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.True(Mathf.Approximately(v3, f.Evaluate(t3)), v2 + ", " + f.Evaluate(t3));
            Assert.AreEqual(0f, f.a);
        }

        [Test]
        [TestCase(3f, 3f, 6f, 2f, 0f, 1f, 3f, 2f)]
        public void Process3PointsAndVelocity(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Process3PointsAndVelocity(t1, v1, t2, v2, t3, v3, t4, v4);
            Assert.True(Mathf.Approximately(v1, f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.True(Mathf.Approximately(v3, f.Evaluate(t3)), v3 + ", " + f.Evaluate(t3));
            Assert.True(Mathf.Approximately(v4, f.Velocity(t4)));
        }

        [Test]
        [TestCase(3f, 1f, 2f, 2f, 1f, 1f, 0f, 4f)]
        public void Process4Points(float t1, float v1, float t2, float v2, float t3, float v3, float t4, float v4)
        {
            var f = TSKT.CubicFunction.Process4Points(t1, v1, t2, v2, t3, v3, t4, v4);
            Assert.True(Mathf.Approximately(v1, f.Evaluate(t1)), v1 + ", " + f.Evaluate(t1));
            Assert.True(Mathf.Approximately(v2, f.Evaluate(t2)), v2 + ", " + f.Evaluate(t2));
            Assert.True(Mathf.Approximately(v3, f.Evaluate(t3)), v3 + ", " + f.Evaluate(t3));
            Assert.True(Mathf.Approximately(v4, f.Evaluate(t4)), v3 + ", " + f.Evaluate(t4));
        }
    }
}

