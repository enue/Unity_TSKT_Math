using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class AStarSearch
    {
        [Test]
        [TestCase(100, 100, 1f)]
        [TestCase(10, 10, 0f)]
        public void Test(int width, int height, float costRandom)
        {
            var start = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
            var board = new Board(width, height);
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (costRandom > 0f)
                    {
                        board.SetCost(i, j, 1.0 + UnityEngine.Random.Range(0f, costRandom));
                    }
                    if (UnityEngine.Random.Range(0f, 1f) < 0.1f)
                    {
                        board.Disable(i, j);
                    }
                }
            }

            var distanceMap = board.ComputeDistancesFrom(start);
            var aStarSearch = new AStarSearch<Vector2Int>(board, start, (a, b) => TSKT.Vector2IntUtil.GetManhattanDistance(a, b));

            for (int i = 0; i < 10; ++i)
            {
                var goal = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));

                var dijkstraRoutes = distanceMap.SearchPaths(goal).ToArray();
                distanceMap.Distances.TryGetValue(goal, out var goalDistanceByDijkstra);

                var aStarPath = aStarSearch.FindPath(goal);
                if (dijkstraRoutes.Length == 0)
                {
                    Assert.AreEqual(null, aStarPath);
                }
                else
                {
                    Assert.AreEqual(goalDistanceByDijkstra, aStarPath[goal]);
                    Assert.IsTrue(dijkstraRoutes.Any(_ => _.SequenceEqual(aStarPath.Keys)));
                }

                var aStarPaths = aStarSearch.FindAllPaths(goal).ToArray();
                Assert.AreEqual(dijkstraRoutes.Length, aStarPaths.Length);
                foreach (var it in aStarPaths)
                {
                    Assert.IsTrue(dijkstraRoutes.Any(_ => _.SequenceEqual(it.Keys)));
                }

                var path = AStarSearch<Vector2Int>.FindPath(board, start, goal, (a, b) => TSKT.Vector2IntUtil.GetManhattanDistance(a, b));
                Assert.AreEqual(aStarPath?.Last(), path?.Last());
            }
        }
    }
}
