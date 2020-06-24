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

        readonly DistanceMap<T> distanceMap;
        public T Start => distanceMap.Pivot;
        public Dictionary<T, double> Distances => distanceMap.Distances;
        public Dictionary<T, (T node, double distance)[]> Edges => distanceMap.Edges;

        public T Goal { get; }

        public AStarSearch(IGraph<T> graph, T start, T goal, System.Func<T, T, double> heuristicFunction)
        {
            Goal = goal;
            distanceMap = new DistanceMap<T>(
                start,
                new Dictionary<T, double>(),
                new Dictionary<T, (T node, double distance)[]>());
            Distances.Add(start, 0.0);

            var tasks = new PriorityQueue();
            tasks.Enqueue(heuristicFunction(start, goal), 0.0, start);

            while (tasks.Count > 0)
            {
                var (expectedDistance, _, currentNode) = tasks.Dequeue();
                if (Distances.TryGetValue(Goal, out var startToGoalDistance))
                {
                    if (expectedDistance >= startToGoalDistance)
                    {
                        break;
                    }
                }

                if (!Edges.TryGetValue(currentNode, out var nexts))
                {
                    nexts = graph.GetNextNodeDistancesFrom(currentNode)?.ToArray();
                    Edges.Add(currentNode, nexts);
                }

                var startToCurrentNodeDistance = Distances[currentNode];

                foreach (var (next, edgeWeight) in nexts)
                {
                    var startToNextNodeDistance = edgeWeight + startToCurrentNodeDistance;

                    if (Distances.TryGetValue(next, out var oldDistance))
                    {
                        if (oldDistance <= startToNextNodeDistance)
                        {
                            continue;
                        }
                    }

                    Distances[next] = startToNextNodeDistance;

                    var nextExpectedDistance = heuristicFunction(next, goal) + startToNextNodeDistance;
                    // nextExpectedDistanceは昇順、startToNextNodeDistanceは降順で処理する
                    tasks.Enqueue(nextExpectedDistance, -startToNextNodeDistance, next);
                }
            }
        }

        public Dictionary<T, List<T>> EdgesToStart => distanceMap.EdgesToPivot;
        public T[] ComputePathFromGoalToStart() => distanceMap.ComputeRoutesToPivotFrom(Goal).FirstOrDefault();
        public bool FoundPath => Distances.ContainsKey(Goal);

        static public Dictionary<T, double> FindPath(IGraph<T> graph, T start, T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearch<T>(graph, start, goal, heuristicFunction);
            if (!search.FoundPath)
            {
                return null;
            }

            return search.ComputePathFromGoalToStart()
                .Reverse()
                .ToDictionary(_ => _, _ => search.Distances[_]);
        }
    }
}
