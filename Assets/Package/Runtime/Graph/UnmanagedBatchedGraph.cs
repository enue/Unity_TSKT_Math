using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Buffers;
using System;
using Unity.Collections;
#nullable enable

namespace TSKT
{
    public class UnmanagedBatchedGraph<T> where T : unmanaged, IEquatable<T>
    {
        public class Batch
        {
            public T Root => DistanceMap.Start;
            public UnmanagedDistanceMap<T> DistanceMap { get; }
            public AStarSearch<Batch> BatchSearch { get;}

            public Batch(UnmanagedBatchedGraph<T> owner, T root, float radius)
            {
                DistanceMap = new UnmanagedDistanceMap<T>(owner.graph, root);
                DistanceMap.SolveWithin(radius);
                BatchSearch = owner.CreateAStarForBatch(this);
            }
        }

        class PathCombine : IDisposable
        {
            NativeList<T> combinedPath = new(Allocator.Temp);
            int fixedCount = 0;
            int writtenCount;

            public T[] ToArray()
            {
                return combinedPath.AsReadOnly().AsReadOnlySpan()[..writtenCount].ToArray();
            }
            public void Append(in ReadOnlySpan<T> path)
            {
                var nextFixedCount = combinedPath.Length;
                foreach (var it in path)
                {
                    var index = combinedPath.AsReadOnly().AsReadOnlySpan()[fixedCount..].IndexOf(it);
                    if (index >= 0)
                    {
                        writtenCount = index + 1 + fixedCount;
                        nextFixedCount = Mathf.Min(nextFixedCount, writtenCount);
                    }
                    else if (writtenCount == combinedPath.Length)
                    {
                        combinedPath.Add(it);
                        ++writtenCount;
                    }
                    else
                    {
                        combinedPath[writtenCount] = it;
                        ++writtenCount;
                    }
                }
                fixedCount = nextFixedCount;
            }

            public void Dispose()
            {
                combinedPath.Dispose();
            }
        }

        public readonly struct StartingPoint
        {
            readonly UnmanagedBatchedGraph<T> owner;
            readonly T[] startToFirstRoot;
            readonly Batch? firstBatch;
            readonly UnmanagedAStarSearch<T> aStar;

            public StartingPoint(UnmanagedBatchedGraph<T> owner, in T start)
            {
                this.owner = owner;
                aStar = owner.CreateAStar(start);

                if (owner.nodeBatchMap.TryGetValue(start, out firstBatch))
                {
                    startToFirstRoot = aStar.SearchPath(firstBatch.Root);
                }
                else
                {
                    startToFirstRoot = System.Array.Empty<T>();
                }
            }

            public readonly T[] To(T goal)
            {
                if (startToFirstRoot.Length == 0)
                {
                    return aStar.SearchPath(goal);
                }
                if (firstBatch == null)
                {
                    return aStar.SearchPath(goal);
                }
                if (!owner.nodeBatchMap.TryGetValue(goal, out var lastBatch))
                {
                    return aStar.SearchPath(goal);
                }

                var pathCombine = new PathCombine();
                pathCombine.Append(startToFirstRoot);
                owner.GetBatchToGoalPath(firstBatch, lastBatch, goal, ref pathCombine);

                var result = pathCombine.ToArray();
                pathCombine.Dispose();
                return result;
            }
        }

        public readonly Graph<Batch> batchGraph = new();
        public readonly Dictionary<T, Batch> nodeBatchMap = new();
        public readonly IUnmanagedGraph<T> graph;
        readonly System.Func<T, T, float> heuristicFunction;

        public UnmanagedBatchedGraph(IUnmanagedGraph<T> graph, in T startNode, float batchRadius, float batchEdgeLength, System.Func<T, T, float> heuristicFunction)
        {
            this.graph = graph;
            this.heuristicFunction = heuristicFunction;

            Span<(T, float)> buffer = stackalloc (T, float)[graph.MaxEdgeCountFromOneNode];
            var batches = new List<Batch>();
            var referenceCountMap = new NativeHashMap<T, int>(32, Allocator.Temp);
            using var taskFinishedNodes = new NativeHashSet<T>(32, Allocator.Temp);

            using var tasks = new Graphs.UnmanagedPriorityQueue<T>(32, Allocator.Temp);
            tasks.Enqueue(0f, 0f, startNode);
            while (tasks.Count > 0)
            {
                var root = tasks.Dequeue();

                if (taskFinishedNodes.Contains(root))
                {
                    continue;
                }
                taskFinishedNodes.Add(root);

                var foundUnknownEdge = false;
                graph.GetEdgesFrom(root, buffer, out var writtenCount);
                foreach (var (endNode, _) in buffer[..writtenCount])
                {
                    if (!nodeBatchMap.ContainsKey(endNode))
                    {
                        foundUnknownEdge = true;
                        break;
                    }
                }
                if (!foundUnknownEdge)
                {
                    continue;
                }

                var newBatch = new Batch(this, root, batchRadius);
                batches.Add(newBatch);
                nodeBatchMap[root] = newBatch;

                foreach (var it in newBatch.DistanceMap.Distances)
                {
                    var node = it.Key;
                    {
                        referenceCountMap.TryGetValue(node, out var value);
                        referenceCountMap[node] = value + 1;
                    }
                    if (nodeBatchMap.TryGetValue(node, out var currentBatch))
                    {
                        if (currentBatch == newBatch)
                        {
                            continue;
                        }

                        currentBatch.DistanceMap.Distances.TryGetValue(node, out var currentDistance);

                        if (currentDistance > it.Value)
                        {
                            nodeBatchMap[node] = newBatch;
                        }
                    }
                    else
                    {
                        nodeBatchMap.Add(node, newBatch);

                        if (!taskFinishedNodes.Contains(node))
                        {
                            referenceCountMap.TryGetValue(node, out var value);
                            tasks.Enqueue(-value, -heuristicFunction(root, node), node);
                        }
                    }
                }
                if (batchEdgeLength > batchRadius)
                {
                    newBatch.DistanceMap.SolveWithin(batchEdgeLength);
                }
            }
            referenceCountMap.Dispose();

            foreach (var start in batches)
            {
                foreach (var end in batches)
                {
                    if (start.DistanceMap.Distances.TryGetValue(end.Root, out var newDistance))
                    {
                        if (newDistance == 0.0)
                        {
                            continue;
                        }
                        if (batchGraph.TryGetWeight(start, end, out var currentDistance))
                        {
                            if (currentDistance > newDistance)
                            {
                                batchGraph.Link(start, end, newDistance);
                            }
                        }
                        else
                        {
                            batchGraph.Link(start, end, newDistance);
                        }
                    }
                }
            }

            var startNodeBatch = nodeBatchMap[startNode];
            Span<T> linkedBatches = stackalloc T[batches.Count];
            int linkedBatchesWrittenCount = 0;
            var unlinkedBatches = new List<Batch>();
            foreach (var it in batches)
            {
                if (it.BatchSearch.SearchNearestNode(new[] { startNodeBatch }, float.PositiveInfinity, out _))
                {
                    linkedBatches[linkedBatchesWrittenCount] = it.Root;
                    ++linkedBatchesWrittenCount;
                }
                else
                {
                    unlinkedBatches.Add(it);
                }
            }
            var _startNode = startNode;
            unlinkedBatches.Sort((x, y) =>
                heuristicFunction(_startNode, x.Root).CompareTo(heuristicFunction(_startNode, y.Root)));
            foreach (var it in unlinkedBatches)
            {
                if (it.DistanceMap.TrySolveAny(linkedBatches[..linkedBatchesWrittenCount], out var linkedBatch))
                {
                    batchGraph.Link(it, nodeBatchMap[linkedBatch], it.DistanceMap.Distances[linkedBatch]);
                    linkedBatches[linkedBatchesWrittenCount] = it.Root;
                    ++linkedBatchesWrittenCount;
                }
            }
        }

        UnmanagedAStarSearch<T> CreateAStar(in T start)
        {
            return new UnmanagedAStarSearch<T>(graph, start, heuristicFunction);
        }
        AStarSearch<Batch> CreateAStarForBatch(Batch start)
        {
            return batchGraph.CreateAStarSearch(start, (x, y) => heuristicFunction(x.Root, y.Root));
        }

        void GetBatchToGoalPath(Batch startBatch, Batch lastBatch, T goal, ref PathCombine pathCombine)
        {
            var path = startBatch.BatchSearch.SearchPath(lastBatch);

            for (int i = 0; i < path.Length; ++i)
            {
                var fromBatch = path[i];
                if (fromBatch.DistanceMap.Distances.ContainsKey(goal))
                {
                    var nodePath = fromBatch.DistanceMap.SearchPath(goal);
                    pathCombine.Append(nodePath);
                    break;
                }
                else
                {
                    var toBatch = path[i + 1];
                    var nodePath = fromBatch.DistanceMap.SearchPath(toBatch.Root);
                    pathCombine.Append(nodePath);
                }
            }
        }

        public StartingPoint From(in T start) => new(this, start);
    }
}
