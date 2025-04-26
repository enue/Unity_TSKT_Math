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

        public readonly T[] SearchPath(T goal, double maxDistance = double.PositiveInfinity)
        {
            if (TrySolve(goal, searchAllPaths: false, maxDistance))
            {
                return memo.SearchPath(goal);

            }
            return System.Array.Empty<T>();
        }
        public readonly void SearchAllPaths(T goal, double maxDistance, Span<T[]> destination, out int writtenCount)
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
        public readonly T[] SearchPathToNearestGoal(params T[] goals)
        {
            return SearchPathToNearestGoal(goals.AsSpan());
        }

        public readonly T[] SearchPathToNearestGoal(in ReadOnlySpan<T> goals)
        {
            return SearchPathToNearestGoal(double.PositiveInfinity, goals);
        }
        public readonly T[] SearchPathToNearestGoal(double maxDistance, params T[] goals)
        {
            return SearchPathToNearestGoal(maxDistance, goals.AsSpan());
        }

        public readonly T[] SearchPathToNearestGoal(double maxDistance, in ReadOnlySpan<T> goals)
        {
            if (TrySolveAny(goals, searchAllPaths: false, maxDistance: maxDistance, out var nearestGoal))
            {
                return memo.SearchPath(nearestGoal);
            }
            return System.Array.Empty<T>();
        }

        public readonly void SearchAllPathsToNearestGoal(in ReadOnlySpan<T> goals, Span<T[]> destination, out int writtenCount)
        {
            SearchAllPathsToNearestGoal(double.PositiveInfinity, goals, destination, out writtenCount);
        }

        public readonly void SearchAllPathsToNearestGoal(double maxDistance, in ReadOnlySpan<T> goals, Span<T[]> destination, out int writtenCount)
        {
            if (TrySolveAny(goals, searchAllPaths: true, maxDistance: maxDistance, out var nearestGoal))
            {
                memo.SearchPaths(nearestGoal, destination, out writtenCount);
            }
            else
            {
                writtenCount = 0;
            }
        }

        public readonly bool TrySolveAny(in ReadOnlySpan<T> goals, bool searchAllPaths, double maxDistance, out T nearestGoal)
        {
            using var sortedGoals = new Graphs.UnmanagedFloatPriorityQueue<T>(tasksToResume.Count, Allocator.Temp);
            foreach (var it in goals)
            {
                var expectedDistance = heuristicFunction(Start, it);
                sortedGoals.Enqueue(expectedDistance, it);
            }

            nearestGoal = default;
            float nearestGoalDistance = float.PositiveInfinity;
            bool foundNearestGoal = false;
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
                        nearestGoal = goal;
                        foundNearestGoal = true;
                    }
                }
            }

            if (foundNearestGoal)
            {
                if (searchAllPaths)
                {
                    TrySolve(nearestGoal, searchAllPaths: true, maxDistance);
                }
                return true;
            }
            return false;
        }

        public readonly bool TrySolve(in T goal, bool searchAllPaths, double maxDistance)
        {
            using var tasks = new Graphs.UnmanagedPriorityQueue<(T node, float expectedDistance)>(tasksToResume.Count, Allocator.Temp);
            foreach (var it in tasksToResume)
            {
                memo.Distances.TryGetValue(it, out var startToItDistance);
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
                            T[] newNearNodes = nearNodes;
                            if (oldDistance > startToNextNodeDistance)
                            {
                                if (newNearNodes.Length == 1)
                                {
                                    newNearNodes[0] = currentNode;
                                }
                                else
                                {
                                    newNearNodes = new T[] { currentNode };
                                }
                            }
                            else if (Array.IndexOf(newNearNodes, currentNode) == -1)
                            {
                                newNearNodes = newNearNodes.Append(currentNode).ToArray();
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

        public readonly Dictionary<T, float> GetDistances(params T[] nodes)
        {
            var distances = memo.Distances;
            return nodes.ToDictionary(_ => _, _ => distances[_]);
        }
    }
}
