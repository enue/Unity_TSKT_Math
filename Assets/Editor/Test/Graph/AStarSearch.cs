using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System;

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
                        board.SetCost(i, j, 1f + UnityEngine.Random.Range(0f, costRandom));
                    }
                    if (UnityEngine.Random.Range(0f, 1f) < 0.1f)
                    {
                        board.Disable(i, j);
                    }
                }
            }

            var distanceMap = board.CreateDistanceMapFrom(start);
            var aStarSearch = new UnmanagedAStarSearch<Vector2Int>(board, start, (a, b) => TSKT.Vector2IntUtil.GetManhattanDistance(a, b));

            for (int i = 0; i < 1000; ++i)
            {
                var goal = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));

                Span<Vector2Int[]> dijkstraRoutes = new Vector2Int[150][];
                distanceMap.SearchPaths(goal, dijkstraRoutes, out var dijkstraWrittenCount);
                distanceMap.Distances.TryGetValue(goal, out var goalDistanceByDijkstra);

                var aStarPath = aStarSearch.SearchPath(goal);
                if (dijkstraWrittenCount == 0)
                {
                    Assert.AreEqual(0, aStarPath.Length);
                }
                else
                {
                    Assert.AreEqual(goalDistanceByDijkstra, aStarSearch.memo.Distances[goal]);
                    // 経路が多くてwrittenCountが溢れた場合にはaStarとダイクストラで結果が一致しないことがある
                    if (dijkstraWrittenCount < dijkstraRoutes.Length)
                    {
                        Assert.IsTrue(dijkstraRoutes.Slice(0, dijkstraWrittenCount).ToArray().Any(_ => _.SequenceEqual(aStarPath)), string.Join(", ", aStarPath));
                    }
                }

                Span<Vector2Int[]> aStarPaths = new Vector2Int[150][];
                aStarSearch.SearchAllPaths(goal, float.PositiveInfinity, aStarPaths, out var aStarWrittenCount);
                Assert.AreEqual(dijkstraWrittenCount, aStarWrittenCount, (start, goal).ToString());
                if (dijkstraWrittenCount < dijkstraRoutes.Length)
                {
                    foreach (var it in aStarPaths.Slice(0, aStarWrittenCount))
                    {
                        Assert.IsTrue(dijkstraRoutes.Slice(0, dijkstraWrittenCount).ToArray().Any(_ => _.SequenceEqual(it)));
                    }
                }

                var path = board.CreateAStarSearch(start).SearchPath(goal);
                if (aStarPath.Length > 0)
                {
                    Assert.AreEqual(aStarPath.Last(), path.Last());
                }
            }
        }
        [Test]
        [TestCase(100, 100, 10)]
        [TestCase(10, 10, 1000)]
        public void AStarTest(int width, int height, int iteration)
        {
            var start = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
            var board = new Board(width, height);

            var aStarSearch = board.CreateAStarSearch(start);

            for (int i = 0; i < iteration; ++i)
            {
                var goal = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));

                var path = aStarSearch.SearchPath(goal);
                var distanceMap = board.CreateDistanceMapFrom(start);
                distanceMap.SolveWithin(float.PositiveInfinity);
                Assert.AreEqual(distanceMap.Distances[goal], aStarSearch.memo.Distances[goal], string.Join(", ", path));
            }
        }
        [Test]
        [TestCase(100, 100, 1f, 10)]
        [TestCase(10, 10, 0f, 1000)]
        public void MuitlGoalTest(int width, int height, float costRandom, int iteration)
        {
            var start = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
            var board = new Board(width, height);
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (costRandom > 0f)
                    {
                        board.SetCost(i, j, 1f + UnityEngine.Random.Range(0f, costRandom));
                    }
                }
            }

            var aStarSearch = new UnmanagedAStarSearch<Vector2Int>(board, start, (a, b) => TSKT.Vector2IntUtil.GetManhattanDistance(a, b));

            for (int i = 0; i < iteration; ++i)
            {
                var goal1 = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
                var goal2 = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));

                aStarSearch.SearchNearestNode(new[] { goal1, goal2 }, float.PositiveInfinity, out var node);
                var aStarPath = aStarSearch.SearchPath(node);

                var distanceMap = board.CreateDistanceMapFrom(start);
                distanceMap.SearchPath(goal1);
                distanceMap.SearchPath(goal2);
                var d1 = distanceMap.Distances[goal1];
                var d2 = distanceMap.Distances[goal2];

                aStarSearch.SearchPath(goal1);
                Assert.AreEqual(d1, aStarSearch.memo.Distances[goal1]);
                aStarSearch.SearchPath(goal2);
                Assert.AreEqual(d2, aStarSearch.memo.Distances[goal2]);

                if (aStarPath.Last() == goal1)
                {
                    Assert.LessOrEqual(d1, d2);
                }
                else if (aStarPath.Last() == goal2)
                {
                    Assert.GreaterOrEqual(d1, d2, i.ToString());
                }
                else
                {
                    Assert.Fail();
                }
            }
        }
        [Test]
        public void MultiGoalWithMemoTest()
        {
            var start = new Vector2Int(0, 0);
            var board = new Board(10, 2);
            var aStarSearch = new AStarSearch<Vector2Int>(board, start, (a, b) => TSKT.Vector2IntUtil.GetManhattanDistance(a, b));

            var goal1 = new Vector2Int(9, 0);
            var goal2 = new Vector2Int(0, 1);

            var i = new Vector2Int[99][];
            aStarSearch.SearchAllPaths(goal1, float.MaxValue, i, out _);

            var found =  aStarSearch.SearchNearestNode(new[] { goal1, goal2 }, float.PositiveInfinity, out var nearest);
            Assert.IsTrue(found);
            var paths = aStarSearch.SearchPath(nearest);
            Assert.AreEqual(goal2, paths.Last());
        }
    }
}