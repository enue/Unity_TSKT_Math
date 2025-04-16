using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
#nullable enable

namespace TSKT
{
    public readonly struct AStarSearchU<T> where T : unmanaged, IEquatable<T>
    {
        public readonly IGraphU<T> graph;
        readonly System.Func<T, T, double> heuristicFunction;
        public readonly DistanceMapU<T> memo;
        public readonly T Start => memo.Start;
        readonly List<T> tasksToResume;

        public AStarSearchU(IGraphU<T> graph, in T start, System.Func<T, T, double> heuristicFunction)
        {
            this.heuristicFunction = heuristicFunction;
            this.graph = graph;

            memo = new DistanceMapU<T>(
                start,
                new Dictionary<T, double>(),
                new Dictionary<T, T[]>());
            memo.Distances.Add(start, 0.0);
            tasksToResume = new List<T>() { start };
        }

        public readonly T[] SearchPath(T goal, double maxDistance = double.PositiveInfinity)
        {
            return SearchPathToNearestGoal(maxDistance, goal);
        }

        public readonly T[] SearchPathToNearestGoal(params T[] goals)
        {
            return SearchPathToNearestGoal(goals.AsSpan());
        }

        public readonly T[] SearchPathToNearestGoal(ReadOnlySpan<T> goals)
        {
            return SearchPathToNearestGoal(double.PositiveInfinity, goals);
        }
        public readonly T[] SearchPathToNearestGoal(double maxDistance, params T[] goals)
        {
            return SearchPathToNearestGoal(maxDistance, goals.AsSpan());
        }

        public readonly T[] SearchPathToNearestGoal(double maxDistance, ReadOnlySpan<T> goals)
        {
            var containsAllGoals = true;
            var minDistance = double.PositiveInfinity;
            T nearestGoal = default;
            foreach (var goal in goals)
            {
                if (tasksToResume.Contains(goal))
                {
                    containsAllGoals = false;
                    break;
                }
                else if (memo.Distances.TryGetValue(goal, out var distance))
                {
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        nearestGoal = goal;
                    }
                }
                else
                {
                    containsAllGoals = false;
                    break;
                }
            }
            if (containsAllGoals)
            {
                return memo.SearchPath(nearestGoal!);
            }
            if (SolvePath(goals, searchAllPaths: false, maxDistance: maxDistance, out nearestGoal))
            {
                return memo.SearchPath(nearestGoal);
            }
            return System.Array.Empty<T>();
        }
        public readonly void SearchAllPaths(T goal, double maxDistance, Span<T[]> destination, out int writtenCount)
        {
            Span<T> goals = stackalloc T[1] { goal };
            SearchAllPathsToNearestGoal(maxDistance, goals, destination, out writtenCount);
        }

        public readonly void SearchAllPathsToNearestGoal(ReadOnlySpan<T> goals, Span<T[]> destination, out int writtenCount)
        {
            SearchAllPathsToNearestGoal(double.PositiveInfinity, goals, destination, out writtenCount);
        }

        public readonly void SearchAllPathsToNearestGoal(double maxDistance, ReadOnlySpan<T> goals, Span<T[]> destination, out int writtenCount)
        {
            if (SolvePath(goals, searchAllPaths: true, maxDistance: maxDistance, out var nearestGoal))
            {
                memo.SearchPaths(nearestGoal, destination, out writtenCount);
            }
            else
            {
                writtenCount = 0;
            }
        }

        readonly bool SolvePath(in ReadOnlySpan<T> goals, bool searchAllPaths, double maxDistance, out T nearestGoal)
        {
            using var tasks = new Graphs.DoublePriorityQueueU<(T node, double expectedDistance)>();
            foreach (var it in tasksToResume)
            {
                memo.Distances.TryGetValue(it, out var startToItDistance);

                double expectedDistance = double.PositiveInfinity;
                foreach (var goal in goals)
                {
                    var h = startToItDistance + heuristicFunction(it, goal);
                    if (expectedDistance > h)
                    {
                        expectedDistance = h;
                    }
                }
                tasks.Enqueue(expectedDistance, -startToItDistance, (it, expectedDistance));
            }
            tasksToResume.Clear();
            bool memoModified = true;

            Span<(T, double)> buffer = stackalloc (T, double)[graph.MaxEdgeCount];
            while (tasks.Count > 0)
            {
                var (currentNode, expectedDistance) = tasks.Peek;
                if (memoModified)
                {
                    bool shouldBreak = false;
                    foreach (var it in goals)
                    {
                        if (memo.Distances.TryGetValue(it, out var startToGoalDistance))
                        {
                            if (searchAllPaths)
                            {
                                if (expectedDistance > startToGoalDistance)
                                {
                                    shouldBreak = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (expectedDistance >= startToGoalDistance)
                                {
                                    shouldBreak = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (shouldBreak)
                    {
                        break;
                    }
                    memoModified = false;
                }
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
                                newNearNodes = new T[] { currentNode };
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

                    double nextExpectedDistance = double.PositiveInfinity;
                    foreach (var goal in goals)
                    {
                        var h = heuristicFunction(next, goal) + startToNextNodeDistance;
                        if (nextExpectedDistance > h)
                        {
                            nextExpectedDistance = h;
                        }
                    }

                    // nextExpectedDistanceは昇順、startToNextNodeDistanceは降順で処理する
                    tasks.Enqueue(nextExpectedDistance, -startToNextNodeDistance, (next, nextExpectedDistance));
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

            var nearestGoalDistance = double.PositiveInfinity;
            nearestGoal = default!;
            bool pathExist = false;
            foreach (var it in goals)
            {
                if (memo.Distances.TryGetValue(it, out var d))
                {
                    if (nearestGoalDistance > d)
                    {
                        nearestGoalDistance = d;
                        nearestGoal = it;
                        pathExist = true;
                    }
                }
            }

            return pathExist;
        }

        public readonly Dictionary<T, double> GetDistances(params T[] nodes)
        {
            var distances = memo.Distances;
            return nodes.ToDictionary(_ => _, _ => distances[_]);
        }

        public static T[] SearchPath(IGraphU<T> graph, in T start, in T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearchU<T>(graph, start, heuristicFunction);
            return search.SearchPath(goal);
        }
        public static void SearchAllPaths(IGraphU<T> graph, in T start, in T goal, System.Func<T, T, double> heuristicFunction, Span<T[]> destination, out int writtenCount)
        {
            var search = new AStarSearchU<T>(graph, start, heuristicFunction);
            search.SearchAllPaths(goal, double.PositiveInfinity, destination, out writtenCount);
        }
    }
}
