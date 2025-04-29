using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.Collections;
#nullable enable

namespace TSKT
{
    public readonly struct UnmanagedAStarSearch<T> where T : unmanaged, IEquatable<T>
    {
        public readonly IUnmanagedGraph<T> graph;
        readonly System.Func<T, T, float> heuristicFunction;
        public readonly UnmanagedDistanceMapCore<T> memo;
        public readonly T Start => memo.Start;
        readonly List<T> tasksToResume;

        public UnmanagedAStarSearch(IUnmanagedGraph<T> graph, in T start, System.Func<T, T, float> heuristicFunction)
        {
            this.heuristicFunction = heuristicFunction;
            this.graph = graph;

            memo = new UnmanagedDistanceMapCore<T>(
                start,
                new Dictionary<T, float>
                {
                    { start, 0f }
                },
                new Dictionary<T, T[]>());
            tasksToResume = new List<T>() { start };
        }

        public readonly bool AnyPath(in T goal, float maxDistance = float.PositiveInfinity)
        {
            return TrySolve(goal, searchAllPaths: false, maxDistance);
        }
        public readonly T[] SearchPath(in T goal, float maxDistance = float.PositiveInfinity)
        {
            if (TrySolve(goal, searchAllPaths: false, maxDistance))
            {
                return memo.SearchPath(goal);
            }
            return System.Array.Empty<T>();
        }
        public readonly void SearchAllPaths(in T goal, float maxDistance, Span<T[]> destination, out int writtenCount)
        {
            if (TrySolve(goal, searchAllPaths: true, maxDistance: maxDistance))
            {
                memo.SearchPaths(goal, destination, out writtenCount);
            }
            else
            {
                writtenCount = 0;
            }
        }

        public readonly bool SearchNearestNode(in ReadOnlySpan<T> goals, float maxDistance, out T result)
        {
            using var sortedGoals = new Graphs.UnmanagedFloatPriorityQueue<T>(tasksToResume.Count, Allocator.Temp);
            foreach (var it in goals)
            {
                var expectedDistance = heuristicFunction(Start, it);
                sortedGoals.Enqueue(expectedDistance, it);
            }

            result = default;
            float nearestGoalDistance = float.PositiveInfinity;
            bool found = false;
            while (sortedGoals.Count > 0)
            {
                var (expectedDistance, goal) = sortedGoals.DequeueKeyAndValue();
                if (expectedDistance >= nearestGoalDistance)
                {
                    break;
                }
                if (TrySolve(goal, searchAllPaths: false, maxDistance))
                {
                    var distance = memo.Distances[goal];
                    if (distance < nearestGoalDistance)
                    {
                        nearestGoalDistance = distance;
                        result = goal;
                        found = true;
                    }
                }
            }

            return found;
        }

        readonly bool TrySolve(in T goal, bool searchAllPaths, float maxDistance)
        {
            using var tasks = new Graphs.UnmanagedPriorityQueue<(T node, float expectedDistance)>(tasksToResume.Count, Allocator.Temp);
            foreach (var it in tasksToResume)
            {
                var startToItDistance = memo.Distances[it];
                var h = heuristicFunction(it, goal);
                var expectedDistance = startToItDistance + h;
                tasks.Enqueue(expectedDistance, h, (it, h));
            }
            tasksToResume.Clear();
            bool memoModified = true;

            Span<(T, float)> buffer = stackalloc (T, float)[graph.MaxEdgeCountFromOneNode];
            while (tasks.Count > 0)
            {
                var (currentNode, expectedDistance) = tasks.Peek;
                if (memoModified)
                {
                    if (memo.Distances.TryGetValue(goal, out var startToGoalDistance))
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
                }
                memoModified = false;
                tasks.Dequeue();
                var startToCurrentNodeDistance = memo.Distances[currentNode];

                graph.GetEdgesFrom(currentNode, buffer, out var writtenCount);
                foreach (var (next, edgeWeight) in buffer[..writtenCount])
                {
                    var startToNextNodeDistance = edgeWeight + startToCurrentNodeDistance;
                    if (startToNextNodeDistance > maxDistance)
                    {
                        if (!tasksToResume.Contains(currentNode))
                        {
                            tasksToResume.Add(currentNode);
                        }
                        continue;
                    }

                    if (memo.Distances.TryGetValue(next, out var oldDistance))
                    {
                        if (oldDistance >= startToNextNodeDistance)
                        {
                            var nearNodes = memo.ReversedEdges[next];
                            T[] newNearNodes;
                            if (oldDistance > startToNextNodeDistance)
                            {
                                if (nearNodes.Length == 1)
                                {
                                    newNearNodes = nearNodes;
                                    newNearNodes[0] = currentNode;
                                }
                                else
                                {
                                    newNearNodes = new T[] { currentNode };
                                }
                            }
                            else if (Array.IndexOf(nearNodes, currentNode) == -1)
                            {
                                newNearNodes = new T[nearNodes.Length + 1];
                                nearNodes.CopyTo(newNearNodes, 0);
                                newNearNodes[^1] = currentNode;
                            }
                            else
                            {
                                newNearNodes = nearNodes;
                            }
                            if (newNearNodes != nearNodes)
                            {
                                memo.ReversedEdges[next] = newNearNodes;
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
                        memo.ReversedEdges.Add(next, nearNodes);
                    }

                    memo.Distances[next] = startToNextNodeDistance;
                    memoModified = true;

                    var h = heuristicFunction(next, goal);
                    var nextExpectedDistance = startToNextNodeDistance + h;
                    tasks.Enqueue(nextExpectedDistance, h, (next, nextExpectedDistance));
                }
            }
            while (tasks.Count > 0)
            {
                var n = tasks.Dequeue().node;
                if (!tasksToResume.Contains(n))
                {
                    tasksToResume.Add(n);
                }
            }

            return memo.Distances.ContainsKey(goal);
        }
    }
}
