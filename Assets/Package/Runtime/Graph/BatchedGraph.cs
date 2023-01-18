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
            public T Root => distanceMap.Start;
            readonly public DistanceMap<T> distanceMap;
            public AStarSearch<Batch>? BatchSearch { get; set; }

            public Batch(DistanceMap<T> distanceMap)
            {
                this.distanceMap = distanceMap;
            }
        }

        class PathCombine
        {
            public readonly List<T> combinedPath = new();
            int fixedCount = 0;

            public void Append(ReadOnlySpan<T> path)
            {
                var nextFixedCount = combinedPath.Count;
                foreach (var it in path)
                {
                    var index = combinedPath.IndexOf(it, fixedCount);
                    if (index >= 0)
                    {
                        combinedPath.RemoveRange(index + 1, combinedPath.Count - index - 1);
                        nextFixedCount = Mathf.Min(nextFixedCount, index + 1);
                    }
                    else
                    {
                        combinedPath.Add(it);
                    }
                }
                fixedCount = nextFixedCount;
            }
        }

        public readonly struct StartintPoint
        {
            readonly BatchedGraph<T> owner;
            readonly T[] startToFirstRoot;
            public readonly T start;
            readonly AStarSearch<T> aStar;

            public StartintPoint(BatchedGraph<T> owner, in T start)
            {
                this.start = start;
                this.owner = owner;

                startToFirstRoot = owner.SearchRootToNearestRoot(start, out aStar);
            }

            public readonly T[] GetPath(T goal)
            {
                if (!owner.nodeBatchMap.TryGetValue(goal, out var lastBatch))
                {
                    if (owner.heuristicFunction == null)
                    {
                        var distanceMap = new DistanceMap<T>(owner.graph, start, new T[] { goal });
                        var path = distanceMap.SearchPath(goal);
                        return path;
                    }
                    else
                    {
                        var path = aStar.SearchPath(goal);
                        if (path != null)
                        {
                            return path;
                        }
                    }

                    return System.Array.Empty<T>();
                }

                if (startToFirstRoot.Length == 0)
                {
                    return System.Array.Empty<T>();
                }

                var firstRoot = startToFirstRoot[^1];
                owner.nodeBatchMap.TryGetValue(firstRoot, out var firstBatch);
                var pathCombine = new PathCombine();
                pathCombine.Append(startToFirstRoot);
                owner.GetBatchToGoalPath(firstBatch, lastBatch, goal, ref pathCombine);

                return pathCombine.combinedPath.ToArray();
            }
        }

        public readonly Graph<Batch> batchGraph = new();
        public readonly Dictionary<T, Batch> nodeBatchMap = new();
        public readonly IGraph<T> graph;
        public readonly System.Func<T, T, double>? heuristicFunction;

        public BatchedGraph(IGraph<T> graph, in T startNode, double batchRadius, double batchEdgeLength, System.Func<T, T, double>? heuristicFunction = null)
        {
            this.graph = graph;
            this.heuristicFunction = heuristicFunction;

            var batches = new List<Batch>();
            var referenceCountMap = new Dictionary<T, int>();
            var taskFinishedNodes = new HashSet<T>();

            var tasks = new Graphs.DoublePriorityQueue<T>();
            tasks.Enqueue(0.0, 0.0, startNode);
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

                var newBatch = new Batch(new DistanceMap<T>(graph, root, batchRadius));

                batches.Add(newBatch);
                nodeBatchMap[root] = newBatch;

                foreach (var it in newBatch.distanceMap.Distances)
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

                        var currentDistance = currentBatch.distanceMap.Distances[node];

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
                            if (heuristicFunction == null)
                            {
                                referenceCountMap.TryGetValue(node, out var value);
                                tasks.Enqueue(-value, -it.Value, node);
                            }
                            else
                            {
                                referenceCountMap.TryGetValue(node, out var value);
                                tasks.Enqueue(-value, -heuristicFunction(root, node), node);
                            }
                        }
                    }
                }
                if (batchEdgeLength > batchRadius)
                {
                    newBatch.distanceMap.Solve(null, batchEdgeLength);
                }
            }

            foreach (var start in batches)
            {
                foreach (var end in batches)
                {
                    if (start.distanceMap.Distances.TryGetValue(end.Root, out var newDistance))
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
                var map = new DistanceMap<Batch>(batchGraph, it, startNodeBatch);
                if (map.Distances.ContainsKey(startNodeBatch))
                {
                    linkedBatches.Add(it.Root);
                }
                else
                {
                    unlinkedBatches.Add(it);
                }
            }
            if (heuristicFunction != null)
            {
                var _startNode = startNode;
                unlinkedBatches.Sort((x, y) =>
                    heuristicFunction(_startNode, x.Root).CompareTo(heuristicFunction(_startNode, y.Root)));
            }
            foreach (var it in unlinkedBatches)
            {
                it.distanceMap.Solve(linkedBatches.Memory.Span);
                var linked = false;
                foreach (var linkedBatch in linkedBatches.Memory.Span)
                {
                    if (it.distanceMap.Distances.TryGetValue(linkedBatch, out var distance))
                    {
                        batchGraph.Link(it, nodeBatchMap[linkedBatch], distance);
                        linked = true;
                    }
                }
                if (linked)
                {
                    linkedBatches.Add(it.Root);
                }
            }
        }

        T[] SearchRootToNearestRoot(in T start, out AStarSearch<T> aStar)
        {
            using var buffer = MemoryPool<T>.Shared.Rent(batchGraph.StartingNodes.Count);
            var rootsBuilder = new Math.MemoryBuilder<T>(buffer.Memory);
            ReadOnlySpan<T> roots;
            {
                foreach (var it in batchGraph.StartingNodes)
                {
                    rootsBuilder.Add(it.Root);
                }
                roots = rootsBuilder.Memory.Span;
            }

            if (heuristicFunction == null)
            {
                aStar = default;
                var startToBatch = new DistanceMap<T>(graph, start, roots);

                T? firstRoot = default;
                var foundStartToFirstRootPath = false;
                foreach (var it in roots)
                {
                    if (startToBatch.Distances.ContainsKey(it))
                    {
                        firstRoot = it;
                        foundStartToFirstRootPath = true;
                        break;
                    }
                }
                if (!foundStartToFirstRootPath)
                {
                    return System.Array.Empty<T>();
                }
                
                return startToBatch.SearchPath(firstRoot!);
            }
            else
            {
                aStar = new AStarSearch<T>(graph, start, heuristicFunction);
                return aStar.SearchPathToNearestGoal(roots);
            }
        }

        void GetBatchToGoalPath(Batch startBatch, Batch lastBatch, T goal, ref PathCombine pathCombine)
        {
            Batch[] path;
            if (heuristicFunction == null)
            {
                var batchDistance = new DistanceMap<Batch>(batchGraph, startBatch, lastBatch);
                path = batchDistance.SearchPath(lastBatch);
            }
            else
            {
                if (!startBatch.BatchSearch.HasValue)
                {
                    startBatch.BatchSearch = new AStarSearch<Batch>(batchGraph, startBatch, (x, y) => heuristicFunction(x.Root, y.Root));
                }
                path = startBatch.BatchSearch.Value.SearchPath(lastBatch);
            }

            for (int i = 0; i < path.Length; ++i)
            {
                var fromBatch = path[i];

                if (fromBatch.distanceMap.Distances.ContainsKey(goal))
                {
                    var nodePath = fromBatch.distanceMap.SearchPath(goal);
                    pathCombine.Append(nodePath);
                    break;
                }
                else
                {
                    var toBatch = path[i + 1];
                    var nodePath = fromBatch.distanceMap.SearchPath(toBatch.Root);
                    pathCombine.Append(nodePath);
                }
            }
        }

        public StartintPoint GetStartintPoint(in T start)
        {
            return new StartintPoint(this, start);
        }

        public T[] GetPath(in T start, in T goal)
        {
            return GetStartintPoint(start).GetPath(goal);
        }
    }
}
