using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

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
            readonly public int from;
            readonly public int to;
            readonly public double weight;

            public Edge(int from, int to, double weight)
            {
                this.from = from;
                this.to = to;
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

        public static DistanceMap<Vector2Int> Calculate(Vector2Int pivot, List<Vector2Int> allNodes, IGraph<Vector2Int> graph, double maxDistance)
        {
            var sortedCells = allNodes.ToArray();
            Array.Sort(sortedCells, Vector2IntUtil.Compare);

            var edges = new Dictionary<int, List<(int to, double weight)>>();
            var comparer = new Vector2IntUtil.Comparer();
            for (int fromId = 0; fromId < sortedCells.Length; ++fromId)
            {
                List<(int, double)> toWeightMap = null;
                foreach (var (next, weight) in graph.GetNextNodeDistancesFrom(sortedCells[fromId]))
                {
                    var nextId = Array.BinarySearch(sortedCells, next, comparer);
                    if (nextId >= 0)
                    {
                        if (toWeightMap == null)
                        {
                            if (!edges.TryGetValue(fromId, out toWeightMap))
                            {
                                toWeightMap = new List<(int, double)>();
                                edges.Add(fromId, toWeightMap);
                            }
                        }
                        toWeightMap.Add((nextId, weight));
                    }
                }
            }
            var pivotId = Array.BinarySearch(sortedCells, pivot, comparer);
            Debug.Assert(pivotId >= 0);

            return Calculate(pivotId, sortedCells, edges, maxDistance);
        }

        public static DistanceMap<T> Calculate<T>(
            int pivotIndex,
            T[] nodes,
            Dictionary<int, List<(int to, double weight)>> edges,
            double maxDistance)
        {
            using (var processor = new BoardProcessor(pivotIndex, edges, maxDistance))
            {
                var jobHandle = processor.Schedule();

                // jobが回っている間に処理する
                var finalEdges = new Dictionary<T, (T, double)[]>(edges.Count);
                foreach (var it in edges)
                {
                    var builder = new ArrayBuilder<(T, double)>(it.Value.Count);
                    foreach (var (to, weight) in it.Value)
                    {
                        builder.Add((nodes[to], weight));
                    }

                    finalEdges.Add(nodes[it.Key], builder.Array);
                }

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

                return new DistanceMap<T>(
                    pivot: nodes[pivotIndex],
                    distances: distances,
                    edges: finalEdges);
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
                    if (edge.from == node)
                    {
                        found = true;
                        var newWeight = edge.weight + distances[node];
                        if (newWeight > maxDistance)
                        {
                            continue;
                        }

                        if (double.IsPositiveInfinity(distances[edge.to])
                            || distances[edge.to] > newWeight)
                        {
                            distances[edge.to] = newWeight;
                            if (tasks.Count >= tasks.Capacity)
                            {
                                tasks.Distinct();
                            }
                            tasks.Enqueue(edge.to);
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

        int SearchEdges(int fromValue)
        {
            int left = 0;
            int right = edges.Length;
            int mid = 0;

            while (left <= right)
            {
                mid = (left + right) / 2;
                if (edges[mid].from == fromValue)
                {
                    return mid;
                }
                else if (edges[mid].from < fromValue)
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
