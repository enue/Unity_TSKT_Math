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
    }
}
