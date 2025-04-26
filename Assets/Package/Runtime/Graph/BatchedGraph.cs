using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Buffers;
using System;
#nullable enable

namespace TSKT
{
    public class BatchedGraph<T>
    {
        public class Batch
        {
            public T Root => DistanceMap.Start;
            public DistanceMap<T> DistanceMap { get; }
            public AStarSearch<Batch> BatchSearch { get;}

            public Batch(BatchedGraph<T> owner, T root, float radius)
            {
                DistanceMap = new DistanceMap<T>(owner.graph, root);
                DistanceMap.SolveWithin(radius);
                BatchSearch = owner.CreateAStarForBatch(this);
            }
        }

        class PathCombine
        {
            readonly List<T> combinedPath = new();
            int fixedCount = 0;
            int writtenCount;

            public T[] ToArray()
            {
                var result = new T[writtenCount];
                combinedPath.CopyTo(0, result, 0, writtenCount);
                return result;
            }
            public void Append(in ReadOnlySpan<T> path)
            {
                var nextFixedCount = combinedPath.Count;
                foreach (var it in path)
                {
                    var index = combinedPath.IndexOf(it, fixedCount);
                    if (index >= 0)
                    {
                        writtenCount = index + 1;
                        nextFixedCount = Mathf.Min(nextFixedCount, index + 1);
                    }
                    else if (writtenCount == combinedPath.Count)
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
        }

        public readonly struct StartingPoint
        {
            readonly BatchedGraph<T> owner;
            readonly T[] startToFirstRoot;
            readonly Batch? firstBatch;
            readonly AStarSearch<T> aStar;
            public StartingPoint(BatchedGraph<T> owner, in T start)
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

                return pathCombine.ToArray();
            }
        }

        public readonly Graph<Batch> batchGraph = new();
        public readonly Dictionary<T, Batch> nodeBatchMap = new();
        public readonly IGraph<T> graph;
        public readonly System.Func<T, T, float> heuristicFunction;

        public BatchedGraph(IGraph<T> graph, in T startNode, float batchRadius, float batchEdgeLength, System.Func<T, T, float> heuristicFunction)
        {
            this.graph = graph;
            this.heuristicFunction = heuristicFunction;

            var batches = new List<Batch>();
            var referenceCountMap = new Dictionary<T, int>();
            var taskFinishedNodes = new HashSet<T>();

            var tasks = new Graphs.PriorityQueue<T>();
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
                foreach (var (endNode, _) in graph.GetEdgesFrom(root))
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

                        var currentDistance = currentBatch.DistanceMap.Distances[node];

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
            using var buffer = MemoryPool<T>.Shared.Rent(batches.Count);
            var linkedBatches = new Math.MemoryBuilder<T>(buffer.Memory);
            var unlinkedBatches = new List<Batch>();
            foreach (var it in batches)
            {
                if (it.BatchSearch.AnyPath(startNodeBatch))
                {
                    linkedBatches.Add(it.Root);
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
                foreach (var linkedBatch in linkedBatches.Memory.Span)
                {
                    if (it.DistanceMap.AnyPath(linkedBatch))
                    {
                        batchGraph.Link(it, nodeBatchMap[linkedBatch], it.DistanceMap.Distances[linkedBatch]);
                        linkedBatches.Add(it.Root);
                        break;
                    }
                }
            }
        }

        AStarSearch<T> CreateAStar(in T start)
        {
            return new AStarSearch<T>(graph, start, heuristicFunction);
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
