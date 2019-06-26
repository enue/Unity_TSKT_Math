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

            var routes = distanceMap.ComputeRoutesToPivotFrom(new Vector2Int(7, 7)).ToArray();
            Assert.AreEqual(6, routes.Length);
        }

        [Test]
        public void LimitedDistances()
        {
            var board = new Board(10, 10);
            var distanceMap = board.ComputeDistancesFrom(new Vector2Int(5, 5), 4.0);
            var edgesToPivot = distanceMap.EdgesToPivot;
            Assert.AreEqual(6, distanceMap.ComputeRoutesToPivotFrom(new Vector2Int(3, 3)).ToArray().Length);
            Assert.AreEqual(0, distanceMap.ComputeRoutesToPivotFrom(new Vector2Int(7, 8)).ToArray().Length);
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
            distanceMap.ComputeRoutesToPivotFrom(new Vector2Int(99, 999)).Take(256).ToArray();
            watch.Stop();
            Debug.Log(watch.ElapsedMilliseconds);
        }
    }
}

