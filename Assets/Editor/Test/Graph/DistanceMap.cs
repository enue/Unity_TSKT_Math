﻿#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Unity.PerformanceTesting;
using System;

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

            var paths = new Vector2Int[10][];
            distanceMap.SearchPaths(new Vector2Int(7, 7), paths, out var writtenCount);
            Assert.AreEqual(6, writtenCount);
        }

        [Test]
        public void LimitedDistances()
        {
            var board = new Board(10, 10);
            var distanceMap = board.ComputeDistancesFrom(new Vector2Int(5, 5), 4.0);
            var edgesToPivot = distanceMap.ReversedEdges;
            var paths = new Vector2Int[10][];
            distanceMap.SearchPaths(new Vector2Int(3, 3), paths, out var writtenCount);
            Assert.AreEqual(6, writtenCount);
            distanceMap.SearchPaths(new Vector2Int(7, 8), paths, out writtenCount);
            Assert.AreEqual(0, writtenCount);
        }

        [Test]
        public void LimitedDistances2()
        {
            var board = new Board(10, 10);
            var distanceMap = new DistanceMap<Vector2Int>(board, new Vector2Int(5, 5), new Vector2Int(9, 6));
            var paths = new Vector2Int[10][];
            distanceMap.SearchPaths(new Vector2Int(9, 6), paths, out var writtenCount);
            Assert.AreNotEqual(0, writtenCount);
            distanceMap.SearchPaths(new Vector2Int(9, 7), paths, out writtenCount);
            Assert.AreEqual(0, writtenCount);
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
            var paths = new Span<char[]>(new char[10][]);
            distanceMap.SearchPaths('d', paths, out var writtenCount);
            Assert.AreEqual(1, writtenCount);
            Assert.AreEqual(3, paths[0].Length);
            Assert.AreEqual('d', paths[0][2]);
            Assert.AreEqual('b', paths[0][1]);
            Assert.AreEqual('a', paths[0][0]);
        }

        [Test]
        public void RandomBoardPath()
        {
            var board = new Board(10, 10);
            for (int i = 0; i < board.Width; ++i)
            {
                for (int j = 0; j < board.Height; ++j)
                {
                    board.SetCost(i, j, UnityEngine.Random.Range(1f, 2f));
                }
            }
            var distance = board.ComputeDistancesFrom(new Vector2Int(0, 0));
            var paths = new Vector2Int[10][];
            distance.SearchPaths(new Vector2Int(9, 9), paths, out var writtenCount);

            Assert.AreEqual(1, writtenCount);
        }

        [Test]
        [Performance]
        public void Performance()
        {
            var board = new Board(100, 1000);
             for (int i=0; i<board.Width; ++i)
            {
                for(int j=0; j<board.Height; ++j)
                {
                    var cost = (Mathf.PerlinNoise((float)i / 256f, (float)j / 256f) + 1f) * 10f + 1f;
                    Assert.Greater(cost, 0f);
                    board.SetCost(i, j, cost);
                }
            }

            Measure.Method(() =>
            {
                var distanceMap = new DistanceMap<Vector2Int>(board, new Vector2Int(5, 5));
                var paths = new Vector2Int[256][];
                distanceMap.SearchPaths(new Vector2Int(99, 999), paths, out _);
            }).Run();
        }
    }
}

