using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
            public readonly List<T> combinedPath = new List<T>();
            int fixedCount = 0;

            public void Append(T[] path)
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

        public readonly Graph<Batch> batchGraph = new Graph<Batch>();
        public readonly Dictionary<T, Batch> nodeBatchMap = new Dictionary<T, Batch>();
        public readonly IGraph<T> graph;
        public readonly System.Func<T, T, double> heuristicFunction;

        public BatchedGraph(IGraph<T> graph, T startNode, double batchRadius, double batchEdgeLength, System.Func<T, T, double> heuristicFunction = null)
        {
            this.graph = graph;
            this.heuristicFunction = heuristicFunction;

            var batches = new List<Batch>();
            var reversedGraph = new Graph<Batch>();
            var referenceCountMap = new IntDictionary<T>();
            var taskFinishedNodes = new HashSet<T>();

            var tasks = new Graphs.DoublePriorityQueue<T>();
            tasks.Enqueue(0.0, 0.0, startNode);
            while (tasks.Count > 0)
            {
                var root = tasks.Dequeue().item;

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
                    ++referenceCountMap[node];
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
                                tasks.Enqueue(-referenceCountMap[node], -it.Value, node);
                            }
                            else
                            {
                                tasks.Enqueue(-referenceCountMap[node], -heuristicFunction(root, node), node);
                            }
                        }
                    }
                }
                if (batchEdgeLength > batchRadius)
                {
                    newBatch.distanceMap.Continue(null, batchEdgeLength);
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
                                reversedGraph.Link(end, start, newDistance);
                            }
                        }
                        else
                        {
                            batchGraph.Link(start, end, newDistance);
                            reversedGraph.Link(end, start, newDistance);
                        }
                    }
                }
            }

            var startNodeBatch = nodeBatchMap[startNode];
            var linkedBatches = new HashSet<T>();
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
            IEnumerable<Batch> sortedUnlinkcedBatches;
            if (heuristicFunction == null)
            {
                sortedUnlinkcedBatches = unlinkedBatches;
            }
            else
            {
                sortedUnlinkcedBatches = unlinkedBatches.OrderBy(_ => heuristicFunction(startNode, _.Root));
            }
            foreach (var it in sortedUnlinkcedBatches)
            {
                it.distanceMap.Continue(linkedBatches);
                var linked = false;
                foreach (var linkedBatch in linkedBatches)
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


        IEnumerable<T[]> GetBatchToGoalPath(Batch startBatch, Batch lastBatch, T goal)
        {
            Batch[] path;
            if (heuristicFunction == null)
            {
                var batchDistance = new DistanceMap<Batch>(batchGraph, startBatch, lastBatch);
                path = batchDistance.SearchPaths(lastBatch).First();
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
                    var nodePath = fromBatch.distanceMap.SearchPaths(goal).First();
                    yield return nodePath;
                    break;
                }
                else
                {
                    var toBatch = path[i + 1];
                    var nodePath = fromBatch.distanceMap.SearchPaths(toBatch.Root).First();
                    yield return nodePath;
                }
            }
        }

        public IEnumerable<T> GetPath(T start, T goal)
        {
            if (!nodeBatchMap.TryGetValue(goal, out var lastBatch))
            {
                if (heuristicFunction == null)
                {
                    var distanceMap = new DistanceMap<T>(graph, start, new HashSet<T>() { goal });
                    var path = distanceMap.SearchPaths(goal).FirstOrDefault();
                    if (path != null)
                    {
                        foreach (var it in path)
                        {
                            yield return it;
                        }
                    }
                }
                else
                {
                    var aStar = new AStarSearch<T>(graph, start, heuristicFunction);
                    var path = aStar.SearchPath(goal);
                    if (path != null)
                    {
                        foreach (var it in path)
                        {
                            yield return it;
                        }
                    }
                }

                yield break;
            }

            T firstRoot;
            T[] startToFirstRoot;
            if (heuristicFunction == null)
            {
                var roots = new HashSet<T>(batchGraph.StartingNodes.Select(_ => _.Root).Append(goal));
                var startToBatch = new DistanceMap<T>(graph, start, roots);

                firstRoot = default;
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
                    yield break;
                }

                startToFirstRoot = startToBatch.SearchPaths(firstRoot).First();
            }
            else
            {
                var roots = batchGraph.StartingNodes.Select(_ => _.Root).Append(goal).ToArray();
                var aStarSearch = new AStarSearch<T>(graph, start, heuristicFunction);
                startToFirstRoot = aStarSearch.SearchPath(roots);
                if (startToFirstRoot == null)
                {
                    yield break;
                }
                firstRoot = startToFirstRoot[startToFirstRoot.Length - 1];
            }

            if (!nodeBatchMap.TryGetValue(firstRoot, out var firstBatch)
                || (firstBatch.distanceMap.Distances[firstRoot] > 0.0))
            {
                foreach (var it in startToFirstRoot)
                {
                    yield return it;
                }
                yield break;
            }

            var pathCombine = new PathCombine();
            pathCombine.Append(startToFirstRoot);
            foreach (var it in GetBatchToGoalPath(firstBatch, lastBatch, goal))
            {
                pathCombine.Append(it);
            }
            foreach (var it in pathCombine.combinedPath)
            {
                yield return it;
            }
        }
    }
}
