using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class SnapAngleTest
    {
        [Test]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        [TestCase(23)]
        public void GetSnappedAngle(int devide)
        {
            for (int index = 0; index < devide; ++index)
            {
                var angle = Mathf.PI * 2f * index / devide;
                var x = Mathf.Cos(angle);
                var y = Mathf.Sin(angle);

                var snapped = SnapAngle.Snap(x, y, devide);
                SnapAngle.GetSnappedAngle(x, y, devide, out var _index);

                Assert.AreEqual(index, _index);
            }
        }
    }
}

