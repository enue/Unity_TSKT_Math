using System.Collections.Generic;
using System.Linq;

namespace TSKT
{
    public readonly struct DistanceMap<T>
    {
        public T Start { get; }
        public Dictionary<T, double> Distances { get; }
        public Dictionary<T, HashSet<T>> ReversedEdges { get; }

        readonly Graphs.PriorityQueue<T> tasks;
        readonly IGraph<T> graph;
        readonly HashSet<T> continueNodes;

        public bool Finished => (tasks == null || tasks.Count == 0) && (continueNodes == null || continueNodes.Count == 0);

        public DistanceMap(T start, Dictionary<T, double> distances, Dictionary<T, HashSet<T>> reversedEdges)
        {
            Start = start;
            Distances = distances;
            ReversedEdges = reversedEdges;

            graph = null;
            tasks = null;
            continueNodes = null;
        }

        public DistanceMap(IGraph<T> graph, T start, double maxDistance = double.PositiveInfinity)
            : this(graph, start, null, maxDistance)
        {
        }

        public DistanceMap(IGraph<T> graph, T start, T goal, double maxDistance = double.PositiveInfinity)
            : this(graph, start, new HashSet<T>() { goal }, maxDistance)
        {
        }

        public DistanceMap(IGraph<T> graph, T start, HashSet<T> goals, double maxDistance = double.PositiveInfinity)
        {
            this.graph = graph;
            Start = start;
            Distances = new Dictionary<T, double>();
            ReversedEdges = new Dictionary<T, HashSet<T>>();
            tasks = new Graphs.PriorityQueue<T>();
            continueNodes = new HashSet<T>();
            tasks.Enqueue(OrderKeyConvert.ToUint64(0.0), Start);
            Distances.Add(Start, 0.0);

            Continue(goals, maxDistance);
        }

        public void Continue(HashSet<T> goals, double maxDistance = double.PositiveInfinity)
        {
            foreach (var it in continueNodes)
            {
                tasks.Enqueue(OrderKeyConvert.ToUint64(0.0), it);
            }
            continueNodes.Clear();

            while (tasks.Count > 0)
            {
                var currentNode = tasks.Peek;

                if (goals != null && goals.Contains(currentNode))
                {
                    break;
                }

                tasks.Dequeue();

                var startToCurrentNodeDistance = Distances[currentNode];
                foreach (var (nextNode, edgeWeight) in graph.GetEdgesFrom(currentNode))
                {
                    UnityEngine.Debug.Assert(edgeWeight > 0.0, "weight must be greater than 0.0");

                    var startToNextNodeDistance = startToCurrentNodeDistance + edgeWeight;
                    if (startToNextNodeDistance > maxDistance)
                    {
                        continueNodes.Add(currentNode);
                    }
                    else
                    {
                        if (Distances.TryGetValue(nextNode, out var oldDistance))
                        {
                            if (oldDistance >= startToNextNodeDistance)
                            {
                                var nearNodes = ReversedEdges[nextNode];
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
                            ReversedEdges.Add(nextNode, nearNodes);
                            nearNodes.Add(currentNode);
                        }

                        Distances[nextNode] = startToNextNodeDistance;
                        tasks.Enqueue(OrderKeyConvert.ToUint64(startToNextNodeDistance), nextNode);
                    }
                }
            }
        }

        public IEnumerable<T[]> SearchPaths(T goal)
        {
            if (!Distances.ContainsKey(goal))
            {
                yield break;
            }

            var tasks = new Stack<T[]>();
            tasks.Push(new [] { goal });

            while (tasks.Count > 0)
            {
                var path = tasks.Pop();

                if (!ReversedEdges.TryGetValue(path[0], out var nearNodes))
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

        public T[] SearchPath(T goal)
        {
            var result = new List<T>();
            SearchPath(goal, ref result);
            return result.ToArray();
        }

        public void SearchPath(T goal, ref List<T> result)
        {
            result.Clear();

            if (!Distances.ContainsKey(goal))
            {
                return;
            }

            result.Add(goal);

            while (true)
            {
                if (!ReversedEdges.TryGetValue(result[0], out var nearNodes))
                {
                    break;
                }
                result.Insert(0, nearNodes.First());
            }
        }
    }
}
