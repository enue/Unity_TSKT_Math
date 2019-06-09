using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class RectMap
    {
        [Test]
        [TestCase(1f, 0.5f)]
        [TestCase(0.8f, 0.4f)]
        [TestCase(1.2f, 0.6f)]
        public void TryGetFirst(float cellSize, float offset)
        {
            var map = new RectMap<string>(cellSize, offset);
            map.Add(new Rect(0f, 0f, 1f, 1f), "hoge");

            {
                var found = map.TryGetFirst(new Vector2(0.5f, 0.5f), out var pair);
                Assert.IsTrue(found);
                Assert.AreEqual("hoge", pair.value);
            }
            {
                var found = map.TryGetFirst(new Vector2(0f, 0f), out var pair);
                Assert.IsTrue(found);
                Assert.AreEqual("hoge", pair.value);
            }
            {
                var found = map.TryGetFirst(new Vector2(1f, 1f), out var pair);
                Assert.IsTrue(found);
                Assert.AreEqual("hoge", pair.value);
            }
            {
                var found = map.TryGetFirst(new Vector2(1.1f, 1f), out _);
                Assert.IsFalse(found);
            }
        }
    }
}

