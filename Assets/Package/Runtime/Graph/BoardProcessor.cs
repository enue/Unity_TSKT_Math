using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Linq;

#if TSKT_MATH_BURST_SUPPORT

namespace TSKT
{
    [BurstCompile]
    public struct BoardProcessor : IJob, IDisposable
    {
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
        readonly int start;
        [ReadOnly]
        readonly double maxDistance;

        NativeQueue<int> tasks;
        NativeArray<double> distances;

        public static DistanceMap<int> Calculate(int startNode, int nodeCount, IGraph<int> graph, double maxDistance)
        {
            using (var processor = new BoardProcessor(startNode, nodeCount, graph, maxDistance))
            {
                var jobHandle = processor.Schedule();
                jobHandle.Complete();

                return processor.Result;
            }
        }

        public BoardProcessor(int start, int nodeCount, IGraph<int> graph, double maxDistance)
        {
            int edgeCount = 0;
            for (int node = 0; node<nodeCount; ++node)
            {
                edgeCount += graph.GetEdgesFrom(node).Count();
            }

            edges = new NativeArray<Edge>(edgeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            {
                int index = 0;
                for (int node = 0; node < nodeCount; ++node)
                {
                    foreach(var (endNode, weight) in graph.GetEdgesFrom(node))
                    {
                        edges[index] = new Edge(node, endNode, weight);
                        ++index;
                    }
                }
            }

            tasks = new NativeQueue<int>(Allocator.TempJob);
            distances = new NativeArray<double>(edgeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            this.start = start;
            this.maxDistance = maxDistance;
        }

        void IJob.Execute()
        {
            for (int i = 0; i < distances.Length; ++i)
            {
                distances[i] = double.PositiveInfinity;
            }
            distances[start] = 0f;
            tasks.Enqueue(start);

            while (tasks.Count > 0)
            {
                var node = tasks.Dequeue();
                if (SearchEdges(node, out var index, out var count))
                {
                    for (int i = 0; i < count; ++i)
                    {
                        var edge = edges[i + index];
                        var newWeight = edge.weight + distances[node];
                        if (newWeight > maxDistance)
                        {
                            continue;
                        }

                        if (double.IsPositiveInfinity(distances[edge.end])
                            || distances[edge.end] > newWeight)
                        {
                            distances[edge.end] = newWeight;
                            tasks.Enqueue(edge.end);
                        }
                    }
                }
            }
        }

        DistanceMap<int> Result
        {
            get
            {
                var distanceMap = new Dictionary<int, double>(distances.Length);
                for (int i = 0; i < distances.Length; ++i)
                {
                    var value = distances[i];
                    if (!double.IsPositiveInfinity(value))
                    {
                        distanceMap.Add(i, value);
                    }
                }

                var reversedEdges = new Dictionary<int, HashSet<int>>();
                foreach (var it in edges)
                {
                    var startNodeDistance = distances[it.start];
                    var endNodeDistance = distances[it.end];
                    var weight = it.weight;

                    if (startNodeDistance + weight == endNodeDistance)
                    {
                        if (!reversedEdges.TryGetValue(it.end, out var startNodes))
                        {
                            startNodes = new HashSet<int>();
                            reversedEdges.Add(it.end, startNodes);
                        }
                        startNodes.Add(it.start);
                    }
                }

                return new DistanceMap<int>(
                    start: start,
                    distances: distanceMap,
                    reversedEdges: reversedEdges);
            }
        }

        void IDisposable.Dispose()
        {
            edges.Dispose();
            tasks.Dispose();
            distances.Dispose();
        }

        bool SearchEdges(int startNode, out int index, out int count)
        {
            int left = 0;
            int right = edges.Length;
            int mid = 0;

            while (left <= right)
            {
                mid = (left + right) / 2;
                if (edges[mid].start == startNode)
                {
                    index = 0;
                    var endIndex = edges.Length - 1;

                    for (int i = mid - 1; i >= 0; --i)
                    {
                        if (edges[i].start != startNode)
                        {
                            index = i + 1;
                            break;
                        }
                    }
                    for (int i = mid; i < edges.Length; ++i)
                    {
                        if (edges[i].start != startNode)
                        {
                            endIndex = i - 1;
                            break;
                        }
                    }
                    count = endIndex - index + 1;
                    return true;
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

            index = ~mid;
            count = 0;
            return false;
        }
    }
}

#endif
