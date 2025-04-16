#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace TSKT
{
    public readonly struct DistanceMapU<T> where T : unmanaged, IEquatable<T>
    {
        public readonly T Start { get; }
        public readonly Dictionary<T, double> Distances { get; }
        public readonly Dictionary<T, T[]> ReversedEdges { get; }

        readonly Graphs.PriorityQueue<T>? tasks;
        readonly IGraphU<T>? graph;

        readonly public bool Finished => (tasks == null || tasks.Count == 0);

        public DistanceMapU(in T start, Dictionary<T, double> distances, Dictionary<T, T[]> reversedEdges)
        {
            Start = start;
            Distances = distances;
            ReversedEdges = reversedEdges;

            graph = null;
            tasks = null;
        }

        public DistanceMapU(IGraphU<T> graph, in T start, double maxDistance = double.PositiveInfinity)
            : this(graph, start, null, maxDistance)
        {
        }

        public DistanceMapU(IGraphU<T> graph, in T start, in T goal, double maxDistance = double.PositiveInfinity)
            : this(graph, start, new[] { goal }, maxDistance)
        {
        }

        public DistanceMapU(IGraphU<T> graph, in T start, ReadOnlySpan<T> goals, double maxDistance = double.PositiveInfinity)
        {
            this.graph = graph;
            Start = start;
            Distances = new Dictionary<T, double>();
            ReversedEdges = new Dictionary<T, T[]>();
            tasks = new Graphs.PriorityQueue<T>();
            tasks.Enqueue(OrderKeyConvert.ToUint64(0.0), Start);
            Distances.Add(Start, 0.0);

            Solve(goals, maxDistance);
        }
        public readonly void Solve(T[]? goals, double maxDistance = double.PositiveInfinity)
        {
            if (goals == null)
            {
                Solve(Span<T>.Empty, maxDistance);
            }
            else
            {
                Solve(goals.AsSpan(), maxDistance);
            }
        }

        public readonly void Solve(ReadOnlySpan<T> goals, double maxDistance = double.PositiveInfinity)
        {
            if (tasks == null)
            {
                throw new System.NullReferenceException();
            }
            if (graph == null)
            {
                throw new System.NullReferenceException();
            }

            foreach (var it in goals)
            {
                if (Distances.ContainsKey(it))
                {
                    return;
                }
            }

            var continueNodes = new NativeHashMap<T, double>(32, Allocator.Temp);

            Span<(T, double)> buffer = stackalloc (T, double)[graph.MaxEdgeCount];
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
                graph.GetEdgesFrom(currentNode, buffer, out var writtenCount);
                foreach (var (nextNode, edgeWeight) in buffer[..writtenCount])
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
                                    ReversedEdges[nextNode] = newNearNodes;
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
                            ReversedEdges.Add(nextNode, nearNodes);
                        }

                        Distances[nextNode] = startToNextNodeDistance;
                        tasks.Enqueue(OrderKeyConvert.ToUint64(startToNextNodeDistance), nextNode);
                    }
                }
            }

            foreach (var it in continueNodes)
            {
                tasks.Enqueue(OrderKeyConvert.ToUint64(it.Value), it.Key);
            }
            continueNodes.Dispose();
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
                        path.CopyTo(next.GetSubArray(1, next.Length - 1));
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
