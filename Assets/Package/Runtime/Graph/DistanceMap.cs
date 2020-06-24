using System.Collections.Generic;
using System.Linq;

namespace TSKT
{
    public readonly struct DistanceMap<T>
    {
        public T Start { get; }
        public Dictionary<T, double> Distances { get; }
        public Dictionary<T, (T endNode, double weight)[]> Edges { get; }

        public DistanceMap(T start, Dictionary<T, double> distances, Dictionary<T, (T node, double distance)[]> edges)
        {
            Start = start;
            Distances = distances;
            Edges = edges;
        }

        public DistanceMap(IGraph<T> graph, T start, double maxDistance = double.PositiveInfinity)
            : this(graph, start, false, default, maxDistance)
        {
        }

        public DistanceMap(IGraph<T> graph, T start, T goal, double maxDistance = double.PositiveInfinity)
            : this(graph, start, true, goal, maxDistance)
        {
        }

        DistanceMap(IGraph<T> graph, T start, bool goalExists, T goal, double maxDistance = double.PositiveInfinity)
        {
            Start = start;
            Distances = new Dictionary<T, double>();
            Edges = new Dictionary<T, (T node, double distance)[]>();

            var tasks = new Queue<T>();
            tasks.Enqueue(start);

            Distances.Add(start, 0.0);

            while (tasks.Count > 0)
            {
                var currentNode = tasks.Dequeue();

                if (goalExists)
                {
                    if (Distances.TryGetValue(goal, out var startToGoalDistance))
                    {
                        if (maxDistance > startToGoalDistance)
                        {
                            maxDistance = startToGoalDistance;
                        }
                        if (Distances[currentNode] > startToGoalDistance)
                        {
                            continue;
                        }
                    }
                }

                if (!Edges.TryGetValue(currentNode, out var nexts))
                {
                    nexts = graph.GetEdgesFrom(currentNode)?.ToArray();
                    Edges.Add(currentNode, nexts);
                }

                if (nexts != null && nexts.Length > 0)
                {
                    var startToCurrentNodeDistance = Distances[currentNode];
                    foreach (var (nextNode, edgeWeight) in nexts)
                    {
                        UnityEngine.Debug.Assert(edgeWeight > 0.0, "weight must be greater than 0.0");

                        var startToNextNodeDistance = startToCurrentNodeDistance + edgeWeight;
                        if (startToNextNodeDistance <= maxDistance)
                        {
                            if (Distances.TryGetValue(nextNode, out var oldDistance))
                            {
                                if (oldDistance > startToNextNodeDistance)
                                {
                                    tasks.Enqueue(nextNode);
                                    Distances[nextNode] = startToNextNodeDistance;
                                }
                            }
                            else
                            {
                                tasks.Enqueue(nextNode);
                                Distances.Add(nextNode, startToNextNodeDistance);
                            }
                        }
                    }
                }
            }
        }

        public Dictionary<T, List<T>> ReversedEdges
        {
            get
            {
                var result = new Dictionary<T, List<T>>();
                foreach (var edge in Edges)
                {
                    if (edge.Value == null)
                    {
                        continue;
                    }
                    if (edge.Value.Length == 0)
                    {
                        continue;
                    }
                    var nearNode = edge.Key;
                    if (!Distances.TryGetValue(nearNode, out var nearNodeDistance))
                    {
                        continue;
                    }
                    foreach (var (farNode, weight) in edge.Value)
                    {
                        if (!Distances.TryGetValue(farNode, out var farNodeDistance))
                        {
                            continue;
                        }
                        if (nearNodeDistance + weight != farNodeDistance)
                        {
                            continue;
                        }

                        if (!result.TryGetValue(farNode, out var nexts))
                        {
                            nexts = new List<T>();
                            result.Add(farNode, nexts);
                        }
                        nexts.Add(nearNode);
                    }
                }
                return result;
            }
        }

        public IEnumerable<T[]> SearchPaths(T goal)
        {
            if (!Distances.ContainsKey(goal))
            {
                yield break;
            }

            var reversedEdge = ReversedEdges;
            var tasks = new Stack<T[]>();
            tasks.Push(new [] { goal });

            while (tasks.Count > 0)
            {
                var path = tasks.Pop();

                if (!reversedEdge.TryGetValue(path[0], out var nearNodes))
                {
                    yield return path;
                    continue;
                }
                foreach (var nearNode in nearNodes)
                {
                    var builder = new ArrayBuilder<T>(path.Length + 1);
                    builder.Add(nearNode);
                    foreach (var it in path)
                    {
                        builder.Add(it);
                    }
                    tasks.Push(builder.Array);
                }
            }
        }
    }
}
