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
        public T Start => memo.Start;
        
        public AStarSearch(IGraph<T> graph, T start, System.Func<T, T, double> heuristicFunction, DistanceMap<T> memo = default)
        {
            this.heuristicFunction = heuristicFunction;
            this.graph = graph;

            if (memo.Distances == null)
            {
                this.memo = new DistanceMap<T>(
                    start,
                    new Dictionary<T, double>(),
                    new Dictionary<T, HashSet<T>>());
                this.memo.Distances.Add(start, 0.0);
            }
            else
            {
                this.memo = memo;
            }
        }
        public Dictionary<T, double> SearchPath(params T[] goals)
        {
            var containsAllGoals = true;
            var minDistance = double.PositiveInfinity;
            T nearestGoal = default;
            foreach (var goal in goals)
            {
                if (memo.Distances.TryGetValue(goal, out var distance))
                {
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        nearestGoal = goal;
                    }
                }
                else
                {
                    containsAllGoals = false;
                    break;
                }
            }
            if (containsAllGoals)
            {
                var distances = memo.Distances;
                return memo.SearchPaths(nearestGoal)
                    .First()
                    .ToDictionary(_ => _, _ => distances[_]);
            }

            return SearchPaths(goals, searchAllPaths: false).FirstOrDefault();
        }

        public IEnumerable<Dictionary<T, double>> SearchAllPaths(params T[] goals)
        {
            return SearchPaths(goals, searchAllPaths: true);
        }

        IEnumerable<Dictionary<T, double>> SearchPaths(T[] goals, bool searchAllPaths)
        {
            var cloneReversedEdges = new Dictionary<T, HashSet<T>>(memo.ReversedEdges.Count);
            foreach (var it in memo.ReversedEdges)
            {
                cloneReversedEdges.Add(it.Key, new HashSet<T>(it.Value));
            }

            var distanceMap = new DistanceMap<T>(
                Start,
                new Dictionary<T, double>(memo.Distances),
                cloneReversedEdges);

            var tasks = new PriorityQueue();

            foreach (var it in distanceMap.Distances)
            {
                var startToItDistance = it.Value;
                var h = heuristicFunction;
                var expectedDistance = startToItDistance + goals.Min(_ => h(it.Key, _));
                tasks.Enqueue(expectedDistance, -startToItDistance, it.Key);
            }

            while (tasks.Count > 0)
            {
                var (expectedDistance, _, currentNode) = tasks.Dequeue();
                {
                    bool shouldBreak = false;
                    foreach (var it in goals)
                    {
                        if (distanceMap.Distances.TryGetValue(it, out var startToGoalDistance))
                        {
                            if (searchAllPaths)
                            {
                                if (expectedDistance > startToGoalDistance)
                                {
                                    shouldBreak = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (expectedDistance >= startToGoalDistance)
                                {
                                    shouldBreak = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (shouldBreak)
                    {
                        break;
                    }
                }

                var startToCurrentNodeDistance = distanceMap.Distances[currentNode];

                foreach (var (next, edgeWeight) in graph.GetEdgesFrom(currentNode))
                {
                    var startToNextNodeDistance = edgeWeight + startToCurrentNodeDistance;

                    if (distanceMap.Distances.TryGetValue(next, out var oldDistance))
                    {
                        if (oldDistance >= startToNextNodeDistance)
                        {
                            var nearNodes = distanceMap.ReversedEdges[next];
                            if (oldDistance > startToNextNodeDistance)
                            {
                                nearNodes.Clear();
                            }
                            nearNodes.Add(currentNode);
                        }

                        if (oldDistance <= startToNextNodeDistance)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        var nearNodes = new HashSet<T>();
                        distanceMap.ReversedEdges.Add(next, nearNodes);
                        nearNodes.Add(currentNode);
                    }

                    distanceMap.Distances[next] = startToNextNodeDistance;

                    var h = heuristicFunction;
                    var nextExpectedDistance = goals.Min(_ => h(next, _)) + startToNextNodeDistance;
                    // nextExpectedDistanceは昇順、startToNextNodeDistanceは降順で処理する
                    tasks.Enqueue(nextExpectedDistance, -startToNextNodeDistance, next);
                }
            }

            foreach (var goal in goals)
            {
                foreach (var path in distanceMap.SearchPaths(goal))
                {
                    var result = path.ToDictionary(_ => _, _ => distanceMap.Distances[_]);

                    // goalまでの経路は最適なので蓄積しておく
                    foreach (var it in result)
                    {
                        UnityEngine.Assertions.Assert.IsTrue(!memo.Distances.TryGetValue(it.Key, out var value) || value >= it.Value);
                        memo.Distances[it.Key] = it.Value;
                    }

                    for (int i = 1; i < path.Length; ++i)
                    {
                        var nearNode = path[i - 1];
                        var farNode = path[i];
                        UnityEngine.Assertions.Assert.IsTrue(memo.Distances[nearNode] <= memo.Distances[farNode]);

                        if (memo.ReversedEdges.TryGetValue(farNode, out var nearNodes))
                        {
                            nearNodes.Add(nearNode);
                        }
                        else
                        {
                            nearNodes = new HashSet<T>();
                            memo.ReversedEdges.Add(farNode, nearNodes);
                            nearNodes.Add(nearNode);
                        }
                    }

                    yield return result;
                }
            }
        }

        static public Dictionary<T, double> SearchPath(IGraph<T> graph, T start, T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearch<T>(graph, start, heuristicFunction, default);
            return search.SearchPath(goal);
        }
        static public IEnumerable<Dictionary<T, double>> SearchAllPaths(IGraph<T> graph, T start, T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearch<T>(graph, start, heuristicFunction, default);
            return search.SearchAllPaths(goal);
        }
    }
}
