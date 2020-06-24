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
            var goal = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
            var board = new Board(width, height);
            if (costRandom > 0f)
            {
                for (int i = 0; i < width; ++i)
                {
                    for (int j = 0; j < height; ++j)
                    {
                        board.SetCost(i, j, 1.0 + UnityEngine.Random.Range(0f, costRandom));
                    }
                }
            }

            var distanceMap = board.ComputeDistancesFrom(start);
            var dijkstraRoutes = distanceMap.ComputeRoutesToPivotFrom(goal).ToArray();
            var goalDistanceByDijkstra = distanceMap.Distances[goal];

            var aStarSearch = new AStarSearch<Vector2Int>(board, start, goal, (a, b) => TSKT.Vector2IntUtil.GetManhattanDistance(a, b));
            var aStarPath = aStarSearch.ComputePathFromGoalToStart();
            var goalDistanceByAStarSearch = aStarSearch.Distances[goal];
            Assert.AreEqual(goalDistanceByDijkstra, goalDistanceByAStarSearch);
            Assert.IsTrue(dijkstraRoutes.Any(_ => _.SequenceEqual(aStarPath)));

            var path = AStarSearch<Vector2Int>.FindPath(board, start, goal, (a, b) => TSKT.Vector2IntUtil.GetManhattanDistance(a, b));
            Assert.IsTrue(aStarPath.SequenceEqual(path.Keys.Reverse()));
            Assert.AreEqual(goalDistanceByAStarSearch, path[goal]);
        }
    }
}