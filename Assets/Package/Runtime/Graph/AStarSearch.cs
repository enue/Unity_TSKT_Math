using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT
{
    public readonly struct AStarSearch<T>
    {
        class PriorityQueue
        {
            public readonly struct Key
            {
                public class Comparer : IComparer<Key>
                {
                    public int Compare(Key x, Key y)
                    {
                        if (x.primary > y.primary)
                        {
                            return 1;
                        }
                        if (x.primary < y.primary)
                        {
                            return -1;
                        }
                        if (x.secondary > y.secondary)
                        {
                            return 1;
                        }
                        if (x.secondary < y.secondary)
                        {
                            return -1;
                        }
                        return 0;
                    }
                }

                public readonly double primary;
                public readonly double secondary;

                public Key(double primary, double secondary)
                {
                    this.primary = primary;
                    this.secondary = secondary;
                }
            }

            readonly List<T> items = new List<T>();
            readonly List<Key> keys = new List<Key>();
            readonly Key.Comparer keyComparer = new Key.Comparer();
            public int Count => keys.Count;

            public void Enqueue(double primaryKey, double secondaryKey, T item)
            {
                // Dequeueの時に末尾からとりたいのでキーをマイナスにしておく
                var key = new Key(-primaryKey, -secondaryKey);
                var index = keys.BinarySearch(key, keyComparer);
                if (index < 0)
                {
                    index = ~index;
                }
                items.Insert(index, item);
                keys.Insert(index, key);
            }

            public (double primaryKey, double secondaryKey, T item) Dequeue()
            {
                var index = items.Count - 1;
                var item = items[index];
                var key = keys[index];
                items.RemoveAt(index);
                keys.RemoveAt(index);
                return (-key.primary, -key.secondary, item);
            }
        }

        readonly IGraph<T> graph;
        readonly System.Func<T, T, double> heuristicFunction;
        public readonly DistanceMap<T> memo;
        public T Start => memo.Pivot;
        
        public AStarSearch(IGraph<T> graph, T start, System.Func<T, T, double> heuristicFunction, DistanceMap<T> memo = default)
        {
            this.heuristicFunction = heuristicFunction;
            this.graph = graph;

            if (memo.Distances == null)
            {
                this.memo = new DistanceMap<T>(
                    start,
                    new Dictionary<T, double>(),
                    new Dictionary<T, (T node, double distance)[]>());
                this.memo.Distances.Add(start, 0.0);
            }
            else
            {
                this.memo = memo;
            }
        }
        public Dictionary<T, double> FindPath(T goal)
        {
            return SearchPaths(goal, searchAllPaths: false).FirstOrDefault();
        }
        public IEnumerable<Dictionary<T, double>> FindAllPaths(T goal)
        {
            return SearchPaths(goal, searchAllPaths: true);
        }

        IEnumerable<Dictionary<T, double>> SearchPaths(T goal, bool searchAllPaths)
        {
            var distanceMap = new DistanceMap<T>(
                Start,
                new Dictionary<T, double>(memo.Distances),
                memo.Edges);

            var tasks = new PriorityQueue();

            foreach (var it in distanceMap.Distances)
            {
                var startToItDistance = it.Value;
                var expectedDistance = it.Value + heuristicFunction(it.Key, goal);
                tasks.Enqueue(expectedDistance, -startToItDistance, it.Key);
            }

            while (tasks.Count > 0)
            {
                var (expectedDistance, _, currentNode) = tasks.Dequeue();
                if (distanceMap.Distances.TryGetValue(goal, out var startToGoalDistance))
                {
                    if (searchAllPaths)
                    {
                        if (expectedDistance > startToGoalDistance)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (expectedDistance >= startToGoalDistance)
                        {
                            break;
                        }
                    }
                }

                if (!distanceMap.Edges.TryGetValue(currentNode, out var nexts))
                {
                    nexts = graph.GetNextNodeDistancesFrom(currentNode)?.ToArray();
                    distanceMap.Edges.Add(currentNode, nexts);
                }

                var startToCurrentNodeDistance = distanceMap.Distances[currentNode];

                foreach (var (next, edgeWeight) in nexts)
                {
                    var startToNextNodeDistance = edgeWeight + startToCurrentNodeDistance;

                    if (distanceMap.Distances.TryGetValue(next, out var oldDistance))
                    {
                        if (oldDistance <= startToNextNodeDistance)
                        {
                            continue;
                        }
                    }

                    distanceMap.Distances[next] = startToNextNodeDistance;

                    var nextExpectedDistance = heuristicFunction(next, goal) + startToNextNodeDistance;
                    // nextExpectedDistanceは昇順、startToNextNodeDistanceは降順で処理する
                    tasks.Enqueue(nextExpectedDistance, -startToNextNodeDistance, next);
                }
            }

            foreach (var reversedPath in distanceMap.ComputeRoutesToPivotFrom(goal))
            {
                var path = reversedPath.Reverse()
                    .ToDictionary(_ => _, _ => distanceMap.Distances[_]);

                // goalまでの経路は最適なので蓄積しておく
                foreach (var it in path)
                {
                    memo.Distances[it.Key] = it.Value;
                }

                yield return path;
            }
        }

        static public Dictionary<T, double> FindPath(IGraph<T> graph, T start, T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearch<T>(graph, start, heuristicFunction, default);
            return search.FindPath(goal);
        }
        static public IEnumerable<Dictionary<T, double>> FindAllPaths(IGraph<T> graph, T start, T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearch<T>(graph, start, heuristicFunction, default);
            return search.FindAllPaths(goal);
        }
    }
}
