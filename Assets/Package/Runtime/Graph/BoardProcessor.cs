﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

#if TSKT_MATH_BURST_SUPPORT

namespace TSKT
{
    [BurstCompile]
    public struct BoardProcessor : IJob, IDisposable
    {
        struct Queue : IDisposable
        {
            NativeArray<int> array;
            int index;
            public int Count { get; private set; }
            public int Capacity => array.Length;

            public Queue(int capacity)
            {
                array = new NativeArray<int>(capacity, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                index = 0;
                Count = 0;
            }

            public void Enqueue(int value)
            {
                array[(index + Count) % array.Length] = value;
                ++Count;
            }
            public int Dequeue()
            {
                var result = array[index];
                index = (index + 1) % array.Length;
                --Count;
                return result;
            }

            public void Distinct()
            {
                var copyedCount = 0;

                for (int i=0; i<Count; ++i)
                {
                    var value = array[(index + i) % array.Length];

                    var found = false;
                    for (var j = 0; j < copyedCount; ++i)
                    {
                        if (value == array[(index + j) % array.Length])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        array[(index + copyedCount) % array.Length] = value;
                        ++copyedCount;
                    }
                }

                Count = copyedCount;
            }

            public void Dispose()
            {
                array.Dispose();
            }
        }

        readonly struct Edge
        {
            readonly public int start;
            readonly public int end;
            readonly public double weight;

            public Edge(int start, int end, double weight)
            {
                this.start = start;
                this.end = end;
                this.weight = weight;
            }
        }

        [ReadOnly]
        readonly NativeArray<Edge> edges;
        [ReadOnly]
        readonly int pivot;
        [ReadOnly]
        readonly double maxDistance;

        Queue tasks;
        NativeArray<double> distances;

        public static DistanceMap<Vector2Int> Calculate(Vector2Int startNode, List<Vector2Int> nodes, IGraph<Vector2Int> graph, double maxDistance)
        {
            var sortedCells = nodes.ToArray();
            Array.Sort(sortedCells, Vector2IntComparer.Comparison);

            var edges = new Dictionary<int, List<(int end, double weight)>>();
            for (var startNodeId = 0; startNodeId < sortedCells.Length; ++startNodeId)
            {
                List<(int, double)> weightMap = null;
                foreach (var (endNode, weight) in graph.GetEdgesFrom(sortedCells[startNodeId]))
                {
                    var endNodeId = Array.BinarySearch(sortedCells, endNode, Vector2IntComparer.Instance);
                    if (endNodeId >= 0)
                    {
                        if (weightMap == null)
                        {
                            if (!edges.TryGetValue(startNodeId, out weightMap))
                            {
                                weightMap = new List<(int, double)>();
                                edges.Add(startNodeId, weightMap);
                            }
                        }
                        weightMap.Add((endNodeId, weight));
                    }
                }
            }
            var pivotId = Array.BinarySearch(sortedCells, startNode, Vector2IntComparer.Instance);
            Debug.Assert(pivotId >= 0);

            return Calculate(pivotId, sortedCells, edges, maxDistance);
        }

        public static DistanceMap<T> Calculate<T>(
            int startNodeIndex,
            T[] nodes,
            Dictionary<int, List<(int endNodeIndex, double weight)>> edges,
            double maxDistance)
        {
            using (var processor = new BoardProcessor(startNodeIndex, edges, maxDistance))
            {
                var jobHandle = processor.Schedule();
                jobHandle.Complete();

                var distances = new Dictionary<T, double>(edges.Count);
                for (int i = 0; i < processor.distances.Length; ++i)
                {
                    var value = processor.distances[i];
                    if (!double.IsPositiveInfinity(value))
                    {
                        distances.Add(nodes[i], value);
                    }
                }

                var reversedEdges = new Dictionary<T, HashSet<T>>(edges.Count);
                foreach (var it in edges)
                {
                    var startNode = it.Key;
                    var startNodeDistance = processor.distances[startNode];
                    foreach (var (endNodeIndex, weight) in it.Value)
                    {
                        var endNodeDistance = processor.distances[endNodeIndex];
                        if (startNodeDistance + weight == endNodeDistance)
                        {
                            if (!reversedEdges.TryGetValue(nodes[endNodeIndex], out var startNodes))
                            {
                                startNodes = new HashSet<T>();
                                reversedEdges.Add(nodes[endNodeIndex], startNodes);
                            }
                            startNodes.Add(nodes[startNode]);
                        }
                    }
                }

                return new DistanceMap<T>(
                    start: nodes[startNodeIndex],
                    distances: distances,
                    reversedEdges: reversedEdges);
            }
        }

        public BoardProcessor(int pivot, Dictionary<int, List<(int to, double weight)>> edges, double maxDistance)
        {
            int edgeCount = 0;
            foreach (var it in edges)
            {
                edgeCount += it.Value.Count;
            }

            this.edges = new NativeArray<Edge>(edgeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            {
                int index = 0;
                foreach (var pair in edges)
                {
                    foreach (var (to, weight) in pair.Value)
                    {
                        this.edges[index] = new Edge(pair.Key, to, weight);
                        ++index;
                    }
                }
            }

            tasks = new Queue(edges.Count);
            distances = new NativeArray<double>(edges.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            this.pivot = pivot;
            this.maxDistance = maxDistance;
        }

        void IJob.Execute()
        {
            for (int i = 0; i < distances.Length; ++i)
            {
                distances[i] = double.PositiveInfinity;
            }
            distances[pivot] = 0f;
            tasks.Enqueue(pivot);

            while (tasks.Count > 0)
            {
                var node = tasks.Dequeue();

                var index = SearchEdges(node - 1);
                if (index < 0)
                {
                    index = ~index;
                }
                ++index;
                bool found = false;
                for (int i = index; i < edges.Length; ++i)
                {
                    var edge = edges[i];
                    if (edge.start == node)
                    {
                        found = true;
                        var newWeight = edge.weight + distances[node];
                        if (newWeight > maxDistance)
                        {
                            continue;
                        }

                        if (double.IsPositiveInfinity(distances[edge.end])
                            || distances[edge.end] > newWeight)
                        {
                            distances[edge.end] = newWeight;
                            if (tasks.Count >= tasks.Capacity)
                            {
                                tasks.Distinct();
                            }
                            tasks.Enqueue(edge.end);
                        }
                    }
                    else if (found)
                    {
                        break;
                    }
                }
            }
        }

        void IDisposable.Dispose()
        {
            edges.Dispose();
            tasks.Dispose();
            distances.Dispose();
        }

        int SearchEdges(int startNode)
        {
            int left = 0;
            int right = edges.Length;
            int mid = 0;

            while (left <= right)
            {
                mid = (left + right) / 2;
                if (edges[mid].start == startNode)
                {
                    return mid;
                }
                else if (edges[mid].start < startNode)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            return ~mid;

        }
    }
}

#endif