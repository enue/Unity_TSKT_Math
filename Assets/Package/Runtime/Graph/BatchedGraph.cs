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

            public Batch(DistanceMap<T> distanceMap)
            {
                this.distanceMap = distanceMap;
            }
        }
        public readonly Graph<Batch> batchGraph = new Graph<Batch>();
        public readonly Dictionary<T, Batch> nodeBatchMap = new Dictionary<T, Batch>();
        public readonly IGraph<T> graph;

        public BatchedGraph(IGraph<T> graph, T startNode, double batchRadius)
        {
            this.graph = graph;

            var batches = new List<Batch>();
            var reversedGraph = new Graph<Batch>();
            var referenceCountMap = new IntDictionary<T>();

            var tasks = new Graphs.DoublePriorityQueue<T>();
            tasks.Enqueue(0.0, 0.0, startNode);
            while (tasks.Count > 0)
            {
                var root = tasks.Dequeue().item;

                var foundUnknownEdge = false;
                foreach (var (endNode, weight) in graph.GetEdgesFrom(root))
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

                        tasks.Enqueue(-referenceCountMap[node], -it.Value, node);
                    }
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


        IEnumerable<T> GetBatchToBatchPath(Batch start, Batch goal)
        {
            yield return start.Root;

            var batchDistance = new DistanceMap<Batch>(batchGraph, start, goal);
            var batchPath = batchDistance.SearchPaths(goal).First();
            for (int i = 1; i < batchPath.Length; ++i)
            {
                var fromBatch = batchPath[i - 1];
                var toBatch = batchPath[i];

                var toTransitPath = fromBatch.distanceMap.SearchPaths(toBatch.Root).First();
                foreach (var it in toTransitPath.Skip(1))
                {
                    yield return it;
                }
            }

        }

        public IEnumerable<T> GetPath(T start, T goal)
        {
            if (!nodeBatchMap.TryGetValue(goal, out var goalBatch))
            {
                yield break;
            }

            var roots = new HashSet<T>(batchGraph.StartingNodes.Select(_ => _.Root));
            var startToBatch = new DistanceMap<T>(graph, start, roots);
            var startBatchRoot = roots.First(_ => startToBatch.Distances.ContainsKey(_));
            var startBatch = nodeBatchMap[startBatchRoot];

            {
                var startToRoot = startToBatch.SearchPaths(startBatchRoot).First();
                foreach (var it in startToRoot)
                {
                    yield return it;
                }
            }

            foreach(var it in GetBatchToBatchPath(startBatch, goalBatch).Skip(1))
            {
                yield return it;
            }
            {
                var goalFromRoot = goalBatch.distanceMap.SearchPaths(goal).First().Skip(1);
                foreach (var it in goalFromRoot)
                {
                    yield return it;
                }
            }
        }
    }
}
