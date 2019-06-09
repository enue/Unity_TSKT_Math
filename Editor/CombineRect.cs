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
    }
}

