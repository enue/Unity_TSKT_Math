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

            foreach (var it in batches)
            {
                // 自分に枝を向けているノードに対して双方向になるまで検索範囲を伸ばす。
                // ただし完全に一方通行なノードだった場合は全検索になってしまうので、うまく連結グラフを成立させるよう修正する必要がある
                var requiredGoals = new HashSet<T>(reversedGraph.GetEdgesFrom(it).Select(_ => _.endNode.Root));
                requiredGoals.ExceptWith(batchGraph.GetEdgesFrom(it).Select(_ => _.endNode.Root));

                while (requiredGoals.Count > 0)
                {
                    if (it.distanceMap.Finished)
                    {
                        break;
                    }

                    it.distanceMap.Continue(requiredGoals);
                    foreach (var batch in batches)
                    {
                        if (it.distanceMap.Distances.TryGetValue(batch.Root, out var distance))
                        {
                            if (distance != 0.0)
                            {
                                batchGraph.Link(it, batch, distance);
                                requiredGoals.Remove(batch.Root);
                            }
                        }
                    }
                }
            }
        }


        IEnumerable<T> GetBatchToGoalPath(Batch startBatch, Batch lastBatch, T goal)
        {
            yield return startBatch.Root;

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
                    foreach (var it in nodePath.Skip(1))
                    {
                        yield return it;
                    }
                    break;
                }
                else
                {
                    var toBatch = path[i + 1];
                    var nodePath = fromBatch.distanceMap.SearchPaths(toBatch.Root).First();
                    foreach (var it in nodePath.Skip(1))
                    {
                        yield return it;
                    }
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

            foreach (var it in startToFirstRoot)
            {
                yield return it;
            }
            foreach (var it in GetBatchToGoalPath(firstBatch, lastBatch, goal).Skip(1))
            {
                yield return it;
            }
        }
    }
}
