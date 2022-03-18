using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class WeightedRandom
    {
        [Test]
        public void Probability()
        {
            var selector = new WeightedRandom<string>();
            selector.Add(1f, "a");
            selector.Add(2f, "b");
            selector.Add(3f, "c");
            selector.Add(2f, "d");

            Assert.AreEqual(1f / 8f, selector.SearchProbability("a"));
            Assert.AreEqual(2f / 8f, selector.SearchProbability("b"));
            Assert.AreEqual(3f / 8f, selector.SearchProbability("c"));
            Assert.AreEqual(2f / 8f, selector.SearchProbability("d"));
        }

        [Test]
        public void TotalWeight()
        {
            var selector = new WeightedRandom<string>();
            Assert.AreEqual(0f, selector.TotalWeight);
        }
    }
}

