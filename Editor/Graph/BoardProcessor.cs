using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class BoardProcessor
    {
        [Test]
        public void Test()
        {
            var board = new Board(10, 10);
            for (int i = 0; i < board.Width; ++i)
            {
                for (int j = 0; j < board.Height; ++j)
                {
                    var cost = Random.Range(1f, 100f);
                    board.SetCost(i, j, cost);
                }
            }
            var pivot = new Vector2Int(5, 5);
            var distanceMapWithBurst = board.ComputeDistancesWithBurst(pivot);
            var distanceMapWithoutBurst = board.ComputeDistancesFrom(pivot);

            Assert.AreEqual(distanceMapWithoutBurst.Distances.Count, distanceMapWithBurst.Distances.Count);
            foreach (var it in distanceMapWithBurst.Distances)
            {
                Assert.AreEqual(distanceMapWithoutBurst.Distances[it.Key], it.Value);
            }

            Assert.AreEqual(distanceMapWithoutBurst.Pivot, distanceMapWithBurst.Pivot);

            Assert.AreEqual(distanceMapWithoutBurst.Edges.Count, distanceMapWithBurst.Edges.Count);

            foreach (var it in distanceMapWithBurst.Edges)
            {
                Assert.AreEqual(distanceMapWithoutBurst.Edges[it.Key], it.Value);
            }
        }

        [Test]
        public void Performance()
        {
            var board = new Board(200, 200);
            for(int i=0; i<board.Width; ++i)
            {
                for(int j=0; j<board.Height; ++j)
                {
                    var cost = Random.Range(1f, 100f);
                    board.SetCost(i, j, cost);
                }
            }

            var pivot = new Vector2Int(5, 5);

            var watch = new System.Diagnostics.Stopwatch();

            watch.Restart();
            var distanceMapWithBurst = board.ComputeDistancesWithBurst(pivot);
            watch.Stop();
            var elapsedTimeWithBurst = watch.ElapsedMilliseconds;

            watch.Restart();
            var distanceMapWithoutBurst = board.ComputeDistancesFrom(pivot);
            watch.Stop();
            var elapsedTimeWithoutBurst = watch.ElapsedMilliseconds;

            Debug.Log("with burst :" + elapsedTimeWithBurst);
            Debug.Log("without burst : " + elapsedTimeWithoutBurst);

        }
    }
}

