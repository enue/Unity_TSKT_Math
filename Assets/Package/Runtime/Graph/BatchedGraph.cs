using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT
{
    public class BatchedGraph<T>
    {
        class PriorityQueue
        {
            readonly List<T> items = new List<T>();
            readonly List<double> keys = new List<double>();
            public int Count => keys.Count;

            public void Enqueue(double key, T item)
            {
                // Dequeueの時に末尾からとりたいのでキーをマイナスにしておく
                var index = keys.BinarySearch(-key);
                if (index < 0)
                {
                    index = ~index;
                }
                items.Insert(index, item);
                keys.Insert(index, -key);
            }

            public T Dequeue()
            {
                var index = items.Count - 1;
                var item = items[index];
                items.RemoveAt(index);
                keys.RemoveAt(index);
                return item;
            }
        }

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

                        if (currentDistance == 0.0)
                        {
                            // node is root
                            if (batchGraph.TryGetWeight(newBatch, currentBatch, out var currentDistanceBetweenBatches))
                            {
                                if (currentDistanceBetweenBatches > it.Value)
                                {
                                    batchGraph.DoubleOrderedLink(newBatch, currentBatch, it.Value);
                                }
                            }
                            else
                            {
                                batchGraph.DoubleOrderedLink(newBatch, currentBatch, it.Value);
                            }
                        }
                        else
                        {
                            if (currentDistance > it.Value)
                            {
                                nodeBatchMap[node] = newBatch;
                            }
                        }
                    }
                    else
                    {
                        nodeBatchMap.Add(node, newBatch);
                        // 距離は遠い順
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
                if (toBatch.distanceMap.Distances.ContainsKey(fromBatch.Root))
                {
                    var batchToBatch = toBatch.distanceMap.SearchPaths(fromBatch.Root).First().Reverse().Skip(1);
                    foreach (var it in batchToBatch)
                    {
                        yield return it;
                    }
                }
                else
                {
                    var batchToBatch = fromBatch.distanceMap.SearchPaths(toBatch.Root).First().Skip(1);
                    foreach (var it in batchToBatch)
                    {
                        yield return it;
                    }
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
