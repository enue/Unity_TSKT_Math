#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace TSKT
{
    public readonly struct UnmanagedDistanceMap<T> where T : unmanaged, IEquatable<T>
    {
        readonly UnmanagedDistanceMapCore<T> core;
        public T Start => core.Start;
        public IReadOnlyDictionary<T, float> Distances => core.Distances;
        public IReadOnlyDictionary<T, T[]> ReversedEdges => core.ReversedEdges;

        readonly Graphs.PriorityQueue<T> tasks;
        readonly IUnmanagedGraph<T> graph;

        public bool Completed => tasks.Count == 0;


        public UnmanagedDistanceMap(IUnmanagedGraph<T> graph, in T start)
        {
            core = new UnmanagedDistanceMapCore<T>(start,
                new Dictionary<T, float>
            {
                { start, 0f }
            },new Dictionary<T, T[]>());
            this.graph = graph;

            tasks = new Graphs.PriorityQueue<T>();
            tasks.Enqueue(0f, 0f, start);
        }

        public readonly void SolveWithin(float maxDistance)
        {
            TrySolveAny(Span<T>.Empty, out _, maxDistance);
        }

        public readonly bool TrySolveAny(ReadOnlySpan<T> goals, out T result, float maxDistance = float.PositiveInfinity)
        {
            foreach (var it in goals)
            {
                if (core.Distances.ContainsKey(it))
                {
                    result = it;
                    return true;
                }
            }

            var found = false;
            result = default;
            var continueNodes = new NativeHashMap<T, float>(32, Allocator.Temp);

            Span<(T, float)> buffer = stackalloc (T, float)[graph.MaxEdgeCountFromOneNode];
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

                var startToCurrentNodeDistance = core.Distances[currentNode];
                graph.GetEdgesFrom(currentNode, buffer, out var writtenCount);
                foreach (var (nextNode, edgeWeight) in buffer[..writtenCount])
                {
                    UnityEngine.Debug.Assert(edgeWeight > 0f, "weight must be greater than 0.0");

                    var startToNextNodeDistance = startToCurrentNodeDistance + edgeWeight;
                    if (startToNextNodeDistance > maxDistance)
                    {
                        continueNodes[currentNode] = startToCurrentNodeDistance;
                    }
                    else
                    {
                        if (core.Distances.TryGetValue(nextNode, out var oldDistance))
                        {
                            if (oldDistance >= startToNextNodeDistance)
                            {
                                var nearNodes = ReversedEdges[nextNode];
                                T[] newNearNodes = nearNodes;
                                if (oldDistance > startToNextNodeDistance)
                                {
                                    newNearNodes = new T[] { currentNode };
                                }
                                else if (Array.IndexOf(newNearNodes, currentNode) == -1)
                                {
                                    newNearNodes = newNearNodes.Append(currentNode).ToArray();
                                }
                                if (newNearNodes != nearNodes)
                                {
                                    core.ReversedEdges[nextNode] = newNearNodes;
                                }
                            }
                            if (oldDistance <= startToNextNodeDistance)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            var nearNodes = new T[] { currentNode };
                            core.ReversedEdges.Add(nextNode, nearNodes);
                        }

                        core.Distances[nextNode] = startToNextNodeDistance;
                        tasks.Enqueue(startToNextNodeDistance, 0f, nextNode);
                    }
                }
            }

            foreach (var it in continueNodes)
            {
                tasks.Enqueue(it.Value, 0f, it.Key);
            }
            continueNodes.Dispose();

            return found;
        }

        readonly void Solve(T goal)
        {
            Span<T> goals = stackalloc T[] { goal };
            TrySolveAny(goals, out _);
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


    public readonly struct UnmanagedDistanceMapCore<T> where T : unmanaged, IEquatable<T>
    {
        public T Start { get; }
        public Dictionary<T, float> Distances { get; }
        public Dictionary<T, T[]> ReversedEdges { get; }

        public UnmanagedDistanceMapCore(in T start, Dictionary<T, float> distances, Dictionary<T, T[]> reversedEdges)
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
            var tasks = new Stack<NativeArray<T>>();
            try
            {
                var first = new NativeArray<T>(1, Allocator.Temp);
                first[0] = goal;
                tasks.Push(first);

                while (tasks.Count > 0)
                {
                    var path = tasks.Pop();
                    using var o = path;
                    if (!ReversedEdges.TryGetValue(path[0], out var nearNodes))
                    {
                        destination[writtenCount] = path.ToArray();
                        ++writtenCount;
                        if (destination.Length <= writtenCount)
                        {
                            return;
                        }
                        continue;
                    }
                    foreach (var nearNode in nearNodes)
                    {
                        var next = new NativeArray<T>(path.Length + 1, Allocator.Temp);
                        next[0] = nearNode;
                        next.GetSubArray(1, next.Length - 1).CopyFrom(path);
                        tasks.Push(next);
                    }
                }
            }
            finally
            {
                foreach (var it in tasks)
                {
                    it.Dispose();
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
