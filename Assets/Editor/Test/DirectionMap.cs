using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class DirectionMap
    {
        [Test]
        public void GetSet()
        {
            var map = new DirectionMap<string>();
            map[1, 0] = "hoge";
            map[-1, 0] = "fuga";
            map[0, 1] = "piyo";
            map[0, -1] = "foo";

            Assert.AreEqual("hoge", map[Vector2Int.right]);
            Assert.AreEqual("fuga", map[Vector2Int.left]);
            Assert.AreEqual("piyo", map[Vector2Int.up]);
            Assert.AreEqual("foo", map[Vector2Int.down]);
            Assert.AreEqual(default(string), map[0, 0]);
            Assert.Catch<System.ArgumentOutOfRangeException>(() => map[0, 0] = "bar");
        }

        [Test]
        public void Enumerable()
        {
            var map = new DirectionMap<string>();
            map[0, 1] = "u";
            map[0, -1] = "d";
            map[1, 0] = "r";
            map[-1, 0] = "l";

            foreach (var it in map)
            {
                Assert.AreEqual(map[it.Key], it.Value);
            }
        }
    }
}

