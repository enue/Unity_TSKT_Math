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
    public class BatchedGraphU<T> where T : unmanaged, IEquatable<T>
    {
        public class Batch
        {
            public T Root => distanceMap.Start;
            public readonly DistanceMapU<T> distanceMap;
            public AStarSearch<Batch>? BatchSearch { get; set; }

            public Batch(DistanceMapU<T> distanceMap)
            {
                this.distanceMap = distanceMap;
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
            readonly BatchedGraphU<T> owner;
            readonly T[] startToFirstRoot;
            public readonly T start;
            readonly AStarSearchU<T> aStar;

            public StartingPoint(BatchedGraphU<T> owner, in T start)
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
                        Span<T> goals = stackalloc T[] { goal };
                        var distanceMap = new DistanceMapU<T>(owner.graph, start);
                        distanceMap.SolveAny(goals);
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

                var result = pathCombine.ToArray();
                pathCombine.Dispose();
                return result;
            }
        }

        public readonly Graph<Batch> batchGraph = new();
        public readonly Dictionary<T, Batch> nodeBatchMap = new();
        public readonly IGraphU<T> graph;
        public readonly System.Func<T, T, double>? heuristicFunction;

        public BatchedGraphU(IGraphU<T> graph, in T startNode, double batchRadius, double batchEdgeLength, System.Func<T, T, double>? heuristicFunction = null)
        {
            this.graph = graph;
            this.heuristicFunction = heuristicFunction;

            Span<(T, double)> buffer = stackalloc (T, double)[graph.MaxEdgeCount];
            var batches = new List<Batch>();
            var referenceCountMap = new Dictionary<T, int>();
            var taskFinishedNodes = new HashSet<T>();

            using var tasks = new Graphs.DoublePriorityQueueU<T>();
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

                var newBatch = new Batch(new DistanceMapU<T>(graph, root));
                newBatch.distanceMap.SolveWithin(batchRadius);
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

                        currentBatch.distanceMap.Distances.TryGetValue(node, out var currentDistance);

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
                    newBatch.distanceMap.SolveWithin(batchEdgeLength);
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
            Span<T> linkedBatches = stackalloc T[batches.Count];
            int linkedBatchesWrittenCount = 0;
            var unlinkedBatches = new List<Batch>();
            foreach (var it in batches)
            {
                var map = new DistanceMap<Batch>(batchGraph, it, startNodeBatch);
                if (map.Distances.ContainsKey(startNodeBatch))
                {
                    linkedBatches[linkedBatchesWrittenCount] = it.Root;
                    ++linkedBatchesWrittenCount;
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
                it.distanceMap.SolveAny(linkedBatches[..linkedBatchesWrittenCount]);
                var linked = false;
                foreach (var linkedBatch in linkedBatches[..linkedBatchesWrittenCount])
                {
                    if (it.distanceMap.Distances.TryGetValue(linkedBatch, out var distance))
                    {
                        batchGraph.Link(it, nodeBatchMap[linkedBatch], distance);
                        linked = true;
                    }
                }
                if (linked)
                {
                    linkedBatches[linkedBatchesWrittenCount] = it.Root;
                    ++linkedBatchesWrittenCount;
                }
            }
        }

        T[] SearchRootToNearestRoot(in T start, out AStarSearchU<T> aStar)
        {
            Span<T> roots = stackalloc T[batchGraph.StartingNodes.Count];
            var rootsWrittenCount = 0;
            foreach (var it in batchGraph.StartingNodes)
            {
                roots[rootsWrittenCount] = it.Root;
                ++rootsWrittenCount;
            }

            if (heuristicFunction == null)
            {
                aStar = default;
                var startToBatch = new DistanceMapU<T>(graph, start);
                startToBatch.SolveAny(roots);

                T firstRoot = default;
                var foundStartToFirstRootPath = false;
                foreach (var it in roots)
                {
                    if (startToBatch.Distances.TryGetValue(it, out _))
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
                aStar = new AStarSearchU<T>(graph, start, heuristicFunction);
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

                if (fromBatch.distanceMap.Distances.TryGetValue(goal, out _))
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

        public StartingPoint GetStartingPoint(in T start)
        {
            return new StartingPoint(this, start);
        }

        public T[] GetPath(in T start, in T goal)
        {
            return GetStartingPoint(start).GetPath(goal);
        }
    }
}
