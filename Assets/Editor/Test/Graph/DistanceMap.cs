using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class DistanceMap
    {
        [Test]
        public void Test()
        {
            var board = new Board(10, 10);
            var distanceMap = board.ComputeDistancesFrom(new Vector2Int(5, 5));
            Assert.AreEqual(7.0, distanceMap.Distances[new Vector2Int(9, 8)]);

            var paths = distanceMap.SearchPaths(new Vector2Int(7, 7)).ToArray();
            Assert.AreEqual(6, paths.Length);
        }

        [Test]
        public void LimitedDistances()
        {
            var board = new Board(10, 10);
            var distanceMap = board.ComputeDistancesFrom(new Vector2Int(5, 5), 4.0);
            var edgesToPivot = distanceMap.ReversedEdges;
            Assert.AreEqual(6, distanceMap.SearchPaths(new Vector2Int(3, 3)).ToArray().Length);
            Assert.AreEqual(0, distanceMap.SearchPaths(new Vector2Int(7, 8)).ToArray().Length);
        }

        [Test]
        public void LimitedDistances2()
        {
            var board = new Board(10, 10);
            var distanceMap = new DistanceMap<Vector2Int>(board, new Vector2Int(5, 5), new Vector2Int(9, 6));
            Assert.AreNotEqual(0, distanceMap.SearchPaths(new Vector2Int(9, 6)).Count());
            Assert.AreEqual(0, distanceMap.SearchPaths(new Vector2Int(9, 7)).Count());
        }

        [Test]
        public void Path()
        {
            var graph = new Graph<char>();
            graph.CreateNode('a');
            graph.CreateNode('b');
            graph.CreateNode('c');
            graph.CreateNode('d');

            // a-b-d経路
            graph.DoubleOrderedLink('a', 'b', 1.0 / 3.0);
            graph.DoubleOrderedLink('b', 'd', 1.0 / 9.0);

            // a-c-d経路
            graph.DoubleOrderedLink('a', 'c', 1.0 / 3.0);
            graph.DoubleOrderedLink('c', 'd', 2.0 / 7.0);

            var distanceMap = new DistanceMap<char>(graph, 'a');
            var route = distanceMap.SearchPaths('d').ToArray();
            Assert.AreEqual(1, route.Length);
            Assert.AreEqual(3, route[0].Length);
            Assert.AreEqual('d', route[0][2]);
            Assert.AreEqual('b', route[0][1]);
            Assert.AreEqual('a', route[0][0]);
        }

        public void Performance()
        {
            var board = new Board(100, 1000);
            for(int i=0; i<board.Width; ++i)
            {
                for(int j=0; j<board.Height; ++j)
                {
                    var cost = (Mathf.PerlinNoise((float)i / 256f, (float)j / 256f) + 1f) * 10f + 1f;
                    Assert.Greater(cost, 0f);
                    board.SetCost(i, j, cost);
                }
            }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var distanceMap = new DistanceMap<Vector2Int>(board, new Vector2Int(5, 5));

            watch.Stop();
            Debug.Log(watch.ElapsedMilliseconds);

            watch.Restart();
            distanceMap.SearchPaths(new Vector2Int(99, 999)).Take(256).ToArray();
            watch.Stop();
            Debug.Log(watch.ElapsedMilliseconds);
        }
    }
}

