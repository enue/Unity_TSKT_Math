using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Unity.Mathematics;

namespace TSKT.Tests
{
    public class MatrixUtil
    {
        [Test]
        public void SolveSimultaneousEquations()
        {
            var left = new double4x4();
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    left[i][j] = Random.value;
                }
            }

            var right = new double4(Random.value, Random.value, Random.value, Random.value);
            var k = TSKT.MatrixUtil.SolveSimultaneousEquations(left, right);

            var inversed = math.inverse(left);
            var l = math.mul(inversed, right);
            Assert.AreEqual(l, k);
        }

        [Test]
        public void LUDecomposition()
        {
            var source = new double4x4();
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    source[i][j] = Random.value;
                }
            }
            var success = TSKT.MatrixUtil.TryLUDecomposition(source, out var L, out var U);
            Assert.IsTrue(success);
            Assert.AreEqual(1, L.c0.x);
            Assert.AreEqual(0, L.c1.x);
            Assert.AreEqual(0, L.c2.x);
            Assert.AreEqual(0, L.c3.x);
            Assert.AreEqual(1, L.c1.y);
            Assert.AreEqual(0, L.c2.y);
            Assert.AreEqual(0, L.c3.y);
            Assert.AreEqual(1, L.c2.z);
            Assert.AreEqual(0, L.c3.z);
            Assert.AreEqual(1, L.c3.w);

            Assert.AreEqual(0, U.c0.y);
            Assert.AreEqual(0, U.c0.z);
            Assert.AreEqual(0, U.c1.z);
            Assert.AreEqual(0, U.c0.w);
            Assert.AreEqual(0, U.c1.w);
            Assert.AreEqual(0, U.c2.w);
            Assert.AreEqual(source, math.mul(L, U));
        }

        [Test]
        public void LUDecomposition2()
        {
            var source = new double4x4();
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    source[i][j] = Random.value;
                }
            }
            source.c0.z = 0;
            source.c1.z = 0;
            source.c2.z = 0;
            var success = TSKT.MatrixUtil.TryLUDecomposition(source, out var L, out var U);
            Assert.IsFalse(success);
        }
        [Test]
        public void LUDecomposition3x3()
        {
            var source = new double3x3();
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    source[i][j] = Random.value;
                }
            }
            TSKT.MatrixUtil.LUDecomposition(source, out var L, out var U);
            Assert.AreEqual(1, L.c0.x);
            Assert.AreEqual(0, L.c1.x);
            Assert.AreEqual(0, L.c2.x);
            Assert.AreEqual(1, L.c1.y);
            Assert.AreEqual(0, L.c2.y);
            Assert.AreEqual(1, L.c2.z);

            Assert.AreEqual(0, U.c0.y);
            Assert.AreEqual(0, U.c0.y);
            Assert.AreEqual(0, U.c1.z);
            Assert.AreEqual(source, math.mul(L, U));
        }

        [Test]
        public void LUDecomposition2x2()
        {
            var source = new double2x2();
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    source[i][j] = Random.value;
                }
            }
            TSKT.MatrixUtil.LUDecomposition(source, out var L, out var U);
            Assert.AreEqual(1, L.c0.x);
            Assert.AreEqual(0, L.c1.x);
            Assert.AreEqual(1, L.c1.y);

            Assert.AreEqual(0, U.c0.y);
            Assert.AreEqual(source, math.mul(L, U));
        }
    }
}

