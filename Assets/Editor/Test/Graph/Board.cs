using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class BoardTest
    {
        [Test]
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(100)]
        public void GetHeuristicFunctionForAStarSearch(int size)
        {
            var board = new Board(size, size);
            for (int i = 0; i < board.Width; ++i)
            {
                for (int j = 0; j < board.Height; ++j)
                {
                    board.SetCost(i, j, 1f / 3f);
                }
            }
            var f = board.GetHeuristicFunctionForAStarSearch();

            var start = Vector2Int.zero;
            var distanceMap = board.CreateDistanceMapFrom(start);
            foreach (var it in distanceMap.Distances)
            {
                var goal = it.Key;
                var distance = it.Value;

                Assert.LessOrEqual(f(start, goal), distance);
            }
        }

        [Test]
        public void GetHeuristicFunctionForAStarSearchAtRandomTest()
        {
            var cost = TSKT.Random.Range(1f, 3f);
            var board = new Board(100, 100);
            for (int i = 0; i < board.Width; ++i)
            {
                for (int j = 0; j < board.Height; ++j)
                {
                    board.SetCost(i, j, cost);
                }
            }
            var f = board.GetHeuristicFunctionForAStarSearch();

            var start = Vector2Int.zero;
            var distanceMap = board.CreateDistanceMapFrom(start);
            foreach (var it in distanceMap.Distances)
            {
                var goal = it.Key;
                var distance = it.Value;

                Assert.LessOrEqual(f(start, goal), distance);
            }
        }
    }
}

