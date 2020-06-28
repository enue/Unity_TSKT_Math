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
        public readonly Dictionary<Batch, Dictionary<Batch, T>> transitMap = new Dictionary<Batch, Dictionary<Batch, T>>();

        public BatchedGraph(IGraph<T> graph, T startNode, double batchRadius)
        {
            var tasks = new PriorityQueue();
            tasks.Enqueue(0.0, startNode);
            while (tasks.Count > 0)
            {
                var root = tasks.Dequeue();

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
                nodeBatchMap[root] = newBatch;

                foreach (var it in newBatch.distanceMap.Distances)
                {
                    var node = it.Key;
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

                        var newDistanceBetweenBatches = currentDistance + it.Value;
                        if (batchGraph.TryGetWeight(newBatch, currentBatch, out var currentDistanceBetweenBatches))
                        {
                            if (currentDistanceBetweenBatches > newDistanceBetweenBatches)
                            {
                                batchGraph.DoubleOrderedLink(newBatch, currentBatch, newDistanceBetweenBatches);

                                if (!transitMap.TryGetValue(newBatch, out var newToCurentTransits))
                                {
                                    newToCurentTransits = new Dictionary<Batch, T>();
                                    transitMap.Add(newBatch, newToCurentTransits);
                                }
                                newToCurentTransits[currentBatch] = node;

                                if (!transitMap.TryGetValue(currentBatch, out var currentToNewTransits))
                                {
                                    currentToNewTransits = new Dictionary<Batch, T>();
                                    transitMap.Add(currentBatch, currentToNewTransits);
                                }
                                currentToNewTransits[newBatch] = node;
                            }
                        }
                        else
                        {
                            batchGraph.DoubleOrderedLink(newBatch, currentBatch, newDistanceBetweenBatches);

                            if (!transitMap.TryGetValue(newBatch, out var newToCurentTransits))
                            {
                                newToCurentTransits = new Dictionary<Batch, T>();
                                transitMap.Add(newBatch, newToCurentTransits);
                            }
                            newToCurentTransits[currentBatch] = node;

                            if (!transitMap.TryGetValue(currentBatch, out var currentToNewTransits))
                            {
                                currentToNewTransits = new Dictionary<Batch, T>();
                                transitMap.Add(currentBatch, currentToNewTransits);
                            }
                            currentToNewTransits[newBatch] = node;
                        }
                    }
                    else
                    {
                        nodeBatchMap.Add(node, newBatch);
                        tasks.Enqueue(-it.Value, node);
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

                var transit = transitMap[fromBatch][toBatch];

                var toTransitPath = fromBatch.distanceMap.SearchPaths(transit).First();
                foreach (var it in toTransitPath.Skip(1))
                {
                    yield return it;
                }

                var fromTransitPath = toBatch.distanceMap.SearchPaths(transit).First().Reverse();
                foreach (var it in fromTransitPath.Skip(1))
                {
                    yield return it;
                }
            }

        }

        public IEnumerable<T> GetPath(T start, T goal)
        {
            if (!nodeBatchMap.TryGetValue(start, out var startBatch))
            {
                yield break;
            }
            if (!nodeBatchMap.TryGetValue(goal, out var goalBatch))
            {
                yield break;
            }

            {
                var startToRoot = startBatch.distanceMap.SearchPaths(start).First().Reverse();
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
