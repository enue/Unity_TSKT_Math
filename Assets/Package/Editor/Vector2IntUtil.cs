using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class Vector2IntUtil
    {
        [Test]
        [TestCase(1, 0, 0f, 1, 0)]
        [TestCase(1, 0, 90f, 0, 1)]
        [TestCase(1, 0, 180f, -1, 0)]
        [TestCase(1, 0, -90f, 0, -1)]
        public void Rotate(int x, int y, float degree, int expectedX, int expectedY)
        {
            var sin = Mathf.Sin(Mathf.Deg2Rad * degree);
            var cos = Mathf.Cos(Mathf.Deg2Rad * degree);

            {
                var rotated = TSKT.Vector2IntUtil.Rotate(new Vector2Int(x, y), Mathf.Deg2Rad * degree);
                Assert.AreEqual(new Vector2Int(expectedX, expectedY), rotated);
            }
            {
                var rotated = TSKT.Vector2IntUtil.Rotate(new Vector2Int(x, y), cos: cos, sin: sin);
                Assert.AreEqual(new Vector2Int(expectedX, expectedY), rotated);
            }

            {
                var rotated = TSKT.Vector2IntUtil.Rotate(new Vector2Int(x, y), new Vector2Int((int)cos, (int)sin));
                Assert.AreEqual(new Vector2Int(expectedX, expectedY), rotated);
            }
        }
    }
}

