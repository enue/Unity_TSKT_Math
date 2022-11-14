using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace TSKT
{
    public readonly struct DistanceMap<T>
    {
        readonly public T Start { get; }
        readonly public Dictionary<T, double> Distances { get; }
        readonly public Dictionary<T, List<T>> ReversedEdges { get; }

        readonly Graphs.PriorityQueue<T>? tasks;
        readonly IGraph<T>? graph;

        readonly public bool Finished => (tasks == null || tasks.Count == 0);

        public DistanceMap(in T start, Dictionary<T, double> distances, Dictionary<T, List<T>> reversedEdges)
        {
            Start = start;
            Distances = distances;
            ReversedEdges = reversedEdges;

            graph = null;
            tasks = null;
        }

        public DistanceMap(IGraph<T> graph, in T start, double maxDistance = double.PositiveInfinity)
            : this(graph, start, null, maxDistance)
        {
        }

        public DistanceMap(IGraph<T> graph, in T start, in T goal, double maxDistance = double.PositiveInfinity)
            : this(graph, start, new[] { goal }, maxDistance)
        {
        }

        public DistanceMap(IGraph<T> graph, in T start, T[]? goals, double maxDistance = double.PositiveInfinity)
        {
            this.graph = graph;
            Start = start;
            Distances = new Dictionary<T, double>();
            ReversedEdges = new Dictionary<T, List<T>>();
            tasks = new Graphs.PriorityQueue<T>();
            tasks.Enqueue(OrderKeyConvert.ToUint64(0.0), Start);
            Distances.Add(Start, 0.0);

            Solve(goals, maxDistance);
        }

        readonly public void Solve(T[]? goals, double maxDistance = double.PositiveInfinity)
        {
            if (tasks == null)
            {
                throw new System.NullReferenceException();
            }
            if (graph == null)
            {
                throw new System.NullReferenceException();
            }

            if (goals != null)
            {
                foreach (var it in goals)
                {
                    if (Distances.ContainsKey(it))
                    {
                        return;
                    }
                }
            }

            var continueNodes = new Dictionary<T, double>();

            while (tasks.Count > 0)
            {
                var currentNode = tasks.Peek;
                if (goals != null && Array.IndexOf(goals, currentNode) >= 0)
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
                        continueNodes[currentNode] = startToCurrentNodeDistance;
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
                                if (!nearNodes.Contains(currentNode))
                                {
                                    nearNodes.Add(currentNode);
                                }
                            }
                            if (oldDistance <= startToNextNodeDistance)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            var nearNodes = new List<T>() { currentNode };
                            ReversedEdges.Add(nextNode, nearNodes);
                        }

                        Distances[nextNode] = startToNextNodeDistance;
                        tasks.Enqueue(OrderKeyConvert.ToUint64(startToNextNodeDistance), nextNode);
                    }
                }
            }

            foreach (var (node, distance) in continueNodes)
            {
                tasks.Enqueue(OrderKeyConvert.ToUint64(distance), node);
            }
        }

        public readonly void SearchPaths(T goal, Span<T[]> destination, out int writtenCount)
        {
            if (!Distances.ContainsKey(goal))
            {
                writtenCount = 0;
                return;
            }
            if (destination.Length == 0)
            {
                writtenCount = 0;
                return;
            }
            writtenCount = 0;
            var tasks = new Stack<(IMemoryOwner<T> owner, Memory<T> memory)>();
            try
            {
                var first = MemoryPool<T>.Shared.Rent(1);
                first.Memory.Span[0] = goal;
                tasks.Push((first, first.Memory[..1]));

                while (tasks.Count > 0)
                {
                    var path = tasks.Pop();
                    using var o = path.owner;
                    if (!ReversedEdges.TryGetValue(path.memory.Span[0], out var nearNodes))
                    {
                        destination[writtenCount] = path.memory.ToArray();
                        ++writtenCount;
                        if (destination.Length <= writtenCount)
                        {
                            return;
                        }
                        continue;
                    }
                    foreach (var nearNode in nearNodes)
                    {
                        var owner = MemoryPool<T>.Shared.Rent(path.memory.Length + 1);
                        var memory = owner.Memory[..(path.memory.Length + 1)];
                        memory.Span[0] = nearNode;
                        path.memory.CopyTo(memory[1..]);
                        tasks.Push((owner, memory));
                    }
                }
            }
            finally
            {
                foreach (var (owner, _) in tasks)
                {
                    owner.Dispose();
                }
            }
        }

        public readonly T[] SearchPath(in T goal)
        {
            var result = new List<T>();
            SearchPath(goal, ref result);
            return result.ToArray();
        }

        public readonly void SearchPath(in T goal, ref List<T> result)
        {
            result.Clear();

            if (!Distances.ContainsKey(goal))
            {
                return;
            }

            result.Add(goal);

            while (true)
            {
                if (!ReversedEdges.TryGetValue(result[^1], out var nearNodes))
                {
                    break;
                }
                result.Add(nearNodes.First());
            }

            result.Reverse();
        }
    }
}
