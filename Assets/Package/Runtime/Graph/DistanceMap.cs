#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace TSKT
{
    public readonly struct DistanceMap<T>
    {
        readonly DistanceMapCore<T> core;
        public readonly T Start => core.Start; 
        public readonly IReadOnlyDictionary<T, float> Distances => core.Distances;
        public readonly IReadOnlyDictionary<T, List<T>> ReversedEdges => core.ReversedEdges;

        readonly Graphs.PriorityQueue<T> tasks;
        readonly IGraph<T> graph;

        public readonly bool Completed => tasks.Count == 0;


        public DistanceMap(IGraph<T> graph, in T start)
        {
            core = new(start, new Dictionary<T, float>(), new Dictionary<T, List<T>>());
            this.graph = graph;
            tasks = new Graphs.PriorityQueue<T>();
            tasks.Enqueue(0f, 0f, start);
            core.Distances.Add(start, 0f);
        }
        public readonly void SolveWithin(float maxDistance)
        {
            TrySolveAny(Span<T>.Empty, out _, maxDistance);
        }

        public readonly bool TrySolveAny(ReadOnlySpan<T> goals, out T result, double maxDistance = double.PositiveInfinity)
        {
            foreach (var it in goals)
            {
                if (Distances.ContainsKey(it))
                {
                    result = it;
                    return true;
                }
            }

            var found = false;
            result = default;
            var continueNodes = new Dictionary<T, float>();

            var comparer = EqualityComparer<T>.Default;
            while (tasks.Count > 0)
            {
                var currentNode = tasks.Peek;
                {
                    var shouldBreak = false;
                    foreach (var goal in goals)
                    {
                        if (comparer.Equals(goal, currentNode))
                        {
                            found = true;
                            result = goal;
                            shouldBreak = true;
                            break;
                        }
                    }
                    if (shouldBreak)
                    {
                        break;
                    }
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
                            core.ReversedEdges.Add(nextNode, nearNodes);
                        }

                        core.Distances[nextNode] = startToNextNodeDistance;
                        tasks.Enqueue(startToNextNodeDistance, 0f, nextNode);
                    }
                }
            }

            foreach (var (node, distance) in continueNodes)
            {
                tasks.Enqueue(distance, 0f, node);
            }
            return found;
        }
        readonly void Solve(T goal)
        {
            TrySolveAny(new[] { goal }, out _);
        }
        public readonly void SearchPaths(T goal, Span<T[]> destination, out int writtenCount)
        {
            Solve(goal);
            core.SearchPaths(goal, destination, out writtenCount);
        }
        public readonly T[] SearchPath(in T goal)
        {
            Solve(goal);
            return core.SearchPath(goal);
        }
        public readonly void SearchPath(in T goal, ref List<T> result)
        {
            Solve(goal);
            core.SearchPath(goal, ref result);
        }
    }

    public readonly struct DistanceMapCore<T>
    {
        public readonly T Start { get; }
        public readonly Dictionary<T, float> Distances { get; }
        public readonly Dictionary<T, List<T>> ReversedEdges { get; }

        public DistanceMapCore(in T start, Dictionary<T, float> distances, Dictionary<T, List<T>> reversedEdges)
        {
            Start = start;
            Distances = distances;
            ReversedEdges = reversedEdges;
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
