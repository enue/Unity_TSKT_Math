using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class BatchedGraph
    {
        [Test]
        public void Test()
        {
            var board = new Board(100, 100);

            var graph = new TSKT.BatchedGraph<Vector2Int>(board, Vector2Int.zero, 10.0, 10.0);

            for (int i = 0; i < 100; ++i)
            {
                var start = new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height));
                var goal = new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height));
                var path = graph.GetPath(start, goal).ToArray();
            }
        }

        [Test]
        public void CompleteTest()
        {
            var size = 10;
            var board = new Board(size, size);
            var graph = new TSKT.BatchedGraph<Vector2Int>(board, Vector2Int.zero, 3.0, 3.0);

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    var start = new Vector2Int(i, j);
                    for (int x = 0; x < size; ++x)
                    {
                        for (int y = 0; y < size; ++y)
                        {
                            var goal = new Vector2Int(x, y);
                            var path = graph.GetPath(start, goal).ToArray();
                        }
                    }
                }
            }
        }

        [Test]
        //[TestCase(0, 0, 20, 20)]
        //[TestCase(20, 0, 20, 20)]
        //[TestCase(40, 0, 20, 20)]
        //[TestCase(60, 0, 20, 20)]
        [TestCase(80, 0, 20, 20)]
        //[TestCase(0, 20, 20, 20)]
        [TestCase(20, 20, 20, 20)]
        [TestCase(40, 20, 20, 20)]
        [TestCase(60, 20, 20, 20)]
        //[TestCase(80, 20, 20, 20)]
        [TestCase(0, 40, 20, 20)]
        //[TestCase(20, 40, 20, 20)]
        //[TestCase(40, 40, 20, 20)]
        //[TestCase(60, 40, 20, 20)]
        //[TestCase(80, 40, 20, 20)]
        //[TestCase(0, 60, 20, 20)]
        [TestCase(20, 60, 20, 20)]
        [TestCase(40, 60, 20, 20)]
        [TestCase(60, 60, 20, 20)]
        //[TestCase(80, 60, 20, 20)]
        //[TestCase(0, 80, 20, 20)]
        //[TestCase(20, 80, 20, 20)]
        //[TestCase(40, 80, 20, 20)]
        //[TestCase(60, 80, 20, 20)]
        //[TestCase(80, 80, 20, 20)]
        public void ConstantTest(int startX, int startY, int xCount, int yCount)
        {
            var board = new Board(100, 100);

            for (int i = 0; i < board.Width; ++i)
            {
                for (int j = 0; j < board.Height; ++j)
                {
                    var r = Mathf.PerlinNoise(i * 0.5f, j * 0.5f) * 8f;
                    board.SetCost(i, j, r + 1f);
                }
            }

            var batchedGraph = new BatchedGraph<Vector2Int>(board,
                new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height)),
                25.0, 50.0,
                board.GetHeuristicFunctionForAStarSearch());

            for (int i = startX; i < startX + xCount; ++i)
            {
                for (int j = startY; j < startY + yCount; ++j)
                {
                    var start = new Vector2Int(i, j);
                    var s = batchedGraph.GetStartintPoint(start);
                    for (int p = startX; p < startX + xCount; ++p)
                    {
                        for (int q = startY; q < startY + yCount; ++q)
                        {
                            var goal = new Vector2Int(p, q);
                            s.GetPath(goal).ToArray();
                        }
                    }
                }
            }

        }

        public void Performance()
        {
            var board = new Board(100, 100);

            var graph = new TSKT.BatchedGraph<Vector2Int>(board, Vector2Int.zero, 10.0, 10.0);

            for (int i = 0; i < 100; ++i)
            {
                var start = new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height));
                var goal = new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height));
                var path = graph.GetPath(start, goal).ToArray();

                var completeMap = new DistanceMap<Vector2Int>(board, start, goal);
                var completePath = completeMap.SearchPaths(goal).First();

                Debug.Log(graph.batchGraph.ComputeAllNodes().Count);
                Debug.Log(path.Length + " / " + completePath.Length + " from " + start + " to " + goal);
            }
        }

        [Test]
        public void CompareAStarAndBatched()
        {
            var board = new Board(100, 100);
            for (int i = 0; i < board.Width; ++i)
            {
                for (int j = 0; j < board.Height; ++j)
                {
                    board.SetCost(i, j, TSKT.Random.Range(1.0, 2.0));
                }
            }

            var problems = new List<(Vector2Int start, Vector2Int goal)>();
            for (int i = 0; i < 100; ++i)
            {
                problems.Add(
                    (new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height)),
                    new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height))));
            }

            long batchedScore;
            long aStarScore;
            long dijkstraScore;

            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                var graph = new BatchedGraph<Vector2Int>(board, Vector2Int.zero, 10.0, 10.0, board.GetHeuristicFunctionForAStarSearch());
                foreach (var (start, goal) in problems)
                {
                    var path = graph.GetPath(start, goal).ToArray();
                }
                sw.Stop();
                batchedScore = sw.ElapsedMilliseconds;
            }
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Restart();
                foreach (var (start, goal) in problems)
                {
                    var aStar = board.CreateAStarSearch(start);
                    var path = aStar.SearchPath(goal).ToArray();
                }
                sw.Stop();
                aStarScore = sw.ElapsedMilliseconds;
            }

            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Restart();
                foreach (var (start, goal) in problems)
                {
                    var distanceMap = board.ComputeDistancesFrom(start);
                    var path = distanceMap.SearchPaths(goal).First();
                }
                sw.Stop();
                dijkstraScore = sw.ElapsedMilliseconds;
            }

            Debug.Log("Batched : " + batchedScore + "ms");
            Debug.Log("aStar : " + aStarScore + "ms");
            Debug.Log("dijkstra : " + dijkstraScore + "ms");
        }
    }
}
