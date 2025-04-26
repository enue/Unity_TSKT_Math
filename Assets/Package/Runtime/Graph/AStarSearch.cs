using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
#nullable enable

namespace TSKT
{
    public readonly struct AStarSearch<T>
    {
        public readonly IGraph<T> graph;
        readonly System.Func<T, T, float> heuristicFunction;
        public readonly DistanceMapCore<T> memo;
        public readonly T Start => memo.Start;
        readonly List<T> tasksToResume;

        public AStarSearch(IGraph<T> graph, in T start, System.Func<T, T, float> heuristicFunction)
        {
            this.heuristicFunction = heuristicFunction;
            this.graph = graph;

            memo = new DistanceMapCore<T>(
                start,
                new Dictionary<T, float>(),
                new Dictionary<T, List<T>>());
            memo.Distances.Add(start, 0f);
            tasksToResume = new List<T>() { start };
        }

        public readonly T[] SearchPath(T goal, float maxDistance = float.PositiveInfinity)
        {
            return SearchPathToNearestGoal(maxDistance, goal);
        }
        public readonly void SearchAllPaths(T goal, float maxDistance, Span<T[]> destination, out int writtenCount)
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

        public readonly T[] SearchPathToNearestGoal(ReadOnlySpan<T> goals)
        {
            return SearchPathToNearestGoal(float.PositiveInfinity, goals);
        }
        public readonly T[] SearchPathToNearestGoal(float maxDistance, params T[] goals)
        {
            return SearchPathToNearestGoal(maxDistance, goals.AsSpan());
        }

        public readonly T[] SearchPathToNearestGoal(float maxDistance, ReadOnlySpan<T> goals)
        {
            if (TrySolveAny(goals, searchAllPaths: false, maxDistance: maxDistance, out var nearestGoal))
            {
                return memo.SearchPath(nearestGoal);
            }
            return System.Array.Empty<T>();
        }

        public readonly void SearchAllPathsToNearestGoal(in ReadOnlySpan<T> goals, Span<T[]> destination, out int writtenCount)
        {
            SearchAllPathsToNearestGoal(float.PositiveInfinity, goals, destination, out writtenCount);
        }

        public readonly void SearchAllPathsToNearestGoal(float maxDistance, in ReadOnlySpan<T> goals, Span<T[]> destination, out int writtenCount)
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
        public readonly bool TrySolveAny(in ReadOnlySpan<T> goals, bool searchAllPaths, float maxDistance, out T nearestGoal)
        {
            var sortedGoals = new Graphs.FloatPriorityQueue<T>();
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

        public readonly bool TrySolve(T goal, bool searchAllPaths, float maxDistance)
        {
            var tasks = new Graphs.PriorityQueue<(T node, float expectedDistance)>();
            foreach (var it in tasksToResume)
            {
                memo.Distances.TryGetValue(it, out var startToItDistance);

                var h = heuristicFunction(it, goal);
                var expectedDistance = startToItDistance +h;
                tasks.Enqueue(expectedDistance, h, (it, expectedDistance));
            }
            tasksToResume.Clear();
            bool memoModified = true;

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
                    memoModified = false;
                }
                tasks.Dequeue();
                var startToCurrentNodeDistance = memo.Distances[currentNode];

                foreach (var (next, edgeWeight) in graph.GetEdgesFrom(currentNode))
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
                        memo.ReversedEdges.Add(next, nearNodes);
                    }

                    memo.Distances[next] = startToNextNodeDistance;
                    memoModified = true;

                    float h = heuristicFunction(next, goal);
                    float nextExpectedDistance = startToNextNodeDistance = h;

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
