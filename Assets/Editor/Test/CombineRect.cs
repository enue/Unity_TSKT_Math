using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class CombineRect
    {
        [Test]
        public void Combine()
        {
            Rect a = Rect.MinMaxRect(0f, 0f, 1f, 1f);
            Rect b = Rect.MinMaxRect(1f, 0f, 2f, 1f);
            var combineRect = new TSKT.CombineRect();
            combineRect.Append(a);
            combineRect.Append(b);
            Assert.AreEqual(Rect.MinMaxRect(0f, 0f, 2f, 1f), combineRect.Rects[0]);
            Assert.AreEqual(new Vector2(1f, 0.5f), combineRect.Rects[0].center);
        }

        [Test]
        // max側の辺を共有する
        [TestCase(1f, 1f, 2f, 2f)]
        // min側の辺を共有する
        [TestCase(0f, 0f, 1f, 1f)]
        // 辺を共有しない
        [TestCase(0.5f, 0.5f, 1.5f, 1.5f)]
        // 同じ矩形
        [TestCase(0f, 0f, 2f, 2f)]
        public void Contained(float xMin, float yMin, float xMax, float yMax)
        {
            var outer = Rect.MinMaxRect(0f, 0f, 2f, 2f);
            var inner = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            {
                var combineRect = new TSKT.CombineRect();
                combineRect.Append(outer);
                combineRect.Append(inner);
                Assert.AreEqual(1, combineRect.Rects.Count);
                Assert.AreEqual(outer, combineRect.Rects[0]);
            }
            {
                var combineRect = new TSKT.CombineRect();
                combineRect.Append(inner);
                combineRect.Append(outer);
                Assert.AreEqual(1, combineRect.Rects.Count);
                Assert.AreEqual(outer, combineRect.Rects[0]);
            }
        }
    }
}

