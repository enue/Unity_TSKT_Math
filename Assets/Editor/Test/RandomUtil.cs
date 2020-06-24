using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class RandomUtil
    {
        [Test]
        [TestCase(0, 10)]
        [TestCase(100, 10)]
        public void GenerateShuffledRange(int start, int count)
        {
            var original = Enumerable.Range(start, count);
            var shuffled = TSKT.RandomUtil.GenerateShuffledRange(start, count);
            Assert.True(original.All(_ => shuffled.Contains(_)));
        }

        [Test]
        [TestCase(1, 2, 3, 4, 5, 6, 7, 8, 9)]
        public void GenerateShuffledArray(params object[] parameters)
        {
            var original = new List<object>(parameters);
            var shuffled = TSKT.RandomUtil.GenerateShuffledArray(original);
            Assert.True(original.All(_ => shuffled.Contains(_)));
        }

        [Test]
        [TestCase(1, 2, 3, 4, 5, 6, 7, 8, 9)]
        public void ShuffleList(params object[] parameters)
        {
            var original = new List<object>(parameters);
            var shuffled = new List<object>(original);
            TSKT.RandomUtil.Shuffle(ref shuffled);
            Assert.True(original.All(_ => shuffled.Contains(_)));
        }

        [Test]
        [TestCase(1, 2, 3, 4, 5, 6, 7, 8, 9)]
        public void ShuffleArray(params object[] parameters)
        {
            var shuffled = new object[parameters.Length];
            for(int i=0; i<parameters.Length; ++i)
            {
                shuffled[i] = parameters[i];
            }

            TSKT.RandomUtil.Shuffle(ref shuffled);
            Assert.True(parameters.All(_ => shuffled.Contains(_)));
        }
    }
}

